# TripleP Backend

ASP.NET Core (.NET 10) backend for the TripleP task manager. See [AGENTS.md](AGENTS.md) for architecture and contribution rules.

## Getting started

Use the .NET 10 SDK. Build and run the host project:

```sh
dotnet run --project src/TripleP.Backend.Web
```

The HTTP profile serves the API at http://localhost:5047. In Development the OpenAPI document and the Scalar API reference are available at http://localhost:5047/scalar.

The database connection string is required configuration and is read from `ConnectionStrings:PostgresConnection`. It is not committed: set it through environment variables, `appsettings.Development.secrets.json` (gitignored) or user secrets:

```sh
dotnet user-secrets set "ConnectionStrings:PostgresConnection" "Host=localhost;Port=5432;Database=triplep;Username=triplep;Password=triplep" --project src/TripleP.Backend.Web
```

The host fails fast with a clear error when the connection string is missing.

### Database and migrations

Persistence uses Entity Framework Core with the `Npgsql.EntityFrameworkCore.PostgreSQL` provider (pinned in `src/TripleP.Backend.Database`). EF entities are dependency-free POCOs in `src/TripleP.Backend.Abstractions/Database/Entities` (currently `User`); the ASP.NET Identity user is a separate class, `ApplicationUser : IdentityUser`, in `src/TripleP.Backend.Database/Identity`. Both are mapped by `TriplePDbContext` (`AspNetUsers` and `Users`).

Schema changes are made through EF migrations under `src/TripleP.Backend.Database/Migrations`. The `dotnet-ef` tool is pinned in the local tool manifest:

```sh
dotnet tool restore
dotnet ef migrations add <Name> --project src/TripleP.Backend.Database --startup-project src/TripleP.Backend.Web --output-dir Migrations
dotnet ef database update --project src/TripleP.Backend.Database --startup-project src/TripleP.Backend.Web
```

The tooling starts the Web host at design time, so it needs the same configuration as the app. It runs as `Development` by default, where the connection string is read from user secrets; otherwise set `ConnectionStrings__PostgresConnection`. `database update` needs a reachable PostgreSQL; pass `--connection "<connection string>"` to target another host, for example from inside a container.

The API itself does not apply migrations. `docker-compose.yml` overrides the `app` service command to run the EF Core migrations bundle (`./efbundle`, built in the Dockerfile `migrations` stage) with `ConnectionStrings__PostgresConnection` and only then start the API. Running the app outside compose (for example `dotnet run`) still requires the explicit `dotnet ef database update` command above.

### Identity endpoints

The standard ASP.NET Identity API endpoints are mapped next to the rest of the API, under the `/api` prefix (`app.MapGroup("api").MapIdentityApi<ApplicationUser>()` in `Program.cs`):

| Method | Route |
| --- | --- |
| `POST` | `/api/register` |
| `POST` | `/api/login` (`?useCookies=false` returns bearer tokens) |
| `POST` | `/api/refresh` |
| `GET` | `/api/confirmEmail` |
| `POST` | `/api/resendConfirmationEmail` |
| `POST` | `/api/forgotPassword` |
| `POST` | `/api/resetPassword` |
| `POST` | `/api/manage/2fa` |
| `GET`, `POST` | `/api/manage/info` |

`/api/manage/*` requires authentication with the bearer token returned by `/api/login?useCookies=false` or with the Identity cookie. Email delivery is not configured yet: `NoOpEmailSender` (`src/TripleP.Backend.Database/Identity`) satisfies the endpoints but sends nothing, so confirmation and password-reset links are not delivered. The database schema must be up to date (`dotnet ef database update`) before the endpoints can be used.

### Database tests

`tests/TripleP.Backend.Tests.Database` contains only database-backed tests and runs against a real PostgreSQL. `Startup` loads `tests/TripleP.Backend.Tests.Database/appsettings.json` and registers the real services through `AddTriplePDatabase`, so the tests use the same wiring as the application. Environment variables override the file (for example `ConnectionStrings__PostgresConnection` in CI). There is no fixture and the tests do not create or migrate the database: the schema must be applied before the run.

