using Dynamic.Employees.Application.Commands;
using Dynamic.Employees.Application.Interfaces;
using Dynamic.Employees.Domain.Models;

namespace Dynamic.Employees.Application.Services;

/// <summary>
/// Implements business logic operations for employee types.
/// </summary>
public class EmployeeTypeService : IEmployeeTypeService
{
    private readonly IEmployeeTypeReader _reader;
    private readonly IEmployeeTypeWriter _writer;

    public EmployeeTypeService(
        IEmployeeTypeReader reader,
        IEmployeeTypeWriter writer)
    {
        _reader = reader;
        _writer = writer;
    }

    /// <inheritdoc />
    public async Task<List<EmployeeType>> GetAllAsync()
    {
        return await _reader.GetAllAsync();
    }

    /// <inheritdoc />
    public async Task<EmployeeType?> GetByIdAsync(Guid id)
    {
        return await _reader.GetByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<EmployeeType> CreateAsync(CreateEmployeeTypeCommand command)
    {
        EmployeeType type = new()
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            Fields = command.Fields.Select(ToField).ToList(),
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
        };

        await _writer.AddAsync(type);

        return type;
    }

    /// <inheritdoc />
    public async Task<EmployeeType?> UpdateAsync(Guid id, CreateEmployeeTypeCommand command)
    {
        EmployeeType? type = await _reader.GetByIdAsync(id);

        if (type is null)
        {
            return null;
        }

        type.Name = command.Name;
        type.Description = command.Description;
        type.Fields = command.Fields.Select(ToField).ToList();
        type.UpdatedDate = DateTime.UtcNow;

        await _writer.UpdateAsync(type);

        return type;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        EmployeeType? type = await _reader.GetByIdAsync(id);

        if (type is null)
        {
            return false;
        }

        await _writer.DeleteAsync(type);

        return true;
    }

    private static EmployeeTypeField ToField(CreateEmployeeTypeFieldCommand field) => new()
    {
        Id = Guid.NewGuid(),
        Name = field.Name,
        Label = field.Label,
        FieldType = field.FieldType,
        Required = field.Required,
        Options = field.Options.Select(o => new FieldOption { Label = o.Label, Value = o.Value }).ToList(),
        Order = field.Order,
    };
}
