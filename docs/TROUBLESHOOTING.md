# Running ForgeOps and troubleshooting

## Local startup

Install a compatible .NET 10 SDK, then from the repository root:

~~~sh
./scripts/run.sh
~~~

Open **http://127.0.0.1:5080**. Stop with Ctrl+C.

To change the local port:

~~~sh
./scripts/run.sh --no-launch-profile --urls http://127.0.0.1:5082
~~~

If the port is already in use, stop the existing application process or use a different port.

## Dependencies and tests

Use **dotnet --list-sdks** to verify the SDK. The scripts restore NuGet packages, which requires network access on the first run. For automated checks:

~~~sh
./scripts/test.sh
./scripts/check.sh
~~~

The full check restores packages, builds Release, executes tests, and publishes local output.

## Database

By default, local SQLite data resides at **src/ForgeOps.Web/App_Data/forgeops.db**. The repository ignores this directory, and it should not be committed.

To reset a local demo, stop the application and back up/remove the local database only after confirming it contains no information you need. A missing SQLite database is migrated and populated with fictional seed data at startup. Use a separate database for automated test runs.

SQL Server is an optional, independently configured provider; its runtime operation has not been verified for v0.1.0. See [architecture](DESIGN.md).

## Request failures

Inspect local application logs for request identifiers and error details. Validation failures return field-specific messages; ordinary HTTP responses do not expose stack traces.

## Hosted demonstration

The [Render demo](https://forgeops-mbelsabah.onrender.com) may take longer to start after inactivity and may reset its fictional data on redeployment or restart. **Never enter real personal or production data into the public demo.**
