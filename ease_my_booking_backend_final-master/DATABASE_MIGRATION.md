# Database Migration: SQL Server to PostgreSQL

This guide helps you migrate your local SQL Server database to PostgreSQL for production deployment.

## Step 1: Install PostgreSQL NuGet Package

Add to your `.csproj`:

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

Your [EaseMyBooking.Api.csproj](EaseMyBooking.Api.csproj) already has the flexibility to support both providers via `Program.cs`.

## Step 2: Local Testing with PostgreSQL

### Run PostgreSQL Locally (Docker)

```bash
docker run --name rthhub-postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=password \
  -e POSTGRES_DB=resumetemplatehub \
  -p 5432:5432 \
  -d postgres:latest
```

### Update appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=resumetemplatehub;Username=postgres;Password=password;Include Error Detail=true;"
  }
}
```

### Run Migrations

```bash
dotnet ef database update
```

## Step 3: Export Data from SQL Server (Optional)

If you have existing data, export it:

```bash
# Export from SQL Server to CSV
sqlcmd -S (localdb)\mssqllocaldb -d ResumeTemplateHubDB_Location -Q "SELECT * FROM Users" -o users.csv
```

Then import into PostgreSQL (manual or via Entity Framework).

## Step 4: Production Deployment

In Render.com dashboard:
1. Set `DATABASE_URL=postgresql://user:pass@host:5432/db`
2. First deployment will run migrations automatically (if enabled in Program.cs)

## Program.cs Already Handles Both!

Your updated `Program.cs` automatically detects:
- **SQL Server**: If connection string contains `Server=`
- **PostgreSQL**: If connection string contains `postgres://`

```csharp
if (conn.Contains("postgres", StringComparison.OrdinalIgnoreCase))
    o.UseNpgsql(conn);
else
    o.UseSqlServer(conn);
```

No code changes needed - just swap connection strings!

## Verify Everything Works

```bash
# Test locally with PostgreSQL
dotnet run

# Check migrations applied
dotnet ef migrations list

# Rollback if needed
dotnet ef database update LastMigrationName
```

## Troubleshooting

### PostgreSQL Connection Fails
```
Check: Host, Port, Database, Username, Password
Format: Host=localhost;Port=5432;Database=db;Username=user;Password=pass;
```

### Migration Errors
```bash
# Remove last migration if not applied
dotnet ef migrations remove

# Add new migration
dotnet ef migrations add MigrationName

# Apply it
dotnet ef database update
```

### Connection String Not Found
```csharp
// Check in appsettings file
"ConnectionStrings": {
  "DefaultConnection": "..."  // Must match Program.cs expectation
}
```

