namespace Dynamic.Employees.Application.Models;

/// <summary>
/// Represents one page of employee search results.
/// </summary>
public record EmployeeSearchResult(
    IReadOnlyCollection<EmployeeSearchItem> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
