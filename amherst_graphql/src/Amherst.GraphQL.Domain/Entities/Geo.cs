using HotChocolate.Types.Spatial;
using NetTopologySuite.Geometries;

namespace Amherst.GraphQL.Domain.Entities;

/// <summary>
/// Clean POCO representing a row from the geo_shapes table.
/// Maps the seven non-spatial columns; geo_polygon is excluded.
/// Composite key: (GeoTypeCode, GeoValue).
/// </summary>
public class Geo
{
    public string? GeoSrc { get; set; }
    public string GeoTypeCode { get; set; } = string.Empty;
    public string? GeoTypeName { get; set; }
    public string GeoValue { get; set; } = string.Empty;
    public string? GeoName { get; set; }
    public string? WktPolygon { get; set; }
    public long? SpatialIndex { get; set; }
    // public MultiPolygon GeoPolygon { get; set; } = new MultiPolygon([]);
}
