# Interview and demo preparation

## Five technical questions

1. **Why put business rules in a service?** MVC and REST controllers both call WorkOrderService. Resolution validation and audit creation therefore behave consistently, while pure rules can be unit-tested without HTTP or a database.
2. **What did you actually build with TDD?** Resolution-note enforcement and overdue calculation. Each began with an observed xUnit assertion failure, followed by implementation and a passing rerun. Other features are not claimed as test-first.
3. **What distinguishes your integration tests?** They run the ASP.NET request pipeline with WebApplicationFactory and a unique SQLite database. A POST/GET test recreates the host against the same file to verify persisted data rather than a mocked collection.
4. **How do invalid updates avoid damaging data?** Validation runs before tracked fields change. Valid mutations and audit entries are committed together with SaveChangesAsync. Tests verify failed requests leave fields, timestamps and audit counts unchanged.
5. **How is overdue work calculated, and what about SQL Server?** A date-only deadline is compared with today's Halifax date; resolved/archived orders are excluded. SQLite is tested. SQL Server configuration and offline schema generation exist, but server execution is not claimed.

## Truthful resume bullets

- Built a C#/.NET 10 manufacturing work-order application using ASP.NET Core MVC, Entity Framework Core and SQLite, with database-derived dashboards, technician assignment, audit history and a responsive Razor/Bootstrap interface.
- Implemented and verified five REST endpoints and Fetch-based status updates, with 45 passing xUnit unit/integration cases, two recorded TDD cycles, and isolated SQLite persistence tests.

These describe the completed portfolio project. Do not present solo iterations as professional Scrum-team experience or the configured workflow as a successful GitHub-hosted CI run.

## Two-minute demonstration

- **0:00–0:20:** Open Overview. Explain that counts and priority/status summaries come from SQLite; click overdue work.
- **0:20–0:50:** Create a High-priority fictional issue and assign a technician. Point out the generated reference and persisted details.
- **0:50–1:20:** Change to In progress through Fetch. Attempt resolution without notes, show the server error, then enter a repair/verification note and resolve.
- **1:20–1:40:** Show the audit history and return to the dashboard. Explain why resolved orders are excluded from overdue counts.
- **1:40–2:00:** Show the OpenAPI document and the 45-test result. Explain the shared service and the two test-first rules. State that SQL Server runtime verification and human UAT remain pending.
