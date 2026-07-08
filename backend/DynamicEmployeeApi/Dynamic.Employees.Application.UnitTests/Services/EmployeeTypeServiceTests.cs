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

public class EmployeeTypeServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IEmployeeTypeReader> _reader;
    private readonly Mock<IEmployeeTypeWriter> _writer;

    public EmployeeTypeServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _reader = _fixture.Freeze<Mock<IEmployeeTypeReader>>();
        _writer = _fixture.Freeze<Mock<IEmployeeTypeWriter>>();
    }

    [Fact]
    public async Task GetAllAsync_WhenEmployeeTypesExist_ReturnsReaderResults()
    {
        // Arrange
        List<EmployeeType> expectedTypes =
        [
            new EmployeeType { Id = Guid.NewGuid(), Name = "Trolls (2016)" },
            new EmployeeType { Id = Guid.NewGuid(), Name = "Trolls World Tour (2020)" },
        ];
        EmployeeTypeService service = _fixture.Create<EmployeeTypeService>();

        _reader
            .Setup(reader => reader.GetAllAsync())
            .ReturnsAsync(expectedTypes);

        // Act
        List<EmployeeType> employeeTypes = await service.GetAllAsync();

        // Assert
        employeeTypes.Should().BeSameAs(expectedTypes);

        _reader.Verify(reader => reader.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeTypeExists_ReturnsReaderResult()
    {
        // Arrange
        Guid employeeTypeId = Guid.NewGuid();
        EmployeeType expectedType = new() { Id = employeeTypeId, Name = "Trolls Band Together (2023)" };
        EmployeeTypeService service = _fixture.Create<EmployeeTypeService>();

        _reader
            .Setup(reader => reader.GetByIdAsync(employeeTypeId))
            .ReturnsAsync(expectedType);

        // Act
        EmployeeType? employeeType = await service.GetByIdAsync(employeeTypeId);

        // Assert
        employeeType.Should().BeSameAs(expectedType);

        _reader.Verify(reader => reader.GetByIdAsync(employeeTypeId), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenCommandIsProvided_CreatesEmployeeTypeAndPersistsIt()
    {
        // Arrange
        EmployeeType? capturedType = null;
        EmployeeTypeService service = _fixture.Create<EmployeeTypeService>();
        CreateEmployeeTypeCommand command = CreateTrollsTourCommand("Trolls Tour Performer");

        _writer
            .Setup(writer => writer.AddAsync(It.IsAny<EmployeeType>()))
            .Callback<EmployeeType>(employeeType => capturedType = employeeType)
            .Returns(Task.CompletedTask);

        // Act
        EmployeeType employeeType = await service.CreateAsync(command);

        // Assert
        using (new AssertionScope())
        {
            employeeType.Should().BeSameAs(capturedType);
            employeeType.Id.Should().NotBeEmpty();
            employeeType.Name.Should().Be("Trolls Tour Performer");
            employeeType.Description.Should().Be("Performers grouped by Trolls movie era.");
            employeeType.Fields.Should().HaveCount(2);
            employeeType.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            employeeType.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            employeeType.Fields.Should().ContainEquivalentOf(new EmployeeTypeField
            {
                Name = "favoriteSongName",
                Label = "Favorite Song",
                FieldType = FieldType.Text,
                Required = true,
                Order = 1,
                Options = [],
            }, options => options.Excluding(field => field.Id));

            employeeType.Fields.Should().ContainEquivalentOf(new EmployeeTypeField
            {
                Name = "movieVersion",
                Label = "Movie Version",
                FieldType = FieldType.Select,
                Required = true,
                Order = 2,
                Options =
                [
                    new FieldOption { Label = "Trolls (2016)", Value = "trolls-2016" },
                    new FieldOption { Label = "Trolls World Tour (2020)", Value = "world-tour-2020" },
                    new FieldOption { Label = "Trolls Band Together (2023)", Value = "band-together-2023" },
                ],
            }, options => options.Excluding(field => field.Id));
        }

        _writer.Verify(writer => writer.AddAsync(It.IsAny<EmployeeType>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmployeeTypeDoesNotExist_ReturnsNullAndDoesNotPersist()
    {
        // Arrange
        Guid employeeTypeId = Guid.NewGuid();
        EmployeeTypeService service = _fixture.Create<EmployeeTypeService>();

        _reader
            .Setup(reader => reader.GetByIdAsync(employeeTypeId))
            .ReturnsAsync((EmployeeType?)null);

        // Act
        EmployeeType? employeeType = await service.UpdateAsync(
            employeeTypeId,
            CreateTrollsTourCommand("Band Together Performer"));

        // Assert
        employeeType.Should().BeNull();

        _writer.Verify(writer => writer.UpdateAsync(It.IsAny<EmployeeType>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmployeeTypeExists_UpdatesEmployeeTypeAndPersistsIt()
    {
        // Arrange
        Guid employeeTypeId = Guid.NewGuid();
        DateTime originalUpdatedDate = DateTime.UtcNow.AddDays(-1);
        EmployeeType existingType = new()
        {
            Id = employeeTypeId,
            Name = "Old Tour Performer",
            Description = "Old description",
            UpdatedDate = originalUpdatedDate,
        };
        EmployeeTypeService service = _fixture.Create<EmployeeTypeService>();

        _reader
            .Setup(reader => reader.GetByIdAsync(employeeTypeId))
            .ReturnsAsync(existingType);

        // Act
        EmployeeType? employeeType = await service.UpdateAsync(
            employeeTypeId,
            CreateTrollsTourCommand("Band Together Performer"));

        // Assert
        using (new AssertionScope())
        {
            employeeType.Should().BeSameAs(existingType);
            existingType.Name.Should().Be("Band Together Performer");
            existingType.Description.Should().Be("Performers grouped by Trolls movie era.");
            existingType.Fields.Should().HaveCount(2);
            existingType.UpdatedDate.Should().BeAfter(originalUpdatedDate);
        }

        _writer.Verify(writer => writer.UpdateAsync(existingType), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenEmployeeTypeDoesNotExist_ReturnsFalseAndDoesNotPersist()
    {
        // Arrange
        Guid employeeTypeId = Guid.NewGuid();
        EmployeeTypeService service = _fixture.Create<EmployeeTypeService>();

        _reader
            .Setup(reader => reader.GetByIdAsync(employeeTypeId))
            .ReturnsAsync((EmployeeType?)null);

        // Act
        bool deleted = await service.DeleteAsync(employeeTypeId);

        // Assert
        deleted.Should().BeFalse();

        _writer.Verify(writer => writer.DeleteAsync(It.IsAny<EmployeeType>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenEmployeeTypeExists_DeletesEmployeeTypeAndReturnsTrue()
    {
        // Arrange
        Guid employeeTypeId = Guid.NewGuid();
        EmployeeType existingType = new() { Id = employeeTypeId, Name = "Trolls (2016)" };
        EmployeeTypeService service = _fixture.Create<EmployeeTypeService>();

        _reader
            .Setup(reader => reader.GetByIdAsync(employeeTypeId))
            .ReturnsAsync(existingType);

        // Act
        bool deleted = await service.DeleteAsync(employeeTypeId);

        // Assert
        deleted.Should().BeTrue();

        _writer.Verify(writer => writer.DeleteAsync(existingType), Times.Once);
    }

    private static CreateEmployeeTypeCommand CreateTrollsTourCommand(string name)
    {
        return new CreateEmployeeTypeCommand(
            name,
            "Performers grouped by Trolls movie era.",
            [
                new CreateEmployeeTypeFieldCommand(
                    "favoriteSongName",
                    "Favorite Song",
                    FieldType.Text,
                    Required: true,
                    Options: [],
                    Order: 1),
                new CreateEmployeeTypeFieldCommand(
                    "movieVersion",
                    "Movie Version",
                    FieldType.Select,
                    Required: true,
                    Options:
                    [
                        new FieldOptionCommand("Trolls (2016)", "trolls-2016"),
                        new FieldOptionCommand("Trolls World Tour (2020)", "world-tour-2020"),
                        new FieldOptionCommand("Trolls Band Together (2023)", "band-together-2023"),
                    ],
                    Order: 2),
            ]);
    }
}
