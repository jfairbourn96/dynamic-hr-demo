using Dynamic.Employees.Application.Commands;
using Dynamic.Employees.Domain.Models;
using EmployeeApi.Requests;
using EmployeeApi.Responses;

namespace EmployeeApi.Mappers;

/// <summary>
/// Maps employee type API models to application commands and API responses.
/// </summary>
internal static class EmployeeTypeMappings
{
    /// <summary>Maps an employee-type creation request to its application command.</summary>
    public static CreateEmployeeTypeCommand ToCreateCommand(this CreateEmployeeTypeRequest request)
    {
        return new CreateEmployeeTypeCommand(
            request.Name,
            request.Description,
            request.Fields.Select(ToCommand).ToArray());
    }

    /// <summary>Maps an employee-type update request to its application command.</summary>
    public static UpdateEmployeeTypeCommand ToUpdateCommand(this UpdateEmployeeTypeRequest request)
    {
        return new UpdateEmployeeTypeCommand(
            request.Name,
            request.Description,
            request.Fields.Select(ToCommand).ToArray());
    }

    /// <summary>Maps an employee-type domain model to its API response.</summary>
    public static EmployeeTypeResponse ToResponse(this EmployeeType type) => new()
    {
        Id = type.Id.ToString(),
        Name = type.Name,
        Description = type.Description,
        ParentTypeId = null,
        Fields = type.Fields.Select(ToResponse).ToList(),
        CreatedAt = type.CreatedDate,
        UpdatedAt = type.UpdatedDate,
    };

    private static CreateEmployeeTypeFieldCommand ToCommand(this CreateEmployeeTypeFieldRequest field)
    {
        return new CreateEmployeeTypeFieldCommand(
            field.Name,
            field.Label,
            field.FieldType,
            field.Required,
            field.Options.Select(ToCommand).ToArray(),
            field.Order);
    }

    private static FieldOptionCommand ToCommand(this FieldOptionRequest option)
    {
        return new FieldOptionCommand(
            option.Label,
            option.Value);
    }

    private static EmployeeTypeFieldResponse ToResponse(this EmployeeTypeField field) => new()
    {
        Id = field.Id.ToString(),
        Name = field.Name,
        Label = field.Label,
        FieldType = field.FieldType,
        Required = field.Required,
        Options = field.Options.Select(ToResponse).ToList(),
        Order = field.Order,
    };

    private static FieldOptionResponse ToResponse(this FieldOption option) => new()
    {
        Label = option.Label,
        Value = option.Value,
    };
}
