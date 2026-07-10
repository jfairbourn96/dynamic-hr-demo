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
    Task<List<EmployeeType>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific employee type by ID.
    /// </summary>
    Task<EmployeeType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new employee type.
    /// </summary>
    Task<EmployeeType> CreateAsync(CreateEmployeeTypeCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing employee type.
    /// </summary>
    Task<EmployeeType?> UpdateAsync(
        Guid id,
        UpdateEmployeeTypeCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an employee type by ID.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
