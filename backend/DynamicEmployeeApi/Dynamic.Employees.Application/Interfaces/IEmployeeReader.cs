using Dynamic.Employees.Domain.Models;

namespace Dynamic.Employees.Application.Interfaces;

/// <summary>
/// Provides read operations for employees.
/// </summary>
/// <remarks>
/// This narrow port exposes only the capability required by read use cases rather than coupling
/// the application layer to a general-purpose CRUD repository or EF Core abstraction.
/// </remarks>
public interface IEmployeeReader
{
    /// <summary>
    /// Returns the employee with the given <paramref name="id"/>, or <c>null</c> if not found.
    /// </summary>
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
