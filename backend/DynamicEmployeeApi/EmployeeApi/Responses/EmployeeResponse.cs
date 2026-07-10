using System.Text.Json.Nodes;

namespace EmployeeApi.Responses;

/// <summary>
/// Represents an employee in the external API contract without exposing persistence entities.
/// </summary>
public class EmployeeResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Department { get; set; }
    public Guid EmployeeTypeId { get; set; }
    public EmployeeTypeResponse? EmployeeType { get; set; }
    public JsonObject FieldValues { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
