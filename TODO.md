# TODO

## Docker-Backed Integration Tests

Add integration tests for provider-specific Dynamic HR search behavior using Docker/Testcontainers.

### SQL Server

- Add a Dynamic HR integration test project under `backend/DynamicEmployeeApi`.
- Use Testcontainers for .NET to start a SQL Server container during test setup.
- Configure `EmployeeDbContext` with the container-generated SQL Server connection string.
- Seed employee types and employees with JSON-backed field values.
- Execute real employee search queries against SQL Server to verify:
  - dynamic text filters
  - dynamic number filters
  - dynamic date filters
  - dynamic boolean filters
  - dynamic select filters
  - null or missing JSON property behavior
  - invalid decimal/date conversion behavior

### Provider Translation Tests

- Add SQL Server translation tests outside the unit test projects.
- Use `ToQueryString()` to verify generated SQL for:
  - `JSON_VALUE`
  - `TRY_CONVERT(decimal(18, 4), JSON_VALUE(...))`
  - `TRY_CONVERT(date, JSON_VALUE(...))`
  - missing `UseDynamicJsonSqlServer()` registration behavior

## Test Coverage

- Add focused API/controller tests for request mapping, status codes, and validation errors.
- Fold SQL Server integration coverage into the default CI report once the provider tests are stable.
- Keep `docs/test-coverage.md` updated when service, repository, or API behavior changes.

## API Documentation

- Add Swagger/OpenAPI documentation back intentionally after choosing a package/version without known vulnerabilities.
- Document dynamic search query parameters, supported operators, field types, and error responses.
- Include examples for core employee filters and dynamic JSON field filters.
