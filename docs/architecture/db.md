# Database
PostgreSQL is the default database engine via EF Core + Npgsql. The `AppDbContext` in Infrastructure implements the application contract.
Connection string key: `ConnectionStrings:Postgres`.
For migration commands, see `docs/architecture/database.md`.
