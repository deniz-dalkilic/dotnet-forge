# Template

A pragmatic .NET clean architecture template with explicit use-cases and infrastructure boundaries.

## Project structure

- `src/Domain`
  - Core business model and primitives (`Entity<TId>`, `DomainEvent`, `Result`).
  - No dependency on other project layers.
- `src/Application`
  - Use-cases and application contracts/ports (`IAppDbContext`, `IClock`, `ICurrentUser`, `IUnitOfWork`, `ICache`, `IEventBus`).
  - Depends only on Domain.
- `src/Infrastructure`
  - Adapters and implementations for persistence, jobs, messaging, auth, and cache.
  - Implements Application abstractions.
- `src/Api`
  - HTTP delivery layer and composition root.
- `src/Worker`
  - Background processing host.
- `tests/UnitTests`
  - Fast unit tests for domain/application logic.
- `tests/IntegrationTests`
  - Integration tests for infrastructure and external dependencies.
- `docs/architecture`
  - Architecture notes and ADRs.

## Architectural direction

See `docs/architecture/ADR-0001-clean-architecture.md` for the baseline dependency rule and rationale for keeping use-cases explicit without mediator by default.

## Local infrastructure with Docker Compose

1. Copy the sample environment file and adjust values if needed:

```bash
cp infra/.env.example infra/.env
```

2. Start core dependencies:

```bash
docker compose --env-file infra/.env -f infra/docker-compose.core.yml up -d
```

3. Optionally add observability stack:

```bash
docker compose -f infra/docker-compose.core.yml -f infra/docker-compose.observability.yml up -d
```

4. For API/Worker containers, point telemetry and logs to the observability stack:

```bash
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
LOKI_URL=http://loki:3100
```

## Authentication approach

This template does not include a bundled identity provider. Use an external OpenID Connect/OAuth 2.0 provider and apply an **External Id Token Exchange** pattern for service-to-service calls. See `docs/architecture/authentication.md` for details.

## Quickstart (auth configuration)

Before running the API with external login enabled, configure these environment variables:

- `ExternalAuth__Providers__Google__ClientId`
- `ExternalAuth__Providers__Microsoft__ClientId`
- `ExternalAuth__Providers__Apple__ClientId`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__SigningKey`

For local development, copy the project template and fill your own values:

```bash
cp .env.example .env
```

`src/Api` and `src/Worker` load `.env` and `.env.local` from the repository root on startup (without overriding already-defined environment variables), so real deployment environments can continue to use platform-managed secrets.

External login flow endpoint:

- `POST /api/auth/external/google` with body `{ "idToken": "<google-id-token>", "nonce": "<optional-nonce>" }`

On success, the API validates the external `id_token` and returns an internal access token (plus refresh token if enabled).
