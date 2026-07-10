namespace EmployeeApi.Requests;

/// <summary>
/// Represents the external HTTP contract for replacing an employee's editable values.
/// </summary>
/// <remarks>
/// This type is deliberately separate from <see cref="CreateEmployeeRequest"/> even though their
/// current fields are shared. That allows update-specific validation, concurrency data, or
/// immutable-field rules to be introduced without changing the create contract.
/// </remarks>
public class UpdateEmployeeRequest : BaseEmployeeRequest
{
}
