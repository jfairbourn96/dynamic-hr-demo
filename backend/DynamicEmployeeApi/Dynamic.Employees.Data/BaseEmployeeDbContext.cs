using Dynamic.Employees.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamic.Employees.Data;

/// <summary>
/// Defines the shared EF Core model for employee database contexts.
/// </summary>
public abstract class BaseEmployeeDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<EmployeeType> EmployeeTypes => Set<EmployeeType>();
    public DbSet<Employee> Employee => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseEmployeeDbContext).Assembly);
    }
}
