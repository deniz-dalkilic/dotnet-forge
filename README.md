# dotnet-forge

A simple, reusable .NET 10 starter focused on clean boundaries, minimal APIs, and production-friendly defaults.

## What is in the repository

- A naming-safe solution rooted at `DotnetForge`
- Minimal API host with correlation, request logging, and global exception handling
- Application, Domain, Infrastructure, and Worker projects
- Central package management and lock-file support
- API integration tests and placeholder test projects for the other layers
- Docker Compose for local PostgreSQL
- EF Core 10 + PostgreSQL persistence wiring with migration-ready infrastructure

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
