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
    IQueryable<Geo> Query(string geoTypeCode, string geoValue);

    /// <summary>
    /// Spatial point-in-polygon search using PostGIS ST_Covers.
    /// Note: latitude/longitude order here — SQL reverses them for ST_MakePoint(lon, lat).
    /// </summary>
    IQueryable<Geo> QueryContaining(double latitude, double longitude);

    /// <summary>
    /// Spatial polygon-intersects-polygon search using PostGIS ST_Intersects.
    /// </summary>
    IQueryable<Geo> QueryIntersecting(string geoTypeCode, string geoValue, string? geoTypeCodeFilter = null);

    /// <summary>
    /// Radius search — returns geos within radiusMiles of the point,
    /// optionally filtered by geo type code.
    /// </summary>
    IQueryable<Geo> QueryWithinRadius(double latitude, double longitude, double radiusMiles, string? geoTypeCodeFilter = null);
}
