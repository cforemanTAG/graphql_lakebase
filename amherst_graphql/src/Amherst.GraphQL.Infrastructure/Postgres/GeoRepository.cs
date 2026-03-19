using Amherst.GraphQL.Application.Ports;
using Amherst.GraphQL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

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

    public IQueryable<Geo> QueryContainingRaw(double latitude, double longitude)
    {
        // Use FromSql (interpolated overload) for parameterized PostGIS spatial query.
        // SELECT must list all 7 mapped columns explicitly.
        // The ::geography cast ensures type compatibility with the geo_polygon column.
        return db.Geos
            .FromSql(
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

    public IQueryable<Geo> QueryContaining(double latitude, double longitude)
    {
        /*
        NOTE: This does the same thing as QueryContainingRaw right now because I commented out the code
        */

        
        // Use FromSql (interpolated overload) for parameterized PostGIS spatial query.
        // SELECT must list all 7 mapped columns explicitly.
        // The ::geography cast ensures type compatibility with the geo_polygon column.


        // var query =  db.Geos
        //     .AsNoTracking()
        //     .Where(g => g.GeoPolygon.Contains(new Point(longitude, latitude){ SRID = 4326 }));
        
        return db.Geos
            .FromSql(
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

    public IQueryable<Geo> QueryWithinRadius(double latitude, double longitude, double radiusMiles, string? geoTypeCode = null)
    {
        // Test with: (lat,long) 30.6008245, -97.8612775
        // Convert radiusMiles to meters (miles * 1609.344)
        // Use FromSql with ST_DWithin(geo_polygon, point, meters) — same 7-column SELECT as above
        // The point should be ST_SetSRID(ST_MakePoint(lon, lat), 4326)::geography
        // If geoTypeCode is not null, append .Where(g => g.GeoTypeCode == geoTypeCode) via LINQ
        // Return .AsNoTracking()
        var query = db.Geos.FromSql(
            $"""
            SELECT geo_src, geo_type_code, geo_type_name, geo_value,
            geo_name, wkt_polygon, spatial_index
            FROM geo_shapes
            WHERE ST_DWithin(geo_polygon, ST_Point({longitude}, {latitude}, 4326)::geography, {radiusMiles} * 1609.3440006);
            """
        );

        if (geoTypeCode is not null)
        {
            query = query.Where(g => g.GeoTypeCode == geoTypeCode);
        }

        return query.AsNoTracking();
    }

    public IQueryable<Geo> QueryByRadius(double latitude, double longitude, double radiusMiles, string? geoTypeCode = null)
        => throw new NotImplementedException();

    // public IQueryable<Geo> QueryByRadius(double latitude, double longitude, double radiusMiles, string? geoTypeCode = null)
    // {
    //     // Test with: (lat,long) 30.6008245, -97.8612775
    //     // Convert radiusMiles to meters (miles * 1609.344)
    //     // Use FromSql with ST_DWithin(geo_polygon, point, meters) — same 7-column SELECT as above
    //     // The point should be ST_SetSRID(ST_MakePoint(lon, lat), 4326)::geography
    //     // If geoTypeCode is not null, append .Where(g => g.GeoTypeCode == geoTypeCode) via LINQ
    //     // Return .AsNoTracking()
    //     var point = new Point(longitude, latitude) { SRID = 4326 };
    //     var radiusBuffer = point.Buffer()

    //     var query = db.Geos
    //         .AsNoTracking()
    //         .Where(g => g.GeoPolygon.Within())

    //     if (geoTypeCode is not null)
    //     {
    //         query.Where(g => g.GeoTypeCode == geoTypeCode);
    //     }

    //     return query.AsNoTracking();
    // }
}
