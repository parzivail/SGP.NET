namespace Sandbox
{
    internal class AzimuthElevation
    {
        public double Azimuth { get; }
        public double Elevation { get; }
        public double Range { get; }

        public AzimuthElevation(double azimuth, double elevation, double range)
        {
            Azimuth = azimuth;
            Elevation = elevation;
            Range = range;
        }
    }
}