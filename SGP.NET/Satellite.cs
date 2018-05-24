using System;
using System.Collections.Generic;
using SGPdotNET.Coordinate;
using SGPdotNET.Propogation;
using SGPdotNET.TLE;

namespace SGPdotNET
{
    /// <summary>
    ///     A representation of a satellite in orbit
    /// </summary>
    public class Satellite
    {
        private readonly Sgp4 _sgp4;

        /// <inheritdoc />
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="tle1">The first line of the set</param>
        /// <param name="tle2">The second line of the set</param>
        public Satellite(string tle1, string tle2) : this("Unnamed", tle1, tle2)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="name">The name of the satellite</param>
        /// <param name="tle1">The first line of the set</param>
        /// <param name="tle2">The second line of the set</param>
        public Satellite(string name, string tle1, string tle2) : this(new Tle(name, tle1, tle2))
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="tle">The two-line representation of the satellite</param>
        public Satellite(Tle tle)
        {
            _sgp4 = new Sgp4(tle);

            Name = tle.Name;
        }

        /// <summary>
        ///     The name of this satellite
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Predicts the satellite's real-time location
        /// </summary>
        /// <returns>An ECI coordinate set representing the satellite</returns>
        public EciCoordinate Predict()
        {
            return Predict(DateTime.UtcNow);
        }

        /// <summary>
        ///     Predicts the satellite's location at a specific time
        /// </summary>
        /// <param name="time">The time of observation</param>
        /// <returns>An ECI coordinate set representing the satellite at the given time</returns>
        public EciCoordinate Predict(DateTime time)
        {
            return _sgp4.FindPosition(time);
        }

        /// <summary>
        ///     Gets the surface distance (km) from the satellite's location which defines the bounds of the visibility footprint
        /// </summary>
        /// <returns>The foorptint circle radius, in kilometers</returns>
        public double GetFootprintSize()
        {
            return Predict().GetFootprint();
        }

        /// <summary>
        ///     Gets the surface distance (km) from the satellite's location which defines the bounds of the visibility footprint
        ///     at a specific time
        /// </summary>
        /// <param name="time">The time to predict the footprint</param>
        /// <returns>The foorptint circle radius, in kilometers</returns>
        public double GetFootprintSize(DateTime time)
        {
            return Predict(time).GetFootprint();
        }

        /// <summary>
        ///     Gets the surface distance (radians) from the satellite's location which defines the bounds of the visibility
        ///     footprint
        /// </summary>
        /// <returns>The foorptint circle radius, in kilometers</returns>
        public double GetFootprintSizeRadians()
        {
            return Predict().GetFootprintRadians();
        }

        /// <summary>
        ///     Gets the surface distance (radians) from the satellite's location which defines the bounds of the visibility
        ///     footprint at a specific time
        /// </summary>
        /// <param name="time">The time to predict the footprint</param>
        /// <returns>The foorptint circle radius, in kilometers</returns>
        public double GetFootprintSizeRadians(DateTime time)
        {
            return Predict(time).GetFootprintRadians();
        }

        /// <summary>
        ///     Gets a list of geodetic coordinates which define the bounds of the visibility footprint
        /// </summary>
        /// <returns>A list of geodetic coordinates</returns>
        public List<Coordinate.Coordinate> GetFootprintBoundary()
        {
            return GetFootprintBoundary(DateTime.UtcNow);
        }

        /// <summary>
        ///     Gets a list of geodetic coordinates which define the bounds of the visibility footprint at a specific time
        /// </summary>
        /// <param name="time">The time to predict the footprint</param>
        /// <param name="numPoints">The number of points in the resulting circle</param>
        /// <returns>A list of geodetic coordinates for the specified time</returns>
        public List<Coordinate.Coordinate> GetFootprintBoundary(DateTime time, int numPoints = 60)
        {
            var center = Predict(time).ToGeodetic();
            var coords = new List<Coordinate.Coordinate>();

            var lat = center.Latitude;
            var lon = center.Longitude;
            var d = center.GetFootprintRadians();

            for (var i = 0; i < numPoints; i++)
            {
                var perc = i / (float) numPoints * 2 * Math.PI;

                var latRadians = Math.Asin(Math.Sin(lat) * Math.Cos(d) + Math.Cos(lat) * Math.Sin(d) * Math.Cos(perc));
                var lngRadians = lon +
                                 Math.Atan2(Math.Sin(perc) * Math.Sin(d) * Math.Cos(lat),
                                     Math.Cos(d) - Math.Sin(lat) * Math.Sin(latRadians));

                coords.Add(new GeodeticCoordinate(latRadians, lngRadians, 10, true));
            }

            return coords;
        }
    }
}