# SGP.NET
SGP.NET is a multi-function library with support for loading satellites from TLEs, converting between coordinate systems and reference frames, observing satellites from ground stations, and creating schedules of observations over periods of time.

See the [wiki](https://github.com/parzivail/SGP.NET/wiki) for complete documentation.

# NuGet

[https://www.nuget.org/packages/SGP.NET/](https://www.nuget.org/packages/SGP.NET/)

# Getting Started
```csharp
// Create a set of TLE strings
var tle1 = "ISS (ZARYA)";
var tle2 = "1 25544U 98067A   19034.73310439  .00001974  00000-0  38215-4 0  9991";
var tle3 = "2 25544  51.6436 304.9146 0005074 348.4622  36.8575 15.53228055154526";

// Create a satellite from the TLEs
var sat = new Satellite(tle1, tle2, tle3);

// Set up our ground station location
var location = new GeodeticCoordinate(new AngleDegrees(40.689236), new AngleDegrees(-74.044563), Angle.Zero);

// Create a ground station
var groundStation = new GroundStation(location);

// Observe the satellite
var observation = groundStation.Observe(sat, new DateTime(2019, 3, 5, 3, 45, 12, DateTimeKind.Utc));

Console.WriteLine(observation);
// -> TopocentricObservation[Azimuth=Angle[17.7964900382581°], Elevation=Angle[-54.1738348534288°], Range=10962.2688992742km, RangeRate=3.29677171042301km/s]
```

# Creating TLEs
Two-Line Element sets can actually come in two- or three-line formats. SGP.NET supports both.
## From two strings
```csharp
var tleString1 = "1 25544U 98067A   19034.73310439  .00001974  00000-0  38215-4 0  9991";
var tleString2 = "2 25544  51.6436 304.9146 0005074 348.4622  36.8575 15.53228055154526";

var tle = new Tle(tleString1, tleString2);
```
## From three strings (two strings and a name)
```csharp
var tleString0 = "ISS (ZARYA)";
var tleString1 = "1 25544U 98067A   19034.73310439  .00001974  00000-0  38215-4 0  9991";
var tleString2 = "2 25544  51.6436 304.9146 0005074 348.4622  36.8575 15.53228055154526";

var tle = new Tle(tleString0, tleString1, tleString2);
```

# Obtaining TLEs
## From a file
```csharp
// Create a provider
var provider = new LocalTleProvider(true, "tles.txt");

// Get every TLE
var tles = provider.GetTles();

// Alternatively get a specific satellite's TLE
var issTle = provider.GetTle(25544);
```
## From a URL
### Without local caching
```csharp
// Remote URL
var url = new Uri("https://celestrak.com/NORAD/elements/weather.txt");

// Create a provider
var provider = new RemoteTleProvider(true, url);

// Get every TLE
var tles = provider.GetTles();

// Alternatively get a specific satellite's TLE
var issTle = provider.GetTle(25544);
```
### With local caching
```csharp
// Remote URL
var url = new Uri("https://celestrak.com/NORAD/elements/weather.txt");

// Create a provider whose cache is renewed every 12 hours
var provider = new CachingRemoteTleProvider(true, TimeSpan.FromHours(12), "cachedTles.txt", url);

// Get every TLE
var tles = provider.GetTles();

// Alternatively get a specific satellite's TLE
var issTle = provider.GetTle(25544);
```
## More details on Observe

The Observe method of the Groundstation class may accept a time interval (in UTC) in which to find multiple possible visibility overpasses for a satellite. Here we determine passes for the next 24 hour period, using a time step of 10 seconds.
```csharp
var observations = groundStation.Observe(sat, DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromHours(24), TimeSpan.FromSeconds(10));
```

The time step provided to use with groundstation.Observe is used to coarsely perform an initial search for the start, end, and time of max elevation of each overpass, which are then determined to a higher resolution using an interval halving approach. The resolution parameter sets the number of decimal places (in seconds) for determined the time above. This allows using a large time step to for finding the overpasses. Here we use a time step of 10 seconds, but determine the time to a resolution of a hundredth of a second.
```csharp
var observations = groundStation.Observe(sat, DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromHours(24), TimeSpan.FromSeconds(10), resolution: 2);
```
The default resolution is 3.

The Observe method also provides a minElevation parameter, with a default of 0. E.g.
```csharp
observations = groundstation.Observe(sat, DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromHours(24), TimeSpan.FromSeconds(10), minElevation: new AngleDegrees(7.5));
```

As well, the Observe method provides clipToStartTime and clipToEndTime parameters, which will clip the start time of the first observation to the start parameter and the end time of the last observation to the end parameter passed to Observe respectively. The default value for clipToStartTime is true (the assumption being that most users are generally concerned with future upcoming passes only) and the default value for clipToEndTime is false (the assumption being that most users do not want to truncate an overpass). Here we clip the observations to the provided time interval.

```csharp
observations = groundstation.Observe(sat, DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromHours(24), TimeSpan.FromSeconds(10), clipToEndTime: true)
```

# Further Reading
More examples coming soon. See [the wiki](https://github.com/parzivail/SGP.NET/wiki) for more information.
