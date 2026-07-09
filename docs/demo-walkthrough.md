# Demo Walkthrough

This walkthrough is the fastest way to show why the project exists: dynamic fields stay configurable at runtime, but the backend still persists and searches them through a structured architecture.

## 1. Start The App

Terminal 1:

```powershell
dotnet run --project backend\DynamicEmployeeApi\EmployeeApi\EmployeeApi.csproj
```

Terminal 2:

```powershell
cd frontend
npm install
npm run dev
```

Open `http://localhost:5173`.

## 2. Create An Employee Type

Go to Employee Types and create a type such as `Field Technician`.

Add dynamic fields:

| Field | Type | Example |
|---|---|---|
| `certificationLevel` | Select | junior, mid, senior |
| `hourlyRate` | Number | 85 |
| `availableFrom` | Date | 2026-08-01 |
| `remoteEligible` | Boolean | true |
| `primaryTool` | Text | Oscilloscope |

This creates a metadata schema that the frontend uses to render forms and the backend uses to validate search filters.

## 3. Create Employees

Create a few employees using the new type. Core fields such as first name, email, hire date, and department are stored as normal relational columns. Dynamic field values are stored in the employee JSON payload.

The important behavior is that the application code does not need a new database column every time the business adds a field.

## 4. Search Core And Dynamic Fields

Use the Search page to filter by department, email, hire date, or dynamic fields.

The backend maps dynamic filters to `Dynamic.Json.Search` models, validates them against the selected employee type, and the data layer translates supported filters into SQL Server JSON predicates.

Example query shape:

```text
/api/employees/search?employeeTypeId={id}&department_exact=Field%20Ops&certificationLevel=senior&hourlyRate_gte=75&remoteEligible=true
```

## 5. What To Point Out

- `Dynamic.Employees.Domain` has no EF Core, ASP.NET Core, SQL Server, or Dynamic.Json dependency.
- `Dynamic.Employees.Application` owns use-case orchestration and repository ports.
- `Dynamic.Employees.Data` owns EF Core, SQL Server, migrations, and JSON query translation.
- `EmployeeApi` owns HTTP DTOs, controllers, and composition.
- SQL Server integration tests prove the dynamic JSON filters work against a real provider, not only the in-memory EF provider.

