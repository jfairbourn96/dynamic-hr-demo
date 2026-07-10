using Microsoft.EntityFrameworkCore;

namespace Dynamic.Employees.Data;

/// <summary>
/// Provides the SQL Server-backed employee database context.
/// </summary>
public class EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) 
    : BaseEmployeeDbContext(options)
{
}
