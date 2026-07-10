using System.Text.Json.Nodes;
using Dynamic.Employees.Application.Models;
using Dynamic.Employees.Data.Extensions;
using Dynamic.Employees.Data.Repositories;
using Dynamic.Employees.Domain.Enums;
using Dynamic.Employees.Domain.Models;
using Dynamic.Json.EfCore.SqlServer;
using Dynamic.Json.Search;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamic.Employees.Data.IntegrationTests;

[Collection(SqlServerContainerCollection.Name)]
public sealed class EmployeeRepositorySqlServerIntegrationTests
{
    private readonly SqlServerContainerFixture _fixture;

    public EmployeeRepositorySqlServerIntegrationTests(SqlServerContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task EmployeeTypeRepository_RoundTripsOwnedJsonFieldsAndOptions()
    {
        await using EmployeeDbContext context = CreateContext();
        await context.Database.MigrateAsync();
        EfEmployeeTypeRepository repository = new(context);
        EmployeeType employeeType = CreateEmployeeType();

        await repository.AddAsync(employeeType);

        await using EmployeeDbContext reloadContext = CreateContext(context.Database.GetConnectionString()!);
        EfEmployeeTypeRepository reloadRepository = new(reloadContext);
        EmployeeType? persisted = await reloadRepository.GetByIdAsync(employeeType.Id);

        using (new AssertionScope())
        {
            persisted.Should().NotBeNull();
            persisted!.Name.Should().Be("Bluey Character");
            persisted.Fields.Should().HaveCount(5);
            persisted.Fields.Should().Contain(field => field.Name == "favoriteGame" && field.FieldType == FieldType.Text);
            persisted.Fields.Should().Contain(field => field.Name == "badgeCount" && field.FieldType == FieldType.Number);
            persisted.Fields.Should().Contain(field => field.Name == "firstEpisodeDate" && field.FieldType == FieldType.Date);
            persisted.Fields.Should().Contain(field => field.Name == "isHeeler" && field.FieldType == FieldType.Boolean);
            persisted.Fields.Single(field => field.Name == "familyRole").Options.Should().Contain(option => option.Value == "child");
        }
    }

    [Fact]
    public async Task EmployeeRepository_RoundTripsJsonFieldValues()
    {
        await using EmployeeDbContext context = CreateContext();
        await context.Database.MigrateAsync();
        EmployeeType employeeType = CreateEmployeeType();
        Employee employee = CreateEmployee(employeeType.Id, "Bluey", "Heeler", "bluey@heeler.example", "Heeler House", new JsonObject
        {
            ["favoriteGame"] = "Keepy Uppy",
            ["badgeCount"] = 8,
            ["firstEpisodeDate"] = "2018-10-01",
            ["isHeeler"] = true,
            ["familyRole"] = "child",
        });

        context.EmployeeTypes.Add(employeeType);
        await context.SaveChangesAsync();
        EfEmployeeRepository repository = new(context);

        await repository.AddAsync(employee);

        await using EmployeeDbContext reloadContext = CreateContext(context.Database.GetConnectionString()!);
        EfEmployeeRepository reloadRepository = new(reloadContext);
        Employee? persisted = await reloadRepository.GetByIdAsync(employee.Id);

        using (new AssertionScope())
        {
            persisted.Should().NotBeNull();
            persisted!.FirstName.Should().Be("Bluey");
            persisted.EmployeeType.Should().NotBeNull();
            persisted.EmployeeType!.Fields.Should().Contain(field => field.Name == "favoriteGame");
            persisted.FieldValues["favoriteGame"]!.GetValue<string>().Should().Be("Keepy Uppy");
            persisted.FieldValues["badgeCount"]!.GetValue<int>().Should().Be(8);
            persisted.FieldValues["firstEpisodeDate"]!.GetValue<string>().Should().Be("2018-10-01");
            persisted.FieldValues["isHeeler"]!.GetValue<bool>().Should().BeTrue();
            persisted.FieldValues["familyRole"]!.GetValue<string>().Should().Be("child");
        }
    }

    [Fact]
    public async Task EmployeeRepository_SearchAsync_AppliesCoreAndDynamicFiltersAgainstSqlServer()
    {
        string connectionString = CreateDatabaseConnectionString();
        ServiceCollection services = new();
        services.RegisterEmployeeDataServices(connectionString);

        await using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        EmployeeDbContext context = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
        await context.Database.MigrateAsync();

        EfEmployeeTypeRepository employeeTypeRepository = scope.ServiceProvider.GetRequiredService<EfEmployeeTypeRepository>();
        EfEmployeeRepository employeeRepository = scope.ServiceProvider.GetRequiredService<EfEmployeeRepository>();

        EmployeeType employeeType = CreateEmployeeType();
        await employeeTypeRepository.AddAsync(employeeType);

        await employeeRepository.AddAsync(CreateEmployee(employeeType.Id, "Bluey", "Heeler", "bluey@heeler.example", "Heeler House", new JsonObject
        {
            ["favoriteGame"] = "Keepy Uppy",
            ["badgeCount"] = 8,
            ["firstEpisodeDate"] = "2018-10-01",
            ["isHeeler"] = true,
            ["familyRole"] = "child",
        }));
        await employeeRepository.AddAsync(CreateEmployee(employeeType.Id, "Bingo", "Heeler", "bingo@heeler.example", "Heeler House", new JsonObject
        {
            ["favoriteGame"] = "Magic Xylophone",
            ["badgeCount"] = 5,
            ["firstEpisodeDate"] = "2018-10-01",
            ["isHeeler"] = true,
            ["familyRole"] = "child",
        }));
        await employeeRepository.AddAsync(CreateEmployee(employeeType.Id, "Chilli", "Heeler", "chilli@heeler.example", "Airport Security", new JsonObject
        {
            ["favoriteGame"] = "Keepy Uppy",
            ["badgeCount"] = 9,
            ["firstEpisodeDate"] = "2018-10-01",
            ["isHeeler"] = true,
            ["familyRole"] = "parent",
        }));
        await employeeRepository.AddAsync(CreateEmployee(employeeType.Id, "Muffin", "Heeler", "muffin@heeler.example", "Stripe's House", new JsonObject
        {
            ["favoriteGame"] = "Grannies",
            ["badgeCount"] = 3,
            ["firstEpisodeDate"] = "2020-10-01",
            ["isHeeler"] = false,
            ["familyRole"] = "child",
        }));

        EmployeeSearchCriteria criteria = new(
            employeeType.Id,
            [new EmployeeTextSearchFilter("department", SearchOperator.Exact, "Heeler House")],
            "@heeler.example",
            new DateOnly(2018, 1, 1),
            new DateOnly(2019, 12, 31),
            [
                new DynamicSearchFilter("favoriteGame", DynamicSearchFieldType.Text, SearchOperator.Contains, "Keepy"),
                new DynamicSearchFilter("badgeCount", DynamicSearchFieldType.Number, SearchOperator.GreaterThanOrEqual, "7"),
                new DynamicSearchFilter("firstEpisodeDate", DynamicSearchFieldType.Date, SearchOperator.StartDate, "2018-10-01"),
                new DynamicSearchFilter("isHeeler", DynamicSearchFieldType.Boolean, SearchOperator.Exact, "true"),
                new DynamicSearchFilter("familyRole", DynamicSearchFieldType.Select, SearchOperator.Exact, "child"),
            ],
            PageNumber: 1,
            PageSize: 10);

        EmployeeSearchResult result = await employeeRepository.SearchAsync(criteria);

        using (new AssertionScope())
        {
            result.TotalCount.Should().Be(1);
            result.Items.Should().ContainSingle();
            result.Items.Single().FirstName.Should().Be("Bluey");
            result.Items.Single().EmployeeType.Should().NotBeNull();
            result.Items.Single().FieldValues["favoriteGame"]!.GetValue<string>().Should().Be("Keepy Uppy");
        }
    }

    [Fact]
    public async Task EmployeeRepository_SearchAsync_TranslatesRemainingDynamicOperatorsAgainstSqlServer()
    {
        await using EmployeeDbContext context = CreateContext();
        await context.Database.MigrateAsync();
        EmployeeType employeeType = CreateEmployeeType();
        context.EmployeeTypes.Add(employeeType);
        await context.SaveChangesAsync();
        EfEmployeeRepository repository = new(context);

        await repository.AddAsync(CreateEmployee(employeeType.Id, "Bluey", "Heeler", "bluey@heeler.example", "Heeler House", new JsonObject
        {
            ["favoriteGame"] = "Keepy Uppy", ["badgeCount"] = 8, ["firstEpisodeDate"] = "2018-10-01", ["isHeeler"] = true, ["familyRole"] = "child",
        }));
        await repository.AddAsync(CreateEmployee(employeeType.Id, "Bingo", "Heeler", "bingo@heeler.example", "Heeler House", new JsonObject
        {
            ["favoriteGame"] = "Magic Xylophone", ["badgeCount"] = 5, ["firstEpisodeDate"] = "2018-10-01", ["isHeeler"] = true, ["familyRole"] = "child",
        }));
        await repository.AddAsync(CreateEmployee(employeeType.Id, "Chilli", "Heeler", "chilli@heeler.example", "Airport Security", new JsonObject
        {
            ["favoriteGame"] = "Keepy Uppy", ["badgeCount"] = 9, ["firstEpisodeDate"] = "2018-10-01", ["isHeeler"] = true, ["familyRole"] = "parent",
        }));
        await repository.AddAsync(CreateEmployee(employeeType.Id, "Muffin", "Heeler", "muffin@heeler.example", "Stripe's House", new JsonObject
        {
            ["favoriteGame"] = "Grannies", ["badgeCount"] = 3, ["firstEpisodeDate"] = "2020-10-01", ["isHeeler"] = false, ["familyRole"] = "child",
        }));

        EmployeeSearchResult startsWith = await SearchAsync(repository, employeeType.Id,
            new DynamicSearchFilter("favoriteGame", DynamicSearchFieldType.Text, SearchOperator.StartsWith, "Keep"));
        EmployeeSearchResult exact = await SearchAsync(repository, employeeType.Id,
            new DynamicSearchFilter("favoriteGame", DynamicSearchFieldType.Text, SearchOperator.Exact, "Magic Xylophone"));
        EmployeeSearchResult lessThan = await SearchAsync(repository, employeeType.Id,
            new DynamicSearchFilter("badgeCount", DynamicSearchFieldType.Number, SearchOperator.LessThan, "6"));
        EmployeeSearchResult lessThanOrEqual = await SearchAsync(repository, employeeType.Id,
            new DynamicSearchFilter("badgeCount", DynamicSearchFieldType.Number, SearchOperator.LessThanOrEqual, "5"));
        EmployeeSearchResult greaterThan = await SearchAsync(repository, employeeType.Id,
            new DynamicSearchFilter("badgeCount", DynamicSearchFieldType.Number, SearchOperator.GreaterThan, "8"));
        EmployeeSearchResult equal = await SearchAsync(repository, employeeType.Id,
            new DynamicSearchFilter("badgeCount", DynamicSearchFieldType.Number, SearchOperator.Exact, "8"));
        EmployeeSearchResult endDate = await SearchAsync(repository, employeeType.Id,
            new DynamicSearchFilter("firstEpisodeDate", DynamicSearchFieldType.Date, SearchOperator.EndDate, "2018-12-31"));
        EmployeeSearchResult falseValue = await SearchAsync(repository, employeeType.Id,
            new DynamicSearchFilter("isHeeler", DynamicSearchFieldType.Boolean, SearchOperator.Exact, "false"));
        EmployeeSearchResult parent = await SearchAsync(repository, employeeType.Id,
            new DynamicSearchFilter("familyRole", DynamicSearchFieldType.Select, SearchOperator.Exact, "parent"));

        startsWith.Items.Select(item => item.FirstName).Should().BeEquivalentTo(["Bluey", "Chilli"]);
        exact.Items.Select(item => item.FirstName).Should().Equal("Bingo");
        lessThan.Items.Select(item => item.FirstName).Should().BeEquivalentTo(["Bingo", "Muffin"]);
        lessThanOrEqual.Items.Select(item => item.FirstName).Should().BeEquivalentTo(["Bingo", "Muffin"]);
        greaterThan.Items.Select(item => item.FirstName).Should().Equal("Chilli");
        equal.Items.Select(item => item.FirstName).Should().Equal("Bluey");
        endDate.Items.Select(item => item.FirstName).Should().BeEquivalentTo(["Bluey", "Bingo", "Chilli"]);
        falseValue.Items.Select(item => item.FirstName).Should().Equal("Muffin");
        parent.Items.Select(item => item.FirstName).Should().Equal("Chilli");
    }

    private static Task<EmployeeSearchResult> SearchAsync(
        EfEmployeeRepository repository,
        Guid employeeTypeId,
        DynamicSearchFilter filter)
    {
        return repository.SearchAsync(new EmployeeSearchCriteria(
            employeeTypeId,
            [],
            null,
            null,
            null,
            [filter],
            PageNumber: 1,
            PageSize: 20));
    }

    private EmployeeDbContext CreateContext()
        => CreateContext(CreateDatabaseConnectionString());

    private static EmployeeDbContext CreateContext(string connectionString)
    {
        DbContextOptionsBuilder<EmployeeDbContext> builder = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseSqlServer(connectionString);

        builder.UseDynamicJsonSqlServer();

        return new EmployeeDbContext(builder.Options);
    }

    private string CreateDatabaseConnectionString()
    {
        SqlConnectionStringBuilder builder = new(_fixture.ConnectionString)
        {
            InitialCatalog = $"DynamicHrTests_{Guid.NewGuid():N}",
            TrustServerCertificate = true,
        };

        return builder.ConnectionString;
    }

    private static EmployeeType CreateEmployeeType()
    {
        return new EmployeeType
        {
            Id = Guid.NewGuid(),
            Name = "Bluey Character",
            Description = "Bluey-themed integration test employee type.",
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            Fields =
            [
                new EmployeeTypeField
                {
                    Id = Guid.NewGuid(),
                    Name = "favoriteGame",
                    Label = "Favorite Game",
                    FieldType = FieldType.Text,
                    Order = 1,
                },
                new EmployeeTypeField
                {
                    Id = Guid.NewGuid(),
                    Name = "badgeCount",
                    Label = "Badge Count",
                    FieldType = FieldType.Number,
                    Order = 2,
                },
                new EmployeeTypeField
                {
                    Id = Guid.NewGuid(),
                    Name = "firstEpisodeDate",
                    Label = "First Episode Date",
                    FieldType = FieldType.Date,
                    Order = 3,
                },
                new EmployeeTypeField
                {
                    Id = Guid.NewGuid(),
                    Name = "isHeeler",
                    Label = "Is Heeler",
                    FieldType = FieldType.Boolean,
                    Order = 4,
                },
                new EmployeeTypeField
                {
                    Id = Guid.NewGuid(),
                    Name = "familyRole",
                    Label = "Family Role",
                    FieldType = FieldType.Select,
                    Options =
                    [
                        new FieldOption { Label = "Child", Value = "child" },
                        new FieldOption { Label = "Parent", Value = "parent" },
                    ],
                    Order = 5,
                },
            ],
        };
    }

    private static Employee CreateEmployee(
        Guid employeeTypeId,
        string firstName,
        string lastName,
        string email,
        string department,
        JsonObject fieldValues)
    {
        return new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            HireDate = new DateOnly(2018, 10, 1),
            Department = department,
            EmployeeTypeId = employeeTypeId,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            FieldValues = fieldValues,
        };
    }
}
