using System.Text.Json.Nodes;

namespace Dynamic.Employees.Application.Commands;

public record UpdateEmployeeFieldCommand(
    string FieldName,
    JsonNode? Value);
