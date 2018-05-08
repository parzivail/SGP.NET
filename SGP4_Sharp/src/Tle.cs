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
        const int TLE1_COL_NORADNUM = 2;
        const int TLE1_LEN_NORADNUM = 5;
        const int TLE1_COL_INTLDESC_A = 9;
        const int TLE1_LEN_INTLDESC_A = 2;
        const int TLE1_COL_INTLDESC_B = 11;
        const int TLE1_LEN_INTLDESC_B = 3;
        const int TLE1_COL_INTLDESC_C = 14;
        const int TLE1_LEN_INTLDESC_C = 3;
        const int TLE1_COL_EPOCH_A = 18;
        const int TLE1_LEN_EPOCH_A = 2;
        const int TLE1_COL_EPOCH_B = 20;
        const int TLE1_LEN_EPOCH_B = 12;
        const int TLE1_COL_MEANMOTIONDT2 = 33;
        const int TLE1_LEN_MEANMOTIONDT2 = 10;
        const int TLE1_COL_MEANMOTIONDDT6 = 44;
        const int TLE1_LEN_MEANMOTIONDDT6 = 8;
        const int TLE1_COL_BSTAR = 53;
        const int TLE1_LEN_BSTAR = 8;
        const int TLE1_COL_EPHEMTYPE = 62;
        const int TLE1_LEN_EPHEMTYPE = 1;
        const int TLE1_COL_ELNUM = 64;
        const int TLE1_LEN_ELNUM = 4;

        const int TLE2_COL_NORADNUM = 2;
        const int TLE2_LEN_NORADNUM = 5;
        const int TLE2_COL_INCLINATION = 8;
        const int TLE2_LEN_INCLINATION = 8;
        const int TLE2_COL_RAASCENDNODE = 17;
        const int TLE2_LEN_RAASCENDNODE = 8;
        const int TLE2_COL_ECCENTRICITY = 26;
        const int TLE2_LEN_ECCENTRICITY = 7;
        const int TLE2_COL_ARGPERIGEE = 34;
        const int TLE2_LEN_ARGPERIGEE = 8;
        const int TLE2_COL_MEANANOMALY = 43;
        const int TLE2_LEN_MEANANOMALY = 8;
        const int TLE2_COL_MEANMOTION = 52;
        const int TLE2_LEN_MEANMOTION = 11;
        const int TLE2_COL_REVATEPOCH = 63;
        const int TLE2_LEN_REVATEPOCH = 5;

        /**
     * @details Initialise given the two lines of a tle
     * @param[in] line_one Tle line one
     * @param[in] line_two Tle line two
     */
        public Tle(string line_one, string line_two)
        {
            line_one_ = line_one;
            line_two_ = line_two;
            Initialize();
        }

        /**
     * @details Initialise given the satellite name and the two lines of a tle
     * @param[in] name Satellite name
     * @param[in] line_one Tle line one
     * @param[in] line_two Tle line two
     */
        public Tle(string name, string line_one, string line_two)
        {
            name_ = name;
            line_one_ = line_one;
            line_two_ = line_two;
            Initialize();
        }

        /**
     * Copy constructor
     * @param[in] tle Tle object to copy from
     */
        public Tle(Tle tle)
        {
            name_ = tle.name_;
            line_one_ = tle.line_one_;
            line_two_ = tle.line_two_;

            norad_number_ = tle.norad_number_;
            int_designator_ = tle.int_designator_;
            epoch_ = tle.epoch_;
            mean_motion_dt2_ = tle.mean_motion_dt2_;
            mean_motion_ddt6_ = tle.mean_motion_ddt6_;
            bstar_ = tle.bstar_;
            inclination_ = tle.inclination_;
            right_ascending_node_ = tle.right_ascending_node_;
            eccentricity_ = tle.eccentricity_;
            argument_perigee_ = tle.argument_perigee_;
            mean_anomaly_ = tle.mean_anomaly_;
            mean_motion_ = tle.mean_motion_;
            orbit_number_ = tle.orbit_number_;
        }

        /**
     * Get the satellite name
     * @returns the satellite name
     */
        public string Name()
        {
            return name_;
        }

        /**
     * Get the first line of the tle
     * @returns the first line of the tle
     */
        public string Line1()
        {
            return line_one_;
        }

        /**
     * Get the second line of the tle
     * @returns the second line of the tle
     */
        public string Line2()
        {
            return line_two_;
        }

        /**
     * Get the norad number
     * @returns the norad number
     */
        public UInt32 NoradNumber()
        {
            return norad_number_;
        }

        /**
     * Get the international designator
     * @returns the international designator
     */
        public string IntDesignator()
        {
            return int_designator_;
        }

        /**
     * Get the tle epoch
     * @returns the tle epoch
     */
        public DateTime Epoch()
        {
            return epoch_;
        }

        /**
     * Get the first time derivative of the mean motion divided by two
     * @returns the first time derivative of the mean motion divided by two
     */
        public double MeanMotionDt2()
        {
            return mean_motion_dt2_;
        }

        /**
     * Get the second time derivative of mean motion divided by six
     * @returns the second time derivative of mean motion divided by six
     */
        public double MeanMotionDdt6()
        {
            return mean_motion_ddt6_;
        }

        /**
     * Get the BSTAR drag term
     * @returns the BSTAR drag term
     */
        public double BStar()
        {
            return bstar_;
        }

        /**
     * Get the inclination
     * @param in_degrees Whether to return the value in degrees or radians
     * @returns the inclination
     */
        public double Inclination(bool in_degrees)
        {
            if (in_degrees)
            {
                return inclination_;
            }
            else
            {
                return Util.DegreesToRadians(inclination_);
            }
        }

        /**
     * Get the right ascension of the ascending node
     * @param in_degrees Whether to return the value in degrees or radians
     * @returns the right ascension of the ascending node
     */
        public double RightAscendingNode(bool in_degrees)
        {
            if (in_degrees)
            {
                return right_ascending_node_;
            }
            else
            {
                return Util.DegreesToRadians(right_ascending_node_);
            }
        }

        /**
     * Get the eccentricity
     * @returns the eccentricity
     */
        public double Eccentricity()
        {
            return eccentricity_;
        }

        /**
     * Get the argument of perigee
     * @param in_degrees Whether to return the value in degrees or radians
     * @returns the argument of perigee
     */
        public double ArgumentPerigee(bool in_degrees)
        {
            if (in_degrees)
            {
                return argument_perigee_;
            }
            else
            {
                return Util.DegreesToRadians(argument_perigee_);
            }
        }

        /**
     * Get the mean anomaly
     * @param in_degrees Whether to return the value in degrees or radians
     * @returns the mean anomaly
     */
        public double MeanAnomaly(bool in_degrees)
        {
            if (in_degrees)
            {
                return mean_anomaly_;
            }
            else
            {
                return Util.DegreesToRadians(mean_anomaly_);
            }
        }

        /**
     * Get the mean motion
     * @returns the mean motion (revolutions per day)
     */
        public double MeanMotion()
        {
            return mean_motion_;
        }

        /**
     * Get the orbit number
     * @returns the orbit number
     */
        public uint OrbitNumber()
        {
            return orbit_number_;
        }

        /**
     * Get the expected tle line length
     * @returns the tle line length
     */
        public static uint LineLength()
        {
            return TLE_LEN_LINE_DATA;
        }

        /**
     * Dump this object to a string
     * @returns string
     */
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(String.Format("Norad Number:         {0}", NoradNumber()));
            builder.AppendLine(String.Format("Int. Designator:      {0}", IntDesignator()));
            builder.AppendLine(String.Format("Epoch:                {0}", Epoch()));
            builder.AppendLine(String.Format("Orbit Number:         {0}", OrbitNumber()));
            builder.AppendLine(String.Format("Mean Motion Dt2:      {0}", MeanMotionDt2()));
            builder.AppendLine(String.Format("Mean Motion Ddt6:     {0}", MeanMotionDdt6()));
            builder.AppendLine(String.Format("Eccentricity:         {0}", Eccentricity()));
            builder.AppendLine(String.Format("BStar:                {0}", BStar()));
            builder.AppendLine(String.Format("Inclination:          {0}", Inclination(true)));
            builder.AppendLine(String.Format("Right Ascending Node: {0}", RightAscendingNode(true)));
            builder.AppendLine(String.Format("Argument Perigee:     {0}", ArgumentPerigee(true)));
            builder.AppendLine(String.Format("Mean Anomaly:         {0}", MeanAnomaly(true)));
            builder.AppendLine(String.Format("Mean Motion:          {0}", MeanMotion()));
      
            return builder.ToString();
        }

        private void Initialize()
        {
            if (!IsValidLineLength(line_one_))
            {
                throw new TleException("Invalid length for line one");
            }

            if (!IsValidLineLength(line_two_))
            {
                throw new TleException("Invalid length for line two");
            }

            if (line_one_[0] != '1')
            {
                throw new TleException("Invalid line beginning for line one");
            }
        
            if (line_two_[0] != '2')
            {
                throw new TleException("Invalid line beginning for line two");
            }

            uint sat_number_1 = 0;
            uint sat_number_2 = 0;

            ExtractInteger(line_one_.Substring(TLE1_COL_NORADNUM,
                    TLE1_LEN_NORADNUM), ref sat_number_1);
            ExtractInteger(line_two_.Substring(TLE2_COL_NORADNUM,
                    TLE2_LEN_NORADNUM), ref sat_number_2);

            if (sat_number_1 != sat_number_2)
            {
                throw new TleException("Satellite numbers do not match");
            }

            norad_number_ = sat_number_1;

            if (name_ == "")
            {
                name_ = line_one_.Substring(TLE1_COL_NORADNUM, TLE1_LEN_NORADNUM);
            }

            int_designator_ = line_one_.Substring(TLE1_COL_INTLDESC_A,
                TLE1_LEN_INTLDESC_A + TLE1_LEN_INTLDESC_B + TLE1_LEN_INTLDESC_C);

            uint year = 0;
            double day = 0.0;

            ExtractInteger(line_one_.Substring(TLE1_COL_EPOCH_A,
                    TLE1_LEN_EPOCH_A), ref year);
            ExtractDouble(line_one_.Substring(TLE1_COL_EPOCH_B,
                    TLE1_LEN_EPOCH_B), 4, ref day);
            ExtractDouble(line_one_.Substring(TLE1_COL_MEANMOTIONDT2,
                    TLE1_LEN_MEANMOTIONDT2), 2, ref mean_motion_dt2_);
            ExtractExponential(line_one_.Substring(TLE1_COL_MEANMOTIONDDT6,
                    TLE1_LEN_MEANMOTIONDDT6), ref mean_motion_ddt6_);
            ExtractExponential(line_one_.Substring(TLE1_COL_BSTAR,
                    TLE1_LEN_BSTAR), ref bstar_);

            /*
     * line 2
     */
            ExtractDouble(line_two_.Substring(TLE2_COL_INCLINATION,
                    TLE2_LEN_INCLINATION), 4, ref inclination_);
            ExtractDouble(line_two_.Substring(TLE2_COL_RAASCENDNODE,
                    TLE2_LEN_RAASCENDNODE), 4, ref right_ascending_node_);
            ExtractDouble(line_two_.Substring(TLE2_COL_ECCENTRICITY,
                    TLE2_LEN_ECCENTRICITY), -1, ref eccentricity_);
            ExtractDouble(line_two_.Substring(TLE2_COL_ARGPERIGEE,
                    TLE2_LEN_ARGPERIGEE), 4, ref argument_perigee_);
            ExtractDouble(line_two_.Substring(TLE2_COL_MEANANOMALY,
                    TLE2_LEN_MEANANOMALY), 4, ref mean_anomaly_);
            ExtractDouble(line_two_.Substring(TLE2_COL_MEANMOTION,
                    TLE2_LEN_MEANMOTION), 3, ref mean_motion_);
            ExtractInteger(line_two_.Substring(TLE2_COL_REVATEPOCH,
                    TLE2_LEN_REVATEPOCH), ref orbit_number_);
    
            if (year < 57)
                year += 2000;
            else
                year += 1900;
            epoch_ = new DateTime(year, day);
        }

        private static bool IsValidLineLength(string str)
        {
            return str.Length == LineLength() ? true : false;
        }

        private void ExtractInteger(string str, ref uint val)
        {
            bool found_digit = false;
            uint temp = 0;


            for (int i = 0; i != str.Length; ++i)
            {
                if (Char.IsDigit(str[i]))
                {
                    found_digit = true;
                    temp = (temp * 10) + (uint)(str[i] - '0');
                }
                else if (found_digit)
                {
                    throw new TleException("Unexpected non digit");
                }
                else if (str[i] != ' ')
                {
                    throw new TleException("Invalid character");
                }
            }

            if (!found_digit)
            {
                val = 0;
            }
            else
            {
                val = temp;
            }
        }

        private void ExtractDouble(string str, int point_pos, ref double val)
        {
            if (point_pos == -1)
            {
                // Add decimal point at the beginning
                str = "0." + str;
            }
            val = double.Parse(str);
//
//    string temp;
//    bool found_digit = false;
//
//    for (std::string::const_iterator i = str.begin(); i != str.end(); ++i)
//    {
//        /*
//         * integer part
//         */
//        if (point_pos >= 0 && i < str.begin() + point_pos - 1)
//        {
//            bool done = false;
//
//            if (i == str.begin())
//            {
//                if(*i == '-' || *i == '+')
//                {
//                    /*
//                     * first character could be signed
//                     */
//                    temp += *i;
//                    done = true;
//                }
//            }
//
//            if (!done)
//            {
//                if (isdigit(*i))
//                {
//                    found_digit = true;
//                    temp += *i;
//                }
//                else if (found_digit)
//                {
//                    throw TleException("Unexpected non digit");
//                }
//                else if (*i != ' ')
//                {
//                    throw TleException("Invalid character");
//                }
//            }
//        }
//        /*
//         * decimal point
//         */
//        else if (point_pos >= 0 && i == str.begin() + point_pos - 1)
//        {
//            if (temp.length() == 0)
//            {
//                /*
//                 * integer part is blank, so add a '0'
//                 */
//                temp += '0';
//            }
//
//            if (*i == '.')
//            {
//                /*
//                 * decimal point found
//                 */
//                temp += *i;
//            }
//            else
//            {
//                throw TleException("Failed to find decimal point");
//            }
//        }
//        /*
//         * fraction part
//         */
//        else
//        {
//            if (i == str.begin() && point_pos == -1)
//            {
//                /*
//                 * no decimal point expected, add 0. beginning
//                 */
//                temp += '0';
//                temp += '.';
//            }
//            
//            /*
//             * should be a digit
//             */
//            if (isdigit(*i))
//            {
//                temp += *i;
//            }
//            else
//            {
//                throw TleException("Invalid digit");
//            }
//        }
//    }
//
//    if (!Util::FromString<double>(temp, val))
//    {
//        throw TleException("Failed to convert value to double");
//    }
        }

        private void ExtractExponential(string str, ref double val)
        {
            //24909-3
            //2.4909e-3
            //  .00000-E0
            // "0.0000E-0" string
            string correctedString = "";

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

            val = (double)Decimal.Parse(correctedString, System.Globalization.NumberStyles.Float);

            string temp = "";

            for (int i = 0; i != str.Length; ++i)
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
                    if (Char.IsDigit(str[i]))
                    {
                        temp += str[i];
                    }
                    else
                    {
                        throw new TleException("Invalid digit");
                    }
                }
            }
 
            if (!Double.TryParse(temp, out val))
            {
                throw new TleException("Failed to convert value to double");
            }
        }


        private string name_;
        private string line_one_;
        private string line_two_;

        private uint norad_number_;
        private string int_designator_;
        private DateTime epoch_;
        private double mean_motion_dt2_;
        private double mean_motion_ddt6_;
        private double bstar_;
        private double inclination_;
        private double right_ascending_node_;
        private double eccentricity_;
        private double argument_perigee_;
        private double mean_anomaly_;
        private double mean_motion_;
        private uint orbit_number_;

        private static uint TLE_LEN_LINE_DATA = 69;
        private static uint TLE_LEN_LINE_NAME = 22;
    };

}
