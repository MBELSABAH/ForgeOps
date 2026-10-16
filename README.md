# ForgeOps
### Manufacturing Work Order Tracker

A local C#/.NET business application for reporting equipment problems, assigning maintenance work and tracking resolution. Built as an independent portfolio project; it is not affiliated with Amsted or any real manufacturer. Northstar Manufacturing, its facilities, technicians and work orders are fictional.

![ForgeOps operations dashboard](docs/screenshots/dashboard.jpg)

## Start on Apple Silicon macOS

Requires the .NET 10 SDK. This machine was verified with SDK 10.0.402 and runtime 10.0.13 on macOS arm64. No Docker, Node.js, paid service or SQL Server installation is required.

From the repository root:

~~~sh
./scripts/run.sh
~~~

Open [ForgeOps](http://127.0.0.1:5080). Stop the process with Ctrl+C. If it is already running, use the open application rather than starting another copy on the same port.

The script restores packages, builds and starts the application. It keeps CLI/package caches in ignored .local directories to avoid sandbox home-directory restrictions. An initial restore needs Internet access. Standard dotnet commands also work in an unrestricted terminal:

~~~sh
dotnet run --project src/ForgeOps.Web
~~~

SQLite is initialized through the committed EF Core migration on startup. The database is src/ForgeOps.Web/App_Data/forgeops.db. The first run seeds 12 fictional orders and four technicians; subsequent starts preserve changes. Verification records created during development are retained in Archived and do not affect operational metrics.

## What works

- Database-derived totals, overdue counts, recent activity, status and priority breakdowns.
- Create, view and edit orders; assign a technician; search title/equipment/reference and combine status, priority and overdue filters.
- Fetch-driven status changes, server validation and visible audit history.
- Resolve only with notes; reopen while preserving previous resolution in history.
- Archive/restore any order; archived orders are read-only and excluded from the dashboard.
- Responsive light theme with the supplied anvil logo, red accents, horizontal navigation, accessible forms and useful empty states.
- Five REST endpoints, typed DTOs, JSON validation errors and development OpenAPI at [/openapi/v1.json](http://127.0.0.1:5080/openapi/v1.json).

## Architecture and technologies

C# / .NET 10, ASP.NET Core MVC, Razor, EF Core, SQLite, optional Microsoft SQL Server provider, Bootstrap, HTML/CSS, plain JavaScript Fetch, xUnit and WebApplicationFactory.

MVC controllers and API controllers call the same injected work-order service. The service validates before mutation, queries EF asynchronously and saves order changes with audit entries in one transaction. Pure rules and an injectable clock make business behavior independently testable. See [design and relationships](docs/DESIGN.md).

## Verify and prepare a local release

~~~sh
./scripts/test.sh
./scripts/check.sh
~~~

The second command restores, builds Release, runs the suite and publishes to artifacts/publish. Latest observed result: **45 passed, 0 failed** — 27 unit cases and 18 integration cases. Integration tests use unique SQLite databases, never normal demo data. Release build completed with zero warnings/errors. See [verification evidence](docs/VERIFICATION.md).

## REST API examples

Enums use string values: Open / InProgress / Resolved and Low / Medium / High / Critical. Dates use YYYY-MM-DD. Requests and responses use DTOs rather than tracked entities.

~~~sh
curl -s 'http://127.0.0.1:5080/api/workorders?status=Open&priority=High'
curl -s http://127.0.0.1:5080/api/dashboard/summary
curl -i -X POST http://127.0.0.1:5080/api/workorders \
  -H 'Content-Type: application/json' \
  -d '{"title":"Inspect conveyor guard","equipmentName":"Conveyor CV-15","facility":"North plant · Assembly","description":"Inspect bracket and verify fasteners.","priority":"High","dueDate":"2026-10-16"}'
~~~

Expected: GET returns 200 with a DTO array or summary object. POST returns 201, a Location header pointing to the new resource, and its DTO with an Open status and generated reference.

Set ORDER_ID below to the id returned by POST (13 is only an example):

~~~sh
ORDER_ID=13
curl -i "http://127.0.0.1:5080/api/workorders/$ORDER_ID"
curl -i -X PATCH "http://127.0.0.1:5080/api/workorders/$ORDER_ID/status" \
  -H 'Content-Type: application/json' -d '{"status":"InProgress"}'
curl -i -X PATCH "http://127.0.0.1:5080/api/workorders/$ORDER_ID/status" \
  -H 'Content-Type: application/json' -d '{"status":"Resolved"}'
curl -i -X PATCH "http://127.0.0.1:5080/api/workorders/$ORDER_ID/status" \
  -H 'Content-Type: application/json' \
  -d '{"status":"Resolved","resolutionNotes":"Secured bracket and completed an operating check."}'
~~~

Expected: GET and valid PATCH return 200. Resolution without notes returns 400 with an errors.ResolutionNotes message: “Add resolution notes before resolving this work order.” Missing ids return 404. Invalid fields/status values return 400. Non-JSON API writes return 415.

Additional GET filters: search=WO-000001, archived=true (archived records only), overdue=true. Details include ordered history; list responses omit history entries for efficiency.

## Engineering evidence

- [Requirements and acceptance criteria](docs/REQUIREMENTS.md), [design](docs/DESIGN.md) and [solo iteration log](docs/SPRINT_LOG.md).
- [Two observed Red → Green → Refactor cycles](docs/TDD_WALKTHROUGH.md).
- [Self-review and genuine corrections](docs/CODE_REVIEW.md), [human UAT checklist](docs/UAT_CHECKLIST.md).
- [User guide](docs/USER_GUIDE.md), [troubleshooting](docs/TROUBLESHOOTING.md), [release checklist](docs/RELEASE_CHECKLIST.md) and [v0.1.0 notes](docs/RELEASE_NOTES.md).
- [Interview questions, resume bullets and two-minute demo](docs/INTERVIEW_PREP.md).
- Meaningful local Git commits and a GitHub Actions restore/build/test workflow. The workflow has **not run on GitHub**; nothing has been pushed or published.

## SQL Server and limitations

SQLite is the executed and tested provider. The SQL Server package/configuration path compiles, and schema generation was verified without a server connection. See [provider instructions](docs/DESIGN.md#optional-sql-server).

This is an unauthenticated loopback demo; personas are illustrative. Human UAT remains unapproved. SQL Server execution, public hosting and cross-browser certification are unverified. No permanent delete, pagination, technician administration or concurrent-edit conflict detection is implemented. Simultaneous valid edits use last-write-wins behavior. Future work: authentication/authorization, optimistic concurrency, paging and SQL Server-specific migrations/tests.
