using Dynamic.Employees.Application.Commands;
using Dynamic.Employees.Domain.Models;
using EmployeeApi.Requests;
using EmployeeApi.Responses;

namespace EmployeeApi.Mappers;

internal static class EmployeeTypeMappings
{
    public static CreateEmployeeTypeCommand ToCommand(this CreateEmployeeTypeRequest request)
    {
        return new CreateEmployeeTypeCommand(
            request.Name,
            request.Description,
            request.Fields.Select(ToCommand).ToArray());
    }

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
