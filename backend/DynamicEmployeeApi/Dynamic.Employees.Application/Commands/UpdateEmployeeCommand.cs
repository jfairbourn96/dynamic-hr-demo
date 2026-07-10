using System.Text.Json.Nodes;

namespace Dynamic.Employees.Application.Commands;

/// <summary>
/// Contains the editable values used to replace an employee record.
/// </summary>
/// <remarks>
/// A dedicated update command expresses update semantics and can evolve independently from the
/// create use case even while both commands happen to carry similar values.
/// </remarks>
public record UpdateEmployeeCommand(
    string FirstName,
    string LastName,
    string Email,
    DateOnly HireDate,
    DateOnly? EndDate,
    string? Department,
    Guid EmployeeTypeId,
    JsonObject FieldValues);
