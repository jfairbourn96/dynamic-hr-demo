namespace EmployeeApi.Responses;

/// <summary>
/// The API representation of an employee type, shaped for frontend consumption.
/// </summary>
public class EmployeeTypeResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ParentTypeId { get; set; }
    public List<EmployeeTypeFieldResponse> Fields { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
