using Dynamic.Employees.Data;
using Microsoft.EntityFrameworkCore;

namespace Dynamic.Employees.Data.SqlServer;

/// <summary>Provides the SQL Server-backed employee database context.</summary>
public sealed class SqlServerEmployeeDbContext(
    DbContextOptions<SqlServerEmployeeDbContext> options) : BaseEmployeeDbContext(options);
