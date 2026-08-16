using Dynamic.Employees.Application.Interfaces;
using Dynamic.Employees.Data.PostgreSql;
using Dynamic.Employees.Data.PostgreSql.Extensions;
using Dynamic.Employees.Data.SqlServer;
using Dynamic.Employees.Data.SqlServer.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamic.Employees.Data.UnitTests.Extensions;

public sealed class ProviderEmployeeDataExtensionsTests
{
    [Fact]
    public void SqlServerRegistration_ComposesProviderContextAndSharedPorts()
    {
        ServiceCollection services = new();

        services.RegisterSqlServerEmployeeData("Server=(localdb)\\mssqllocaldb;Database=DynamicHr;");

        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(SqlServerEmployeeDbContext));
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(BaseEmployeeDbContext));
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IEmployeeReader));
    }

    [Fact]
    public void PostgreSqlRegistration_ComposesProviderContextAndSharedPorts()
    {
        ServiceCollection services = new();

        services.RegisterPostgreSqlEmployeeData(
            "Host=localhost;Database=DynamicHr;Username=postgres;Password=postgres");

        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(PostgreSqlEmployeeDbContext));
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(BaseEmployeeDbContext));
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IEmployeeReader));
    }

    [Fact]
    public void DomainAndApplicationProjects_DoNotReferenceDatabaseProviders()
    {
        string[] references =
        [
            .. typeof(Dynamic.Employees.Domain.Models.Employee).Assembly.GetReferencedAssemblies().Select(name => name.Name!),
            .. typeof(IEmployeeReader).Assembly.GetReferencedAssemblies().Select(name => name.Name!),
        ];

        references.Should().NotContain(name =>
            name.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Postgre", StringComparison.OrdinalIgnoreCase));
    }
}
