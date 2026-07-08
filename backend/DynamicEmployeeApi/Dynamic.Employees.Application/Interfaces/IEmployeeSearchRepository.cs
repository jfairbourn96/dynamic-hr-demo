using Dynamic.Employees.Application.Models;

namespace Dynamic.Employees.Application.Interfaces;

/// <summary>
/// Provides employee search operations.
/// </summary>
public interface IEmployeeSearchRepository
{
    /// <summary>
    /// Searches employees using validated core and dynamic field filters.
    /// </summary>
    Task<EmployeeSearchResult> SearchAsync(EmployeeSearchCriteria criteria);
}
