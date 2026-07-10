using Dynamic.Employees.Application.Commands;
using Dynamic.Employees.Domain.Models;

namespace Dynamic.Employees.Application.Services;

/// <summary>Provides business logic operations for employees.</summary>
public interface IEmployeeService
{
    /// <summary>Searches employees using request-independent query parameters.</summary>
    Task<EmployeeSearchServiceResult> SearchAsync(Guid? employeeTypeId, int pageNumber, int pageSize,
        IReadOnlyDictionary<string, string?> parameters, CancellationToken cancellationToken = default);

    /// <summary>Creates an employee after validating its runtime-defined values.</summary>
    Task<EmployeeMutationServiceResult> CreateAsync(CreateEmployeeCommand command, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a specific employee by ID.</summary>
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Replaces an employee's editable values after schema validation.</summary>
    Task<EmployeeMutationServiceResult> UpdateAsync(Guid id, UpdateEmployeeCommand command, CancellationToken cancellationToken = default);
}
