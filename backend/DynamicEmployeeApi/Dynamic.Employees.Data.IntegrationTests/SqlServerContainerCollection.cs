namespace Dynamic.Employees.Data.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class SqlServerContainerCollection : ICollectionFixture<SqlServerContainerFixture>
{
    public const string Name = "SqlServerContainer";
}
