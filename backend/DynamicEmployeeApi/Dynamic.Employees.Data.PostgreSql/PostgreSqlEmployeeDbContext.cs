using Dynamic.Employees.Data;
using Dynamic.Employees.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamic.Employees.Data.PostgreSql;

/// <summary>Provides the PostgreSQL-backed employee database context.</summary>
public sealed class PostgreSqlEmployeeDbContext(
    DbContextOptions<PostgreSqlEmployeeDbContext> options) : BaseEmployeeDbContext(options)
{
    protected override void ConfigureProviderModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>()
            .Property(employee => employee.FieldValues)
            .HasColumnType("jsonb");
    }
}
