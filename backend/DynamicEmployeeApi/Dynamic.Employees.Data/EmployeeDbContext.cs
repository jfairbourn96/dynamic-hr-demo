using Microsoft.EntityFrameworkCore;

namespace Dynamic.Employees.Data;

/// <summary>
/// Provides a provider-neutral employee context for tests and consumer-supplied EF Core options.
/// </summary>
public class EmployeeDbContext(DbContextOptions<EmployeeDbContext> options)
    : BaseEmployeeDbContext(options);
