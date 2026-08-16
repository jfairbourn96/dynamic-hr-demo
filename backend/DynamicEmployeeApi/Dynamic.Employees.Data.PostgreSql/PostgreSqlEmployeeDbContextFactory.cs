using Dynamic.Json.EfCore.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dynamic.Employees.Data.PostgreSql;

/// <summary>Creates the PostgreSQL context for EF Core migration tooling.</summary>
public sealed class PostgreSqlEmployeeDbContextFactory
    : IDesignTimeDbContextFactory<PostgreSqlEmployeeDbContext>
{
    public PostgreSqlEmployeeDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<PostgreSqlEmployeeDbContext> builder = new();
        builder.UseNpgsql("Host=localhost;Database=DynamicHr;Username=postgres;Password=postgres");
        builder.UseDynamicJsonPostgreSql();
        return new PostgreSqlEmployeeDbContext(builder.Options);
    }
}
