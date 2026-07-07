using Dynamic.Json.Search;

namespace Dynamic.Employees.Application.Models;

public record EmployeeSearchCriteria(
    Guid? EmployeeTypeId,
    IReadOnlyCollection<EmployeeTextSearchFilter> TextFilters,
    string? Email,
    DateOnly? HireDateStart,
    DateOnly? HireDateEnd,
    IReadOnlyCollection<DynamicSearchFilter> DynamicFilters,
    int PageNumber,
    int PageSize);

public record EmployeeTextSearchFilter(
    string FieldName,
    SearchOperator Operator,
    string Value);
