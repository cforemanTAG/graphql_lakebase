using Amherst.GraphQL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amherst.GraphQL.Infrastructure.Postgres;

/// <summary>
/// EF configuration for Geo entity → geo_shapes table.
/// Maps all seven non-spatial columns with explicit snake_case column names.
/// geo_polygon is NOT mapped — spatial queries use FromSql with raw PostGIS SQL instead.
/// </summary>
public class GeoConfiguration : IEntityTypeConfiguration<Geo>
{
    public void Configure(EntityTypeBuilder<Geo> builder)
    {
        builder.ToTable("geo_shapes");

        builder.HasKey(g => new { g.GeoTypeCode, g.GeoValue });

        builder.Property(g => g.GeoSrc).HasColumnName("geo_src");
        builder.Property(g => g.GeoTypeCode).HasColumnName("geo_type_code");
        builder.Property(g => g.GeoTypeName).HasColumnName("geo_type_name");
        builder.Property(g => g.GeoValue).HasColumnName("geo_value");
        builder.Property(g => g.GeoName).HasColumnName("geo_name");
        builder.Property(g => g.WktPolygon).HasColumnName("wkt_polygon");
        builder.Property(g => g.SpatialIndex).HasColumnName("spatial_index");
    }
}
