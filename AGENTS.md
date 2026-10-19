# AGENTS.md — Task Manager Backend

## 0. Purpose and working agreement

This project is the backend of a Jira/ClickUp-like task manager. Build maintainable, secure, testable services rather than demo-quality prototypes.

- Follow existing repository conventions and inspect nearby implementations before writing code.
- Make the smallest coherent change. Do not perform unrelated refactors.
- Build the solution after every change (`dotnet build TripleP.Backend.slnx`) and fix any errors or new warnings before moving on. Never leave the tree in a non-building state.
- Before a substantial change, summarize the approach and affected layers.
- This repository is the authority for the API contract. Never silently change a published request/response shape, route, status code or permission. When a contract change is required, fetch the frontend repository updates and inspect how the contract is actually consumed before integrating. Do not add mocks or invented contracts. If the consumer's usage is unclear, report the blocker and continue independent work.
- Prefer clear, boring implementations to clever abstractions. No premature generic frameworks.
- Avoid new dependencies unless justified; prefer the pinned .NET and framework libraries.
- Keep source code identifiers and code comments in English. User-facing error messages and documentation must be clear and consistent.
- After changes, report what changed, which checks ran, and any limitations.

## 1. Technology stack

- .NET 10 (`net10.0`) with the ASP.NET Core SDK; use versions already pinned in the `.csproj` files.
- C# with `Nullable` and `ImplicitUsings` enabled. Keep nullable reference types on.
- ASP.NET Core Minimal APIs for HTTP endpoints, grouped by feature.
- Microsoft.AspNetCore.OpenApi + Scalar.AspNetCore for the OpenAPI document and the interactive API reference (`/scalar` in Development).
- Dependency injection throughout; services are registered through per-layer configurators, not scattered in `Program.cs`.
- PostgreSQL is the intended database (see the commented services in `docker-compose.yml`); the persistence stack is not pinned yet. Pin exactly one data-access approach when it is introduced; do not run two in parallel.
- xUnit is the test framework, already scaffolded in `tests/`.
- Docker and Docker Compose for local and containerized runs. dotnet CLI and the `.slnx` solution file as the build entry point.
- Solution layout is defined in `TripleP.Backend.slnx`; add new projects to it.

## 2. Architecture and dependency direction

Use pragmatic layered architecture:

```text
src/
  TripleP.Backend.Web/          # host and composition root: Program.cs, appsettings, Dockerfile, DI wiring
  TripleP.Backend.RestApi/      # HTTP surface: route groups, endpoints, request/response mapping
  TripleP.Backend.Logic/        # application/business rules and orchestration (no HTTP, no SQL details)
  TripleP.Backend.Database/     # persistence: repositories, data access, migrations
  TripleP.Backend.Abstractions/ # shared interfaces and cross-layer contracts (no concrete dependencies)
tests/
  TripleP.Backend.Tests.Logic/
  TripleP.Backend.Tests.Database/
```

- Intended dependency direction: `Web → RestApi → Logic → Database`. `Abstractions` is the shared contract layer and may be referenced from any layer below `Web`.
- A layer must not reference `TripleP.Backend.Web`. The host depends on layers; layers never depend on the host.
- `Abstractions` must not reference concrete projects; keep it a leaf so everyone can reference it.
- Keep `Program.cs` thin: composition and middleware only. No business logic in the host.
- `RestApi` handles transport concerns (routing, binding, status codes, serialization). Business rules live in `Logic`. SQL and storage details live in `Database`. Do not leak EF/`DbContext` types or SQL into `RestApi` or `Logic`.
- Do not create empty projects/directories solely to satisfy a template. Introduce abstraction when a real use case needs it.

### Suggested initial domains

- Domain services (`Logic`): `tasks`, `projects`, `workspaces`, `boards`, `users`, `comments`, `labels`.
- Repositories / persistence (`Database`): one repository per aggregate, with interfaces in `Abstractions/Database/Repositories/`.
- Endpoint groups (`RestApi`): `Api/Tasks`, `Api/Projects`, `Api/Workspaces`, `Api/Boards`, alongside the existing `Api/Health`.

### Naming and module boundaries

- Use PascalCase for type and file names (`TaskService.cs`, `TaskRepository.cs`, `Endpoint.cs`, `RouteBuilder.cs`). Files are named after the primary type they contain.
- Directories group code by feature/layer slice using PascalCase (`Api/Tasks/`, `Services/`, `Repositories/`).
- Use file-scoped namespaces for new files; match the block-scoped style of legacy files when editing them.
- Keep interfaces in `Abstractions`, implementations in the owning layer, and register both through the layer's configurator.
- Do not add a generic `Helpers.cs` or `Utils.cs` dumping ground for domain code.
- Prefer one public type per file; avoid giant barrel/aggregation files and circular references.

