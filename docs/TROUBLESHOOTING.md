# Troubleshooting

- **Start:** from the repository root, run ./scripts/run.sh and open http://127.0.0.1:5080. Ctrl+C stops it.
- **Port already in use:** use the existing instance or stop that process. For a different port run ./scripts/run.sh --no-launch-profile --urls http://127.0.0.1:5082.
- **SDK missing:** check dotnet --list-sdks. Install a .NET 10 Apple Silicon SDK compatible with global.json; this project was verified with 10.0.402.
- **Restore/CLI access errors:** use the supplied scripts. They place caches in .local. Package restore requires access to NuGet. The initial agent sandbox blocked MSBuild IPC; build/test commands succeeded when local process/IPC access was allowed. No additional .NET workloads are needed.
- **Database location:** src/ForgeOps.Web/App_Data/forgeops.db, resolved relative to the web content root. Change ConnectionStrings__ForgeOps to use a separate SQLite file.
- **Reset safely:** stop the application first. Back up the entire App_Data folder, then start again. A missing database is migrated and seeded automatically. Keep backups out of Git.
- **Startup failures:** read the terminal output for SDK, migration, provider and file-access errors. Confirm Database:Provider is Sqlite and the database directory is writable. SQL Server requires a separately initialized database.
- **Tests:** ./scripts/test.sh. Full Release verification: ./scripts/check.sh. Test databases are temporary and isolated.
- **Request failures:** terminal logs contain structured method/path/order identifiers and trace IDs. Use the reference shown in an error response to find its log. Ordinary HTTP responses do not include stack traces.
- **Real defect fixed:** API validation errors lost their field messages during base-type serialization. The integration test exposed it; runtime-type serialization fixed it. See CODE_REVIEW.md.
