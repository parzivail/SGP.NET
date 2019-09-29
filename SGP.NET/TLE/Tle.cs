using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SGPdotNET.Exception;
using SGPdotNET.Util;

namespace SGPdotNET.TLE
{
    /// <summary>
    ///     Extracts OrbitalElements from a two-line or three-line element set
    /// </summary>
    public class Tle
    {
        private const int Tle1ColNoradnum = 2;
        private const int Tle1LenNoradnum = 5;
        private const int Tle1ColIntldescA = 9;
        private const int Tle1LenIntldescA = 2;
        private const int Tle1LenIntldescB = 3;
        private const int Tle1LenIntldescC = 3;
        private const int Tle1ColEpochA = 18;
        private const int Tle1LenEpochA = 2;
        private const int Tle1ColEpochB = 20;
        private const int Tle1LenEpochB = 12;
        private const int Tle1ColMeanmotiondt2 = 33;
        private const int Tle1LenMeanmotiondt2 = 10;
        private const int Tle1ColMeanmotionddt6 = 44;
        private const int Tle1LenMeanmotionddt6 = 8;
        private const int Tle1ColBstar = 53;
        private const int Tle1LenBstar = 8;

        private const int Tle2ColNoradnum = 2;
        private const int Tle2LenNoradnum = 5;
        private const int Tle2ColInclination = 8;
        private const int Tle2LenInclination = 8;
        private const int Tle2ColRaascendnode = 17;
        private const int Tle2LenRaascendnode = 8;
        private const int Tle2ColEccentricity = 26;
        private const int Tle2LenEccentricity = 7;
        private const int Tle2ColArgperigee = 34;
        private const int Tle2LenArgperigee = 8;
        private const int Tle2ColMeananomaly = 43;
        private const int Tle2LenMeananomaly = 8;
        private const int Tle2ColMeanmotion = 52;
        private const int Tle2LenMeanmotion = 11;
        private const int Tle2ColRevatepoch = 63;
        private const int Tle2LenRevatepoch = 5;

        private const uint TleLenLineData = 69;
        private double _argumentPerigee;
        private double _bstar;
        private double _eccentricity;
        private double _inclination;
        private double _meanAnomaly;
        private double _meanMotion;
        private double _meanMotionDdt6;

        private double _meanMotionDt2;
        private uint _orbitNumber;
        private double _rightAscendingNode;

        /// <summary>
        ///     The name of the satellite
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     The first line of the TLE set
        /// </summary>
        public string Line1 { get; }

        /// <summary>
        ///     The second line of the TLE set
        /// </summary>
        public string Line2 { get; }

        /// <summary>
        ///     The identification number assigned to the satellite by NORAD
        /// </summary>
        public uint NoradNumber { get; private set; }

        /// <summary>
        ///     The international designator of the satellite
        /// </summary>
        public string IntDesignator { get; private set; }

        /// <summary>
        ///     The epoch of the satellite
        /// </summary>
        public DateTime Epoch { get; private set; }

        /// <summary>
        ///     The first time derivative of mean motion
        /// </summary>
        public double MeanMotionDtOver2 => _meanMotionDt2;

        /// <summary>
        ///     The second time derivative of mean motion
        /// </summary>
        public double MeanMotionDdtOver6 => _meanMotionDdt6;

        /// <summary>
        ///     The BSTAR drag term of the satellite
        /// </summary>
        public double BStarDragTerm => _bstar;

        /// <summary>
        ///     The mean motion, in revolutions per day
        /// </summary>
        public double MeanMotionRevPerDay => _meanMotion;

        /// <summary>
        ///     The number of orbits at the epoch
        /// </summary>
        public uint OrbitNumber => _orbitNumber;

        /// <summary>
        ///     The eccentricity of the satellite
        /// </summary>
        public double Eccentricity => _eccentricity;

        /// <summary>
        ///     Gets the inclination
        /// </summary>
        /// <returns></returns>
        public Angle Inclination => Angle.FromDegrees(_inclination);

        /// <summary>
        ///     Gets the right ascension of the ascending node
        /// </summary>
        /// <returns></returns>
        public Angle RightAscendingNode => Angle.FromDegrees(_rightAscendingNode);

        /// <summary>
        ///     Gets the argument of perigee
        /// </summary>
        /// <returns></returns>
        public Angle ArgumentPerigee => Angle.FromDegrees(_argumentPerigee);

        /// <summary>
        ///     Gets the mean anomaly
        /// </summary>
        /// <returns></returns>
        public Angle MeanAnomaly => Angle.FromDegrees(_meanAnomaly);

        /// <summary>
        ///     Initialise TLE with two lines
        /// </summary>
        /// <param name="lineOne">The first line of the set</param>
        /// <param name="lineTwo">The second line of the set</param>
        public Tle(string lineOne, string lineTwo)
        {
            Line1 = lineOne;
            Line2 = lineTwo;
            Initialize();
        }

