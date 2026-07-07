using Dynamic.Employees.Application.Commands;
using Dynamic.Employees.Domain.Models;

namespace Dynamic.Employees.Application.Services;

/// <summary>
/// Provides business logic operations for employee types.
/// </summary>
public interface IEmployeeTypeService
{
    /// <summary>
    /// Retrieves all employee types.
    /// </summary>
    Task<List<EmployeeType>> GetAllAsync();

    /// <summary>
    /// Retrieves a specific employee type by ID.
    /// </summary>
    Task<EmployeeType?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new employee type.
    /// </summary>
    Task<EmployeeType> CreateAsync(CreateEmployeeTypeCommand command);

    /// <summary>
    /// Updates an existing employee type.
    /// </summary>
    Task<EmployeeType?> UpdateAsync(Guid id, CreateEmployeeTypeCommand command);

    /// <summary>
    /// Deletes an employee type by ID.
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
}
