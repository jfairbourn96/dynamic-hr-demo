using Dynamic.Employees.Data.Repositories;
using Dynamic.Employees.Domain.Enums;
using Dynamic.Employees.Domain.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;

namespace Dynamic.Employees.Data.UnitTests.Repositories;

public class EfEmployeeTypeRepositoryTests
{
    [Fact]
    public async Task AddAsync_WhenEmployeeTypeIsProvided_PersistsEmployeeType()
    {
        // Arrange
        await using EmployeeDbContext context = CreateContext();
        EfEmployeeTypeRepository repository = new(context);
        EmployeeType employeeType = CreateTrollsTourPerformer();

        // Act
        await repository.AddAsync(employeeType);

        // Assert
        EmployeeType? persisted = await context.EmployeeTypes.SingleOrDefaultAsync();

        using (new AssertionScope())
        {
            persisted.Should().NotBeNull();
            persisted!.Id.Should().Be(employeeType.Id);
            persisted.Name.Should().Be("Trolls Tour Performer");
            persisted.Fields.Should().Contain(field => field.Name == "movieVersion");
        }
    }

    [Fact]
    public async Task GetAllAsync_WhenEmployeeTypesExist_ReturnsAllEmployeeTypes()
    {
        // Arrange
        await using EmployeeDbContext context = CreateContext();
        EmployeeType first = CreateTrollsTourPerformer("Trolls (2016)");
        EmployeeType second = CreateTrollsTourPerformer("Trolls World Tour (2020)");
        context.EmployeeTypes.AddRange(first, second);
        await context.SaveChangesAsync();
        EfEmployeeTypeRepository repository = new(context);

        // Act
        List<EmployeeType> employeeTypes = await repository.GetAllAsync();

        // Assert
        employeeTypes.Should().BeEquivalentTo(
            [first, second],
            options => options.ComparingByMembers<EmployeeType>());
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeTypeExists_ReturnsEmployeeType()
    {
        // Arrange
        await using EmployeeDbContext context = CreateContext();
        EmployeeType expected = CreateTrollsTourPerformer("Trolls Band Together (2023)");
        context.EmployeeTypes.Add(expected);
        await context.SaveChangesAsync();
        EfEmployeeTypeRepository repository = new(context);

        // Act
        EmployeeType? employeeType = await repository.GetByIdAsync(expected.Id);

        // Assert
        employeeType.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmployeeTypeChanged_PersistsChanges()
    {
        // Arrange
        await using EmployeeDbContext context = CreateContext();
        EmployeeType employeeType = CreateTrollsTourPerformer("Trolls (2016)");
        context.EmployeeTypes.Add(employeeType);
        await context.SaveChangesAsync();
        EfEmployeeTypeRepository repository = new(context);

        employeeType.Name = "Trolls World Tour (2020)";
        employeeType.Fields.Add(new EmployeeTypeField
        {
            Id = Guid.NewGuid(),
            Name = "tourStop",
            Label = "Tour Stop",
            FieldType = FieldType.Text,
            Order = 3,
        });

        // Act
        await repository.UpdateAsync(employeeType);

        // Assert
        EmployeeType persisted = await context.EmployeeTypes.SingleAsync();

        using (new AssertionScope())
        {
            persisted.Name.Should().Be("Trolls World Tour (2020)");
            persisted.Fields.Should().Contain(field => field.Name == "tourStop");
        }
    }

    [Fact]
    public async Task DeleteAsync_WhenEmployeeTypeExists_RemovesEmployeeType()
    {
        // Arrange
        await using EmployeeDbContext context = CreateContext();
        EmployeeType employeeType = CreateTrollsTourPerformer();
        context.EmployeeTypes.Add(employeeType);
        await context.SaveChangesAsync();
        EfEmployeeTypeRepository repository = new(context);

        // Act
        await repository.DeleteAsync(employeeType);

        // Assert
        (await context.EmployeeTypes.AnyAsync()).Should().BeFalse();
    }

    private static EmployeeDbContext CreateContext()
    {
        DbContextOptions<EmployeeDbContext> options = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EmployeeDbContext(options);
    }

    private static EmployeeType CreateTrollsTourPerformer(string name = "Trolls Tour Performer")
    {
        return new EmployeeType
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "Performers grouped by Trolls movie era.",
            Fields =
            [
                new EmployeeTypeField
                {
                    Id = Guid.NewGuid(),
                    Name = "movieVersion",
                    Label = "Movie Version",
                    FieldType = FieldType.Select,
                    Required = true,
                    Options =
                    [
                        new FieldOption { Label = "Trolls (2016)", Value = "trolls-2016" },
                        new FieldOption { Label = "Trolls World Tour (2020)", Value = "world-tour-2020" },
                        new FieldOption { Label = "Trolls Band Together (2023)", Value = "band-together-2023" },
                    ],
                    Order = 1,
                },
            ],
        };
    }
}
