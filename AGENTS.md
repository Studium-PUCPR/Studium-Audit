# Studium-Audit — Project State & Agent Context (`AGENTS.md`)

> **MANDATORY AGENT DIRECTIVE:**
> After **every user prompt**, always evaluate whether we reached a useful architectural conclusion, resolved a design question, or made code/configuration changes. If so, **update both the relevant sections and Section 3 (`Recent Changes History`) in this `./AGENTS.md` file before finishing your turn** so any future chat session immediately has full context of the project state.

---

## 1. Architectural Decisions & Conclusions Log

### 1.1 Decoupled Centralized Audit Microservice (5th Microservice)
- **Problem Solved:** Ensures 5 full backend microservices with the Database-per-Service pattern. Previously, audit logs were stored in `Studium-Users`, coupling compliance tracking to user identity and preventing platform-wide cross-service auditing.
- **Chosen Solution:** Extracted `Studium-Audit` as an autonomous .NET 10 Clean Architecture microservice with its own dedicated PostgreSQL database (`studium_audit`).

### 1.2 Universal, Migration-Proof Audit Schema
- **Problem Solved:** Avoids running database migrations every time a new entity or microservice needs auditing.
- **Chosen Solution:** Generic resource schema with `ServiceSource`, `ResourceId`, `ActorId`, `Action`, and flexible `MetadataJson` text column.

### 1.3 Asynchronous Ingestion via RabbitMQ, Inbox Deduplication & Dead-Letter Handling
- **Problem Solved:** Audit logging must never block or degrade transactional operations in upstream services, while ensuring exactly-once processing across network retries.
- **Chosen Solution:** Ingestion via Wolverine and RabbitMQ consuming `studium-audit-events-queue` using `[MessageIdentity("studium.audit.integration-event")]`.
- **Transactional Inbox Pattern:** Implemented in `AuditIntegrationEventHandler` via `InboxMessage` entity (`Id: Guid`, `HandlerName: string`, `ReceivedAtUtc: DateTime`) with unique primary key index and B-Tree index on `ReceivedAtUtc`.
- **Zero-Overhead Lazy Pruning:** Consumer handler performs throttled cleanup (every 6 hours) via `ExecuteDeleteAsync` for inbox messages older than 7 days, avoiding extra background jobs or cron services.
- **Resilience & DLQ:** Configured 3x exponential retry backoff for transient failures (`NpgsqlException`, `TimeoutException`, `HttpRequestException`), moving unfixable/poison messages immediately to the dead-letter error queue via `MoveToErrorQueue()`. Also provides synchronous REST query and post endpoints (`/api/audit/logs`).

### 1.4 Role-Based Access Control (RBAC) & Endpoint Hardening
- **Decision:** Secured sensitive audit logs against unauthorized inspection and tampering.
  - JWT claim mapping: `NameClaimType = "sub"`, `RoleClaimType = "cognito:groups"`.
  - Configured `AdminOnly` authorization policy requiring `Administrator`, `admin`, or `Admin` role.
  - Restricted `GET /api/audit/logs` and `POST /api/audit/logs` with `[Authorize(Policy = "AdminOnly")]`.
  - Restricted `GET /api/audit/resources/{id}` with `[Authorize]` to permit authenticated callers.

### 1.5 Database Resilience & Health Check Segregation
- **Decision:** Segregated liveness (`/health`) from deep database readiness (`/health/ready`). Added `EnableRetryOnFailure(3)` to Npgsql configuration without altering connection lifetimes, preventing temporary socket/network drops during RDS/cluster failovers.

### 1.6 Dynamic CORS Configuration
- **Decision:** Created reusable `CorsExtensions.AddStudiumCors(IConfiguration)` reading comma-separated allowed origins from `Cors:AllowedOrigins` or environment variable `CORS_ALLOWED_ORIGINS` / `Cors__AllowedOrigins` (defaulting to allow all for local development).

---

## 2. Project Structure & File Map
- [`Directory.Build.props`](./Directory.Build.props) & [`Directory.Packages.props`](./Directory.Packages.props) — CPM locking dependencies.
- [`Studium-Audit.slnx`](./Studium-Audit.slnx) — Root solution.
- `Domain/`:
  - `Entities/AuditLog.cs` — Universal audit entity.
  - `Entities/Inbox/InboxMessage.cs` — Inbox message deduplication entity.
