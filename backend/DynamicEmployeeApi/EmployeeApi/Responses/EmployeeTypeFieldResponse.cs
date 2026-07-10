using Dynamic.Employees.Domain.Enums;

namespace EmployeeApi.Responses;

/// <summary>
/// A single dynamic field definition belonging to an employee type.
/// </summary>
public class EmployeeTypeFieldResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public bool Required { get; set; }
    public List<FieldOptionResponse> Options { get; set; } = [];
    public int Order { get; set; }
}
