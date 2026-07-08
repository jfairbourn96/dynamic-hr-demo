using System.Text.Json.Nodes;
using AutoFixture;
using AutoFixture.AutoMoq;
using Dynamic.Employees.Application.Commands;
using Dynamic.Employees.Application.Interfaces;
using Dynamic.Employees.Application.Services;
using Dynamic.Employees.Domain.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

namespace Dynamic.Employees.Application.UnitTests.Services;

public class EmployeeServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IEmployeeReader> _employeeReader;
    private readonly Mock<IEmployeeWriter> _employeeWriter;

    public EmployeeServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _employeeReader = _fixture.Freeze<Mock<IEmployeeReader>>();
        _employeeWriter = _fixture.Freeze<Mock<IEmployeeWriter>>();
    }

    [Fact]
    public async Task CreateAsync_WhenCommandIsProvided_CreatesEmployeeAndPersistsIt()
    {
        // Arrange
        Employee? capturedEmployee = null;
        EmployeeService service = _fixture.Create<EmployeeService>();
        Guid employeeTypeId = Guid.NewGuid();
        JsonObject fieldValues = new()
        {
            ["favoriteSongName"] = "Get Back Up Again",
            ["movieVersion"] = "trolls-2016",
        };

        CreateEmployeeCommand command = new(
            "Poppy",
            "Troll",
            "poppy@trolls.example",
            new DateOnly(2016, 11, 4),
            null,
            "Pop Village",
            employeeTypeId,
            fieldValues);

        _employeeWriter
            .Setup(writer => writer.AddAsync(It.IsAny<Employee>()))
            .Callback<Employee>(employee => capturedEmployee = employee)
            .Returns(Task.CompletedTask);

        // Act
        Employee employee = await service.CreateAsync(command);

        // Assert
        using (new AssertionScope())
        {
            employee.Should().BeSameAs(capturedEmployee);
            employee.Id.Should().NotBeEmpty();
            employee.FirstName.Should().Be("Poppy");
            employee.LastName.Should().Be("Troll");
            employee.Email.Should().Be("poppy@trolls.example");
            employee.HireDate.Should().Be(new DateOnly(2016, 11, 4));
            employee.EndDate.Should().Be(DateOnly.MinValue);
            employee.Department.Should().Be("Pop Village");
            employee.EmployeeTypeId.Should().Be(employeeTypeId);
            ((object)employee.FieldValues).Should().BeSameAs(fieldValues);
            employee.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            employee.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        _employeeWriter.Verify(
            writer => writer.AddAsync(It.IsAny<Employee>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenEndDateIsProvided_UsesEndDate()
    {
        // Arrange
        EmployeeService service = _fixture.Create<EmployeeService>();
        DateOnly endDate = new(2023, 11, 17);

        CreateEmployeeCommand command = new(
            "Viva",
            "Troll",
            "viva@trolls.example",
            new DateOnly(2020, 4, 10),
            endDate,
            "Harmony Hub",
            Guid.NewGuid(),
            []);

        // Act
        Employee employee = await service.CreateAsync(command);

        // Assert
        employee.EndDate.Should().Be(endDate);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeExists_ReturnsReaderResult()
    {
        // Arrange
        Guid employeeId = Guid.NewGuid();
        Employee expectedEmployee = new() { Id = employeeId, FirstName = "Branch" };
        EmployeeService service = _fixture.Create<EmployeeService>();

        _employeeReader
            .Setup(reader => reader.GetByIdAsync(employeeId))
            .ReturnsAsync(expectedEmployee);

        // Act
        Employee? employee = await service.GetByIdAsync(employeeId);

        // Assert
        employee.Should().BeSameAs(expectedEmployee);

        _employeeReader.Verify(
            reader => reader.GetByIdAsync(employeeId),
            Times.Once);
    }

    [Fact]
    public async Task UpdateFieldAsync_WhenCommandIsProvided_DelegatesToWriter()
    {
        // Arrange
        Guid employeeId = Guid.NewGuid();
        JsonValue value = JsonValue.Create("world-tour-2020");
        UpdateEmployeeFieldCommand command = new("movieVersion", value);
        EmployeeService service = _fixture.Create<EmployeeService>();

        _employeeWriter
            .Setup(writer => writer.UpdateFieldAsync(employeeId, "movieVersion", value))
            .ReturnsAsync(true);

        // Act
        bool updated = await service.UpdateFieldAsync(employeeId, command);

        // Assert
        updated.Should().BeTrue();

        _employeeWriter.Verify(
            writer => writer.UpdateFieldAsync(employeeId, "movieVersion", value),
            Times.Once);
    }
}
