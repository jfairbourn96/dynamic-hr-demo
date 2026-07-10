using Dynamic.Employees.Domain.Models;

namespace Dynamic.Employees.Application.Interfaces;

/// <summary>
/// Provides write operations for employees.
/// </summary>
/// <remarks>
/// Read, write, and search capabilities are separate ports so each use case depends only on the
/// operations it needs. One data-layer implementation may still implement all of the ports.
/// </remarks>
public interface IEmployeeWriter
{
    /// <summary>
    /// Adds a new employee and persists the change.
    /// </summary>
    Task AddAsync(Employee employee, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an employee and persists the change.
    /// </summary>
    Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
}
