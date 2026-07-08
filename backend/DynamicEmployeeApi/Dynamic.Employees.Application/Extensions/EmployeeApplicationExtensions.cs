using Dynamic.Employees.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamic.Employees.Application.Extensions;

public static class EmployeeApplicationExtensions
{
    public static IServiceCollection RegisterEmployeeApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IEmployeeTypeService, EmployeeTypeService>();

        return services;
    }
}