### Layer wiring convention

Each layer follows the established registration pattern and must not be bypassed:

- A layer exposes a public extension method in the .NET framework namespace so the host can call it from `Program.cs`:
  - `AddTriplePLogic()` and `AddTriplePDatabase()` in `Microsoft.Extensions.DependencyInjection`.
  - `UseTriplePEndpoints()` in `Microsoft.AspNetCore.Builder`.
- The extension delegates to an internal top-level `ServicesConfigurator` / `RouteBuilder`, which delegates to per-feature configurators (for example `Logic/Services/ServicesConfigurator.cs`, `Database/Repositories/ServicesConfigurator.cs`, `RestApi/Api/RouteBuilder.cs`).
- When adding a feature, register it from the parent configurator/route builder. Do not wire services directly in `Program.cs`.

## 3. ASP.NET Core and Minimal APIs

- Define endpoints in `RestApi/Api/<Feature>/RouteBuilder.cs` and put the handler logic in `Endpoint.cs` next to it. Register the group from `Api/RouteBuilder.cs`.
- Follow the existing pattern: map with an explicit HTTP verb and method reference, and give the route a stable name, e.g. `MapGet("/tasks/{id}", Endpoint.GetById).WithName("Tasks_GetById")`.
- Keep handlers thin: bind and validate input, call a `Logic` service, map the result to an HTTP response.
- Return typed results and meaningful status codes; do not return 200 for failures. Keep error responses consistent (prefer `ProblemDetails`).
- Accept and pass a `CancellationToken` through every async handler; do not ignore client disconnects.
- Validate untrusted input (route/query/body) at the boundary and reject invalid requests with 400 before touching business logic.
- Model requests and responses as explicit DTOs/records, not domain entities. Do not expose persistence models over HTTP.
- Treat list endpoints explicitly: pagination, filtering, sorting and stable ordering. Never return unbounded collections.
- Do not put authorization logic in handlers beyond labeling the requirement; enforce it in the pipeline/policy layer.
- No business logic, ad-hoc SQL or long-running work directly inside an endpoint handler.

## 4. Business logic and domain modeling

- Model identity and ownership explicitly: workspace → project → board/task; use stable IDs, not names, as keys.
- Model task lifecycle using explicit statuses and transitions; avoid free-form magic status strings.
- Permissions are domain data and policy decisions, not scattered `isAdmin` checks.
- Keep services stateless and injectable; resolve dependencies via constructor injection. Avoid service locators and static mutable state.
- Methods should be small, single-purpose and independently testable. Avoid hidden I/O and surprising side effects.
- Handle empty, not-found, conflict and partial-success cases explicitly; return domain results/errors rather than throwing for expected outcomes.
- Preserve task ordering and handle concurrent updates; guard against lost updates where the domain requires it.
- Handle timestamps and timezone-consistently. Store and transmit UTC (ISO 8601); convert at the edges only.

## 5. Persistence

- PostgreSQL is the target database. Configure connections through configuration/`IConfiguration` and secrets, never hardcoded in code.
- Keep connection strings and credentials out of source control. Use `appsettings.*.secrets.json` (gitignored), user secrets or environment variables.
- Define repository interfaces in `Abstractions/Database/Repositories/` and implement them in `Database/Repositories/`.
- Keep query logic inside the `Database` layer. Do not let `Logic` or `RestApi` build queries or reference `DbContext`.
- Do not perform N+1 access patterns; load related data deliberately.
- Make schema changes through migrations committed with the code; never mutate a shared database schema by hand.
- Never expose raw persistence entities or internal IDs that leak storage layout; map to dedicated DTOs.
- Add indexes for the access patterns that list endpoints rely on, and keep pagination keys index-friendly.

## 6. Configuration, secrets and observability

- Provide `appsettings.json` defaults and environment-specific overrides (`appsettings.Development.json`, `appsettings.Production.json`). Do not commit real secrets.
- Read configuration through the options pattern (`IOptions<T>` / typed settings) rather than reading keys ad hoc.
- Log through `ILogger<T>` with structured messages and appropriate levels. Do not use `Console.WriteLine` for application logging.
- Never log secrets, tokens, credentials, connection strings or full private task contents.
- Prefer failing fast on missing required configuration over silently proceeding with defaults.

## 7. TypeScript-equivalent: C# and runtime safety

- `Nullable` reference types are mandatory; `ImplicitUsings` stays enabled. Do not disable nullable to silence warnings.
- Avoid suppressing warnings (`#pragma warning disable`, `!` null-forgiving) unless a documented exception is unavoidable, and keep it narrowly scoped.
- Prefer `async`/`await` end to end. Never use `async void` except for event handlers, and never block on async code (`.Result`, `.Wait()`).
- Use `record` types for immutable DTOs/contracts; use classes where mutable lifecycle is required.
- Use `Try`-style or result types for expected failures; reserve exceptions for exceptional conditions.
- Validate external input with explicit checks (or a validation library) at the boundary; types alone do not validate payloads.
- Treat `TreatWarningsAsErrors` and an `.editorconfig` as the target quality bar once introduced; do not weaken these to make a build pass.

