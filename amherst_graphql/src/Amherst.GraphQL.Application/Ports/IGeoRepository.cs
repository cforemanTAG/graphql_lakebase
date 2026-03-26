using Amherst.GraphQL.Domain.Entities;

namespace Amherst.GraphQL.Application.Ports;

/// <summary>
/// Port for querying geographic shape data.
/// Returns IQueryable to allow HC filtering/sorting middleware to compose LINQ operators.
/// </summary>
public interface IGeoRepository
{
    /// <summary>
    /// Exact-match lookup by composite key (geoTypeCode, geoValue).
    /// </summary>
    IQueryable<Geo> Query(GeoTypeCode geoTypeCode, string geoValue);

    /// <summary>
    /// Spatial point-in-polygon search using PostGIS ST_Covers.
    /// Note: latitude/longitude order here — SQL reverses them for ST_MakePoint(lon, lat).
    /// </summary>
    IQueryable<Geo> QueryCovering(double latitude, double longitude, IEnumerable<GeoTypeCode>? geoTypeCodeFilter = null);

    /// <summary>
    /// Spatial polygon-intersects-polygon search using PostGIS ST_Intersects.
    /// </summary>
    IQueryable<Geo> QueryIntersecting(GeoTypeCode geoTypeCode, string geoValue, GeoTypeCode? geoTypeCodeFilter = null);

    /// <summary>
    /// Radius search — returns geos within radiusMiles of the point,
    /// optionally filtered by geo type code.
    /// Note: latitude/longitude order here — SQL reverses them for ST_Point(lon, lat).
    /// </summary>
    IQueryable<Geo> QueryWithinRadius(double latitude, double longitude, double radiusMiles, GeoTypeCode? geoTypeCodeFilter = null);
}
