using System.Text.Json.Nodes;
using Dynamic.Employees.Application.Models;
using Dynamic.Employees.Domain.Models;
using EmployeeApi.Mappers;
using EmployeeApi.Requests;
using EmployeeApi.Responses;
using FluentAssertions;
using FluentAssertions.Execution;

namespace EmployeeApi.UnitTests.Mappers;

public class EmployeeRequestMappingsTests
{
    [Fact]
    public void ToCreateCommand_WhenRequestIsProvided_MapsEveryValue()
    {
        CreateEmployeeRequest request = CreateRequest();

        var command = request.ToCreateCommand();

        command.Should().BeEquivalentTo(request);
        ((object)command.FieldValues).Should().BeSameAs(request.FieldValues);
    }

    [Fact]
    public void ToUpdateCommand_WhenRequestIsProvided_MapsEveryValue()
    {
        CreateEmployeeRequest create = CreateRequest();
        UpdateEmployeeRequest request = new()
        {
            FirstName = create.FirstName,
            LastName = create.LastName,
            Email = create.Email,
            HireDate = create.HireDate,
            EndDate = create.EndDate,
            Department = create.Department,
            EmployeeTypeId = create.EmployeeTypeId,
            FieldValues = create.FieldValues,
        };

        var command = request.ToUpdateCommand();

        command.Should().BeEquivalentTo(request);
        ((object)command.FieldValues).Should().BeSameAs(request.FieldValues);
    }

    [Fact]
    public void ToResponse_WhenEmployeeIsProvided_MapsStableApiContract()
    {
        Employee employee = CreateEmployee();

        EmployeeResponse response = employee.ToResponse();

        using (new AssertionScope())
        {
            response.Id.Should().Be(employee.Id);
            response.FirstName.Should().Be(employee.FirstName);
            response.EndDate.Should().Be(employee.EndDate);
            response.EmployeeType!.Name.Should().Be(employee.EmployeeType!.Name);
            response.EmployeeType.Fields.Should().ContainSingle(field => field.Name == "movieVersion");
            ((object)response.FieldValues).Should().BeSameAs(employee.FieldValues);
            response.CreatedAt.Should().Be(employee.CreatedDate);
            response.UpdatedAt.Should().Be(employee.UpdatedDate);
        }
    }

    [Fact]
    public void ToResponse_WhenSearchResultIsProvided_MapsItemsAndPaging()
    {
        Employee employee = CreateEmployee();
        EmployeeSearchItem item = new(
            employee.Id, employee.FirstName, employee.LastName, employee.Email,
            employee.HireDate, employee.EndDate, employee.Department, employee.EmployeeTypeId,
            employee.EmployeeType, employee.CreatedDate, employee.UpdatedDate, employee.FieldValues);
        EmployeeSearchResult result = new([item], 12, 2, 5);

        EmployeeSearchResponse response = result.ToResponse();

        using (new AssertionScope())
        {
            response.TotalCount.Should().Be(12);
            response.PageNumber.Should().Be(2);
            response.PageSize.Should().Be(5);
            response.Items.Should().ContainSingle(mapped => mapped.Id == employee.Id);
            response.Items.Single().EmployeeType!.Name.Should().Be("Trolls Tour Performer");
        }
    }

    private static CreateEmployeeRequest CreateRequest() => new()
    {
        FirstName = "Poppy",
        LastName = "Troll",
        Email = "poppy@trolls.example",
        HireDate = new DateOnly(2016, 11, 4),
        EndDate = new DateOnly(2026, 7, 10),
        Department = "Pop Village",
        EmployeeTypeId = Guid.NewGuid(),
        FieldValues = new JsonObject { ["movieVersion"] = "trolls-2016" },
    };

    private static Employee CreateEmployee()
    {
        EmployeeType type = new()
        {
            Id = Guid.NewGuid(),
            Name = "Trolls Tour Performer",
            Fields = [new EmployeeTypeField { Id = Guid.NewGuid(), Name = "movieVersion" }],
        };
        return new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Poppy",
            LastName = "Troll",
            Email = "poppy@trolls.example",
            HireDate = new DateOnly(2016, 11, 4),
            EndDate = new DateOnly(2026, 7, 10),
            Department = "Pop Village",
            EmployeeTypeId = type.Id,
            EmployeeType = type,
            FieldValues = new JsonObject { ["movieVersion"] = "trolls-2016" },
            CreatedDate = DateTime.UtcNow.AddDays(-1),
            UpdatedDate = DateTime.UtcNow,
        };
    }
}
