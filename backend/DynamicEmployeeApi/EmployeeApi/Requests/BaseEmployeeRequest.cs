using System.Text.Json.Nodes;

namespace EmployeeApi.Requests;

/// <summary>
/// Defines the HTTP request values shared by employee creation and replacement.
/// </summary>
/// <remarks>
/// This base type removes duplication from the transport contract only. The concrete
/// <see cref="CreateEmployeeRequest"/> and <see cref="UpdateEmployeeRequest"/> types remain
/// distinct so their API contracts can gain different validation or fields later without
/// coupling the two operations. Request types intentionally do not create application commands;
/// that boundary translation is owned by the mapper.
/// </remarks>
public abstract class BaseEmployeeRequest
{
    public required string FirstName { get; set; } = string.Empty;
    public required string LastName { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public required DateOnly HireDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Department { get; set; }
    public Guid EmployeeTypeId { get; set; }
    public JsonObject FieldValues { get; set; } = new();
}
