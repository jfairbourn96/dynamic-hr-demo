namespace EmployeeApi.Requests;

/// <summary>
/// Represents the external HTTP contract for creating an employee.
/// </summary>
/// <remarks>
/// This transport DTO is mapped to a create-specific application command before business logic
/// is invoked, keeping API serialization concerns out of the application layer.
/// </remarks>
public class CreateEmployeeRequest : BaseEmployeeRequest
{
}
