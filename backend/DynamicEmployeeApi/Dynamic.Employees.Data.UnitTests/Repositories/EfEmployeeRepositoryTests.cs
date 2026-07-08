using System.Text.Json.Nodes;
using Dynamic.Employees.Data.Repositories;
using Dynamic.Employees.Domain.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;

namespace Dynamic.Employees.Data.UnitTests.Repositories;

public class EfEmployeeRepositoryTests
{
    [Fact]
    public async Task AddAsync_WhenEmployeeIsProvided_PersistsEmployee()
    {
        // Arrange
        await using EmployeeDbContext context = CreateContext();
        EmployeeType employeeType = CreateEmployeeType();
        context.EmployeeTypes.Add(employeeType);
        await context.SaveChangesAsync();
        EfEmployeeRepository repository = new(context);
        Employee employee = CreateEmployee(employeeType.Id, "Poppy");

        // Act
        await repository.AddAsync(employee);

        // Assert
        Employee? persisted = await context.Employee.SingleOrDefaultAsync();

        using (new AssertionScope())
        {
            persisted.Should().NotBeNull();
            persisted!.Id.Should().Be(employee.Id);
            persisted.FirstName.Should().Be("Poppy");
            persisted.FieldValues["movieVersion"]!.GetValue<string>().Should().Be("trolls-2016");
        }
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeExists_ReturnsEmployeeWithEmployeeType()
    {
        // Arrange
        await using EmployeeDbContext context = CreateContext();
        EmployeeType employeeType = CreateEmployeeType();
        Employee employee = CreateEmployee(employeeType.Id, "Branch");
        context.EmployeeTypes.Add(employeeType);
        context.Employee.Add(employee);
        await context.SaveChangesAsync();
        EfEmployeeRepository repository = new(context);

        // Act
        Employee? result = await repository.GetByIdAsync(employee.Id);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Id.Should().Be(employee.Id);
            result.FirstName.Should().Be("Branch");
            result.EmployeeType.Should().NotBeNull();
            result.EmployeeType!.Id.Should().Be(employeeType.Id);
            result.EmployeeType.Fields.Should().Contain(field => field.Name == "movieVersion");
        }
    }

    [Fact]
    public async Task UpdateFieldAsync_WhenEmployeeDoesNotExist_ReturnsFalse()
    {
        // Arrange
        await using EmployeeDbContext context = CreateContext();
        EfEmployeeRepository repository = new(context);

        // Act
        bool updated = await repository.UpdateFieldAsync(
            Guid.NewGuid(),
            "movieVersion",
            JsonValue.Create("world-tour-2020"));

        // Assert
        updated.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateFieldAsync_WhenEmployeeExists_UpdatesFieldValueAndReturnsTrue()
    {
        // Arrange
        await using EmployeeDbContext context = CreateContext();
        EmployeeType employeeType = CreateEmployeeType();
        Employee employee = CreateEmployee(employeeType.Id, "Viva");
        DateTime originalUpdatedDate = DateTime.UtcNow.AddDays(-1);
        employee.UpdatedDate = originalUpdatedDate;
        context.EmployeeTypes.Add(employeeType);
        context.Employee.Add(employee);
        await context.SaveChangesAsync();
        EfEmployeeRepository repository = new(context);

        // Act
        bool updated = await repository.UpdateFieldAsync(
            employee.Id,
            "movieVersion",
            JsonValue.Create("band-together-2023"));

        // Assert
        Employee persisted = await context.Employee.SingleAsync();

        using (new AssertionScope())
        {
            updated.Should().BeTrue();
            persisted.FieldValues["movieVersion"]!.GetValue<string>().Should().Be("band-together-2023");
            persisted.UpdatedDate.Should().BeAfter(originalUpdatedDate);
        }
    }

    private static EmployeeDbContext CreateContext()
    {
        DbContextOptions<EmployeeDbContext> options = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EmployeeDbContext(options);
    }

    private static EmployeeType CreateEmployeeType()
    {
        return new EmployeeType
        {
            Id = Guid.NewGuid(),
            Name = "Trolls Tour Performer",
            Fields =
            [
                new EmployeeTypeField
                {
                    Id = Guid.NewGuid(),
                    Name = "movieVersion",
                    Label = "Movie Version",
                    Options =
                    [
                        new FieldOption { Label = "Trolls (2016)", Value = "trolls-2016" },
                        new FieldOption { Label = "Trolls World Tour (2020)", Value = "world-tour-2020" },
                        new FieldOption { Label = "Trolls Band Together (2023)", Value = "band-together-2023" },
                    ],
                },
            ],
        };
    }

    private static Employee CreateEmployee(Guid employeeTypeId, string firstName)
    {
        return new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = "Troll",
            Email = $"{firstName.ToLowerInvariant()}@trolls.example",
            HireDate = new DateOnly(2016, 11, 4),
            Department = "Pop Village",
            EmployeeTypeId = employeeTypeId,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            FieldValues = new JsonObject
            {
                ["movieVersion"] = "trolls-2016",
            },
        };
    }
}
