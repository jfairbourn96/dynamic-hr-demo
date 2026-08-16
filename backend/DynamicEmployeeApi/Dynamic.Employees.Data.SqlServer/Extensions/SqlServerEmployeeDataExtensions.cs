using Dynamic.Employees.Data;
using Dynamic.Employees.Data.Extensions;
using Dynamic.Json.EfCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamic.Employees.Data.SqlServer.Extensions;

/// <summary>Registers SQL Server employee persistence.</summary>
public static class SqlServerEmployeeDataExtensions
{
    /// <summary>Registers SQL Server, Dynamic.Json translation, and shared repositories.</summary>
    public static IServiceCollection RegisterSqlServerEmployeeData(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<SqlServerEmployeeDbContext>(options =>
            options
                .UseSqlServer(connectionString, sql => sql.MigrationsAssembly(
                    typeof(SqlServerEmployeeDbContext).Assembly.GetName().Name))
                .UseDynamicJsonSqlServer());
        services.AddScoped<BaseEmployeeDbContext>(
            provider => provider.GetRequiredService<SqlServerEmployeeDbContext>());
        return services.RegisterEmployeeDataServices();
    }
}
