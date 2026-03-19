using Amherst.GraphQL.Application.Ports;
using Amherst.GraphQL.Domain.Entities;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;

namespace Amherst.GraphQL.Api.Queries;

/// <summary>
/// GraphQL query resolvers for geographic shape data.
/// Delegates to IGeoRepository; HC middleware handles filtering/sorting on the IQueryable.
/// </summary>
[QueryType]
public static class GeoQueries
{
    // [UseSorting]
    public static IQueryable<Geo> GetGeo(
        string geoTypeCode,
        string geoValue,
        [Service] IGeoRepository repository)
    {
        return repository.Query(geoTypeCode, geoValue);
    }

    [UseFiltering]
    [UseSorting]
    public static IQueryable<Geo> GetGeosWithinRadius(
        double latitude,
        double longitude,
        double radiusMiles,
        string? geoTypeCode,
        [Service] IGeoRepository repository)
    {
        // Delegate to repository — note lat/lon flip to match PostGIS lon/lat convention
        // Pass all params through including optional geoTypeCode
        return repository.QueryWithinRadius(latitude, longitude, radiusMiles, geoTypeCode);
    }

    public static IQueryable<Geo> GetGeosByRadius(
        double latitude,
        double longitude,
        double radiusMiles,
        string? geoTypeCode,
        [Service] IGeoRepository repository)
    {
        // Delegate to repository — note lat/lon flip to match PostGIS lon/lat convention
        // Pass all params through including optional geoTypeCode
        return repository.QueryWithinRadius(latitude, longitude, radiusMiles, geoTypeCode);
    }
}
