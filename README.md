# TripleP Backend

ASP.NET Core (.NET 10) backend for the TripleP task manager. See [AGENTS.md](AGENTS.md) for architecture and contribution rules.

## Getting started

Use the .NET 10 SDK. Build and run the host project:

```sh
dotnet run --project src/TripleP.Backend.Web
```

The HTTP profile serves the API at http://localhost:5047. In Development the OpenAPI document and the Scalar API reference are available at http://localhost:5047/scalar. There is no database dependency yet and no required environment variables.

## Running with Docker Compose

```sh
docker compose up --build
```

The API is then available at http://localhost:8080 (Scalar at http://localhost:8080/scalar). The image is built from `src/TripleP.Backend.Web/Dockerfile` with the `./src` build context. A PostgreSQL service and the app's `ConnectionStrings__PostgresConnection` are planned but not wired yet; see the commented block in `docker-compose.yml`.

## Checks

```sh
dotnet build TripleP.Backend.slnx -c Release
dotnet test TripleP.Backend.slnx
```

`dotnet build` restores and compiles the whole solution in Release. `dotnet test` runs the xUnit test projects under `tests/`; the scaffolded tests are placeholders and are being replaced with real coverage. PR CI runs restore and a Release build; `dotnet test` and `dotnet format --verify-no-changes` are being added to the pipeline.

## Structure

- `src/TripleP.Backend.Web`: host and composition root (`Program.cs`, `appsettings`, Dockerfile).
- `src/TripleP.Backend.RestApi`: HTTP surface (Minimal API route groups under `Api/`).
- `src/TripleP.Backend.Logic`: application and business rules.
- `src/TripleP.Backend.Database`: persistence and repositories.
- `src/TripleP.Backend.Abstractions`: shared interfaces and cross-layer contracts.
- `tests`: xUnit projects mirroring the layers under test.

Layers register their services through the framework-namespace extensions `AddTriplePLogic`, `AddTriplePDatabase` and `UseTriplePEndpoints`; the host only calls those extensions. The current API exposes a health endpoint at `GET /Health/health`. PostgreSQL is the intended database, and the persistence stack will be pinned when it is introduced.

Work on `Domain-Feature-Implement` or `Domain-Fix` branches, or `issue/<number>` for GitHub-issue-driven work; `main` receives changes only through PR merges.
