using System.Text.Json.Nodes;

namespace Dynamic.Employees.Application.Commands;

/// <summary>
/// Identifies one dynamic employee field and its replacement value.
/// </summary>
public record UpdateEmployeeFieldCommand(
    string FieldName,
    JsonNode? Value);
