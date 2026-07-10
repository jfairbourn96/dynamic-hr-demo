using Dynamic.Employees.Domain.Enums;
using Dynamic.Employees.Domain.Models;
using EmployeeApi.Mappers;
using EmployeeApi.Requests;
using FluentAssertions;
using FluentAssertions.Execution;

namespace EmployeeApi.UnitTests.Mappers;

public class EmployeeTypeMappingsTests
{
    [Fact]
    public void ToCreateAndUpdateCommand_WhenRequestsAreProvided_MapFieldsAndOptions()
    {
        CreateEmployeeTypeRequest create = new() { Name = string.Empty };
        Populate(create);
        UpdateEmployeeTypeRequest update = new() { Name = string.Empty };
        Populate(update);

        var createCommand = create.ToCreateCommand();
        var updateCommand = update.ToUpdateCommand();

        createCommand.Should().BeEquivalentTo(updateCommand);
        createCommand.Fields.Single().Options.Should().ContainSingle(option =>
            option.Label == "Trolls (2016)" && option.Value == "trolls-2016");
    }

    [Fact]
    public void ToResponse_WhenEmployeeTypeIsProvided_MapsFieldsOptionsAndDates()
    {
        DateTime created = DateTime.UtcNow.AddDays(-1);
        DateTime updated = DateTime.UtcNow;
        EmployeeType type = new()
        {
            Id = Guid.NewGuid(),
            Name = "Trolls Tour Performer",
            Description = "Performers",
            CreatedDate = created,
            UpdatedDate = updated,
            Fields =
            [
                new EmployeeTypeField
                {
                    Id = Guid.NewGuid(),
                    Name = "movieVersion",
                    Label = "Movie Version",
                    FieldType = FieldType.Select,
                    Required = true,
                    Order = 2,
                    Options = [new FieldOption { Label = "Trolls (2016)", Value = "trolls-2016" }],
                },
            ],
        };

        var response = type.ToResponse();

        using (new AssertionScope())
        {
            response.Id.Should().Be(type.Id.ToString());
            response.ParentTypeId.Should().BeNull();
            response.CreatedAt.Should().Be(created);
            response.UpdatedAt.Should().Be(updated);
            response.Fields.Should().ContainSingle();
            response.Fields.Single().Options.Should().ContainSingle(option => option.Value == "trolls-2016");
        }
    }

    private static void Populate(BaseEmployeeTypeRequest request)
    {
        request.Name = "Trolls Tour Performer";
        request.Description = "Performers";
        request.Fields =
        [
            new CreateEmployeeTypeFieldRequest
            {
                Name = "movieVersion",
                Label = "Movie Version",
                FieldType = FieldType.Select,
                Required = true,
                Order = 1,
                Options = [new FieldOptionRequest { Label = "Trolls (2016)", Value = "trolls-2016" }],
            },
        ];
    }
}
