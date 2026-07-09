# Dynamic HR Demo

Dynamic HR Demo is a full-stack sample application for metadata-driven employee records. It lets an admin define employee types at runtime, attach custom fields to each type, create employees from those schemas, and search across both normal columns and JSON-backed dynamic values.

The point of the project is not just CRUD. It demonstrates how a product can offer flexible user-defined fields while still keeping a clean backend architecture, SQL Server persistence, provider-specific EF Core JSON translation, automated tests, and a React UI that is usable enough to show the flow end to end.

![Dynamic HR Demo](frontend/src/assets/hero.png)

## What This Demonstrates

- Runtime-defined employee schemas with text, number, date, boolean, and select fields.
- JSON-backed dynamic field values persisted through EF Core.
- SQL Server search over dynamic JSON fields using the published `Dynamic.Json` packages.
- Clean architecture boundaries between API, application services, domain models, and EF Core infrastructure.
- React + TypeScript forms generated from backend metadata.
- Unit and Docker-backed SQL Server integration tests.
- GitHub Actions for backend/frontend CI, coverage reporting, and package publishing.

## Demo Flow

1. Create an employee type such as "Contractor", "Engineer", or "Technician".
2. Add custom fields for that type, for example certification level, start date, equipment assigned, or remote eligible.
3. Create employees from the selected type. The form is generated from the field metadata.
4. Search employees by core fields such as department and hire date.
5. Filter by dynamic fields stored in JSON and translated to SQL Server predicates.

For a more guided walkthrough, see [docs/demo-walkthrough.md](docs/demo-walkthrough.md).

## Quick Start

Prerequisites: .NET SDK 10.x, Node.js 20 or 22, npm 10.x, and SQL Server LocalDB or another SQL Server instance.

```powershell
git clone https://github.com/jfairbourn96/dynamic-hr-demo.git
cd dynamic-hr-demo

dotnet restore backend\DynamicEmployeeApi\DynamicEmployeeApi.sln
dotnet build backend\DynamicEmployeeApi\DynamicEmployeeApi.sln
dotnet test backend\DynamicEmployeeApi\DynamicEmployeeApi.sln --no-build

dotnet ef database update `
  --project backend\DynamicEmployeeApi\Dynamic.Employees.Data `
  --startup-project backend\DynamicEmployeeApi\EmployeeApi `
  --context EmployeeDbContext
```

Run the API:

```powershell
dotnet run --project backend\DynamicEmployeeApi\EmployeeApi\EmployeeApi.csproj
```

Run the frontend:

```powershell
cd frontend
npm install
npm run dev
```

Default local URLs:

- API: `http://localhost:5154`
- Frontend: `http://localhost:5173`

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

## Tech Stack

| Area | Technology |
|---|---|
| API | ASP.NET Core, controllers, dependency injection |
| Application | C# services, repository ports, dynamic search orchestration |
| Persistence | EF Core 10, SQL Server, JSON-backed dynamic values |
| Dynamic JSON | `Dynamic.Json.Search`, `Dynamic.Json.EfCore.SqlServer`, `Dynamic.Json.AspNetCore` |
| Frontend | React 19, TypeScript, Vite, Tailwind CSS, TanStack Query |
| Tests | xUnit, FluentAssertions, Moq, AutoFixture, Testcontainers for SQL Server |
| CI | GitHub Actions, coverage report generation, frontend lint/build |

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

- `Dynamic.Json.Search` `0.2.1-preview.1` in Application for provider-neutral dynamic search parsing and filter models.
- `Dynamic.Json.EfCore.SqlServer` `0.2.1-preview.1` in Data for SQL Server JSON query translation.
- `Dynamic.Json.AspNetCore` `0.2.1-preview.1` in EmployeeApi for ASP.NET Core service registration/adapters.

When developing Dynamic.Json and Dynamic HR together, these package references can be temporarily swapped for sibling project references.

## Search Examples

Core field filters:

```text
GET /api/employees/search?department_exact=Field%20Ops&hireDate_startDate=2024-01-01&pageNumber=1&pageSize=20
```

Dynamic field filters require an `employeeTypeId` so the backend can validate the requested fields against that type's metadata:

```text
GET /api/employees/search?employeeTypeId={id}&certificationLevel=senior&remoteEligible=true&hourlyRate_gte=75
```

Supported dynamic field categories are text, number, date, boolean, and select. Invalid field names, unsupported operators, bad number/date/boolean values, and invalid select options return validation errors instead of falling through to ambiguous SQL behavior.

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

The API is configured for SQL Server LocalDB by default in `EmployeeApi/appsettings.json`.

## Tests

Backend tests:

```powershell
dotnet test backend\DynamicEmployeeApi\DynamicEmployeeApi.sln
```

The integration test project uses Testcontainers to run SQL Server for provider-specific persistence and JSON search coverage.

Coverage:

```powershell
dotnet test backend\DynamicEmployeeApi\Dynamic.Employees.Application.UnitTests\Dynamic.Employees.Application.UnitTests.csproj --settings backend\DynamicEmployeeApi\coverlet.runsettings --results-directory artifacts\coverage\raw\application --collect "XPlat Code Coverage"
dotnet test backend\DynamicEmployeeApi\Dynamic.Employees.Data.UnitTests\Dynamic.Employees.Data.UnitTests.csproj --settings backend\DynamicEmployeeApi\coverlet.runsettings --results-directory artifacts\coverage\raw\data --collect "XPlat Code Coverage"
```

CI generates an HTML/Cobertura coverage report from the backend unit test suites, publishes the Markdown summary to the GitHub Actions job summary, uploads the full report as a `coverage-report` artifact, and runs frontend lint/build checks.

Coverage notes for the backend live in `docs/test-coverage.md`.
