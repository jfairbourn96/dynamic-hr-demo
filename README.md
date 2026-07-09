# Dynamic HR Demo

Dynamic HR Demo is a sample full-stack application for experimenting with dynamic employee schemas, JSON-backed field values, runtime-generated forms, and dynamic search.

The backend is intentionally structured around clean architecture boundaries so the API, application use cases, domain model, and EF Core infrastructure can evolve independently.

## Project Layout

```text
backend/DynamicEmployeeApi/
  Dynamic.Employees.Domain/       Employee domain models and enums
  Dynamic.Employees.Application/  Use-case services, commands, search models, and repository ports
  Dynamic.Employees.Data/         EF Core DbContext, migrations, configurations, and repository implementations
  EmployeeApi/                    ASP.NET Core controllers, request/response DTOs, mapping, and composition

frontend/
  src/                            React UI for employee types, dynamic forms, and search
```

## Architecture Decisions

The backend follows the dependency direction used in clean architecture:

```text
EmployeeApi -> Dynamic.Employees.Application
EmployeeApi -> Dynamic.Employees.Data
Dynamic.Employees.Application -> Dynamic.Employees.Domain
Dynamic.Employees.Data -> Dynamic.Employees.Application
Dynamic.Employees.Data -> Dynamic.Employees.Domain
```

`Dynamic.Employees.Domain` has no ASP.NET Core, EF Core, SQL Server, or Dynamic.Json package references. It contains the core employee model only.

`Dynamic.Employees.Application` owns use-case orchestration. It defines commands, search criteria/results, service interfaces/implementations, and repository ports such as `IEmployeeSearchRepository`, `IEmployeeReader`, and `IEmployeeWriter`.

`Dynamic.Employees.Data` implements those ports with EF Core and SQL Server. It owns `EmployeeDbContext`, migrations, entity configurations, and Dynamic.Json EF/SQL Server query translation.

`EmployeeApi` is the delivery layer. It owns controllers, HTTP request/response DTOs, mapping extensions, and dependency injection composition. It does not own EF Core migrations or the concrete DbContext.

## Repository Ports

The application layer uses narrow repository ports instead of one large CRUD repository:

- `IEmployeeSearchRepository` handles the JSON-backed employee search use case.
- `IEmployeeReader` handles employee reads.
- `IEmployeeWriter` handles employee writes.
- `IEmployeeTypeReader` and `IEmployeeTypeWriter` split employee type reads from writes.

The EF implementation can still be one concrete class per aggregate area, for example `EfEmployeeRepository`, but it implements several small interfaces. This keeps use cases dependent only on the capabilities they need.

Write methods persist internally, so the application services do not call `SaveChangesAsync` or depend on EF-style unit-of-work details.

## Dynamic.Json Dependencies

The backend consumes the published Dynamic.Json preview packages:

- `Dynamic.Json.Search` in Application for provider-neutral dynamic search parsing and filter models.
- `Dynamic.Json.EfCore.SqlServer` in Data for SQL Server JSON query translation.
- `Dynamic.Json.AspNetCore` in EmployeeApi for ASP.NET Core service registration/adapters.

When developing Dynamic.Json and Dynamic HR together, these package references can be temporarily swapped for sibling project references.

## Migrations

`EmployeeDbContext` and migrations live in `Dynamic.Employees.Data`. Run EF commands from the repository root with Data as the migrations project and EmployeeApi as the startup project:

```powershell
dotnet ef migrations add MigrationName `
  --project backend\DynamicEmployeeApi\Dynamic.Employees.Data `
  --startup-project backend\DynamicEmployeeApi\EmployeeApi `
  --context EmployeeDbContext
```

```powershell
dotnet ef database update `
  --project backend\DynamicEmployeeApi\Dynamic.Employees.Data `
  --startup-project backend\DynamicEmployeeApi\EmployeeApi `
  --context EmployeeDbContext
```

## Running Locally

Backend:

```powershell
dotnet run --project backend\DynamicEmployeeApi\EmployeeApi\EmployeeApi.csproj
```

Frontend:

```powershell
cd frontend
npm install
npm run dev
```

The API is configured for SQL Server LocalDB by default in `EmployeeApi/appsettings.json`.

## Tests

Backend unit tests:

```powershell
dotnet test backend\DynamicEmployeeApi\DynamicEmployeeApi.sln
```

Coverage:

```powershell
dotnet test backend\DynamicEmployeeApi\Dynamic.Employees.Application.UnitTests\Dynamic.Employees.Application.UnitTests.csproj --settings backend\DynamicEmployeeApi\coverlet.runsettings --results-directory artifacts\coverage\raw\application --collect "XPlat Code Coverage"
dotnet test backend\DynamicEmployeeApi\Dynamic.Employees.Data.UnitTests\Dynamic.Employees.Data.UnitTests.csproj --settings backend\DynamicEmployeeApi\coverlet.runsettings --results-directory artifacts\coverage\raw\data --collect "XPlat Code Coverage"
```

CI generates an HTML/Cobertura coverage report from the backend unit test suites, publishes the Markdown summary to the GitHub Actions job summary, uploads the full report as a `coverage-report` artifact, and runs frontend lint/build checks.

Coverage notes for the backend live in `docs/test-coverage.md`.
