# Architecture and design

## Request flow

~~~mermaid
flowchart LR
    Browser[Razor UI] --> MVC[MVC controllers]
    Browser -->|Fetch JSON| API[REST API controllers]
    MVC --> Service[WorkOrderService]
    API --> Service
    Service --> Rules[Validation rules and injectable clock]
    Service --> EF[ForgeOpsDbContext]
    EF --> SQLite[(SQLite)]
    EF -. optional provider .-> SQL[(SQL Server - unverified)]
~~~

The application uses one ASP.NET Core web project with distinct responsibilities for models, DTOs, view models, controllers, services, and EF Core persistence. MVC and API controllers share the same work-order service so validation rules and workflow behavior are consistent.

## Relational model

- One technician can be assigned to many work orders; assignment is optional.
- One work order contains many audit events (creation, edits, assignment, status transitions, archiving, and restoration).
- Each event records its timestamp, event type, and relevant change details. Resolution notes remain accessible through history after an order reopens.
- EF Core maps primary and foreign keys, bounded fields, and indexes for common work-order queries.

SQLite startup applies the committed initial migration and seeds fictional records if no orders exist. Date-only deadlines use the America/Halifax facility calendar; modification and history timestamps are stored in UTC.

## Business rules and consistency

The service validates incoming requests before modifying tracked entities. A successful mutation persists the work order and corresponding audit event through a single EF Core SaveChangesAsync operation. Repeated identical status submissions do not create redundant audit events.

Invalid requests return structured error responses; resource lookups distinguish absent orders. DTOs prevent API responses from exposing EF entities. The interface HTML-encodes stored content and dynamically inserts user-provided text safely.

MVC form writes use antiforgery protection; the API accepts JSON writes and does not enable cross-origin access. The local development profile binds to loopback. **The hosted demo is intentionally public and unauthenticated and must only receive fictional data.** Neither environment is presented as a production access-control solution.

## Optional SQL Server configuration

The EF Core model also supports Microsoft.EntityFrameworkCore.SqlServer. The repository can generate a SQL Server schema script offline:

~~~sh
mkdir -p artifacts
./scripts/run.sh --no-build --no-launch-profile -- --export-sqlserver-schema > artifacts/sqlserver-schema.sql
~~~

For a future SQL Server deployment, initialize a dedicated database, configure ConnectionStrings__SqlServer externally, and select Database__Provider=SqlServer. Do not commit credentials.

SQLite migrations are not applied to SQL Server. The SQL Server provider and offline schema were checked, but no live SQL Server connection or queries were verified. Separate SQL Server migrations and runtime integration testing are future work.
