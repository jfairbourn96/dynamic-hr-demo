namespace EmployeeApi.Requests;

/// <summary>
/// Defines the employee-type values shared by create and update HTTP contracts.
/// </summary>
public abstract class BaseEmployeeTypeRequest
{
    public required string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreateEmployeeTypeFieldRequest> Fields { get; set; } = [];
}