Run them locally against the compose database (`appsettings.json` already points at `triplep_test`):

```sh
docker compose up -d db
dotnet tool restore
dotnet ef database update --project src/TripleP.Backend.Database --startup-project src/TripleP.Backend.Web --connection "Host=localhost;Port=5432;Database=triplep_test;Username=triplep;Password=triplep"
dotnet test tests/TripleP.Backend.Tests.Database
```

## Running with Docker Compose

```sh
docker compose up --build
```

The API is then available at http://localhost:8080 (Scalar at http://localhost:8080/scalar). The image is built from `src/TripleP.Backend.Web/Dockerfile` with the `./src` build context. Compose starts a PostgreSQL 17 service (`db`, local credentials in `docker-compose.yml` for development only) with its data directory on `tmpfs`: the database is intentionally ephemeral, so every start begins empty and the migrations bundle reapplies the schema (and future seed data) instead of reusing stale local data. `ConnectionStrings__PostgresConnection` is passed to the app. Once the `db` healthcheck passes, the `app` container applies pending migrations with the EF Core bundle and then starts the API. The bundle is framework-dependent and runs on the .NET runtime of the base image; for production, prefer a separate one-shot migration step over starting several replicas that migrate concurrently. The container listens on 8080 (the `ASPNETCORE_HTTP_PORTS` default of the base image), which compose publishes as `8080:8080`.

Because the database lives in `tmpfs`, stopping the stack wipes its data; the next `docker compose up --build` starts from an empty database and reapplies migrations. Do not restart only `db` while `app` keeps running: the API would hold a connection pool to a database whose schema has just disappeared.

## Checks

```sh
dotnet build TripleP.Backend.slnx -c Release
dotnet test tests/TripleP.Backend.Tests.Logic
dotnet test tests/TripleP.Backend.Tests.Database
```

`dotnet build` restores and compiles the whole solution in Release. `TripleP.Backend.Tests.Logic` holds the unit tests (still a placeholder); `TripleP.Backend.Tests.Database` holds only database-backed tests and requires a reachable PostgreSQL (see "Database tests" above).

PR CI is split across three workflows. `.github/workflows/pr.yml` runs the `build` job (restore, Release build, `dotnet format --verify-no-changes`) and then calls two reusable workflows in parallel: `unit-tests.yml` (unit tests from `TripleP.Backend.Tests.Logic`) and `database-tests.yml`, which starts a `postgres:17` service container, applies the committed EF Core migrations to it with `dotnet ef database update` and then runs `TripleP.Backend.Tests.Database` against it.

## Structure

- `src/TripleP.Backend.Web`: host and composition root (`Program.cs`, `appsettings`, Dockerfile).
- `src/TripleP.Backend.RestApi`: HTTP surface (Minimal API route groups under `Api/`).
- `src/TripleP.Backend.Logic`: application and business rules.
- `src/TripleP.Backend.Database`: persistence, Identity wiring and repositories.
- `src/TripleP.Backend.Abstractions`: dependency-free shared interfaces, cross-layer contracts and EF entities (`Database/Entities`).
- `tests`: xUnit projects mirroring the layers under test.

Layers register their services through the framework-namespace extensions `AddTriplePLogic`, `AddTriplePDatabase` and `UseTriplePEndpoints`; the host only calls those extensions. The current API exposes a health endpoint at `GET /api/health`. Persistence is pinned to Entity Framework Core with PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`); ASP.NET Identity (`ApplicationUser`, `IdentityDbContext`) is registered by `AddTriplePDatabase`, which requires an `IConfiguration` with `ConnectionStrings:PostgresConnection`.

Work on `Domain-Feature-Implement` or `Domain-Fix` branches, or `issue/<number>` for GitHub-issue-driven work; `main` receives changes only through PR merges.
