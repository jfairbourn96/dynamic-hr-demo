namespace Dynamic.Employees.Application.Commands;

/// <summary>
/// Contains the values used to replace an employee type and its runtime field schema.
/// </summary>
public record UpdateEmployeeTypeCommand(
    string Name,
    string? Description,
    IReadOnlyCollection<CreateEmployeeTypeFieldCommand> Fields);
