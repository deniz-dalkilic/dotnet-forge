# DotnetForge

A reusable .NET 10 starter focused on clean boundaries, minimal APIs, and production-friendly defaults.

## What is in the repository

- A naming-safe solution rooted at `DotnetForge`
- Minimal API host with correlation, request logging, global exception handling, and Problem Details responses
- Application, Domain, Infrastructure, and Worker projects
- Central package management and lock-file support
- API integration tests and layer-level test projects, including a Worker test foundation
- Docker Compose for local PostgreSQL and Seq
- EF Core 10 + PostgreSQL persistence wiring with migration-ready infrastructure
- HybridCache-based caching foundation with optional future distributed cache integration
- Hangfire-backed background processing with API enqueue endpoints and a dedicated Worker host for execution
- Serilog + Seq logging and a practical OpenTelemetry trace/metric baseline

## Reference scenario flow

The repository includes a canonical **Reference Scenario** for onboarding, debugging, and deriving new features. It lives in these areas:

- `src/DotnetForge.Api/Extensions/EndpointRouteBuilderExtensions.cs`
- `src/DotnetForge.Application/ReferenceScenarios/Greetings/*`
- `src/DotnetForge.Infrastructure/ReferenceScenarios/ReferenceScenarioJobDispatcher.cs`

Reference scenario endpoints:

- `POST /api/reference-scenarios/greetings/execute`
- `GET /api/reference-scenarios/greetings/{id}`

What this flow demonstrates end-to-end:

- request handling in Minimal API endpoints
- FluentValidation-driven request validation
- application-layer orchestration in a dedicated use case service
- domain entity creation
- EF Core persistence through the repository abstraction
- cache population and cache-aside reads
- background job enqueueing through an application-facing abstraction
- structured logging, correlation id propagation, and trace-friendly execution
- consistent success, validation, and not-found handling

How to use it as a starting point for new features:

1. Start debugging from the `POST /api/reference-scenarios/greetings/execute` endpoint.
2. Step into `ReferenceScenarioGreetingService` to follow validation, orchestration, domain creation, persistence, caching, and background dispatch.
3. Use `GET /api/reference-scenarios/greetings/{id}` to inspect the read/query path and cache-aside retrieval behavior.
4. Copy the vertical slice structure for future features rather than adding logic directly to `Program.cs` or the endpoint body.

## Template packaging and local template testing

The repository is prepared to be packaged later as a custom `dotnet new` template.

### Template identity

- `identity`: `DenizDalkilic.DotnetForge.CleanArchitecture`
- `name`: `.NET Forge Clean Architecture Template`
- `shortName`: `dnf-cleanapi`
- `sourceName`: `DotnetForge`

### How sourceName replacement works

`sourceName` is `DotnetForge`, so running a command such as:

```bash
dotnet new dnf-cleanapi -n MyProject
```

replaces `DotnetForge` in solution names, project names, assembly names, namespaces, and file contents with `MyProject` where the template engine applies source-name replacement.

This is why the repository intentionally keeps the internal source root consistent as `DotnetForge` instead of using a vague name such as `Template`.

### Strings that must not be changed casually

Do not casually change these values unless you are intentionally re-authoring the template package itself:

- `.template.config/template.json`
- template `identity`: `DenizDalkilic.DotnetForge.CleanArchitecture`
- template `shortName`: `dnf-cleanapi`
- template `sourceName`: `DotnetForge`
- the baseline solution/project naming pattern rooted at `DotnetForge`

Changing these carelessly can break rename behavior, reduce template discoverability, or leave generated projects with mixed names.

### Install the template locally

From the repository root:

```bash
dotnet new install .
```

If you need to reinstall after template changes:

```bash
dotnet new uninstall DenizDalkilic.DotnetForge.CleanArchitecture
dotnet new install .
```

### Test the template locally

Create a scratch directory outside this repository and run:

```bash
dotnet new dnf-cleanapi -n MyProject
cd MyProject
dotnet restore
dotnet build
dotnet test
```

Recommended checks after generation:

- verify the solution file is renamed to `MyProject.sln`
- verify project names and namespaces are rooted at `MyProject`
- verify README, Docker Compose defaults, and application settings do not leave unnecessary `DotnetForge` leftovers

### Pack it later as a NuGet template package

A common next step is to create a dedicated packaging project or `.nuspec` that includes the repository content and `.template.config/template.json`, then produce a `.nupkg` template package.

