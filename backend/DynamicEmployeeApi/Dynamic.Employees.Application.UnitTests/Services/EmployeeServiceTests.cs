using System.Text.Json.Nodes;
using AutoFixture;
using AutoFixture.AutoMoq;
using Dynamic.Employees.Application.Commands;
using Dynamic.Employees.Application.Interfaces;
using Dynamic.Employees.Application.Services;
using Dynamic.Employees.Domain.Enums;
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
    private readonly Mock<IEmployeeTypeReader> _employeeTypeReader;

    public EmployeeServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _employeeReader = _fixture.Freeze<Mock<IEmployeeReader>>();
        _employeeWriter = _fixture.Freeze<Mock<IEmployeeWriter>>();
        _employeeTypeReader = _fixture.Freeze<Mock<IEmployeeTypeReader>>();
        _employeeTypeReader
            .Setup(reader => reader.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new EmployeeType
            {
                Id = id,
                Name = "Trolls Tour Performer",
                Fields =
                [
                    new EmployeeTypeField { Name = "favoriteSongName" },
                    new EmployeeTypeField
                    {
                        Name = "movieVersion",
                        FieldType = Dynamic.Employees.Domain.Enums.FieldType.Select,
                        Options = [new FieldOption { Value = "trolls-2016" }],
                    },
                ],
            });
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
            .Setup(writer => writer.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Callback<Employee, CancellationToken>((employee, _) => capturedEmployee = employee)
            .Returns(Task.CompletedTask);

        // Act
        EmployeeMutationServiceResult result = await service.CreateAsync(command);
        Employee employee = result.Employee!;

        // Assert
        using (new AssertionScope())
        {
            employee.Should().BeSameAs(capturedEmployee);
            employee.Id.Should().NotBeEmpty();
            employee.FirstName.Should().Be("Poppy");
            employee.LastName.Should().Be("Troll");
            employee.Email.Should().Be("poppy@trolls.example");
            employee.HireDate.Should().Be(new DateOnly(2016, 11, 4));
            employee.EndDate.Should().BeNull();
            employee.Department.Should().Be("Pop Village");
            employee.EmployeeTypeId.Should().Be(employeeTypeId);
            ((object)employee.FieldValues).Should().BeSameAs(fieldValues);
            employee.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            employee.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        _employeeWriter.Verify(
            writer => writer.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
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
        EmployeeMutationServiceResult result = await service.CreateAsync(command);

        // Assert
        result.Employee!.EndDate.Should().Be(endDate);
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
    public async Task CreateAsync_WhenEmployeeTypeDoesNotExist_ReturnsValidationFailureWithoutPersisting()
    {
        // Arrange
        Guid employeeTypeId = Guid.NewGuid();
        EmployeeService service = _fixture.Create<EmployeeService>();
        _employeeTypeReader
            .Setup(reader => reader.GetByIdAsync(employeeTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeType?)null);

        // Act
        EmployeeMutationServiceResult result = await service.CreateAsync(CreateCommand(employeeTypeId, []));

        // Assert
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Employee.Should().BeNull();
            result.NotFound.Should().BeFalse();
            result.Errors.Should().Equal("Employee type was not found.");
        }

        _employeeWriter.Verify(
            writer => writer.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenDynamicValuesAreInvalid_ReturnsEveryValidationErrorWithoutPersisting()
    {
        // Arrange
        Guid employeeTypeId = Guid.NewGuid();
        ConfigureEmployeeType(
            employeeTypeId,
            new EmployeeTypeField { Name = "requiredText", FieldType = FieldType.Text, Required = true },
            new EmployeeTypeField { Name = "address", FieldType = FieldType.Address },
            new EmployeeTypeField { Name = "number", FieldType = FieldType.Number },
            new EmployeeTypeField { Name = "date", FieldType = FieldType.Date },
            new EmployeeTypeField { Name = "boolean", FieldType = FieldType.Boolean },
            new EmployeeTypeField { Name = "object", FieldType = FieldType.Text },
            new EmployeeTypeField { Name = "unsupported", FieldType = (FieldType)999 },
            new EmployeeTypeField
            {
                Name = "select",
                FieldType = FieldType.Select,
                Options = [new FieldOption { Value = "valid" }],
            });
        JsonObject values = new()
        {
            ["requiredText"] = " ",
            ["address"] = 123,
            ["number"] = "not-a-number",
            ["date"] = "not-a-date",
            ["boolean"] = "not-a-boolean",
            ["object"] = new JsonObject { ["nested"] = true },
            ["unsupported"] = "value",
            ["select"] = "unknown",
            ["unknownField"] = "value",
        };
        EmployeeService service = _fixture.Create<EmployeeService>();

        // Act
        EmployeeMutationServiceResult result = await service.CreateAsync(CreateCommand(employeeTypeId, values));

        // Assert
        result.Errors.Should().BeEquivalentTo(
        [
            "Dynamic field 'unknownField' does not exist on employee type 'Configured Type'.",
            "Dynamic field 'requiredText' is required.",
            "Dynamic field 'address' has an invalid address value.",
            "Dynamic field 'number' has an invalid number value.",
            "Dynamic field 'date' has an invalid date value.",
            "Dynamic field 'boolean' has an invalid boolean value.",
            "Dynamic field 'object' has an invalid text value.",
            "Dynamic field 'unsupported' has an invalid 999 value.",
            "Dynamic field 'select' has an invalid select value.",
        ]);
        _employeeWriter.Verify(
            writer => writer.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenAllDynamicFieldTypesAreValid_PersistsEmployee()
    {
        // Arrange
        Guid employeeTypeId = Guid.NewGuid();
        ConfigureEmployeeType(
            employeeTypeId,
            new EmployeeTypeField { Name = "text", FieldType = FieldType.Text, Required = true },
            new EmployeeTypeField { Name = "address", FieldType = FieldType.Address },
            new EmployeeTypeField { Name = "number", FieldType = FieldType.Number },
            new EmployeeTypeField { Name = "date", FieldType = FieldType.Date },
            new EmployeeTypeField { Name = "boolean", FieldType = FieldType.Boolean },
            new EmployeeTypeField
            {
                Name = "select",
                FieldType = FieldType.Select,
                Options = [new FieldOption { Value = "valid" }],
            },
            new EmployeeTypeField { Name = "optional", FieldType = FieldType.Text });
        JsonObject values = new()
        {
            ["text"] = "value",
            ["address"] = "123 Pop Village",
            ["number"] = 12.5,
            ["date"] = "2026-07-10",
            ["boolean"] = true,
            ["select"] = "valid",
            ["optional"] = null,
        };
        EmployeeService service = _fixture.Create<EmployeeService>();

        // Act
        EmployeeMutationServiceResult result = await service.CreateAsync(CreateCommand(employeeTypeId, values));

        // Assert
        result.IsValid.Should().BeTrue();
        _employeeWriter.Verify(
            writer => writer.AddAsync(result.Employee!, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmployeeDoesNotExist_ReturnsNotFoundWithoutValidatingOrPersisting()
    {
        // Arrange
        Guid employeeId = Guid.NewGuid();
        EmployeeService service = _fixture.Create<EmployeeService>();
        _employeeReader
            .Setup(reader => reader.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act
        EmployeeMutationServiceResult result = await service.UpdateAsync(
            employeeId,
            CreateUpdateCommand(Guid.NewGuid(), []));

        // Assert
        using (new AssertionScope())
        {
            result.NotFound.Should().BeTrue();
            result.IsValid.Should().BeTrue();
            result.Employee.Should().BeNull();
            result.Errors.Should().BeEmpty();
        }
        _employeeTypeReader.Verify(
            reader => reader.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _employeeWriter.Verify(
            writer => writer.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenValuesAreValid_ReplacesEditableValuesAndPersistsEmployee()
    {
        // Arrange
        Guid employeeId = Guid.NewGuid();
        Guid employeeTypeId = Guid.NewGuid();
        DateTime originalUpdatedDate = DateTime.UtcNow.AddDays(-1);
        Employee existing = new()
        {
            Id = employeeId,
            FirstName = "Old",
            EmployeeTypeId = employeeTypeId,
            UpdatedDate = originalUpdatedDate,
        };
        JsonObject fieldValues = new() { ["favoriteSongName"] = "Better Place" };
        UpdateEmployeeCommand command = new(
            "Branch",
            "Troll",
            "branch@trolls.example",
            new DateOnly(2023, 11, 17),
            new DateOnly(2026, 7, 10),
            "Pop Village",
            employeeTypeId,
            fieldValues);
        EmployeeService service = _fixture.Create<EmployeeService>();
        _employeeReader
            .Setup(reader => reader.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        EmployeeMutationServiceResult result = await service.UpdateAsync(employeeId, command);

        // Assert
        using (new AssertionScope())
        {
            result.Employee.Should().BeSameAs(existing);
            existing.FirstName.Should().Be("Branch");
            existing.LastName.Should().Be("Troll");
            existing.Email.Should().Be("branch@trolls.example");
            existing.HireDate.Should().Be(new DateOnly(2023, 11, 17));
            existing.EndDate.Should().Be(new DateOnly(2026, 7, 10));
            existing.Department.Should().Be("Pop Village");
            existing.EmployeeTypeId.Should().Be(employeeTypeId);
            ((object)existing.FieldValues).Should().BeSameAs(fieldValues);
            existing.UpdatedDate.Should().BeAfter(originalUpdatedDate);
        }
        _employeeWriter.Verify(
            writer => writer.UpdateAsync(existing, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenValuesAreInvalid_DoesNotMutateOrPersistEmployee()
    {
        // Arrange
        Guid employeeId = Guid.NewGuid();
        Guid employeeTypeId = Guid.NewGuid();
        Employee existing = new() { Id = employeeId, FirstName = "Original" };
        ConfigureEmployeeType(
            employeeTypeId,
            new EmployeeTypeField { Name = "required", FieldType = FieldType.Text, Required = true });
        EmployeeService service = _fixture.Create<EmployeeService>();
        _employeeReader
            .Setup(reader => reader.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        EmployeeMutationServiceResult result = await service.UpdateAsync(
            employeeId,
            CreateUpdateCommand(employeeTypeId, []));

        // Assert
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Equal("Dynamic field 'required' is required.");
            existing.FirstName.Should().Be("Original");
        }
        _employeeWriter.Verify(
            writer => writer.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTokenIsProvided_PropagatesTokenToReader()
    {
        // Arrange
        Guid employeeId = Guid.NewGuid();
        using CancellationTokenSource cancellationSource = new();
        EmployeeService service = _fixture.Create<EmployeeService>();

        // Act
        await service.GetByIdAsync(employeeId, cancellationSource.Token);

        // Assert
        _employeeReader.Verify(
            reader => reader.GetByIdAsync(employeeId, cancellationSource.Token),
            Times.Once);
    }

    private void ConfigureEmployeeType(Guid employeeTypeId, params EmployeeTypeField[] fields)
    {
        _employeeTypeReader
            .Setup(reader => reader.GetByIdAsync(employeeTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeType
            {
                Id = employeeTypeId,
                Name = "Configured Type",
                Fields = fields.ToList(),
            });
    }

    private static CreateEmployeeCommand CreateCommand(Guid employeeTypeId, JsonObject fieldValues) => new(
        "Poppy",
        "Troll",
        "poppy@trolls.example",
        new DateOnly(2016, 11, 4),
        null,
        "Pop Village",
        employeeTypeId,
        fieldValues);

    private static UpdateEmployeeCommand CreateUpdateCommand(Guid employeeTypeId, JsonObject fieldValues) => new(
        "Poppy",
        "Troll",
        "poppy@trolls.example",
        new DateOnly(2016, 11, 4),
        null,
        "Pop Village",
        employeeTypeId,
        fieldValues);

}
