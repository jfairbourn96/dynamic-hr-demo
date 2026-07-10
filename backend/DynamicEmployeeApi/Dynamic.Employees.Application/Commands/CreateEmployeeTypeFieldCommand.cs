using Dynamic.Employees.Domain.Enums;

namespace Dynamic.Employees.Application.Commands;

/// <summary>
/// Describes a dynamic field in an employee type command.
/// </summary>
public record CreateEmployeeTypeFieldCommand(
    string Name,
    string Label,
    FieldType FieldType,
    bool Required,
    IReadOnlyCollection<FieldOptionCommand> Options,
    int Order);
