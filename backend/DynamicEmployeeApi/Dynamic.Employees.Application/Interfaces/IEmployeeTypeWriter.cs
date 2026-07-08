using Dynamic.Employees.Domain.Models;

namespace Dynamic.Employees.Application.Interfaces;

/// <summary>
/// Provides write operations for employee types.
/// </summary>
public interface IEmployeeTypeWriter
{
    /// <summary>
    /// Adds a new employee type and persists the change.
    /// </summary>
    Task AddAsync(EmployeeType employeeType);

    /// <summary>
    /// Updates an existing employee type and persists the change.
    /// </summary>
    Task UpdateAsync(EmployeeType employeeType);

    /// <summary>
    /// Deletes an employee type and persists the change.
    /// </summary>
    Task DeleteAsync(EmployeeType employeeType);
}
