# dotnet-forge

Production-oriented reusable Clean Architecture starter skeleton for .NET 10.

## Current phase

Phase 1 completed:
- Naming-safe `DotnetForge` solution and project skeleton
- Clean dependency direction between layers
- Central package management and lock-file strategy enabled
- Docker starter files added

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

## Notes

No third-party package dependencies are introduced in Phase 1.
