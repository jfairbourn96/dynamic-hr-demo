using Dynamic.Json.Search;

namespace Dynamic.Employees.Application.Models;

/// <summary>
/// Defines normalized criteria for an employee repository search.
/// </summary>
public record EmployeeSearchCriteria(
    Guid? EmployeeTypeId,
    IReadOnlyCollection<EmployeeTextSearchFilter> TextFilters,
    string? Email,
    DateOnly? HireDateStart,
    DateOnly? HireDateEnd,
    IReadOnlyCollection<DynamicSearchFilter> DynamicFilters,
    int PageNumber,
    int PageSize);

/// <summary>
/// Defines a text filter against a core employee field.
/// </summary>
public record EmployeeTextSearchFilter(
    string FieldName,
    SearchOperator Operator,
    string Value);
