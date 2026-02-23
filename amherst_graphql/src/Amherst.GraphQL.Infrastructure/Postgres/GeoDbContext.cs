using Amherst.GraphQL.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Amherst.GraphQL.Infrastructure.Postgres;

/// <summary>
/// EF Core DbContext for geo_shapes. Read-only usage (NoTracking set at factory level).
/// </summary>
public class GeoDbContext(DbContextOptions<GeoDbContext> options) : DbContext(options)
{
    public DbSet<Geo> Geos => Set<Geo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Explicitly apply the single entity configuration — no assembly scanning.
        modelBuilder.ApplyConfiguration(new GeoConfiguration());
    }
}
