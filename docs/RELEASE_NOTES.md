# ForgeOps v0.1.0 — Release notes

**Initial portfolio release:** October 2026

ForgeOps is an independent manufacturing work-order tracker built with C#/.NET 10, ASP.NET Core MVC, Entity Framework Core, SQLite, Razor and JavaScript.

## Included

- Database-backed work orders with creation, editing, assignment, searching, and filtering.
- Dashboard metrics for status, priority, and overdue work.
- API-backed status changes, resolution-note validation, reopening, audit history, and reversible archiving.
- Responsive desktop/mobile interface and fictional starter data.
- Five REST endpoints and development OpenAPI metadata.
- Unit, integration, and browser smoke-test coverage.

## Verification

The verified Release build completed without warnings or errors, and the suite passed **45 tests** (27 unit, 18 integration). The repository's [GitHub Actions build-and-test run](https://github.com/MBELSABAH/ForgeOps/actions/runs/38944079344) completed successfully.

A public fictional-data demo is deployed to [Render](https://forgeops-mbelsabah.onrender.com). The free hosting tier may cold-start and reset data.

## Scope limitations

No user authentication, production data, advanced permissions, optimistic concurrency, or independent user acceptance signoff is included. Microsoft SQL Server support has been configured and its schema generated offline, but it has not been tested against a running SQL Server instance.