        /// <summary>
        ///     Initialise 3LE with a name and two lines
        /// </summary>
        /// <param name="name">The 0th line (name) of the set</param>
        /// <param name="lineOne">The first line of the set</param>
        /// <param name="lineTwo">The second line of the set</param>
        public Tle(string name, string lineOne, string lineTwo)
        {
            Name = name;
            Line1 = lineOne;
            Line2 = lineTwo;
            Initialize();
        }

        /// <summary>
        ///     Create a new Tle as a copy of the specified one
        /// </summary>
        /// <param name="tle">Object to copy from</param>
        public Tle(Tle tle)
        {
            Name = tle.Name;
            Line1 = tle.Line1;
            Line2 = tle.Line2;

            NoradNumber = tle.NoradNumber;
            IntDesignator = tle.IntDesignator;
            Epoch = tle.Epoch;
            _meanMotionDt2 = tle._meanMotionDt2;
            _meanMotionDdt6 = tle._meanMotionDdt6;
            _bstar = tle._bstar;
            _inclination = tle._inclination;
            _rightAscendingNode = tle._rightAscendingNode;
            _eccentricity = tle._eccentricity;
            _argumentPerigee = tle._argumentPerigee;
            _meanAnomaly = tle._meanAnomaly;
            _meanMotion = tle._meanMotion;
            _orbitNumber = tle._orbitNumber;
        }

        /// <summary>
        ///     Parses a list of TLEs from a list of TLE lines
        /// </summary>
        /// <param name="lines">Each line of the each element set, sequentially</param>
        /// <param name="threeLine">True if the TLEs contain a third, preceding name line (3le format)</param>
        /// <returns>A list of the TLEs parsed from the lines</returns>
        public static List<Tle> ParseElements(string[] lines, bool threeLine)
        {
            return lines // take the file
                .Select((value, index) =>
                    new {PairNum = index / (threeLine ? 3 : 2), value}) // pair TLEs by index
                .GroupBy(pair => pair.PairNum) // group TLEs by index
                .Select(grp => grp.Select(g => g.value).ToArray()) // select groups of TLEs
                .Select(s => s.Length == 2 ? new Tle(s[0], s[1]) : new Tle(ExtractSatName(s[0]), s[1], s[2]))
                // convert lines into TLEs
                .ToList();
        }

        private static string ExtractSatName(string s)
        {
            return (s.StartsWith("0 ") ? s.Substring(2) : s).Trim();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Norad Number:         {NoradNumber}");
            builder.AppendLine($"Int. Designator:      {IntDesignator}");
            builder.AppendLine($"GetEpoch:             {Epoch}");
            builder.AppendLine($"Orbit Number:         {OrbitNumber}");
            builder.AppendLine($"Mean Motion Dt2:      {MeanMotionDtOver2}");
            builder.AppendLine($"Mean Motion Ddt6:     {MeanMotionDdtOver6}");
            builder.AppendLine($"GetEccentricity:      {Eccentricity}");
            builder.AppendLine($"GetBStar:             {BStarDragTerm}");
            builder.AppendLine($"GetInclination:       {Inclination}");
            builder.AppendLine($"Right Ascending Node: {RightAscendingNode}");
            builder.AppendLine($"Argument GetPerigee:  {ArgumentPerigee}");
            builder.AppendLine($"Mean Anomaly:         {MeanAnomaly}");
            builder.AppendLine($"Mean Motion:          {MeanMotionRevPerDay}");

            return builder.ToString();
        }

