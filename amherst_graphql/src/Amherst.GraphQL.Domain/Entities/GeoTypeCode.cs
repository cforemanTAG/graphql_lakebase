namespace Amherst.GraphQL.Domain.Entities;

public enum GeoTypeCode
{
    AmherstRegion,
    Cbsa,
    CbsaDivision,
    CensusBlock,
    CensusBlockGroup,
    CensusTract,
    County,
    MinorCivilDivision,
    NeighborhoodLevel1,
    NeighborhoodLevel2,
    NeighborhoodLevel3,
    NeighborhoodLevel4,
    Place,
    SchoolAttendanceArea,
    SchoolDistrict,
    State,
    ZipCode
}

public static class GeoTypeCodeExtensions
{
    public static string ToSnakeCase(this GeoTypeCode code) => code switch
    {
        GeoTypeCode.AmherstRegion => "amherst_region",
        GeoTypeCode.Cbsa => "cbsa",
        GeoTypeCode.CbsaDivision => "cbsa_division",
        GeoTypeCode.CensusBlock => "census_block",
        GeoTypeCode.CensusBlockGroup => "census_block_group",
        GeoTypeCode.CensusTract => "census_tract",
        GeoTypeCode.County => "county",
        GeoTypeCode.MinorCivilDivision => "minor_civil_division",
        GeoTypeCode.NeighborhoodLevel1 => "neighborhood_level_1",
        GeoTypeCode.NeighborhoodLevel2 => "neighborhood_level_2",
        GeoTypeCode.NeighborhoodLevel3 => "neighborhood_level_3",
        GeoTypeCode.NeighborhoodLevel4 => "neighborhood_level_4",
        GeoTypeCode.Place => "place",
        GeoTypeCode.SchoolAttendanceArea => "school_attendance_area",
        GeoTypeCode.SchoolDistrict => "school_district",
        GeoTypeCode.State => "state",
        GeoTypeCode.ZipCode => "zip_code",
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
    };

    public static GeoTypeCode FromSnakeCase(string snakeCase) => snakeCase switch
    {
        "amherst_region" => GeoTypeCode.AmherstRegion,
        "cbsa" => GeoTypeCode.Cbsa,
        "cbsa_division" => GeoTypeCode.CbsaDivision,
        "census_block" => GeoTypeCode.CensusBlock,
        "census_block_group" => GeoTypeCode.CensusBlockGroup,
        "census_tract" => GeoTypeCode.CensusTract,
        "county" => GeoTypeCode.County,
        "minor_civil_division" => GeoTypeCode.MinorCivilDivision,
        "neighborhood_level_1" => GeoTypeCode.NeighborhoodLevel1,
        "neighborhood_level_2" => GeoTypeCode.NeighborhoodLevel2,
        "neighborhood_level_3" => GeoTypeCode.NeighborhoodLevel3,
        "neighborhood_level_4" => GeoTypeCode.NeighborhoodLevel4,
        "place" => GeoTypeCode.Place,
        "school_attendance_area" => GeoTypeCode.SchoolAttendanceArea,
        "school_district" => GeoTypeCode.SchoolDistrict,
        "state" => GeoTypeCode.State,
        "zip_code" => GeoTypeCode.ZipCode,
        _ => throw new ArgumentOutOfRangeException(nameof(snakeCase), snakeCase, null)
    };
}
