using Amherst.GraphQL.Application.Ports;
using Amherst.GraphQL.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Amherst.GraphQL.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Infrastructure services: pooled DbContext factory, scoped DbContext, and repository.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Read connection string from the "Postgres" config section.
        var settings = configuration
            .GetSection(PostgresSettings.SectionName)
            .Get<PostgresSettings>()!;

        // Register pooled DbContext factory with Npgsql and NoTracking.
        services.AddPooledDbContextFactory<GeoDbContext>(options =>
            options
                .UseNpgsql(settings.ConnectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        // Register scoped DbContext resolved from the factory (for direct repository injection).
        services.AddScoped(sp =>
            sp.GetRequiredService<IDbContextFactory<GeoDbContext>>().CreateDbContext());

        // Register repository.
        services.AddScoped<IGeoRepository, GeoRepository>();

        return services;
    }
}
