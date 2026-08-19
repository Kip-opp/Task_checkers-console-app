# Database migration notes

`Shared/Migrations/20260819113228_AddTodoOwnership` is the first EF Core migration for the current model. It creates `Users`, `Todos`, the normalized username index, and the required `TodoItem.OwnerId` foreign key.

## Fresh development database

Set `Jwt:key` through an environment variable or user secrets, then run:

```powershell
dotnet ef database update --project Shared\Shared.csproj --startup-project TodoApi\TodoApi.csproj
```

The API intentionally does not run `EnsureCreated()` or apply migrations at startup.

## Existing `EnsureCreated()` database

Do not run this initial migration directly against an existing `todos.db`; that database has no migration history and may already contain tables with the old shape. Back up the file first. Because legacy rows have no owner, choose and record an ownership policy before importing them, such as assigning them to a specifically chosen account. Export the old users and todos, provision the chosen account through registration, move the old database aside, run the command above to create the new schema, and import only validated rows with that account's `OwnerId`.

This repository does not silently guess ownership or destroy the old database. A future deployment with non-disposable legacy data should add a reviewed, environment-specific backfill migration before rollout.
