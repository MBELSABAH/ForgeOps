# Code quality and review

This project includes an **author-led self-review**, automated tests, and corrective changes. No independent developer review is claimed.

## Review areas

- **Structure:** MVC and API controllers share a service rather than duplicating business rules.
- **Validation:** server-side checks cover required fields, invalid status/priority, length constraints, technician IDs, and resolution notes.
- **Queries:** read-only queries use appropriate no-tracking operations, filters, and aggregations.
- **Error handling:** structured logging, friendly responses, and useful field errors without HTTP stack traces.
- **Security basics:** Razor output encoding, safe DOM text insertion, antiforgery for MVC writes, and JSON-only API writes.
- **Testability:** pure rule helpers, injectable time, isolated SQLite HTTP integration tests, and repeatable scripts.

## Issues identified and corrected

**Validation error response:** An HTTP integration test revealed that a failed resolution returned 400 without the expected field details. The exception handler now serializes the runtime ValidationProblemDetails type, and the test checks the returned field message. The full suite subsequently passed.

**Dependency advisory:** Dependency verification detected a vulnerable transitive System.Security.Cryptography.Pkcs 9.0.11 reference. The package was updated to patched 10.0.13 based on the [Microsoft advisory](https://github.com/advisories/GHSA-2fmv-rv2c-59x7), followed by a successful restore and Release build.

## Remaining limitations

Concurrent updates currently use last-write-wins rather than optimistic concurrency. The application also lacks authentication and authorization, so it is suitable only as a fictional portfolio demo—not as a real production maintenance system.
