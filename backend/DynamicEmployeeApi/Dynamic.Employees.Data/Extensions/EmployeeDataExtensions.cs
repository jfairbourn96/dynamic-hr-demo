using Dynamic.Employees.Application.Interfaces;
using Dynamic.Employees.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamic.Employees.Data.Extensions;

/// <summary>
/// Provides dependency injection registration for employee data services.
/// </summary>
/// <remarks>
/// Each narrow employee port resolves to the same scoped repository instance. This preserves the
/// application's capability-based interfaces while sharing one EF Core context per request.
/// </remarks>
public static class EmployeeDataExtensions
{
    /// <summary>
    /// Registers the provider-neutral repository ports for an employee database context.
    /// </summary>
    public static IServiceCollection RegisterEmployeeDataServices(
        this IServiceCollection services)
    {
        services.AddScoped<EfEmployeeRepository>();
        services.AddScoped<IEmployeeSearchRepository>(sp => sp.GetRequiredService<EfEmployeeRepository>());
        services.AddScoped<IEmployeeReader>(sp => sp.GetRequiredService<EfEmployeeRepository>());
        services.AddScoped<IEmployeeWriter>(sp => sp.GetRequiredService<EfEmployeeRepository>());

        services.AddScoped<EfEmployeeTypeRepository>();
        services.AddScoped<IEmployeeTypeReader>(sp => sp.GetRequiredService<EfEmployeeTypeRepository>());
        services.AddScoped<IEmployeeTypeWriter>(sp => sp.GetRequiredService<EfEmployeeTypeRepository>());

        return services;
    }
}
