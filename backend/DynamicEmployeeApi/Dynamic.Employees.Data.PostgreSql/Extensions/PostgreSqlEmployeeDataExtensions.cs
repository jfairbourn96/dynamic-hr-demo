using Dynamic.Employees.Data;
using Dynamic.Employees.Data.Extensions;
using Dynamic.Json.EfCore.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamic.Employees.Data.PostgreSql.Extensions;

/// <summary>Registers PostgreSQL employee persistence.</summary>
public static class PostgreSqlEmployeeDataExtensions
{
    /// <summary>Registers PostgreSQL, Dynamic.Json translation, and shared repositories.</summary>
    public static IServiceCollection RegisterPostgreSqlEmployeeData(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<PostgreSqlEmployeeDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(
                    typeof(PostgreSqlEmployeeDbContext).Assembly.GetName().Name))
                .UseDynamicJsonPostgreSql());
        services.AddScoped<BaseEmployeeDbContext>(
            provider => provider.GetRequiredService<PostgreSqlEmployeeDbContext>());
        return services.RegisterEmployeeDataServices();
    }
}
