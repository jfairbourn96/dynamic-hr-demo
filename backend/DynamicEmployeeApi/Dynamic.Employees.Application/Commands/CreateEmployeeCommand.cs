using System.Text.Json.Nodes;

namespace Dynamic.Employees.Application.Commands;

public record CreateEmployeeCommand(
    string FirstName,
    string LastName,
    string Email,
    DateOnly HireDate,
    DateOnly? EndDate,
    string? Department,
    Guid EmployeeTypeId,
    JsonObject FieldValues);
