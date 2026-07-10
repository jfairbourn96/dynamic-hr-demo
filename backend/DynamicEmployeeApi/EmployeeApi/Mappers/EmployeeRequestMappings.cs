using Dynamic.Employees.Application.Commands;
using EmployeeApi.Requests;

namespace EmployeeApi.Mappers;

/// <summary>
/// Translates employee HTTP request models into application use-case commands.
/// </summary>
/// <remarks>
/// Keeping this translation at the API boundary allows request DTOs to describe the external
/// JSON contract while commands describe what the application layer needs. The application
/// therefore remains independent of ASP.NET Core and can evolve, be tested, or be invoked by
/// another delivery mechanism without depending on HTTP-specific models. A separate mapper also
/// keeps transport DTOs as data-only types and gives create, update, and field-update operations
/// independent mappings even when some of their values currently overlap.
/// </remarks>
internal static class EmployeeRequestMappings
{
    public static CreateEmployeeCommand ToCreateCommand(this CreateEmployeeRequest request)
    {
        return new CreateEmployeeCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.HireDate,
            request.EndDate,
            request.Department,
            request.EmployeeTypeId,
            request.FieldValues);
    }

    public static UpdateEmployeeFieldCommand ToUpdateFieldCommand(this UpdateEmployeeFieldRequest request)
    {
        return new UpdateEmployeeFieldCommand(
            request.FieldName,
            request.Value);
    }

    public static UpdateEmployeeCommand ToUpdateCommand(this UpdateEmployeeRequest request)
    {
        return new UpdateEmployeeCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.HireDate,
            request.EndDate,
            request.Department,
            request.EmployeeTypeId,
            request.FieldValues);
    }
}
