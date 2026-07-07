using Dynamic.Employees.Domain.Models;

namespace Dynamic.Employees.Application.Interfaces;

/// <summary>
/// Provides read operations for employees.
/// </summary>
public interface IEmployeeReader
{
    /// <summary>
    /// Returns the employee with the given <paramref name="id"/>, or <c>null</c> if not found.
    /// </summary>
    Task<Employee?> GetByIdAsync(Guid id);
}
