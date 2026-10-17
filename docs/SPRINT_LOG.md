# Development iterations and backlog

ForgeOps was developed iteratively as an individual portfolio project. This document shows an Agile-inspired backlog and completed increments; it does **not** represent experience working on a staffed Scrum team.

| User story | Acceptance criteria | Delivery |
|---|---|---|
| As a coordinator, I can create and assign work orders | Valid submissions persist; invalid entries do not | Implemented |
| As a technician, I can record progress and resolution | Status changes are audited; resolution requires notes | Implemented |
| As an operations manager, I can prioritize overdue tasks | Dashboard uses persisted data and Halifax deadline rules | Implemented |
| As a coordinator, I can find and archive records | Search/filter work; archives can be restored | Implemented |
| As a developer, I can verify changes before release | Automated build/tests and repeatable local startup | Implemented |

## Increment 1 — Core functionality

Built the data model and EF Core migration, work-order creation and listing, core business rules, initial APIs, dashboard calculations, and test-first implementations for resolution validation and overdue rules.

## Increment 2 — Integration and refinement

Added interactive status changes, search and filters, audit history, archiving, responsive layouts, HTTP integration tests, and technical documentation.

## Engineering outcomes

The v0.1.0 verification records 27 unit cases and 18 integration cases passing. Integration testing exposed a validation-error serialization issue, which was fixed and covered by a regression assertion. Other documented refinements include clearer feedback and dependency updates.

Future improvements include authentication/authorization, optimistic concurrency, pagination, SQL Server runtime testing, and independent end-user evaluation.
