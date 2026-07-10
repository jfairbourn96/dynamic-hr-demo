using System.Text.Json.Nodes;

namespace EmployeeApi.Requests;

/// <summary>
/// Request body for updating one dynamic employee field.
/// </summary>
public class UpdateEmployeeFieldRequest
{
    public string FieldName { get; set; } = string.Empty;
    public JsonNode? Value { get; set; }
}
