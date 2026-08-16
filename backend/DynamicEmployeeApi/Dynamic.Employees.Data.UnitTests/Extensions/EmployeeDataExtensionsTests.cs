using Dynamic.Employees.Application.Interfaces;
using Dynamic.Employees.Data.Extensions;
using Dynamic.Employees.Data.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamic.Employees.Data.UnitTests.Extensions;

public class EmployeeDataExtensionsTests
{
    [Fact]
    public void RegisterEmployeeDataServices_WhenCalled_RegistersDataServices()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        IServiceCollection result = services.RegisterEmployeeDataServices();

        // Assert
        result.Should().BeSameAs(services);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(EfEmployeeRepository) &&
            descriptor.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IEmployeeSearchRepository) &&
            descriptor.Lifetime == ServiceLifetime.Scoped &&
            descriptor.ImplementationFactory != null);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IEmployeeReader) &&
            descriptor.Lifetime == ServiceLifetime.Scoped &&
            descriptor.ImplementationFactory != null);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IEmployeeWriter) &&
            descriptor.Lifetime == ServiceLifetime.Scoped &&
            descriptor.ImplementationFactory != null);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(EfEmployeeTypeRepository) &&
            descriptor.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IEmployeeTypeReader) &&
            descriptor.Lifetime == ServiceLifetime.Scoped &&
            descriptor.ImplementationFactory != null);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IEmployeeTypeWriter) &&
            descriptor.Lifetime == ServiceLifetime.Scoped &&
            descriptor.ImplementationFactory != null);
    }
}
