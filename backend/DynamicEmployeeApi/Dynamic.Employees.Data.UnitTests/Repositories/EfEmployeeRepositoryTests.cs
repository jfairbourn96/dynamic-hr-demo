using System.Text.Json.Nodes;
using Dynamic.Employees.Data.Repositories;
using Dynamic.Employees.Application.Models;
using Dynamic.Employees.Domain.Models;
using Dynamic.Json.Search;
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
        Employee? persisted = await context.Employees.SingleOrDefaultAsync();

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
        context.Employees.Add(employee);
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
    public async Task GetByIdAsync_WhenEmployeeDoesNotExist_ReturnsNull()
    {
        await using EmployeeDbContext context = CreateContext();
        EfEmployeeRepository repository = new(context);

        Employee? result = await repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenDetachedEmployeeIsProvided_PersistsOnlyEmployeeChanges()
    {
        // Arrange
        await using EmployeeDbContext context = CreateContext();
        EmployeeType employeeType = CreateEmployeeType();
        Employee employee = CreateEmployee(employeeType.Id, "Poppy");
        context.EmployeeTypes.Add(employeeType);
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        employee.FirstName = "Viva";
        employee.FieldValues["movieVersion"] = "band-together-2023";
        EfEmployeeRepository repository = new(context);

        // Act
        await repository.UpdateAsync(employee);
        context.ChangeTracker.Clear();

        // Assert
        Employee persisted = await context.Employees.SingleAsync();
        using (new AssertionScope())
        {
            persisted.FirstName.Should().Be("Viva");
            persisted.FieldValues["movieVersion"]!.GetValue<string>()
                .Should().Be("band-together-2023");
            (await context.EmployeeTypes.SingleAsync()).Name.Should().Be("Trolls Tour Performer");
        }
    }

    [Fact]
    public async Task SearchAsync_WhenNoFiltersAreProvided_ReturnsRequestedPageAndTotalCount()
    {
        // Arrange
        await using EmployeeDbContext context = CreateContext();
        EmployeeType employeeType = CreateEmployeeType();
        Employee first = CreateEmployee(employeeType.Id, "Poppy");
        Employee second = CreateEmployee(employeeType.Id, "Branch");
        Employee third = CreateEmployee(employeeType.Id, "Viva");
        context.EmployeeTypes.Add(employeeType);
        context.Employees.AddRange(first, second, third);
        await context.SaveChangesAsync();
        EfEmployeeRepository repository = new(context);

        EmployeeSearchCriteria criteria = new(
            EmployeeTypeId: null,
            TextFilters: [],
            Email: null,
            HireDateStart: null,
            HireDateEnd: null,
            DynamicFilters: [],
            PageNumber: 2,
            PageSize: 2);

        // Act
        EmployeeSearchResult result = await repository.SearchAsync(criteria);

        // Assert
        using (new AssertionScope())
        {
            result.TotalCount.Should().Be(3);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(2);
            result.Items.Should().ContainSingle();
            result.Items.Single().EmployeeType.Should().NotBeNull();
            result.Items.Single().EmployeeType!.Fields.Should().Contain(field => field.Name == "movieVersion");
            result.Items.Single().FieldValues["movieVersion"]!.GetValue<string>().Should().Be("trolls-2016");
        }
    }

    [Fact]
    public async Task SearchAsync_WhenCoreFiltersAreProvided_ReturnsMatchingEmployees()
    {
        // Arrange
        await using EmployeeDbContext context = CreateContext();
        EmployeeType employeeType = CreateEmployeeType();
        Employee poppy = CreateEmployee(employeeType.Id, "Poppy");
        poppy.LastName = "Popstar";
        poppy.Email = "poppy@trolls.example";
        poppy.HireDate = new DateOnly(2016, 11, 4);

        Employee branch = CreateEmployee(employeeType.Id, "Branch");
        branch.LastName = "Survivalist";
        branch.Email = "branch@trolls.example";
        branch.HireDate = new DateOnly(2023, 11, 17);

        context.EmployeeTypes.Add(employeeType);
        context.Employees.AddRange(poppy, branch);
        await context.SaveChangesAsync();
        EfEmployeeRepository repository = new(context);

        EmployeeSearchCriteria criteria = new(
            employeeType.Id,
            [new EmployeeTextSearchFilter("firstName", SearchOperator.Exact, "Poppy")],
            "poppy@",
            new DateOnly(2016, 1, 1),
            new DateOnly(2016, 12, 31),
            [],
            PageNumber: 1,
            PageSize: 10);

        // Act
        EmployeeSearchResult result = await repository.SearchAsync(criteria);

        // Assert
        using (new AssertionScope())
        {
            result.TotalCount.Should().Be(1);
            result.Items.Should().ContainSingle();
            result.Items.Single().Id.Should().Be(poppy.Id);
            result.Items.Single().FirstName.Should().Be("Poppy");
        }
    }

    [Fact]
    public async Task SearchAsync_WhenCoreTextFiltersUseStartsWithAndContains_ReturnsMatchingEmployees()
    {
        // Arrange
        await using EmployeeDbContext context = CreateContext();
        EmployeeType employeeType = CreateEmployeeType();
        Employee poppy = CreateEmployee(employeeType.Id, "Poppy");
        poppy.LastName = "Popstar";
        poppy.Department = "Pop Village";

        Employee branch = CreateEmployee(employeeType.Id, "Branch");
        branch.LastName = "Survivalist";
        branch.Department = "Lonesome Flats";

        context.EmployeeTypes.Add(employeeType);
        context.Employees.AddRange(poppy, branch);
        await context.SaveChangesAsync();
        EfEmployeeRepository repository = new(context);

        EmployeeSearchCriteria criteria = new(
            EmployeeTypeId: null,
            TextFilters:
            [
                new EmployeeTextSearchFilter("lastName", SearchOperator.StartsWith, "Pop"),
                new EmployeeTextSearchFilter("department", SearchOperator.Contains, "Village"),
            ],
            Email: null,
            HireDateStart: null,
            HireDateEnd: null,
            DynamicFilters: [],
            PageNumber: 1,
            PageSize: 10);

        // Act
        EmployeeSearchResult result = await repository.SearchAsync(criteria);

        // Assert
        using (new AssertionScope())
        {
            result.TotalCount.Should().Be(1);
            result.Items.Should().ContainSingle();
            result.Items.Single().Id.Should().Be(poppy.Id);
        }
    }

    [Fact]
    public async Task SearchAsync_WhenEmployeeTypeIsProvided_ExcludesOtherEmployeeTypes()
    {
        // Arrange
        await using EmployeeDbContext context = CreateContext();
        EmployeeType requestedType = CreateEmployeeType();
        EmployeeType otherType = CreateEmployeeType();
        otherType.Name = "Other Type";
        Employee expected = CreateEmployee(requestedType.Id, "Poppy");
        context.EmployeeTypes.AddRange(requestedType, otherType);
        context.Employees.AddRange(expected, CreateEmployee(otherType.Id, "Branch"));
        await context.SaveChangesAsync();
        EfEmployeeRepository repository = new(context);
        EmployeeSearchCriteria criteria = new(
            requestedType.Id, [], null, null, null, [], PageNumber: 1, PageSize: 20);

        // Act
        EmployeeSearchResult result = await repository.SearchAsync(criteria);

        // Assert
        result.Items.Should().ContainSingle(item => item.Id == expected.Id);
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
