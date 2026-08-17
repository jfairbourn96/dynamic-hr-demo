using Dynamic.Employees.Data;
using Dynamic.Employees.Data.PostgreSql.Extensions;
using Dynamic.Employees.Data.SqlServer.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Extensions;

/// <summary>Composes the configured employee database provider.</summary>
public static class EmployeeDatabaseExtensions
{
    public const string SqlServerProvider = "SqlServer";
    public const string PostgreSqlProvider = "PostgreSql";

    /// <summary>
    /// Registers employee persistence using <c>Database:Provider</c>. SQL Server remains the
    /// default for backward compatibility.
    /// </summary>
    public static IServiceCollection RegisterConfiguredEmployeeDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string provider = configuration["Database:Provider"] ?? SqlServerProvider;

        return provider.ToUpperInvariant() switch
        {
            "SQLSERVER" => services.RegisterSqlServerEmployeeData(
                GetRequiredConnectionString(configuration, SqlServerProvider)),
            "POSTGRESQL" => services.RegisterPostgreSqlEmployeeData(
                GetRequiredConnectionString(configuration, PostgreSqlProvider)),
            _ => throw new InvalidOperationException(
                $"Unsupported database provider '{provider}'. Supported values are '{SqlServerProvider}' and '{PostgreSqlProvider}'."),
        };
    }

    /// <summary>Applies migrations for the context selected during service registration.</summary>
    public static async Task ApplyEmployeeDatabaseMigrationsAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        BaseEmployeeDbContext context = scope.ServiceProvider.GetRequiredService<BaseEmployeeDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }

    private static string GetRequiredConnectionString(
        IConfiguration configuration,
        string provider)
    {
        string? connectionString = configuration.GetConnectionString(provider);
        return connectionString ?? throw new InvalidOperationException(
            $"Connection string '{provider}' is not configured for database provider '{provider}'.");
    }
}
