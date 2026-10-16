# Self-review

Review performed during tool-assisted solo development. No independent developer review is claimed.

- [x] Readability and naming: controller, DTO, service and entity responsibilities are explicit; remove unused template pages/libraries.
- [x] Duplication: MVC and REST workflows share one service rather than repeating business rules.
- [x] Input validation: required/whitespace fields, enum ranges, lengths, technician IDs and resolution rules checked server-side.
- [x] Error handling: friendly errors, structured logs and trace identifiers; no HTTP stack traces.
- [x] Queries: no-tracking reads, server-side filters/aggregates, bounded recent-order query; no EF entities in JSON.
- [x] Security: Razor encoding, safe DOM text rendering, antiforgery on MVC writes, JSON-only API writes and a loopback launch profile.
- [x] Maintainability: committed schema, injectable time, reproducible scripts and documented provider limits.
- [x] Coverage: unit rules plus actual MVC/API requests against isolated SQLite databases.

## Genuine findings corrected

1. The first integration run passed 44/45 tests. A failed resolution returned 400 but omitted the useful field errors because ValidationProblemDetails was serialized through its ProblemDetails base type. The exception handler now specifies the runtime type. The regression test checks the actual message; the complete suite subsequently passed 45/45.
2. NuGet restore reported NU1902/NU1903 for transitive System.Security.Cryptography.Pkcs 9.0.11. Added an explicit 10.0.13 reference, the patched version identified in the [Microsoft advisory](https://github.com/advisories/GHSA-2fmv-rv2c-59x7). Restore and Release build then completed without warnings. The related [high-severity advisory](https://github.com/advisories/GHSA-846f-f764-76mx) was also inspected.

Remaining review limitation: valid concurrent edits are last-write-wins; optimistic concurrency is future work. Workflow references were checked against the official [checkout](https://github.com/actions/checkout) and [setup-dotnet](https://github.com/actions/setup-dotnet) instructions; no GitHub-hosted run is claimed.
