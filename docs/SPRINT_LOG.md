# Solo development log

These are Agile-inspired solo iterations, not experience on a professional Scrum team. Estimates below are rough planning estimates, not measured elapsed time. Completion status reflects the delivered local release and observed verification.

| Story | Priority | Acceptance | Tasks | Estimate | Status |
|---|---|---|---|---|---|
| Coordinator creates and assigns work | P1 | Valid input persists; invalid input does not | Model, schema, forms, service | 2–3 h | Complete |
| Technician progresses and resolves work | P1 | Resolution requires notes; transitions audited | TDD rule, status API, Fetch UI | 1–2 h | Complete |
| Manager prioritizes overdue work | P1 | Correct Halifax date boundary and real metrics | TDD rule, queries, dashboard | 1–2 h | Complete |
| Coordinator finds and archives work | P2 | Search/filter; archive/restore preserves history | Queries, archive actions, table | 1–2 h | Complete |
| Developer verifies and releases demo | P2 | Build/tests pass; startup documented | Integration tests, review, CI, docs | 2–3 h | Complete |

Iteration 1: models, persistence, creation/listing, rules, dashboard, API and initial tests.

Iteration 2: complete workflows, responsive UI, audit/archiving, integration checks, defect fixes and concise release material.

## Iteration outcomes and retrospective

Iteration 1 completed the data model, SQLite migration, shared workflow rules, dashboard/API foundations and two genuine TDD cycles. Iteration 2 completed the interface, filters, audit/archiving, integration tests and release checks.

Completed: the specified local workflows, five REST endpoints, 45 passing tests and Release publish output. Difficulties: sandbox CLI/IPC permissions and a validation-error serialization defect exposed by integration testing. Improvements: shared service validation, clearer API field messages, larger UI reading sizes and removal of unused template assets. Remaining: human UAT, a real GitHub Actions run and SQL Server runtime verification; production authentication/concurrency are future work.

No daily standups, external reviews or stakeholder approval occurred. This log records tool-assisted solo work; estimates are not claimed as actual development duration.
