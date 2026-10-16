# Requirements — v0.1.0

ForgeOps tracks fictional equipment problems from reporting through resolution. It is an independent portfolio application, not an Amsted system.

## Illustrative users
- Maintenance coordinator: create, edit, assign, search, archive and restore orders.
- Technician: inspect orders, update status and record resolution notes.
- Operations manager: see workload, urgency and overdue work.

## Acceptance criteria
- Required fields and enum values are validated on the server; failed writes preserve existing information.
- Orders persist across restarts. Seed data is fictional and is not duplicated on restart.
- Resolving requires notes; status changes and other meaningful mutations preserve history.
- Resolved orders can reopen; previous resolution notes remain in history.
- Any order can be archived and restored. Archived orders are read-only and excluded from operational metrics.
- Overdue means due before today in America/Halifax and unresolved/unarchived. Due today is not overdue.
- Dashboard totals and summaries come from stored data.
- MVC and REST endpoints share business services; the browser changes status through Fetch.
- Automated unit and isolated SQLite integration tests pass; human UAT remains pending.

## Constraints
.NET 10, one MVC project plus one xUnit project, SQLite, Bootstrap and plain JavaScript. Local unauthenticated demo; no public deployment, Docker or real manufacturing data. SQL Server runtime verification and advanced reporting are optional. Usable responsive forms, clear errors and a repeatable local launch are required.
