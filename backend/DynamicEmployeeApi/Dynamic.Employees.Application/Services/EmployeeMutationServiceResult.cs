using Dynamic.Employees.Domain.Models;

namespace Dynamic.Employees.Application.Services;

/// <summary>Represents the outcome of an employee create or update operation.</summary>
public record EmployeeMutationServiceResult(Employee? Employee, IReadOnlyCollection<string> Errors, bool NotFound = false)
{
    public bool IsValid => Errors.Count == 0;
    /// <summary>Creates a successful mutation result.</summary>
    public static EmployeeMutationServiceResult Success(Employee employee) => new(employee, []);
    /// <summary>Creates a mutation result containing validation errors.</summary>
    public static EmployeeMutationServiceResult Failure(IReadOnlyCollection<string> errors) => new(null, errors);
    /// <summary>Creates a mutation result for an employee that was not found.</summary>
    public static EmployeeMutationServiceResult Missing() => new(null, [], true);
}
