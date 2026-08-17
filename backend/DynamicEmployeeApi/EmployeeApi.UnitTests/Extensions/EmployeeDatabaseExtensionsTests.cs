using Dynamic.Employees.Data;
using Dynamic.Employees.Data.PostgreSql;
using Dynamic.Employees.Data.SqlServer;
using EmployeeApi.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeApi.UnitTests.Extensions;

public sealed class EmployeeDatabaseExtensionsTests
{
    [Fact]
    public void RegisterConfiguredEmployeeDatabase_DefaultsToSqlServerAndSupportsLegacyConnectionString()
    {
        IConfiguration configuration = Configuration(
            ("ConnectionStrings:DefaultConnection", "Server=localhost;Database=DynamicHr;Integrated Security=true"));
        ServiceCollection services = new();

        services.RegisterConfiguredEmployeeDatabase(configuration);

        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(SqlServerEmployeeDbContext));
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(BaseEmployeeDbContext));
    }

    [Fact]
    public void RegisterConfiguredEmployeeDatabase_WithSqlServer_UsesNamedProviderConfiguration()
    {
        IConfiguration configuration = Configuration(
            ("Database:Provider", "SqlServer"),
            ("ConnectionStrings:SqlServer", "Server=localhost;Database=DynamicHr;Integrated Security=true"));
        ServiceCollection services = new();

        services.RegisterConfiguredEmployeeDatabase(configuration);

        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(SqlServerEmployeeDbContext));
        services.Should().NotContain(descriptor => descriptor.ServiceType == typeof(PostgreSqlEmployeeDbContext));
    }

    [Fact]
    public void RegisterConfiguredEmployeeDatabase_WithPostgreSql_RegistersPostgreSqlContext()
    {
        IConfiguration configuration = Configuration(
            ("Database:Provider", "PostgreSql"),
            ("ConnectionStrings:PostgreSql", "Host=localhost;Database=DynamicHr;Username=postgres;Password=postgres"));
        ServiceCollection services = new();

        services.RegisterConfiguredEmployeeDatabase(configuration);

        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(PostgreSqlEmployeeDbContext));
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(BaseEmployeeDbContext));
        services.Should().NotContain(descriptor => descriptor.ServiceType == typeof(SqlServerEmployeeDbContext));
    }

    [Fact]
    public void RegisterConfiguredEmployeeDatabase_WithUnsupportedProvider_FailsFast()
    {
        IConfiguration configuration = Configuration(("Database:Provider", "Oracle"));
        ServiceCollection services = new();

        Action act = () => services.RegisterConfiguredEmployeeDatabase(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Unsupported database provider 'Oracle'*SqlServer*PostgreSql*");
    }

    [Theory]
    [InlineData("SqlServer")]
    [InlineData("PostgreSql")]
    public void RegisterConfiguredEmployeeDatabase_WithoutProviderConnectionString_FailsFast(string provider)
    {
        IConfiguration configuration = Configuration(("Database:Provider", provider));
        ServiceCollection services = new();

        Action act = () => services.RegisterConfiguredEmployeeDatabase(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*Connection string '{provider}' is not configured*");
    }

    private static IConfiguration Configuration(params (string Key, string Value)[] values)
    {
        Dictionary<string, string?> settings = values.ToDictionary(value => value.Key, value => (string?)value.Value);
        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }
}