        private void Initialize()
        {
            if (!IsValidLineLength(Line1))
                throw new TleException("Invalid length for line one");

            if (!IsValidLineLength(Line2))
                throw new TleException("Invalid length for line two");

            if (Line1[0] != '1')
                throw new TleException("Invalid line beginning for line one");

            if (Line2[0] != '2')
                throw new TleException("Invalid line beginning for line two");

            ExtractInteger(Line1.Substring(Tle1ColNoradnum,
                Tle1LenNoradnum), out var satNumber1);
            ExtractInteger(Line2.Substring(Tle2ColNoradnum,
                Tle2LenNoradnum), out var satNumber2);

            if (satNumber1 != satNumber2)
                throw new TleException("Satellite numbers do not match");

            NoradNumber = satNumber1;

            if (Name == "")
                Name = Line1.Substring(Tle1ColNoradnum, Tle1LenNoradnum);

            IntDesignator = Line1.Substring(Tle1ColIntldescA,
                Tle1LenIntldescA + Tle1LenIntldescB + Tle1LenIntldescC);

            ExtractInteger(Line1.Substring(Tle1ColEpochA,
                Tle1LenEpochA), out var year);
            ExtractDouble(Line1.Substring(Tle1ColEpochB,
                Tle1LenEpochB), 4, out var day);
            ExtractDouble(Line1.Substring(Tle1ColMeanmotiondt2,
                Tle1LenMeanmotiondt2), 2, out _meanMotionDt2);
            ExtractExponential(Line1.Substring(Tle1ColMeanmotionddt6,
                Tle1LenMeanmotionddt6), out _meanMotionDdt6);
            ExtractExponential(Line1.Substring(Tle1ColBstar,
                Tle1LenBstar), out _bstar);

            ExtractDouble(Line2.Substring(Tle2ColInclination,
                Tle2LenInclination), 4, out _inclination);
            ExtractDouble(Line2.Substring(Tle2ColRaascendnode,
                Tle2LenRaascendnode), 4, out _rightAscendingNode);
            ExtractDouble(Line2.Substring(Tle2ColEccentricity,
                Tle2LenEccentricity), -1, out _eccentricity);
            ExtractDouble(Line2.Substring(Tle2ColArgperigee,
                Tle2LenArgperigee), 4, out _argumentPerigee);
            ExtractDouble(Line2.Substring(Tle2ColMeananomaly,
                Tle2LenMeananomaly), 4, out _meanAnomaly);
            ExtractDouble(Line2.Substring(Tle2ColMeanmotion,
                Tle2LenMeanmotion), 3, out _meanMotion);
            ExtractInteger(Line2.Substring(Tle2ColRevatepoch,
                Tle2LenRevatepoch), out _orbitNumber);

            if (year < 57)
                year += 2000;
            else
                year += 1900;
            Epoch = new DateTime((int) year, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(day - 1);
        }

        private static bool IsValidLineLength(string str)
        {
            return str.Length == TleLenLineData;
        }

        private static void ExtractInteger(string str, out uint val)
        {
            var foundDigit = false;
            uint temp = 0;

            for (var i = 0; i != str.Length; ++i)
                if (char.IsDigit(str[i]))
                {
                    foundDigit = true;
                    temp = temp * 10 + (uint) (str[i] - '0');
                }
                else if (foundDigit)
                {
                    throw new TleException("Unexpected non digit");
                }
                else if (str[i] != ' ')
                {
                    throw new TleException("Invalid character");
                }

            val = !foundDigit ? 0 : temp;
        }

        private static void ExtractDouble(string str, int pointPos, out double val)
        {
            if (pointPos == -1)
                str = "0." + str;
            val = double.Parse(str, CultureInfo.InvariantCulture);
        }

        private static void ExtractExponential(string str, out double val)
        {
            //24909-3
            //2.4909e-3
            //  .00000-E0
            // "0.0000E-0" string
            var correctedString = "";

            if (str[0] == '-')
            {
                correctedString += "-";

                // requires LastIndexOf to skip the first '-' in the string
                if (str.LastIndexOf("+") > 1)
                    correctedString += "0." + str.Substring(1, str.LastIndexOf("+") - 1) + "E" +
                                       str.Substring(str.LastIndexOf("+"));
                else
                    correctedString += "0." + str.Substring(1, str.LastIndexOf("-") - 1) + "E" +
                                       str.Substring(str.LastIndexOf("-"));
            }
            else
            {
                if (str.LastIndexOf("+") > 1)
                    correctedString += "0." + str.Substring(1, str.LastIndexOf("+") - 1) + "E" +
                                       str.Substring(str.LastIndexOf("+"));
                else
                    correctedString += "0." + str.Substring(1, str.IndexOf("-") - 1) + "E" +
                                       str.Substring(str.IndexOf("-"));
            }

            val = (double) decimal.Parse(correctedString, NumberStyles.Float, CultureInfo.InvariantCulture);

            var temp = "";

            for (var i = 0; i != str.Length; ++i)
                if (i == 0)
                {
                    if (str[i] == '-' || str[i] == '+' || str[i] == ' ')
                    {
                        if (str[i] == '-')
                            temp += str[i];
                        temp += '0';
                        temp += '.';
                    }
                    else
                    {
                        throw new TleException("Invalid sign");
                    }
                }
                else if (i == 0 + str.Length - 2)
                {
                    if (str[i] == '-' || str[i] == '+')
                    {
                        temp += 'e';
                        temp += str[i];
                    }
                    else
                    {
                        throw new TleException("Invalid exponential sign");
                    }
                }
                else
                {
                    if (char.IsDigit(str[i]))
                        temp += str[i];
                    else
                        throw new TleException("Invalid digit");
                }

            if (!double.TryParse(temp, NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                throw new TleException("Failed to convert value to double");
        }

        /// <inheritdoc />
        protected bool Equals(Tle other)
        {
            return string.Equals(Line1, other.Line1) && string.Equals(Line2, other.Line2);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Tle t && Equals(t);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Line1.GetHashCode() * 397) ^ Line2.GetHashCode();
            }
        }

        /// <inheritdoc />
        public static bool operator ==(Tle left, Tle right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc />
        public static bool operator !=(Tle left, Tle right)
        {
            return !Equals(left, right);
        }
    }
}