using System;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Propagation;
using SGPdotNET.Propagation.Bodies;
using SGPdotNET.Util;

namespace SGPSandbox
{
	class Program
	{
		private const double ObserverLat = 28.3737081;
		private const double ObserverLon = -81.5518777;
		private const double HorizonRefractionDeg = -0.833;

		private static readonly TimeZoneInfo LocalTz =
			TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

		static void Main(string[] args)
		{
			var today = DateTime.UtcNow.Date;
			var observer = new GeodeticCoordinate(
				Angle.FromDegrees(ObserverLat),
				Angle.FromDegrees(ObserverLon),
				0);

			var localToday = TimeZoneInfo.ConvertTimeFromUtc(today, LocalTz);

			Console.WriteLine($"Sunrise/Sunset for {localToday:yyyy-MM-dd}");
			Console.WriteLine($"Observer: {ObserverLat:F4}°, {ObserverLon:F4}°");
			Console.WriteLine($"Timezone: {LocalTz.DisplayName}");
			Console.WriteLine();

			var step = TimeSpan.FromSeconds(30);
			var endTime = today.AddHours(36);

			DateTime? sunrise = null;
			DateTime? sunset = null;
			DateTime? solarNoon = null;
			double maxElevation = double.MinValue;

			var prevObs = observer.Observe(Sun.Predict(today), today);
			var prevAbove = prevObs.Elevation.Degrees >= HorizonRefractionDeg;

			for (var t = today + step; t <= endTime; t += step)
			{
				var sun = Sun.Predict(t);
				var obs = observer.Observe(sun, t);

				var currAbove = obs.Elevation.Degrees >= HorizonRefractionDeg;

				if (!prevAbove && currAbove && !sunrise.HasValue)
				{
					sunrise = t;
				}

				if (sunrise.HasValue && obs.Elevation.Degrees > maxElevation)
				{
					maxElevation = obs.Elevation.Degrees;
					solarNoon = t;
				}

				if (sunrise.HasValue && prevAbove && !currAbove && !sunset.HasValue)
				{
					sunset = t;
				}

				if (sunrise.HasValue && sunset.HasValue)
					break;

				prevAbove = currAbove;
			}

			Console.WriteLine($"Sunrise: {FormatTime(sunrise)}");
			Console.WriteLine($"Solar Noon: {FormatTime(solarNoon)} (elevation {maxElevation:F1}°)");
			Console.WriteLine($"Sunset: {FormatTime(sunset)}");

			if (sunrise.HasValue && sunset.HasValue)
				Console.WriteLine($"Day Length: {(sunset.Value - sunrise.Value):hh\\:mm\\:ss}");
		}

		private static string FormatTime(DateTime? utcTime)
		{
			if (!utcTime.HasValue)
				return "N/A";

			var local = TimeZoneInfo.ConvertTimeFromUtc(utcTime.Value, LocalTz);
			var tzAbbr = LocalTz.IsDaylightSavingTime(local) ? LocalTz.DaylightName : LocalTz.StandardName;
			return $"{local:hh:mm:ss tt} {tzAbbr}";
		}
	}
}
