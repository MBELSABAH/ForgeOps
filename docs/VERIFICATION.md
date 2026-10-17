# Build, test, and demo verification

## Automated verification

The October 2026 v0.1.0 verification recorded:

| Category | Result |
|---|---|
| Release build | 0 warnings, 0 errors |
| Unit tests | 27 passed — validation, statuses, resolution, due-date and time-zone boundaries |
| Integration tests | 18 passed — MVC/API requests using isolated SQLite databases |
| Total | **45 passed, 0 failed** |
| Local release | Framework-dependent publish output generated |
| GitHub Actions CI | [Build-and-test workflow completed successfully](https://github.com/MBELSABAH/ForgeOps/actions/runs/38944079344) |

The integration suite covers POST/GET persistence across application hosts, HTTP response codes, invalid-write immutability, audit entries, query filters, dashboard calculations, assignment, archive/restore, antiforgery, and HTML encoding.

## Functional smoke checks

The local application was exercised for work-order creation, assignment, editing, Fetch-based status updates, rejection of incomplete resolution, valid resolution, reopening, archiving/restoration, search, and empty states. Desktop and mobile layouts were inspected, and screenshots were saved under **docs/screenshots/**.

Persistence was confirmed after an application restart, with the fictional seed records remaining stable. The published Production build was launched locally with a separate SQLite database and basic routes and API responses checked.

## Public demo

The [hosted application](https://forgeops-mbelsabah.onrender.com) was smoke-tested for core navigation, dashboard metrics, a work-order workflow, and status updates. The demo uses fictional data, does not require authentication, and can reset its SQLite state when the free service restarts.

## Boundaries

SQL Server provider configuration and offline schema generation were checked, but no SQL Server database queries were executed. Independent end-user acceptance testing and production readiness are outside the scope of v0.1.0. The reported checks are development and automated verification, not external customer approval.
