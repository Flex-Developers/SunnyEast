# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

SunnyEast ("Солнечный восток") is a production pickup-commerce + back-office platform for a multi-store retail business (live at https://solnechny-vostok.ru/). Blazor WebAssembly client + ASP.NET Core Web API, layered (Clean Architecture). The solution file is `FlexErp.sln` — a legacy name; the product is "SunnyEast". Code comments are largely in Russian.

**Target framework is `net9.0` across every project** (CI uses `9.0.x`). Ignore any global default suggesting .NET 10 — this repo is .NET 9.

## Commands

```bash
# Build whole solution
dotnet build FlexErp.sln

# Run API (Swagger at /swagger in Development; listens on http://localhost:5121)
dotnet run --project src/WebApi/WebApi.csproj

# Run Blazor WASM client
dotnet run --project src/Client/Client/Client.csproj

# Add an EF Core migration (DbContext lives in Infrastructure, host in WebApi)
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/WebApi
```

- **There are no test projects.** CI runs `dotnet test`, but it currently matches nothing. Don't assume a test suite exists.
- To run against an in-memory DB instead of MySQL, set config `UseInMemoryDatabase=True` (read in `Infrastructure/DependencyInitializer.cs`).
- On startup `WebApi/Program.cs` auto-applies pending migrations and seeds base roles; in Development it also seeds a `SuperAdmin`.

## Architecture

Layered, dependencies point inward. `src/`:

```
Domain                -> entities + enums (BaseEntity, Order, Product, Shop, Staff, ApplicationUser...)
Application.Contract  -> Commands/Queries/Responses (DTOs). References ONLY MediatR.Contracts
Application           -> MediatR handlers, AutoMapper Profiles, FluentValidation, business rules
Infrastructure        -> EF Core (ApplicationDbContext), Identity, JWT, SignalR hub, email/SMS/push, DB snapshot
WebApi                -> thin controllers, DI composition, auth, hub mapping
Client/Client         -> Blazor WASM pages/components (MudBlazor)
Client/Client.Infrastructure -> client services, auth state, HTTP, realtime, preferences, theme
```

### CQRS request flow (the core pattern)

```
Controller (: ApiControllerBase) -> Mediator.Send(command/query)
  -> Handler in Application/Features/<Area>/Commands|Queries
  -> IApplicationDbContext (EF Core) -> SaveChangesAsync
  -> AutoMapper Map<Response> -> returned to controller
```

- **Commands and Queries live in `Application.Contract`, not `Application`.** They implement `IRequest<T>` and are shared by both the API and the Blazor client. Handlers live separately in `Application/Features/`. When adding a feature: add the command/query (+ response) under `Application.Contract/<Area>/`, then its handler under `Application/Features/<Area>/`, then a thin controller action.
- Controllers are intentionally trivial — `[Authorize]` + `Mediator.Send(...)` + an `Ok`/`NoContent`/`CreatedAtAction`. `ApiControllerBase` (`src/WebApi/Controllers/ApiControllerBase.cs`) sets `[Route("api/[controller]")]` and lazily resolves `ISender`.
- Each feature folder has a `*Profile.cs` (AutoMapper) and sometimes a `*Validator.cs` (FluentValidation). MediatR/AutoMapper/validators are all registered by assembly scan in `Application/DependencyInitializer.cs`.

### Errors

Handlers throw domain exceptions from `Application/Common/Exceptions/` (`NotFoundException`, `BadRequestException`, `ExistException`, `ForbiddenException`). The global `CustomExceptionsFilterAttribute` (registered in `WebApi/DependencyInitializer.cs`) maps them to RFC7231 `ProblemDetails`. Don't write try/catch-to-status-code in controllers — throw the exception. (Note: `HandleBadRequestException` currently sets the HTTP status to 409 instead of 400 — a known inconsistency.)

### Auth & roles

- ASP.NET Identity (`ApplicationUser`, `IdentityRole<Guid>`) + JWT bearer. Signing key from config `JWT:Secret`. Wired in `Infrastructure/DependencyInitializer.cs`.
- Roles are constants in `Application.Contract/Identity/ApplicationRoles.cs`: `SuperAdmin`, `Administrator`, `Salesman`, `Customer`. Use the CSV helpers `AllStaffCsv` and `AllAdmins` in `[Authorize(Roles = ...)]`.
- Staff access is scoped by **role and by shop assignment** (`Staff` entity), not a single global admin flag.

### Real-time orders (SignalR)

- Hub `OrderHub : Hub<IOrderClient>` mapped at `/hubs/orders` (`Program.cs`).
- JWT for the hub is passed as `?access_token=` query string — the `JwtBearerEvents.OnMessageReceived` hook in `Infrastructure/DependencyInitializer.cs` reads it for paths under `/hubs/orders`. CORS policy `"Client"` uses `AllowCredentials()` specifically for this.
- On connect, the hub runs `GetOrderHubGroupsQuery` (via MediatR) to add the connection to shop/customer/super-admin groups (`OrderGroupNames`). Handlers push updates through `IOrderRealtimeNotifier` (e.g. `notifier.OrderStatusChanged(response)` in `ChangeOrderStatusCommandHandler`).

### DI conventions

- Each layer exposes an extension method: `AddApplication()`, `AddInfrastructure(config)`, `AddWebApi()` — composed in `Program.cs`.
- Client services auto-register by convention: any class implementing marker `IAppService` is bound to its `I{ClassName}` interface in `Client.Infrastructure/Startup.cs` (`AutoRegisterInterfaces<IAppService>`). To add a client service, name it `FooService : IAppService` with interface `IFooService` and it's registered automatically. Config keys are constants in `Client.Infrastructure/Consts/Config.cs`; the API `HttpClient` is named `"SunnyEast"`.
- Client UI swaps layouts by role: `AdminLayout` / `CustomerLayout` / `MainLayout`.

### Operational features

- DB snapshot/backup/restore via `DbSnapshotService` (MySQL dump/import), `Backups` config section, `SuperAdmin`-only. Restore takes an automatic pre-restore backup.
- Web Push uses VAPID keys (`VapidKeys` in API config, public key mirrored in client config). Email via MailKit (`Email` section), SMS + OTP via `SmsInt*` services with daily quota (`SmsQuota`).

## Deployment

- `.github/workflows/server-build.yml`: PR build/test on `master`.
- `.github/workflows/production.yml`: on push to `production` — auto-version tag, inject prod URLs/keys into appsettings via `jq`, build & push Docker images `isruf/se-api` (root `Dockerfile`) and `isruf/se-cl` (`src/Dockerfile`, Blazor WASM → nginx) to Docker Hub.
