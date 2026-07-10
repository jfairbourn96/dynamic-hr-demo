namespace Dynamic.Employees.Application.Commands;

/// <summary>
/// Contains the values required to create or replace an employee type.
/// </summary>
public record CreateEmployeeTypeCommand(
    string Name,
    string? Description,
    IReadOnlyCollection<CreateEmployeeTypeFieldCommand> Fields);
