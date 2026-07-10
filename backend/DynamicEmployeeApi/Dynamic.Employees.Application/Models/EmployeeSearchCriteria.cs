using Dynamic.Json.Search;

namespace Dynamic.Employees.Application.Models;

/// <summary>
/// Defines normalized, validated criteria for a repository search without HTTP concerns.
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
