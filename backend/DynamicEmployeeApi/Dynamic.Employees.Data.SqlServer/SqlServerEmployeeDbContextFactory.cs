using Dynamic.Json.EfCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dynamic.Employees.Data.SqlServer;

/// <summary>Creates the SQL Server context for EF Core migration tooling.</summary>
public sealed class SqlServerEmployeeDbContextFactory
    : IDesignTimeDbContextFactory<SqlServerEmployeeDbContext>
{
    public SqlServerEmployeeDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<SqlServerEmployeeDbContext> builder = new();
        builder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DynamicHr;Trusted_Connection=True;");
        builder.UseDynamicJsonSqlServer();
        return new SqlServerEmployeeDbContext(builder.Options);
    }
}