## 8. Code quality rules

- Keep nullable, implicit usings and `net10.0` consistent across all projects.
- Add a root `.editorconfig` and analyzer configuration when the project grows; enforce them in CI.
- Prefer `dotnet format` for formatting and sorting using directives; keep style decisions in `.editorconfig` instead of ad-hoc.
- Keep `Program.cs` and configurators small; unwire unused registrations and dead code.
- Avoid unused members, `public` surface that nothing consumes, and circular project references.
- Keep async suffix (`Async`) on asynchronous methods by convention, and use meaningful names everywhere.
- Add a `Directory.Build.props` for shared properties (nullable, language version, analyzers) once more than one project repeats them.

## 9. Testing policy

- xUnit is the test framework and the test projects are already scaffolded:
  - `tests/TripleP.Backend.Tests.Logic` for business logic.
  - `tests/TripleP.Backend.Tests.Database` for persistence.
- Add test projects following the same naming convention (`TripleP.Backend.Tests.<Layer>`) and add them to `TripleP.Backend.slnx`.
- Replace placeholder tests (`UnitTest1`) with meaningful tests; do not leave empty placeholder tests behind.
- Cover domain rules, edge cases, error paths and boundary conditions. A bug fix should come with a regression test.
- Prefer fast unit tests; use a real or containerized database only where persistence behavior genuinely needs it.
- Do not add mock/time-freezing frameworks casually; use the pinned stack first.
- Never fake a passing test run or claim tests ran when they did not.

## 10. Performance and security

- Use pagination for all collection endpoints; avoid loading entire tables and avoid N+1 request/query patterns.
- Do not perform unbounded or long-running work inside a request; offload where appropriate.
- Validate and sanitize all input; never build SQL or shell commands by string concatenation.
- Enforce authentication and authorization server-side and by policy. Hiding an endpoint from docs is not access control.
- Return generic error messages to clients; keep internal details and stack traces out of responses.
- Configure CORS, HTTPS redirect and security headers deliberately; do not introduce wildcard permissive policies in production.
- Keep secrets server-side; respect token lifetimes and never log credentials.
- Add structured error reporting without leaking tokens or private task contents.

## 11. Mandatory verification and CI

Use the dotnet CLI and the `.slnx` solution. CI is defined in `.github/workflows/pr.yml`.

Required checks (run from the repository root):

```bash
dotnet restore TripleP.Backend.slnx
dotnet build TripleP.Backend.slnx -c Release --no-restore
dotnet test TripleP.Backend.slnx -c Release --no-build
dotnet format TripleP.Backend.slnx --verify-no-changes
```

- Build after every change, not only before a commit or PR: run the solution build and resolve failures and new warnings as part of the change itself.
- Before merging, CI must restore and build the solution in Release; extend `pr.yml` to run `dotnet test` and `dotnet format --verify-no-changes` once formatting/test configuration is in place.
- Update CI when a new project is added to the solution.
- Never claim checks passed unless actually executed. State which checks could not run.
- All applicable pipelines for the latest PR revision must pass, with no open or unresolved review threads before merge.

### Git workflow

- Never commit or push directly to `main`; update it only by merging pull requests.
- Work autonomously on feature branches using `Domain-Feature-Implement` or `Domain-Fix`.
- When work is driven by a GitHub issue, name the branch after it as `issue/<number>` (for example `issue/42`).
- Use simple English commit messages such as `add feature`, `refactor something` or `drop filename`.
- Fetch updates from the frontend repository (`../TripleP.Frontend`) before changing an API contract. Inspect the appropriate remote revision without discarding or overwriting the frontend team's local changes.
- Do not commit build output (`bin/`, `obj/`), user secrets, IDE files or local databases.

## 12. Definition of Done (for AI agents and humans)

A change is complete only when:

1. Its scope and acceptance criteria are implemented without unrelated rewrites.
2. New code obeys the layering, naming and dependency-direction rules.
3. API contracts are typed, backward compatible or explicitly reported as breaking, and external inputs are validated.
4. Not-found, conflict, empty, success and error paths are handled with correct status codes.
5. Authentication and authorization requirements are enforced server-side where relevant.
6. Relevant tests are added or updated and pass; the solution builds in Release.
7. Format, build and test checks pass when configured; unavailable checks and pending CI additions are disclosed.
8. The final response lists touched areas, behavioral changes, checks performed and remaining risks.
