# dotnet-forge

A simple, reusable .NET 10 starter focused on clean boundaries, minimal APIs, and production-friendly defaults.

## What is in the repository

- A naming-safe solution rooted at `DotnetForge`
- Minimal API host with correlation, request logging, and global exception handling
- Application, Domain, Infrastructure, and Worker projects
- Central package management and lock-file support
- API integration tests and placeholder test projects for the other layers
- Docker Compose for local PostgreSQL

## Current sample flow

The sample application includes a small end-to-end greeting flow:

- request DTO in the Application layer
- FluentValidation validator
- explicit application service
- domain interaction
- API endpoint mapping
- success response and validation failure response

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
