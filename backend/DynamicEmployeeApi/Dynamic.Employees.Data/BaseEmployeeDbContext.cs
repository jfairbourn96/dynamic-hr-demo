using Dynamic.Employees.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Dynamic.Employees.Data;

/// <summary>
/// Defines the shared EF Core model for employee database contexts.
/// </summary>
/// <remarks>
/// Depending on this base context lets repositories share one model while the concrete context
/// retains ownership of provider configuration and migrations.
/// </remarks>
public abstract class BaseEmployeeDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<EmployeeType> EmployeeTypes => Set<EmployeeType>();
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseEmployeeDbContext).Assembly);
        ConfigureProviderModel(modelBuilder);
    }

    /// <summary>Applies database-provider-specific model configuration.</summary>
    protected virtual void ConfigureProviderModel(ModelBuilder modelBuilder)
    {
    }
}
