# Dynamic HR Demo Test Coverage

This checklist tracks meaningful unit test coverage for the Dynamic HR backend. Update it when application, data, or API behavior changes or when new tests close a gap.

## Current Test Status

Last verified with:

```text
dotnet test backend\DynamicEmployeeApi\DynamicEmployeeApi.sln --configuration Release --no-build
Passed: 81, Failed: 0, Skipped: 0
```

Coverage collection:

```text
dotnet test backend\DynamicEmployeeApi\Dynamic.Employees.Application.UnitTests\Dynamic.Employees.Application.UnitTests.csproj --settings backend\DynamicEmployeeApi\coverlet.runsettings --results-directory artifacts\coverage\raw\application --collect "XPlat Code Coverage"
dotnet test backend\DynamicEmployeeApi\Dynamic.Employees.Data.UnitTests\Dynamic.Employees.Data.UnitTests.csproj --settings backend\DynamicEmployeeApi\coverlet.runsettings --results-directory artifacts\coverage\raw\data --collect "XPlat Code Coverage"
dotnet test backend\DynamicEmployeeApi\EmployeeApi.UnitTests\EmployeeApi.UnitTests.csproj --settings backend\DynamicEmployeeApi\coverlet.runsettings --results-directory artifacts\coverage\raw\api --collect "XPlat Code Coverage"
```

The default coverage report excludes test assemblies and EF Core migrations.

Current backend unit-test product-code baseline:

```text
Application unit tests line coverage: 99.00%
Application unit tests branch coverage: 91.40%
Data unit tests line coverage: 80.50%
Data unit tests branch coverage: 48.80%
API unit tests line coverage: 87.10%
API unit tests branch coverage: 90.90%
```

## Coverage Matrix

| Project | Area | Status | Notes |
|---|---|---|---|
| `Dynamic.Employees.Application` | Employee type creation | Covered | Field definitions, options, and validation are covered through service tests. |
| `Dynamic.Employees.Application` | Employee creation and updates | Covered | Dynamic schema validation, complete value replacement, mutation safety, and persistence calls are covered. |
| `Dynamic.Employees.Application` | Employee search orchestration | Covered | Criteria mapping, dynamic field type mapping, parser error formatting, filters, and service/repository collaboration are covered. |
| `Dynamic.Employees.Application` | Application service registration | Covered | Scoped application service registrations are covered. |
| `Dynamic.Employees.Data` | Employee repository reads/writes/search | Mostly covered | EF-backed create, get, update, field value behavior, paging, and core search filters are covered with the in-memory provider. Dynamic JSON translation belongs in provider tests. |
| `Dynamic.Employees.Data` | Employee type repository reads/writes | Covered | EF-backed create and read behavior are covered with the in-memory provider. |
| `Dynamic.Employees.Data` | Data service registration | Covered | DbContext, repository, and repository-port registrations are covered. |
| `Dynamic.Employees.Data` | SQL Server JSON persistence and search | Covered by integration tests | Testcontainers starts SQL Server and verifies owned JSON fields, JSON field values, and provider-backed dynamic search filters. |
| Provider data projects | Dependency and migration isolation | Covered | Unit tests verify both provider registrations and ensure Domain/Application reference no database provider; each provider owns a separate context and migration snapshot. |
| `Dynamic.Employees.Data.PostgreSql` | PostgreSQL `jsonb` persistence | Covered by integration tests | Testcontainers starts PostgreSQL 18, applies its independent migration, and round-trips employee dynamic values through a fresh context. |
| `EmployeeApi` | Controllers and request/response mappings | Covered | Focused unit tests cover status results, request-command mapping, and stable response contracts. |

## Future Coverage Triggers

Add or update tests when any of these change:

- Employee type field validation rules change.
- Supported dynamic field types or option behavior changes.
- Employee search criteria, operators, or filter mapping changes.
- Repository persistence semantics change.
- SQL Server JSON translation behavior is added or changed.
- Controller request/response mapping or HTTP status behavior changes.
