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
var location = new GeodeticCoordinate(new AngleDegrees(40.689236), new AngleDegrees(-74.044563), 0);

// Create a ground station
var groundStation = new GroundStation(location);

// Observe the satellite
var observation = groundStation.Observe(sat, new DateTime(2019, 3, 5, 3, 45, 12, DateTimeKind.Utc));

Console.WriteLine(observation);
// -> TopocentricObservation[Azimuth=Angle[8.96994075340244°], Elevation=Angle[-55.0905825958146°], Range=10962.2688992742km, RangeRate=0km/s]
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
var provider = new LocalTleProvider("tles.txt", true);

// Get every TLE
var tles = provider.GetTles();

// Alternatively get a specific satellite's TLE
var issTle = provider.GetTle(25544);
```
## From a URL
### Without local caching
```csharp
// Remote URL
var url = new Url("https://celestrak.com/NORAD/elements/weather.txt");

// Create a provider
var provider = new RemoteTleProvider(new[] {url}, true);

// Get every TLE
var tles = provider.GetTles();

// Alternatively get a specific satellite's TLE
var issTle = provider.GetTle(25544);
```
### With local caching
```csharp
// Remote URL
var url = new Url("https://celestrak.com/NORAD/elements/weather.txt");

// Create a provider
var provider = new CachingRemoteTleProvider(new[] {url}, true, "cachedTles.txt");

// Get every TLE
var tles = provider.GetTles();

// Alternatively get a specific satellite's TLE
var issTle = provider.GetTle(25544);
```