Typical flow:

```bash
dotnet pack
```

After packing, install the generated package locally for validation:

```bash
dotnet new install /path/to/Your.Template.Package.nupkg
```

## Local infrastructure

Start PostgreSQL and Seq for local development:

```bash
docker compose -f docker/docker-compose.yml -f docker/docker-compose.override.yml up -d
```

- PostgreSQL: `localhost:5432`
- Seq ingestion: `http://localhost:5341`
- Seq UI: `http://localhost:8081`
- Seq first-run admin password: `ChangeMe123!`

The default development connection string and observability settings are configured in the host `appsettings.json` files.

## Background processing foundation

The template uses Hangfire with PostgreSQL storage and splits responsibilities like this:

- **API host**: accepts HTTP requests, exposes the dashboard, and enqueues jobs
- **Worker host**: runs the Hangfire server and registers recurring jobs

This is the most pragmatic template default because it keeps the API process lightweight while preserving a clean extraction path toward a dedicated background-processing deployment later.

Sample background processing endpoints:

- `POST /api/jobs/greetings/fire-and-forget`
- `POST /api/jobs/greetings/scheduled`
- `POST /api/reference-scenarios/greetings/execute`
- Hangfire dashboard: `/hangfire`

The Worker project also registers a recurring heartbeat job to demonstrate how future recurring jobs should be added without coupling them to the API host lifecycle.

## Observability foundation

The template provides a small but production-appropriate observability baseline:

- **Serilog** for structured application logs
- **Seq** as the default local structured log sink
- **OpenTelemetry** traces and metrics for ASP.NET Core, `HttpClient`, runtime metrics, and custom background-job telemetry
- **OTLP** exporter wiring kept optional and disabled by default so local development stays simple

### Correlation and trace alignment

- HTTP requests continue to use `X-Correlation-ID`
- logs include correlation scope data and activity identifiers where available
- OpenTelemetry traces use the same request pipeline and background jobs create internal activities for trace continuity in observability backends

### Sensitive data guidance

By default, the template avoids logging request/response bodies and headers. Keep these values redacted unless there is a strict operational reason to capture them:

- `Authorization`
- `Cookie`
- `Set-Cookie`
- `X-Api-Key`
- connection strings, secrets, tokens, and personal data

Also keep `Database:EnableSensitiveDataLogging` disabled outside carefully controlled local debugging scenarios.

### Enabling OTLP later

Set these values in `Observability:OpenTelemetry:Otlp` when you want to export traces and metrics to a collector:

- `Enabled=true`
- `Endpoint=http://localhost:4317` or your collector endpoint
- `Protocol=grpc` or `http/protobuf`
- optional `Headers` for authenticated collectors

## Caching foundation

The template uses `HybridCache` as the default caching direction for .NET 10. In V1, it runs perfectly well with in-memory storage only, so Redis is not a required runtime dependency.

Set `Caching:Enabled` to `false` to disable cache usage without removing registrations or changing application code.

Use in-memory/hybrid-only caching when:

- you run a single API instance
- cache entries are small and disposable
- a cold cache after restart is acceptable
- you want the simplest local development and early production setup

Move to a distributed cache provider when:

- you scale the API to multiple instances
- cache consistency across nodes matters
- restart cold-starts become expensive
- you need cross-instance cache invalidation guarantees

The `Caching:Distributed` section is intentionally present as an extension point for future `IDistributedCache`/Redis wiring without forcing that complexity into V1.

## Migrations

The Infrastructure project contains the EF Core design-time package and `ForgeDbContextFactory`, so migrations can be created from the repository root:

```bash
dotnet ef migrations add InitialPersistence \
  --project src/DotnetForge.Infrastructure \
  --startup-project src/DotnetForge.Api \
  --output-dir Persistence/Migrations
```

Apply migrations:

```bash
dotnet ef database update \
  --project src/DotnetForge.Infrastructure \
  --startup-project src/DotnetForge.Api
```

## Solution layout

- `src/DotnetForge.Api`
- `src/DotnetForge.Application`
- `src/DotnetForge.Domain`
- `src/DotnetForge.Infrastructure`
- `src/DotnetForge.Worker`
- `tests/DotnetForge.Api.Tests`
- `tests/DotnetForge.Application.Tests`
- `tests/DotnetForge.Domain.Tests`
- `tests/DotnetForge.Infrastructure.Tests`
- `tests/DotnetForge.Worker.Tests`
