# ForgeOps — Product requirements (v0.1.0)

ForgeOps models a maintenance work-order workflow using fictional facilities and personnel. It is an independent portfolio prototype, not software used by a real manufacturer.

## User roles (illustrative only)

- **Maintenance coordinator:** create, edit, assign, search, archive, and restore work orders.
- **Technician:** view assigned work, update status, and record resolution notes.
- **Operations manager:** review operational workload and overdue maintenance.

The personas define use cases; they are not implemented authentication roles.

## Functional requirements

- Capture order title, equipment, facility, description, priority, due date, and optional technician assignment.
- Persist work orders in a relational database; generate unique work-order references.
- Support Open, In Progress, and Resolved workflows with server-side validation.
- Require notes when resolving an order; preserve prior resolution details in audit history after reopening.
- Permit reversible archiving and restoration; archive records remain readable but are omitted from active metrics.
- Mark orders overdue only if unresolved, unarchived, and due before today's America/Halifax calendar date.
- Compute dashboard status and priority summaries from database records.
- Expose work-order and dashboard JSON endpoints and use Fetch for status changes in the interface.

## Quality and acceptance criteria

- Invalid input returns clear errors and must not change persisted records.
- Valid updates and associated audit entries are stored atomically.
- Local SQLite changes survive restarts and initial demo data is not duplicated.
- Interfaces work at desktop and mobile screen sizes.
- Automated unit and integration tests verify principal rules and HTTP interactions.

## Implementation scope

.NET 10, ASP.NET Core MVC, EF Core, SQLite, Razor/Bootstrap, JavaScript, and xUnit. A public demo is hosted using Docker on Render with disposable fictional data. Authentication, real manufacturing data, production reliability, and end-user signoff are outside the prototype scope. SQL Server configuration is provided for future integration but remains runtime-unverified.
