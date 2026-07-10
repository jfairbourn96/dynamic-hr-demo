using System.Text.Json.Nodes;

namespace Dynamic.Employees.Application.Commands;

/// <summary>
/// Contains the values required to create an employee.
/// </summary>
/// <remarks>
/// This application-layer model is independent of the HTTP request that supplied the values, so
/// the create use case can be called and tested without referencing the API project.
/// </remarks>
public record CreateEmployeeCommand(
    string FirstName,
    string LastName,
    string Email,
    DateOnly HireDate,
    DateOnly? EndDate,
    string? Department,
    Guid EmployeeTypeId,
    JsonObject FieldValues);
