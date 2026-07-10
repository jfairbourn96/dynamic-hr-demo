namespace Dynamic.Employees.Application.Commands;

/// <summary>
/// Describes a selectable option in a dynamic field command.
/// </summary>
public record FieldOptionCommand(
    string Label,
    string Value);
