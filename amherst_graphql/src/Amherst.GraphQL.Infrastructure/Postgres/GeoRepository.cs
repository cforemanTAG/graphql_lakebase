using Amherst.GraphQL.Application.Ports;
using Amherst.GraphQL.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Amherst.GraphQL.Infrastructure.Postgres;

/// <summary>
/// Implements IGeoRepository using EF Core and raw PostGIS SQL for spatial queries.
/// </summary>
public class GeoRepository(GeoDbContext db) : IGeoRepository
{
    public IQueryable<Geo> Query(string geoTypeCode, string geoValue)
    {
        // Filter by composite key using standard LINQ Where, with AsNoTracking.
        return db.Geos
            .AsNoTracking()
            .Where(g => g.GeoTypeCode == geoTypeCode && g.GeoValue == geoValue);
    }

    public IQueryable<Geo> QueryContaining(double latitude, double longitude)
    {
        // Use FromSql (interpolated overload) for parameterized PostGIS spatial query.
        // SELECT must list all 7 mapped columns explicitly.
        // ST_MakePoint takes (longitude, latitude) — note the reversed parameter order.
        // The ::geography cast ensures type compatibility with the geo_polygon column.
        return db.Geos.FromSql(
            $"""
            SELECT geo_src, geo_type_code, geo_type_name, geo_value,
                   geo_name, wkt_polygon, spatial_index
            FROM   geo_shapes
            WHERE  ST_Covers(
                       geo_polygon,
                       ST_SetSRID(ST_MakePoint({longitude}, {latitude}), 4326)::geography
                   )
            """)
            .AsNoTracking();
    }
}
