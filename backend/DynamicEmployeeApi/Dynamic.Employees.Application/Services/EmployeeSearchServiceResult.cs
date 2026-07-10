using Dynamic.Employees.Application.Models;

namespace Dynamic.Employees.Application.Services;

/// <summary>Represents either a valid employee search result or validation errors.</summary>
public record EmployeeSearchServiceResult(EmployeeSearchResult? SearchResult, IReadOnlyCollection<string> Errors)
{
    public bool IsValid => Errors.Count == 0;
    /// <summary>Creates a successful search result.</summary>
    public static EmployeeSearchServiceResult Success(EmployeeSearchResult result) => new(result, []);
    /// <summary>Creates a search result containing validation errors.</summary>
    public static EmployeeSearchServiceResult Failure(IReadOnlyCollection<string> errors) => new(null, errors);
}
