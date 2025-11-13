using System;
using SGPdotNET.Observation;
using SGPdotNET.TLE;

namespace SGP.NET.Tests
{
    /// <summary>
    /// Test data for correctness and performance testing
    /// </summary>
    public static class TestData
    {
        // ISS TLE (International Space Station)
        public static readonly string IssName = "ISS (ZARYA)";
        public static readonly string IssLine1 = "1 25544U 98067A   19034.73310439  .00001974  00000-0  38215-4 0  9991";
        public static readonly string IssLine2 = "2 25544  51.6436 304.9146 0005074 348.4622  36.8575 15.53228055154526";

        // NOAA-18 TLE (circular orbit example)
        public static readonly string Noaa18Name = "NOAA-18";
        public static readonly string Noaa18Line1 = "1 28654U 05018A   19034.12345678  .00000001  00000-0  00000+0 0  9998";
        public static readonly string Noaa18Line2 = "2 28654  98.7449 123.4567 0012345 234.5678 125.4321 14.12345678901234";

        // Molniya TLE (highly elliptical orbit) - using valid TLE format
        public static readonly string MolniyaName = "MOLNIYA 1-1";
        public static readonly string MolniyaLine1 = "1 00001U 65001A   19034.12345678  .00000001  00000-0  00000+0 0  9998";
        public static readonly string MolniyaLine2 = "2 00001  63.4000 270.0000 7200000 270.0000  90.0000   2.0000000000000";

        // Geostationary satellite TLE
        public static readonly string GeoName = "INTELSAT 901";
        public static readonly string GeoLine1 = "1 26824U 01019A   19034.12345678  .00000001  00000-0  00000+0 0  9998";
        public static readonly string GeoLine2 = "2 26824   0.0000 270.0000 0000001 270.0000  90.0000   1.00273790934000";

        /// <summary>
        /// Creates an ISS satellite for testing
        /// </summary>
        public static Satellite CreateIssSatellite()
        {
            return new Satellite(IssName, IssLine1, IssLine2);
        }

        /// <summary>
        /// Creates a NOAA-18 satellite for testing
        /// </summary>
        public static Satellite CreateNoaa18Satellite()
        {
            return new Satellite(Noaa18Name, Noaa18Line1, Noaa18Line2);
        }

        /// <summary>
        /// Creates a Molniya satellite for testing
        /// </summary>
        public static Satellite CreateMolniyaSatellite()
        {
            return new Satellite(MolniyaName, MolniyaLine1, MolniyaLine2);
        }

        /// <summary>
        /// Creates a geostationary satellite for testing
        /// </summary>
        public static Satellite CreateGeostationarySatellite()
        {
            return new Satellite(GeoName, GeoLine1, GeoLine2);
        }

        /// <summary>
        /// Reference Julian dates for testing (calculated using known correct algorithm)
        /// Format: (DateTime, ExpectedJulianDate)
        /// </summary>
        public static readonly (DateTime date, double expectedJulian)[] ReferenceJulianDates = new[]
        {
            (new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc), 2451545.0), // J2000.0
            (new DateTime(1900, 1, 1, 12, 0, 0, DateTimeKind.Utc), 2415021.0), // J1900.0 (verified with Meeus algorithm)
            (new DateTime(2019, 2, 3, 4, 5, 6, DateTimeKind.Utc), 2458517.6702083333),
            (new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), 2458849.5),
        };

        /// <summary>
        /// Reference Greenwich Sidereal Times for testing
        /// Format: (DateTime, ExpectedGST in radians)
        /// </summary>
        public static readonly (DateTime date, double expectedGstRadians)[] ReferenceGreenwichSiderealTimes = new[]
        {
            (new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc), 4.894961212823059), // J2000.0 noon
        };

        /// <summary>
        /// Test times for propagation
        /// </summary>
        public static readonly DateTime[] TestTimes = new[]
        {
            new DateTime(2019, 2, 3, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2019, 2, 3, 6, 0, 0, DateTimeKind.Utc),
            new DateTime(2019, 2, 3, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2019, 2, 3, 18, 0, 0, DateTimeKind.Utc),
        };

        /// <summary>
        /// Ground station locations for testing
        /// </summary>
        public static readonly (double lat, double lon, double alt, string name)[] GroundStations = new[]
        {
            (40.689236, -74.044563, 0.0, "New York (Statue of Liberty)"),
            (51.5074, -0.1278, 0.0, "London"),
            (35.6762, 139.6503, 0.0, "Tokyo"),
            (0.0, 0.0, 0.0, "Equator/Prime Meridian"),
            (90.0, 0.0, 0.0, "North Pole"),
            (-90.0, 0.0, 0.0, "South Pole"),
        };
    }
}

