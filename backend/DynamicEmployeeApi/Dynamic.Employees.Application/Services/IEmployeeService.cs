using Dynamic.Employees.Application.Commands;
using Dynamic.Employees.Application.Models;
using Dynamic.Employees.Domain.Models;

namespace Dynamic.Employees.Application.Services;

/// <summary>
/// Provides business logic operations for employees.
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// Searches employees using request-independent query parameters.
    /// </summary>
    Task<EmployeeSearchServiceResult> SearchAsync(
        Guid? employeeTypeId,
        int pageNumber,
        int pageSize,
        IReadOnlyDictionary<string, string?> parameters);

    /// <summary>
    /// Creates a new employee.
    /// </summary>
    Task<Employee> CreateAsync(CreateEmployeeCommand command);

    /// <summary>
    /// Retrieves a specific employee by ID.
    /// </summary>
    Task<Employee?> GetByIdAsync(Guid id);

    /// <summary>
    /// Updates one dynamic field value.
    /// </summary>
    Task<bool> UpdateFieldAsync(Guid id, UpdateEmployeeFieldCommand command);
}

public record EmployeeSearchServiceResult(
    EmployeeSearchResult? SearchResult,
    IReadOnlyCollection<string> Errors)
{
    public bool IsValid => Errors.Count == 0;

    public static EmployeeSearchServiceResult Success(EmployeeSearchResult result) => new(result, []);

    public static EmployeeSearchServiceResult Failure(IReadOnlyCollection<string> errors) => new(null, errors);
}
