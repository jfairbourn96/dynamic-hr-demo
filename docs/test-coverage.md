# Dynamic HR Demo Test Coverage

This checklist tracks meaningful unit test coverage for the Dynamic HR backend. Update it when application, data, or API behavior changes or when new tests close a gap.

## Current Test Status

Last verified with:

```text
dotnet test backend\DynamicEmployeeApi\DynamicEmployeeApi.sln --no-restore
Passed: 43, Failed: 0, Skipped: 0
```

Coverage collection:

```text
dotnet test backend\DynamicEmployeeApi\Dynamic.Employees.Application.UnitTests\Dynamic.Employees.Application.UnitTests.csproj --settings backend\DynamicEmployeeApi\coverlet.runsettings --results-directory artifacts\coverage\raw\application --collect "XPlat Code Coverage"
dotnet test backend\DynamicEmployeeApi\Dynamic.Employees.Data.UnitTests\Dynamic.Employees.Data.UnitTests.csproj --settings backend\DynamicEmployeeApi\coverlet.runsettings --results-directory artifacts\coverage\raw\data --collect "XPlat Code Coverage"
```

The default coverage report excludes test assemblies and EF Core migrations.

Current backend unit-test product-code baseline:

```text
Application unit tests line coverage: 94.68%
Application unit tests branch coverage: 92.00%
Data unit tests line coverage: 41.14%
Data unit tests branch coverage: 23.65%
```

## Coverage Matrix

| Project | Area | Status | Notes |
|---|---|---|---|
| `Dynamic.Employees.Application` | Employee type creation | Covered | Field definitions, options, and validation are covered through service tests. |
| `Dynamic.Employees.Application` | Employee creation and updates | Covered | End date defaults/assignments, field updates, and persistence calls are covered. |
| `Dynamic.Employees.Application` | Employee search orchestration | Covered | Criteria mapping, dynamic field type mapping, parser error formatting, filters, and service/repository collaboration are covered. |
| `Dynamic.Employees.Application` | Application service registration | Covered | Scoped application service registrations are covered. |
| `Dynamic.Employees.Data` | Employee repository reads/writes/search | Mostly covered | EF-backed create, get, update, field value behavior, paging, and core search filters are covered with the in-memory provider. Dynamic JSON translation belongs in provider tests. |
| `Dynamic.Employees.Data` | Employee type repository reads/writes | Covered | EF-backed create and read behavior are covered with the in-memory provider. |
| `Dynamic.Employees.Data` | Data service registration | Covered | DbContext, repository, and repository-port registrations are covered. |
| `Dynamic.Employees.Data` | SQL Server JSON persistence and search | Covered by integration tests | Testcontainers starts SQL Server and verifies owned JSON fields, JSON field values, and provider-backed dynamic search filters. |
| `EmployeeApi` | Controllers and request mappings | Planned API coverage | Add focused controller or minimal API integration tests when API behavior stabilizes. |

## Future Coverage Triggers

Add or update tests when any of these change:

- Employee type field validation rules change.
- Supported dynamic field types or option behavior changes.
- Employee search criteria, operators, or filter mapping changes.
- Repository persistence semantics change.
- SQL Server JSON translation behavior is added or changed.
- Controller request/response mapping or HTTP status behavior changes.
