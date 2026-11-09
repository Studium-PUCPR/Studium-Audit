# Studium-Audit

Stack: `.NET 10.0` Web API + PostgreSQL (`studium_audit`) + RabbitMQ (`WolverineFx.RabbitMQ`) + Amazon Cognito (`JwtBearer`) + Scalar API Reference (`/scalar/v1`).

Centralized, immutable compliance audit ledger for the Studium platform. Decoupled from `Studium-Users` to provide institution-wide auditing across Users, Bookings, and Catalog.

## Core Capabilities
- **Universal Audit Schema:** Captures actions across all microservices (`ServiceSource`, `ResourceId`, `ActorId`, `Action`, `MetadataJson`, `TimestampUtc`) with zero schema migrations required for new entities.
- **Asynchronous Ingestion (RabbitMQ):** Consumes `AuditIntegrationEvent` from `studium-audit-events-queue` without blocking transactional microservices.
- **Synchronous REST API:**
  - `POST /api/audit/logs` — Record audit log entry synchronously.
  - `GET /api/audit/logs` — Query / filter audit trail by resource, service, actor, action, and date range.
  - `GET /api/audit/resources/{resourceId}` — Query audit trail for a specific resource (User, Booking, Room).

## Local Setup & Testing
```bash
dotnet build
dotnet test
dotnet run --project Api/Studium-Audit.Api.csproj
```
Interactive API documentation is accessible at `http://localhost:8080/scalar/v1`.
