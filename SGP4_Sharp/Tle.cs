/*
 * Copyright 2013 Daniel Warner <contact@danrw.com>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Text;


namespace SGP4_Sharp
{


    /**
 * @brief Processes a two-line element set used to convey OrbitalElements.
 *
 * Used to extract the various raw fields from a two-line element set.
 */
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

        private double _meanMotionDt2;
        private double _meanMotionDdt6;
        private double _bstar;
        private double _inclination;
        private double _rightAscendingNode;
        private double _eccentricity;
        private double _argumentPerigee;
        private double _meanAnomaly;
        private double _meanMotion;
        private uint _orbitNumber;

        private const uint TleLenLineData = 69;

        public string Name { get; private set; }
        public string Line1 { get; }
        public string Line2 { get; }
        public uint NoradNumber { get; private set; }
        public string IntDesignator { get; private set; }
        public DateTime Epoch { get; private set; }
        public double MeanMotionDtOver2 => _meanMotionDt2;
        public double MeanMotionDdtOver6 => _meanMotionDdt6;
        public double BStarDragTerm => _bstar;
        public double MeanMotionRevPerDay => _meanMotion;
        public uint OrbitNumber => _orbitNumber;
        public double Eccentricity => _eccentricity;

        /// <summary>
        /// Initialise TLE with two lines
        /// </summary>
        /// <param name="lineOne"></param>
        /// <param name="lineTwo"></param>
        public Tle(string lineOne, string lineTwo)
        {
            Line1 = lineOne;
            Line2 = lineTwo;
            Initialize();
        }

        /// <summary>
        /// Initialise TLE with two lines and a name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lineOne"></param>
        /// <param name="lineTwo"></param>
        public Tle(string name, string lineOne, string lineTwo)
        {
            Name = name;
            Line1 = lineOne;
            Line2 = lineTwo;
            Initialize();
        }

        /// <summary>
        /// Initialise TLE with another TLE
        /// </summary>
        /// <param name="tle">the TLE to copy</param>
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
        /// Gets the inclination
        /// </summary>
        /// <param name="inDegrees">True to return value in degrees</param>
        /// <returns></returns>
        public double GetInclination(bool inDegrees)
        {
            return inDegrees ? _inclination : Util.DegreesToRadians(_inclination);
        }

        /// <summary>
        /// Gets the right ascension of the ascending node
        /// </summary>
        /// <param name="inDegrees">True to return value in degrees</param>
        /// <returns></returns>
        public double GetRightAscendingNode(bool inDegrees)
        {
            return inDegrees ? _rightAscendingNode : Util.DegreesToRadians(_rightAscendingNode);
        }

        /// <summary>
        /// Gets the argument of perigee
        /// </summary>
        /// <param name="inDegrees">True to return value in degrees</param>
        /// <returns></returns>
        public double GetArgumentPerigee(bool inDegrees)
        {
            return inDegrees ? _argumentPerigee : Util.DegreesToRadians(_argumentPerigee);
        }

        /// <summary>
        /// Gets the mean anomaly
        /// </summary>
        /// <param name="inDegrees">True to return value in degrees</param>
        /// <returns></returns>
        public double GetMeanAnomaly(bool inDegrees)
        {
            return inDegrees ? _meanAnomaly : Util.DegreesToRadians(_meanAnomaly);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Norad Number:         {NoradNumber}");
            builder.AppendLine($"Int. Designator:      {IntDesignator}");
            builder.AppendLine($"GetEpoch:                {Epoch}");
            builder.AppendLine($"Orbit Number:         {OrbitNumber}");
            builder.AppendLine($"Mean Motion Dt2:      {MeanMotionDtOver2}");
            builder.AppendLine($"Mean Motion Ddt6:     {MeanMotionDdtOver6}");
            builder.AppendLine($"GetEccentricity:         {Eccentricity}");
            builder.AppendLine($"GetBStar:                {BStarDragTerm}");
            builder.AppendLine($"GetInclination:          {GetInclination(true)}");
            builder.AppendLine($"Right Ascending Node: {GetRightAscendingNode(true)}");
            builder.AppendLine($"Argument GetPerigee:     {GetArgumentPerigee(true)}");
            builder.AppendLine($"Mean Anomaly:         {GetMeanAnomaly(true)}");
            builder.AppendLine($"Mean Motion:          {MeanMotionRevPerDay}");

            return builder.ToString();
        }

        private void Initialize()
        {
            if (!IsValidLineLength(Line1))
            {
                throw new TleException("Invalid length for line one");
            }

            if (!IsValidLineLength(Line2))
            {
                throw new TleException("Invalid length for line two");
            }

            if (Line1[0] != '1')
            {
                throw new TleException("Invalid line beginning for line one");
            }

            if (Line2[0] != '2')
            {
                throw new TleException("Invalid line beginning for line two");
            }

            uint satNumber1 = 0;
            uint satNumber2 = 0;

            ExtractInteger(Line1.Substring(Tle1ColNoradnum,
                    Tle1LenNoradnum), ref satNumber1);
            ExtractInteger(Line2.Substring(Tle2ColNoradnum,
                    Tle2LenNoradnum), ref satNumber2);

            if (satNumber1 != satNumber2)
            {
                throw new TleException("Satellite numbers do not match");
            }

            NoradNumber = satNumber1;

            if (Name == "")
            {
                Name = Line1.Substring(Tle1ColNoradnum, Tle1LenNoradnum);
            }

            IntDesignator = Line1.Substring(Tle1ColIntldescA,
                Tle1LenIntldescA + Tle1LenIntldescB + Tle1LenIntldescC);

            uint year = 0;
            var day = 0.0;

            ExtractInteger(Line1.Substring(Tle1ColEpochA,
                    Tle1LenEpochA), ref year);
            ExtractDouble(Line1.Substring(Tle1ColEpochB,
                    Tle1LenEpochB), 4, ref day);
            ExtractDouble(Line1.Substring(Tle1ColMeanmotiondt2,
                    Tle1LenMeanmotiondt2), 2, ref _meanMotionDt2);
            ExtractExponential(Line1.Substring(Tle1ColMeanmotionddt6,
                    Tle1LenMeanmotionddt6), ref _meanMotionDdt6);
            ExtractExponential(Line1.Substring(Tle1ColBstar,
                    Tle1LenBstar), ref _bstar);

            /*
     * line 2
     */
            ExtractDouble(Line2.Substring(Tle2ColInclination,
                    Tle2LenInclination), 4, ref _inclination);
            ExtractDouble(Line2.Substring(Tle2ColRaascendnode,
                    Tle2LenRaascendnode), 4, ref _rightAscendingNode);
            ExtractDouble(Line2.Substring(Tle2ColEccentricity,
                    Tle2LenEccentricity), -1, ref _eccentricity);
            ExtractDouble(Line2.Substring(Tle2ColArgperigee,
                    Tle2LenArgperigee), 4, ref _argumentPerigee);
            ExtractDouble(Line2.Substring(Tle2ColMeananomaly,
                    Tle2LenMeananomaly), 4, ref _meanAnomaly);
            ExtractDouble(Line2.Substring(Tle2ColMeanmotion,
                    Tle2LenMeanmotion), 3, ref _meanMotion);
            ExtractInteger(Line2.Substring(Tle2ColRevatepoch,
                    Tle2LenRevatepoch), ref _orbitNumber);

            if (year < 57)
                year += 2000;
            else
                year += 1900;
            Epoch = new DateTime((int)year, 1, 1).AddDays(day - 1);
        }

        private static bool IsValidLineLength(string str)
        {
            return str.Length == TleLenLineData;
        }

        private static void ExtractInteger(string str, ref uint val)
        {
            var foundDigit = false;
            uint temp = 0;

            for (var i = 0; i != str.Length; ++i)
            {
                if (char.IsDigit(str[i]))
                {
                    foundDigit = true;
                    temp = (temp * 10) + (uint)(str[i] - '0');
                }
                else if (foundDigit)
                {
                    throw new TleException("Unexpected non digit");
                }
                else if (str[i] != ' ')
                {
                    throw new TleException("Invalid character");
                }
            }

            val = !foundDigit ? 0 : temp;
        }

        private static void ExtractDouble(string str, int pointPos, ref double val)
        {
            if (pointPos == -1)
            {
                // Add decimal point at the beginning
                str = "0." + str;
            }
            val = double.Parse(str);
        }

        private static void ExtractExponential(string str, ref double val)
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
                correctedString += "0." + str.Substring(1, str.LastIndexOf("-") - 1) + "E" + str.Substring(str.LastIndexOf("-"));
            }
            else
            {
                correctedString += "0." + str.Substring(1, str.IndexOf("-") - 1) + "E" + str.Substring(str.IndexOf("-"));
            }

            val = (double)decimal.Parse(correctedString, System.Globalization.NumberStyles.Float);

            var temp = "";

            for (var i = 0; i != str.Length; ++i)
            {
                if (i == 0)
                {
                    if (str[i] == '-' || str[i] == '+' || str[i] == ' ')
                    {
                        if (str[i] == '-')
                        {
                            temp += str[i];
                        }
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
                    {
                        temp += str[i];
                    }
                    else
                    {
                        throw new TleException("Invalid digit");
                    }
                }
            }

            if (!double.TryParse(temp, out val))
            {
                throw new TleException("Failed to convert value to double");
            }
        }
    }
}
