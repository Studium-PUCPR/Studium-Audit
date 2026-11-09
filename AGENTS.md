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

### 1.3 Asynchronous Ingestion via RabbitMQ
- **Problem Solved:** Audit logging must never block or degrade transactional operations in upstream services.
- **Chosen Solution:** Ingestion via Wolverine and RabbitMQ consuming `studium-audit-events-queue`. Also provides synchronous REST query and post endpoints (`/api/audit/logs`).

---

## 2. Project Structure & File Map
- [`Directory.Build.props`](./Directory.Build.props) & [`Directory.Packages.props`](./Directory.Packages.props) — CPM locking dependencies.
- [`Studium-Audit.sln`](./Studium-Audit.sln) — Root solution.
- `Domain/`:
  - `Entities/AuditLog.cs` — Universal audit entity.
- `Data/`:
  - `AuditDbContext.cs` — EF Core context with applied configurations.
  - `Configurations/AuditLogConfig.cs` — Fluent API mapping with B-Tree indexes.
  - `Migrations/` — Auto-applied startup migrations.
- `Services/`:
  - `DTOs/AuditDtos.cs`, `IntegrationEvents.cs` — Request, response, and event contracts.
  - `Handlers/AuditIntegrationEventHandler.cs` — Wolverine RabbitMQ handler.
- `Api/`:
  - `Controllers/AuditController.cs` — REST endpoints (`POST /api/audit/logs`, `GET /api/audit/logs`, `GET /api/audit/resources/{resourceId}`).
  - `Extensions/MessagingExtensions.cs`, `AuthenticationExtensions.cs`.
  - `HostedServices/DatabaseMigrationHostedService.cs`.
- `Tests/` — Unit test suite.
- [`Dockerfile`](./Dockerfile) & [`k8s/deployment.yaml`](./k8s/deployment.yaml) — Production containers and Kubernetes manifests.

---

## 3. Recent Changes History
- **2026-11-09 (Initial Microservice Scaffold & Implementation):**
  - Scaffolded .NET 10 Clean Architecture solution with PostgreSQL database `studium_audit`.
  - Implemented universal audit schema, EF Core migrations, and Wolverine RabbitMQ handler.
  - Added REST controller with pagination and resource-specific filtering.
  - Verified unit tests and generated static Wolverine handlers via `codegen write`.
