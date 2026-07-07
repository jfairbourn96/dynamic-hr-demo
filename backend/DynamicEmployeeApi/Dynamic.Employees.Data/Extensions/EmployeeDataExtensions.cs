using Dynamic.Employees.Application.Interfaces;
using Dynamic.Employees.Data.Repositories;
using Dynamic.Json.EfCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamic.Employees.Data.Extensions;

public static class EmployeeDataExtensions
{
    public static IServiceCollection RegisterEmployeeDataServices(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<EmployeeDbContext>(options =>
            options
                .UseSqlServer(connectionString,
                    x => x.MigrationsAssembly(typeof(EmployeeDbContext).Assembly.GetName().Name))
                .UseDynamicJsonSqlServer());

        services.AddScoped<BaseEmployeeDbContext>(sp => sp.GetRequiredService<EmployeeDbContext>());

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
