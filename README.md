# dotnet-forge

A reusable .NET 10 starter focused on clean boundaries, minimal APIs, and production-friendly defaults.

## What is in the repository

- A naming-safe solution rooted at `DotnetForge`
- Minimal API host with correlation, request logging, global exception handling, and Problem Details responses
- Application, Domain, Infrastructure, and Worker projects
- Central package management and lock-file support
- API integration tests and layer-level test projects, including a Worker test foundation
- Docker Compose for local PostgreSQL
- EF Core 10 + PostgreSQL persistence wiring with migration-ready infrastructure
- HybridCache-based caching foundation with optional future distributed cache integration
- Hangfire-backed background processing with API enqueue endpoints and a dedicated Worker host for execution

## Current sample flow

The sample application includes a small end-to-end greeting flow backed by PostgreSQL:

- request DTO in the Application layer
- FluentValidation validator
- explicit application service
- domain interaction
- EF Core persistence in Infrastructure
- API create/read endpoint mapping
- success, validation failure, and not-found responses

## Local database

Start PostgreSQL for local development:

```bash
docker compose -f docker/docker-compose.yml -f docker/docker-compose.override.yml up -d
```

The default development connection string is configured in `src/DotnetForge.Api/appsettings.json`.

## Background processing foundation

The template uses Hangfire with PostgreSQL storage and splits responsibilities like this:

- **API host**: accepts HTTP requests, exposes the dashboard, and enqueues jobs
- **Worker host**: runs the Hangfire server and registers recurring jobs

This is the most pragmatic template default because it keeps the API process lightweight while preserving a clean extraction path toward a dedicated background-processing deployment later.

Sample background processing endpoints:

- `POST /api/jobs/greetings/fire-and-forget`
- `POST /api/jobs/greetings/scheduled`
- Hangfire dashboard: `/hangfire`

The Worker project also registers a recurring heartbeat job to demonstrate how future recurring jobs should be added without coupling them to the API host lifecycle.

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
