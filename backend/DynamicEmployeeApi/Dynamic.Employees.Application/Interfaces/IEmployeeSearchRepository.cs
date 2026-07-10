using Dynamic.Employees.Application.Models;

namespace Dynamic.Employees.Application.Interfaces;

/// <summary>
/// Provides employee search operations.
/// </summary>
/// <remarks>
/// Search is isolated from ordinary reads because it owns provider-sensitive JSON query behavior
/// while accepting criteria that have already been parsed and validated by the application layer.
/// </remarks>
public interface IEmployeeSearchRepository
{
    /// <summary>
    /// Searches employees using validated core and dynamic field filters.
    /// </summary>
    Task<EmployeeSearchResult> SearchAsync(
        EmployeeSearchCriteria criteria,
        CancellationToken cancellationToken = default);
}
