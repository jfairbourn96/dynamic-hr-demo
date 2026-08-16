namespace Dynamic.Employees.Data.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class PostgreSqlContainerCollection : ICollectionFixture<PostgreSqlContainerFixture>
{
    public const string Name = "PostgreSQL";
}
