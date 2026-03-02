# Database (EF Core + PostgreSQL)

Use the following commands to manage migrations and update the database:

```bash
dotnet tool restore
dotnet ef migrations add Initial --project src/Infrastructure --startup-project src/Api
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```
