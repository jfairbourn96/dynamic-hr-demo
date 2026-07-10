using Dynamic.Employees.Application.Commands;
using Dynamic.Employees.Domain.Models;
using Dynamic.Employees.Application.Models;
using EmployeeApi.Requests;
using EmployeeApi.Responses;

namespace EmployeeApi.Mappers;

/// <summary>
/// Translates employee HTTP request models into application use-case commands.
/// </summary>
/// <remarks>
/// Keeping this translation at the API boundary allows request DTOs to describe the external
/// JSON contract while commands describe what the application layer needs. The application
/// therefore remains independent of ASP.NET Core and can evolve, be tested, or be invoked by
/// another delivery mechanism without depending on HTTP-specific models. A separate mapper also
/// keeps transport DTOs as data-only types and gives create and update operations
/// independent mappings even when some of their values currently overlap.
/// </remarks>
internal static class EmployeeRequestMappings
{
    /// <summary>Maps an employee creation request to its application command.</summary>
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

    /// <summary>Maps an employee replacement request to its application command.</summary>
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

    /// <summary>
    /// Maps a domain employee to the stable API response contract.
    /// </summary>
    public static EmployeeResponse ToResponse(this Employee employee) => new()
    {
        Id = employee.Id,
        FirstName = employee.FirstName,
        LastName = employee.LastName,
        Email = employee.Email,
        HireDate = employee.HireDate,
        EndDate = employee.EndDate,
        Department = employee.Department,
        EmployeeTypeId = employee.EmployeeTypeId,
        EmployeeType = employee.EmployeeType?.ToResponse(),
        FieldValues = employee.FieldValues,
        CreatedAt = employee.CreatedDate,
        UpdatedAt = employee.UpdatedDate,
    };

    /// <summary>Maps an application search result to the paged API response contract.</summary>
    public static EmployeeSearchResponse ToResponse(this EmployeeSearchResult result) => new()
    {
        Items = result.Items.Select(ToResponse).ToArray(),
        TotalCount = result.TotalCount,
        PageNumber = result.PageNumber,
        PageSize = result.PageSize,
    };

    private static EmployeeResponse ToResponse(this EmployeeSearchItem employee) => new()
    {
        Id = employee.Id,
        FirstName = employee.FirstName,
        LastName = employee.LastName,
        Email = employee.Email,
        HireDate = employee.HireDate,
        EndDate = employee.EndDate,
        Department = employee.Department,
        EmployeeTypeId = employee.EmployeeTypeId,
        EmployeeType = employee.EmployeeType?.ToResponse(),
        FieldValues = employee.FieldValues,
        CreatedAt = employee.CreatedDate,
        UpdatedAt = employee.UpdatedDate,
    };
}
