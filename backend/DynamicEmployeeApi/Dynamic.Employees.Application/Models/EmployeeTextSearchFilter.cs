using Dynamic.Json.Search;

namespace Dynamic.Employees.Application.Models;

/// <summary>Defines a text filter against a core employee field.</summary>
public record EmployeeTextSearchFilter(string FieldName, SearchOperator Operator, string Value);
