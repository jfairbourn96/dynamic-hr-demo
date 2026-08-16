using System.Text.Json.Nodes;
using Dynamic.Employees.Data.PostgreSql;
using Dynamic.Employees.Data.Repositories;
using Dynamic.Employees.Domain.Models;
using Dynamic.Json.EfCore.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Dynamic.Employees.Data.IntegrationTests;

[Collection(PostgreSqlContainerCollection.Name)]
public sealed class EmployeeRepositoryPostgreSqlIntegrationTests(
    PostgreSqlContainerFixture fixture)
{
    [Fact]
    public async Task Repositories_RoundTripEmployeeDataThroughPostgreSqlJsonb()
    {
        await using PostgreSqlEmployeeDbContext context = CreateContext();
        await context.Database.MigrateAsync();
        EmployeeType employeeType = new()
        {
            Id = Guid.NewGuid(),
            Name = "PostgreSQL Employee",
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
        };
        Employee employee = new()
        {
            Id = Guid.NewGuid(),
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.test",
            HireDate = new DateOnly(2026, 1, 1),
            EmployeeTypeId = employeeType.Id,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            FieldValues = new JsonObject { ["role"] = "Engineer" },
        };

        await new EfEmployeeTypeRepository(context).AddAsync(employeeType);
        await new EfEmployeeRepository(context).AddAsync(employee);

        await using PostgreSqlEmployeeDbContext reload = CreateContext();
        Employee? persisted = await new EfEmployeeRepository(reload).GetByIdAsync(employee.Id);
        string? storeType = reload.Model.FindEntityType(typeof(Employee))!
            .FindProperty(nameof(Employee.FieldValues))!.GetColumnType();

        persisted.Should().NotBeNull();
        persisted!.FieldValues["role"]!.GetValue<string>().Should().Be("Engineer");
        storeType.Should().Be("jsonb");
    }

    private PostgreSqlEmployeeDbContext CreateContext()
    {
        DbContextOptionsBuilder<PostgreSqlEmployeeDbContext> builder = new();
        builder.UseNpgsql(fixture.ConnectionString);
        builder.UseDynamicJsonPostgreSql();
        return new PostgreSqlEmployeeDbContext(builder.Options);
    }
}
