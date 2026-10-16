# Verification evidence

Observed on October 15, 2026, America/Halifax, on macOS arm64 with SDK 10.0.402 / runtime 10.0.13.

| Check | Observed result |
|---|---|
| ./scripts/check.sh | Restore succeeded; Release build: 0 warnings, 0 errors; 45/45 tests passed; publish output generated |
| Unit cases | 27: required data, lengths/enums, resolution notes, overdue date/status/archive boundaries and Halifax daylight-saving date boundaries |
| Integration cases | 18: real HTTP/MVC pipeline, isolated SQLite migrations, POST/GET persistence across application hosts, status/audit invariants, failed-write preservation, filters/summary, technician assignment, archive/restore, antiforgery and HTML encoding |
| Live API | GET list/detail/summary, POST creation and PATCH status succeeded; verified 200/201/400/404, field errors, invalid filter and OpenAPI document |
| Browser workflow | Created WO-000013, assigned Jordan Ellis, progressed status through Fetch, rejected blank resolution, resolved with notes, verified dashboard, reopened, archived, restored and archived again |
| Persistence/restart | Browser-created order remained after stopping/restarting the application; seed orders were not duplicated |
| Search/empty state | Combined reference/status/priority filters returned the expected single order; unmatched search showed the helpful empty state |
| Responsive UI | Inspected desktop 1440×1000 and mobile 390×844 overrides; mobile navigation worked; table overflow stayed inside its wrapper |
| Published application | Ran the publish output in Production on loopback port 5081 with a separate database; HTML/routes/assets and API metrics passed; OpenAPI correctly returned 404 |
| Optional SQL Server | Offline DDL generation produced all three tables; no server connection or SQL execution tested |

At verification end, the active queue remained 12 seeded orders: 4 Open, 5 InProgress, 3 Resolved, 4 overdue on October 15. Browser/API verification orders were archived, preserving their evidence. Counts change with edits and the facility date.

The TRX output is generated under tests/ForgeOps.Tests/TestResults and is intentionally ignored by Git. Screenshots are under docs/screenshots. Agent browser verification is separate from human acceptance; UAT remains pending. GitHub Actions is configured but has not run remotely.

## Light theme and branding update — October 16, 2026

Installed the supplied anvil image unchanged, replaced the sidebar with horizontal navigation, and applied a light gray/red/charcoal theme across the dashboard, tables, forms and validation feedback. Release tests passed again: 45/45. The application restarted successfully on port 5080. Browser checks covered wide and mobile layouts, navigation, metric refresh and rejected resolution without notes; no demo records were changed. Mobile list/form widths stayed within the viewport. Updated screenshots show the light theme. The queue has six overdue orders on October 16 because the facility date advanced.
