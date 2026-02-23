namespace Amherst.GraphQL.Infrastructure.Postgres;

/// <summary>
/// Strongly-typed config POCO bound to the "Postgres" configuration section.
/// </summary>
public class PostgresSettings
{
    public const string SectionName = "Postgres";

    public string ConnectionString { get; set; } = string.Empty;
}
