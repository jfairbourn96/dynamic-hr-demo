using Dynamic.Employees.Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Dynamic.Employees.Data.UnitTests.Configurations;

public class EmployeeConfigurationTests
{
    [Fact]
    public void Model_WhenBuilt_ConfiguresEmployeeTablePropertiesRelationshipAndIndexes()
    {
        using EmployeeDbContext context = CreateContext();

        IEntityType entity = context.Model.FindEntityType(typeof(Employee))!;

        entity.GetTableName().Should().Be(nameof(Employee));
        entity.FindPrimaryKey()!.Properties.Should().ContainSingle(property => property.Name == nameof(Employee.Id));
        entity.FindProperty(nameof(Employee.FirstName))!.GetMaxLength().Should().Be(50);
        entity.FindProperty(nameof(Employee.LastName))!.GetMaxLength().Should().Be(50);
        entity.FindProperty(nameof(Employee.Email))!.GetMaxLength().Should().Be(200);
        entity.FindProperty(nameof(Employee.Department))!.GetMaxLength().Should().Be(100);
        entity.FindProperty(nameof(Employee.FieldValues))!.GetColumnName().Should().Be("FieldValuesJson");
        entity.GetIndexes().SelectMany(index => index.Properties).Select(property => property.Name)
            .Should().Contain([
                nameof(Employee.Email),
                nameof(Employee.FirstName),
                nameof(Employee.LastName),
                nameof(Employee.EmployeeTypeId),
            ]);
        entity.GetForeignKeys().Should().ContainSingle(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(EmployeeType) &&
            foreignKey.DeleteBehavior == DeleteBehavior.Restrict);
    }

    private static EmployeeDbContext CreateContext()
    {
        DbContextOptions<EmployeeDbContext> options = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new EmployeeDbContext(options);
    }
}
