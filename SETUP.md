# Setup Guide

## Prerequisites

| Tool | Version | Notes |
|---|---|---|
| .NET SDK | 10.x | `dotnet --version` |
| dotnet-ef CLI | 10.x | Required for migrations |
| Node.js | 20.x or 22.x | `node --version` |
| npm | 10.x | Bundled with Node |
| Docker Desktop / Engine | Current | Runs SQL Server, the full stack, and Testcontainers integration tests |
| SQL Server | Optional alternative | Use only when not running SQL Server through Docker |
| Git | Any recent version | |

Install or update the EF Core CLI:

```powershell
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```

## Clone

```powershell
git clone https://github.com/jfairbourn96/dynamic-hr-demo.git
cd dynamic-hr-demo
```

## Run Everything With Docker

If Docker Desktop or Docker Engine is installed, this is the quickest way to run the complete stack:

```powershell
docker compose up --build
```

This starts SQL Server, the ASP.NET Core API, a one-shot seed container, and the React frontend. EF Core migrations are applied automatically by the API container after SQL Server passes its health check. The seed container waits for the migration-created `EmployeeType` table, then loads the professional demo employee types and employees only when the database is empty.

Open the application at `http://localhost:5173`; the API is exposed at `http://localhost:5154`.

Stop the stack with:

```powershell
docker compose down
```

Add `-v` to also remove the persisted SQL Server data volume. The next `docker compose up --build` then creates and seeds a fresh demo database. Compose uses a development-only default password; set `MSSQL_SA_PASSWORD` in a root `.env` file to override it.

## Backend

The backend solution lives at:

```text
backend/DynamicEmployeeApi/DynamicEmployeeApi.sln
```

Current backend projects:

```text
Dynamic.Employees.Domain                 Domain models and enums
Dynamic.Employees.Application            Use-case services, commands, search models, and repository ports
Dynamic.Employees.Data                   EF Core DbContext, migrations, configurations, and repositories
EmployeeApi                              ASP.NET Core controllers, DTOs, mapping, and composition
EmployeeApi.UnitTests                    API controller and mapping unit tests
Dynamic.Employees.Application.UnitTests  Application unit tests
Dynamic.Employees.Data.UnitTests         EF repository unit tests
Dynamic.Employees.Data.IntegrationTests  SQL Server integration tests with Testcontainers
```

The HR projects consume the published Dynamic.Json packages:

```text
Dynamic.Json.Search              0.2.1-preview.1
Dynamic.Json.AspNetCore          0.2.1-preview.1
Dynamic.Json.EfCore.SqlServer    0.2.1-preview.1
```

### Restore And Build

Run from the repository root:

```powershell
dotnet restore backend\DynamicEmployeeApi\DynamicEmployeeApi.sln
dotnet build backend\DynamicEmployeeApi\DynamicEmployeeApi.sln
```

### Database

The default connection string in `backend/DynamicEmployeeApi/EmployeeApi/appsettings.json` targets the Docker-published SQL Server instance:

```json
"DefaultConnection": "Server=localhost,1433;Database=DynamicHr;User Id=sa;Password=DynamicHr!Passw0rd;TrustServerCertificate=True;Encrypt=False;"
```

To use another SQL Server instance, override that connection string before applying migrations. The Compose API container supplies its own connection string and sets `Database__ApplyMigrationsOnStartup=true`, so no manual migration command is needed for the Docker workflow.

### Apply Migrations

`EmployeeDbContext` and migrations live in `Dynamic.Employees.Data`. `EmployeeApi` is still the startup project because it provides configuration and dependency injection.

Run from the repository root:

```powershell
dotnet ef database update `
  --project backend\DynamicEmployeeApi\Dynamic.Employees.Data `
  --startup-project backend\DynamicEmployeeApi\EmployeeApi `
  --context EmployeeDbContext
```

### Add A Migration

```powershell
dotnet ef migrations add MigrationName `
  --project backend\DynamicEmployeeApi\Dynamic.Employees.Data `
  --startup-project backend\DynamicEmployeeApi\EmployeeApi `
  --context EmployeeDbContext
```

Then apply it:

```powershell
dotnet ef database update `
  --project backend\DynamicEmployeeApi\Dynamic.Employees.Data `
  --startup-project backend\DynamicEmployeeApi\EmployeeApi `
  --context EmployeeDbContext
```

### Run The API

```powershell
dotnet run --project backend\DynamicEmployeeApi\EmployeeApi\EmployeeApi.csproj
```

Launch settings configure:

```text
http://localhost:5154
https://localhost:7043
```

The frontend expects the API at `http://localhost:5154` unless you override the frontend environment configuration.

### Run Backend Tests

Run all backend tests:

```powershell
dotnet test backend\DynamicEmployeeApi\DynamicEmployeeApi.sln
```

Run only the Application unit tests:

```powershell
dotnet test backend\DynamicEmployeeApi\Dynamic.Employees.Application.UnitTests\Dynamic.Employees.Application.UnitTests.csproj
```

Run only the Data unit tests:

```powershell
dotnet test backend\DynamicEmployeeApi\Dynamic.Employees.Data.UnitTests\Dynamic.Employees.Data.UnitTests.csproj
```

Collect backend coverage:

```powershell
dotnet test backend\DynamicEmployeeApi\Dynamic.Employees.Application.UnitTests\Dynamic.Employees.Application.UnitTests.csproj --settings backend\DynamicEmployeeApi\coverlet.runsettings --results-directory artifacts\coverage\raw\application --collect "XPlat Code Coverage"
dotnet test backend\DynamicEmployeeApi\Dynamic.Employees.Data.UnitTests\Dynamic.Employees.Data.UnitTests.csproj --settings backend\DynamicEmployeeApi\coverlet.runsettings --results-directory artifacts\coverage\raw\data --collect "XPlat Code Coverage"
dotnet test backend\DynamicEmployeeApi\EmployeeApi.UnitTests\EmployeeApi.UnitTests.csproj --settings backend\DynamicEmployeeApi\coverlet.runsettings --results-directory artifacts\coverage\raw\api --collect "XPlat Code Coverage"
```

Backend unit tests use xUnit, Moq, AutoFixture.AutoMoq, and FluentAssertions. The current test style freezes mocks for important collaborators, lets AutoFixture construct the service under test, uses explicit Arrange/Act/Assert comments, and uses fluent assertions for readable expectations.

Coverage notes and the current backend baseline live in `docs/test-coverage.md`.

## Debug The API Locally With Docker SQL Server

To use normal IDE breakpoints while avoiding a local SQL Server installation, start the Docker database, containerized API migration step, and one-shot seed process:

```powershell
docker compose up -d sqlserver api seed
```

Wait for the seed container to complete, then stop only the API container:

```powershell
docker compose stop api
```

Start `EmployeeApi` from the IDE and run the frontend locally with `npm run dev`. The development appsettings files already target `localhost:1433`, which is the SQL Server container's published port.

## Frontend

### Install

```powershell
cd frontend
npm install
```

### Configure

Copy `frontend/.env.example` to `frontend/.env` if you need to override the API URL:

```text
VITE_API_BASE_URL=http://localhost:5154/api
```

### Run

```powershell
npm run dev
```

Vite serves the app at:

```text
http://localhost:5173
```

### Build

```powershell
npm run build
```

### Lint

```powershell
npm run lint
```

## Running Both Apps

Open two terminals from the repository root.

Terminal 1:

```powershell
dotnet run --project backend\DynamicEmployeeApi\EmployeeApi\EmployeeApi.csproj
```

Terminal 2:

```powershell
cd frontend
npm run dev
```
