using Dynamic.Employees.Domain.Models;

namespace Dynamic.Employees.Application.Interfaces;

/// <summary>
/// Provides read operations for employee types.
/// </summary>
public interface IEmployeeTypeReader
{
    /// <summary>
    /// Returns all employee types.
    /// </summary>
    Task<List<EmployeeType>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the employee type with the given <paramref name="id"/>, or <c>null</c> if not found.
    /// </summary>
    Task<EmployeeType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
