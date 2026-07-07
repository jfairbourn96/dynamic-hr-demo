namespace Dynamic.Employees.Application.Commands;

public record CreateEmployeeTypeCommand(
    string Name,
    string? Description,
    IReadOnlyCollection<CreateEmployeeTypeFieldCommand> Fields);
