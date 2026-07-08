using Dynamic.Employees.Domain.Enums;

namespace Dynamic.Employees.Application.Commands;

public record CreateEmployeeTypeFieldCommand(
    string Name,
    string Label,
    FieldType FieldType,
    bool Required,
    IReadOnlyCollection<FieldOptionCommand> Options,
    int Order);
