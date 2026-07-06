# Dynamic HR Demo

Dynamic HR Demo is a sample full-stack application for experimenting with dynamic employee schemas, JSON-backed field values, runtime-generated forms, and dynamic search.

This repository is intended to consume the `Dynamic.Json.EfCore.*` package set from NuGet once those packages are published.

## Project Layout

```text
backend/DynamicEmployeeApi/
  Dynamic.Employees.Core/  Domain models and enums
  Dynamic.Employees.Data/  EF Core DbContext and data configuration
  EmployeeApi/             ASP.NET Core API

frontend/
  src/                     React UI for employee types, dynamic forms, and search
```

## Package Dependencies

The backend references the dynamic JSON EF Core packages as NuGet packages:

- `Dynamic.Json.EfCore`
- `Dynamic.Json.EfCore.AspNetCore`
- `Dynamic.Json.EfCore.SqlServer`

The current package version placeholder is `0.1.0-preview.1`. Update the package versions after publishing the package repo.

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