- `Data/`:
  - `AuditDbContext.cs` — EF Core context with applied configurations and `InboxMessages` DbSet.
  - `Configurations/AuditLogConfig.cs`, `Configurations/InboxMessageConfig.cs` — Fluent API mapping with B-Tree indexes.
  - `Migrations/` — Auto-applied startup migrations including `20261109225112_Add_InboxMessages_Table`.
- `Services/`:
  - `DTOs/AuditDtos.cs`, `IntegrationEvents.cs` — Request, response, and event contracts (`[MessageIdentity("studium.audit.integration-event")]` with `EventId`).
  - `Handlers/AuditIntegrationEventHandler.cs` — Wolverine RabbitMQ handler with inbox deduplication and lazy cleanup.
- `Api/`:
  - `Controllers/AuditController.cs` — REST endpoints (`POST /api/audit/logs`, `GET /api/audit/logs`, `GET /api/audit/resources/{resourceId}`).
  - `Extensions/MessagingExtensions.cs`, `AuthenticationExtensions.cs`, `CorsExtensions.cs`, `DatabaseHealthCheck.cs`.
  - `HostedServices/DatabaseMigrationHostedService.cs`.
- `Tests/` — Unit test suite.
- [`Dockerfile`](./Dockerfile) & [`k8s/deployment.yaml`](./k8s/deployment.yaml) — Production containers and Kubernetes manifests.

---

## 3. Recent Changes History
- **2026-11-09 (Transactional Inbox Pattern, EventId Deduplication & Migration Snapshot Alignment):**
  - Added `InboxMessage` entity and `InboxMessageConfig` with B-Tree index on `ReceivedAtUtc`.
  - Added `DbSet<InboxMessage> InboxMessages` to `AuditDbContext`.
  - Re-scaffolded EF Core migration `20261109225112_Add_InboxMessages_Table` with updated `AuditDbContextModelSnapshot` resolving `PendingModelChangesWarning`.
  - Added `EventId` to `AuditIntegrationEvent` contract.
  - Implemented inbox deduplication and 6-hour lazy pruning in `AuditIntegrationEventHandler`.
  - Verified 4/4 unit tests pass.
- **2026-11-09 (Dynamic CORS Configuration):**
  - Added `CorsExtensions.cs` reading allowed origins dynamically from configuration/environment variables.
  - Hooked `AddStudiumCors` and `UseCors(CorsExtensions.PolicyName)` into `Program.cs`.
  - Added `Cors:AllowedOrigins: "*"` in `appsettings.json` and `Cors__AllowedOrigins` in `k8s/deployment.yaml`.
  - Verified 4/4 unit tests pass.
- **2026-11-09 (Database Resilience, Deep Readiness Probe & Wolverine DLQ):**
  - Added `EnableRetryOnFailure(3)` inside PostgreSQL `UseNpgsql` registration.
  - Added `DatabaseHealthCheck.cs` verifying database connectivity for Kubernetes `readinessProbe` at `/health/ready`.
  - Configured Wolverine 3x retry policies with cooldown for `NpgsqlException`, `TimeoutException`, `HttpRequestException`, and routed terminal poison messages to dead-letter error queue (`opts.OnException<Exception>().MoveToErrorQueue()`).
  - Regenerated Wolverine static handlers via `codegen write` and verified 4/4 unit tests pass.
- **2026-11-09 (RBAC Hardening, Message Identity & Transactional Outbox):**
  - Configured `AdminOnly` authorization policy and Cognito claim mapping in `AuthenticationExtensions.cs`.
  - Added `[Authorize(Policy = "AdminOnly")]` to `GET /api/audit/logs` and `POST /api/audit/logs`, and `[Authorize]` to `GET /api/audit/resources/{resourceId}`.
  - Added `[MessageIdentity("studium.audit.integration-event")]` to `AuditIntegrationEvent` in `Services/DTOs/IntegrationEvents.cs`.
  - Enabled transactional outbox via `opts.Policies.AutoApplyTransactions()` in `MessagingExtensions.cs`.
  - Regenerated Wolverine static handlers and verified all 4 unit tests pass.
- **2026-11-09 (Initial Microservice Scaffold & Implementation):**
  - Scaffolded .NET 10 Clean Architecture solution with PostgreSQL database `studium_audit`.
  - Implemented universal audit schema, EF Core migrations, and Wolverine RabbitMQ handler.
  - Added REST controller with pagination and resource-specific filtering.
  - Verified unit tests and generated static Wolverine handlers via `codegen write`.
