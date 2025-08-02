# SunnyEast/Солнечный восток

## What is this

A web-based B2B/B2C order management and storefront platform. It combines user accounts, shops, products, categories, carts and orders with role-based administration (super admin, staff, salesman, customers) and full database snapshot backup/restore. It's built to serve businesses that need a multi-store catalog, staff assignment, and order processing with JWT auth.

## Quick purpose

* **Business:** Retail/wholesale e-commerce with staff-managed shops and order workflows. Super-admin can snapshot and restore the entire MySQL database.
* **Core value:** Reliable data portability (full DB snapshot), role-aware UI, and modular clean backend with commands/queries.

## Tech stack

* .NET 9 (Web API backend + EF Core for MySQL)
* Blazor WebAssembly client with MudBlazor UI
* JWT authentication / role-based authorization
* MySqlBackup.NET for full database dumps and restores
* CQRS-style handlers in Application layer (some controllers still use mediator-like patterns)

## Setup (local)

1. **Clone repository** and open solution.
2. **Configure `appsettings.json`/environment**:

   * Connection string named `mySql` pointing to your MySQL instance.
   * Optional `Backups` section (defaults if missing):

     ```json
     "Backups": {
       "Directory": "App_Data/Backups",
       "FilePrefix": "snapshot",
       "RunMigrationsAfterRestore": true,
       "KeepLastLocalCopies": 10
     }
     ```
3. **Run backend**: it applies pending EF migrations on startup and seeds roles/users.
4. **Run WASM client** (default expects backend at configured API base URL in client config). Ensure CORS policy allows the client origin.
5. **Authenticate** as super admin (seeded or created) to access `/admin/database`.

## Features overview

* **Database snapshot**: full dump export to `.sql.gz` with all schema, routines, triggers, events; import restores entire DB with an automatic pre-restore backup.
* **Role-based pages**: SuperAdmin, Staff, Salesman, Customer with granular access in controllers via roles.
* **JWT tokens** for API auth; client stores token in local storage and injects into headers.
* **HttpClient wrapper** with unified error handling, timeout override, and byte download support.

## Backup & Restore behaviour

* `CreateBackupGzipAsync`: grabs a global lock (unless skipped), exports full MySQL database to temporary `.sql`, compresses to `.gz`, logs timings, returns path. Export includes routines/procedures/views/events; does not drop database on restore to avoid destructive blowaway.
* `RestoreFromDumpAsync`: holds the global lock, saves uploaded file, creates an **automatic backup** (without deadlock), decompresses if needed, imports via MySqlBackup.NET (with infinite command timeout), then optionally runs EF migrations if enabled. Cleans temp files.
* Retains last `KeepLastLocalCopies` auto-backups in configured folder; older ones are pruned.

### Idempotent / edge cases

* Restoring the same snapshot over itself is safe: dump includes DROP TABLE and recreates schema; EF migrations are no-ops if already up to date.
* Partial failures: automatic backup failure does not abort restore; import errors are logged, and EF migrations run only if configured.

## Authentication & Authorization

* JWT tokens are issued by the backend; client attaches token to `Authorization: Bearer` header.
* Roles defined (e.g., `SuperAdmin`) guard controllers via `[Authorize(Roles=...)]` attributes.

## Client behavior

* Blazor WASM uses `HttpClientService` to centralize calls, handle unauthorized responses and redirect to login.
* Database admin UI uses JS interop (`downloadFileFromBytes`) to save backups locally and multipart upload for restore with extended timeout.

## Logging

* Uses built-in Microsoft.Extensions.Logging.
* Startup logs to console (development) by default. Configure additional providers in `Program.cs` if needed (e.g., file, external). Logs include:

  * EF Core SQL commands (with timings)
  * Snapshot/restore progress, errors, and durations.
* In production, ensure appropriate log levels and persistence (e.g., redirect console to file or use a sink).

## Common commands

```bash
# run backend
dotnet run --project src/WebApi/WebApi.csproj

# run client
dotnet run --project src/Client/Client.csproj
```

## Folder structure summary

* `Application` / `Application.Contract`: business logic, commands, queries, DTOs.
* `Domain`: entity definitions.
* `Infrastructure`: persistence, services, EF migrations.
* `WebApi`: API surface, snapshot service, controllers, authorization.
* `Client` & `Client.Infrastructure`: Blazor front-end and shared HTTP/auth services.

## Deployment notes

* Increase Kestrel timeouts only for snapshot/restore endpoints; global long timeouts can impact connection throughput—consider using dedicated endpoint with extended limits or chunked processing in heavy scenarios.
* Secure JWT secrets and database credentials via environment variables or secret store.
* For production backups, offload snapshot files to durable storage instead of temp (adapt `DbSnapshotService`).

## Troubleshooting

* If restore hangs: check logs for import errors, ensure MySQL user has necessary privileges, and no other concurrent restore/backup is running due to global semaphore.
* If tokens fail: verify local storage contains valid `JwtTokenResponse` and backend JWT config matches client usage.
* CORS issues: make sure the client origin is allowed in backend policy.

## Recommended improvements

* Add validation/size limits and progress feedback for restore uploads.
* Move backup storage to cloud (S3 / Blob) with signed access.
* Secure UI behind MFA for super admin actions.
* Add checksum/validation for snapshot files before restore.

## Ready for PR checklist

* [ ] All new code has logging and error handling.
* [ ] Snapshot/restore tested locally (export then restore identical snapshot).
* [ ] Roles and auth verified on protected endpoints.
* [ ] Environment-specific configuration (secrets) not leaked.
* [ ] README updated (this file) and concise for reviewers.

---

Last updated: 2025-08
