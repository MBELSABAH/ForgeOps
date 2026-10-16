# Design

## Request flow

~~~mermaid
flowchart LR
    Browser[Razor UI] --> MVC[MVC controllers]
    Browser -->|Fetch JSON| API[API controllers]
    MVC --> Service[WorkOrderService]
    API --> Service
    Service --> Rules[Pure validation rules + injectable clock]
    Service --> EF[ForgeOpsDbContext]
    EF --> SQLite[(SQLite)]
    EF -. optional configuration .-> SQL[(SQL Server: runtime unverified)]
~~~

One web project keeps the layers visible without separate assemblies or a repository wrapper around EF. Controllers handle HTTP/UI concerns; the service owns the workflow. DTOs bound the API surface; MVC form models carry form input and roster options.

## Relational model

- Technician 1 → many WorkOrders; assignment is optional and the foreign key restricts technician deletion.
- WorkOrder 1 → many WorkOrderAudits. Each audit entry stores event, summary, optional resolution notes, status transition and UTC time.
- EF maps entities to WorkOrders, Technicians and WorkOrderAudits, with primary keys, foreign keys, bounded text fields and indexes for queue/updated-time queries. References are deterministic displays of unique IDs.
- SQLite startup applies the initial migration; seeding runs only when there are no orders. EF-generated migrations preserve the schema definition in Git.

DueDate is DateOnly. A TimeProvider-backed clock supplies the Halifax facility date. UTC DateTime timestamps avoid SQLite DateTimeOffset ordering limitations and are restored with UTC kind when read. Overdue predicates used in queries match the pure rule tested at date boundaries.

## Mutations and errors

Details, assignment, status and archive actions validate before changing tracked values. One SaveChangesAsync call atomically persists each mutation with its audit entries. Resolution notes are retained in history when reopening clears the current notes. Repeated identical status submissions do not create audit noise.

API routes are controller-based: list, detail, create, status PATCH, and dashboard summary. JSON enum names reject numeric values; undefined names/values and invalid filters return validation errors. ProblemDetails carries 400/404/500 responses. The exception handler logs structured identifiers and hides stack traces from HTTP responses.

Razor encodes stored text; dynamically rendered history uses textContent. MVC writes use antiforgery tokens. API writes require application/json, cross-origin access is not enabled, and there is no cookie authentication. The app binds to loopback for the demo. This is not a production access-control design.

## Optional SQL Server

The same provider-neutral EF model can use Microsoft.EntityFrameworkCore.SqlServer. To generate an initialization script after building:

~~~sh
mkdir -p artifacts
./scripts/run.sh --no-build --no-launch-profile -- --export-sqlserver-schema > artifacts/sqlserver-schema.sql
~~~

This command was verified offline. To try runtime execution later, apply that script to a new dedicated SQL Server database, configure ConnectionStrings__SqlServer externally, set Database__Provider=SqlServer, then run the application. Do not commit credentials.

SQLite migrations are deliberately not applied to SQL Server. A continuing SQL Server deployment needs its own migration set, execution tests and hosting/authentication configuration. No SQL Server connection or query execution was verified for v0.1.0.
