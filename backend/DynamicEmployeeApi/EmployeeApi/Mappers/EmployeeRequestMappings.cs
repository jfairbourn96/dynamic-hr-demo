using Dynamic.Employees.Application.Commands;
using EmployeeApi.Requests;

namespace EmployeeApi.Mappers;

internal static class EmployeeRequestMappings
{
    public static CreateEmployeeCommand ToCommand(this CreateEmployeeRequest request)
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

    public static UpdateEmployeeFieldCommand ToCommand(this UpdateEmployeeFieldRequest request)
    {
        return new UpdateEmployeeFieldCommand(
            request.FieldName,
            request.Value);
    }
}
