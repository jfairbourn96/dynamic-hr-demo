using Dynamic.Employees.Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Dynamic.Employees.Data.UnitTests.Configurations;

public class EmployeeTypeConfigurationTests
{
    [Fact]
    public void Model_WhenBuilt_ConfiguresEmployeeTypeAndOwnedJsonSchema()
    {
        using EmployeeDbContext context = CreateContext();

        IEntityType entity = context.Model.FindEntityType(typeof(EmployeeType))!;
        IEntityType fieldEntity = context.Model.GetEntityTypes()
            .Single(type => type.ClrType == typeof(EmployeeTypeField));
        IEntityType optionEntity = context.Model.GetEntityTypes()
            .Single(type => type.ClrType == typeof(FieldOption));

        entity.GetTableName().Should().Be(nameof(EmployeeType));
        entity.FindProperty(nameof(EmployeeType.Name))!.GetMaxLength().Should().Be(100);
        entity.FindProperty(nameof(EmployeeType.Description))!.GetMaxLength().Should().Be(500);
        fieldEntity.IsOwned().Should().BeTrue();
        optionEntity.IsOwned().Should().BeTrue();
        fieldEntity.GetContainerColumnName().Should().Be("FieldsJson");
    }

    private static EmployeeDbContext CreateContext()
    {
        DbContextOptions<EmployeeDbContext> options = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new EmployeeDbContext(options);
    }
}
