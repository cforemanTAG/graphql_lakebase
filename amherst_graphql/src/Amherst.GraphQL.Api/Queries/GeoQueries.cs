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
        GeoTypeCode geoTypeCode,
        string geoValue,
        [Service] IGeoRepository repository)
    {
        return repository.Query(geoTypeCode, geoValue);
    }

    // [UseFiltering]
    // [UseSorting]
    public static IQueryable<Geo> GetGeosCovering(
        double latitude,
        double longitude,
        IEnumerable<GeoTypeCode>? geoTypeCodeFilter,
        [Service] IGeoRepository repository)
    {
        // Delegate to repository — note lat/lon flip to match PostGIS lon/lat convention
        // Pass all params through including optional geoTypeCode
        return repository.QueryCovering(latitude, longitude, geoTypeCodeFilter);
    }

    [UseFiltering]
    // [UseSorting]
    public static IQueryable<Geo> GetGeosWithinRadius(
        double latitude,
        double longitude,
        double radiusMiles,
        GeoTypeCode? geoTypeCodeFilter,
        [Service] IGeoRepository repository)
    {
        // Delegate to repository — note lat/lon flip to match PostGIS lon/lat convention
        // Pass all params through including optional geoTypeCode
        return repository.QueryWithinRadius(latitude, longitude, radiusMiles, geoTypeCodeFilter);
    }

    // [UseFiltering]
    // [UseSorting]
    public static IQueryable<Geo> GetGeosIntersersectingGeo(
        GeoTypeCode geoTypeCode,
        string geoValue,
        GeoTypeCode? geoTypeCodeFilter,
        [Service] IGeoRepository repository)
    {
        
        return repository.QueryIntersecting(geoTypeCode, geoValue, geoTypeCodeFilter);
    }
}
