using Dynamic.Employees.Application.Extensions;
using Dynamic.Employees.Application.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamic.Employees.Application.UnitTests.Extensions;

public class EmployeeApplicationExtensionsTests
{
    [Fact]
    public void RegisterEmployeeApplicationServices_WhenCalled_RegistersApplicationServices()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        IServiceCollection result = services.RegisterEmployeeApplicationServices();

        // Assert
        result.Should().BeSameAs(services);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IEmployeeService) &&
            descriptor.ImplementationType == typeof(EmployeeService) &&
            descriptor.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IEmployeeTypeService) &&
            descriptor.ImplementationType == typeof(EmployeeTypeService) &&
            descriptor.Lifetime == ServiceLifetime.Scoped);
    }
}
