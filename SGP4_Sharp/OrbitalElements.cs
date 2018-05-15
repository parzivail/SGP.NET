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

namespace SGP4_Sharp
{
    /// <summary>
    /// Container for the extracted orbital elements used by the SGP4 propagator.
    /// </summary>
    public class OrbitalElements
    {
        private readonly double _meanAnomoly;
        private readonly double _ascendingNode;
        private readonly double _argumentPerigee;
        private readonly double _eccentricity;
        private readonly double _inclination;
        private readonly double _meanMotion;
        private readonly double _bstar;
        private readonly double _recoveredSemiMajorAxis;
        private readonly double _recoveredMeanMotion;
        private readonly double _perigee;
        private readonly double _period;
        private readonly DateTime _epoch;

        public OrbitalElements(Tle tle)
        {
            /*
           * extract and format tle data
           */
            _meanAnomoly = tle.GetMeanAnomaly(false);
            _ascendingNode = tle.GetRightAscendingNode(false);
            _argumentPerigee = tle.GetArgumentPerigee(false);
            _eccentricity = tle.Eccentricity;
            _inclination = tle.GetInclination(false);
            _meanMotion = tle.MeanMotionRevPerDay * Global.KTwopi / Global.KMinutesPerDay;
            _bstar = tle.BStarDragTerm;
            _epoch = tle.Epoch;

            /*
           * recover original mean motion (xnodp) and semimajor axis (aodp)
           * from input elements
           */
            var a1 = Math.Pow(Global.KXke / GetMeanMotion(), Global.KTwothird);
            var cosio = Math.Cos(GetInclination());
            var theta2 = cosio * cosio;
            var x3Thm1 = 3.0 * theta2 - 1.0;
            var eosq = GetEccentricity() * GetEccentricity();
            var betao2 = 1.0 - eosq;
            var betao = Math.Sqrt(betao2);
            var temp = (1.5 * Global.KCk2) * x3Thm1 / (betao * betao2);
            var del1 = temp / (a1 * a1);
            var a0 = a1 * (1.0 - del1 * (1.0 / 3.0 + del1 * (1.0 + del1 * 134.0 / 81.0)));
            var del0 = temp / (a0 * a0);

            _recoveredMeanMotion = GetMeanMotion() / (1.0 + del0);
            /*
           * alternative way to calculate
           * doesnt affect final results
           * recovered_semi_major_axis_ = pow(XKE / GetRecoveredMeanMotion(), TWOTHIRD);
           */
            _recoveredSemiMajorAxis = a0 / (1.0 - del0);

            /*
           * find perigee and period
           */
            _perigee = (GetRecoveredSemiMajorAxis() * (1.0 - GetEccentricity()) - Global.KAe) * Global.EarthRadiusKm;
            _period = Global.KTwopi / GetRecoveredMeanMotion();
        }

        /*
         * XMO
         */
        public double GetMeanAnomoly()
        {
            return _meanAnomoly;
        }

        /*
         * XNODEO
         */
        public double GetAscendingNode()
        {
            return _ascendingNode;
        }

        /*
         * OMEGAO
         */
        public double GetArgumentPerigee()
        {
            return _argumentPerigee;
        }

        /*
         * EO
         */
        public double GetEccentricity()
        {
            return _eccentricity;
        }

        /*
         * XINCL
         */
        public double GetInclination()
        {
            return _inclination;
        }

        /*
         * XNO
         */
        public double GetMeanMotion()
        {
            return _meanMotion;
        }

        /*
         * BSTAR
         */
        public double GetBStar()
        {
            return _bstar;
        }

        /*
         * AODP
         */
        public double GetRecoveredSemiMajorAxis()
        {
            return _recoveredSemiMajorAxis;
        }

        /*
         * XNODP
         */
        public double GetRecoveredMeanMotion()
        {
            return _recoveredMeanMotion;
        }

        /*
         * PERIGE
         */
        public double GetPerigee()
        {
            return _perigee;
        }

        /*
         * GetPeriod in minutes
         */
        public double GetPeriod()
        {
            return _period;
        }

        /*
         * EPOCH
         */
        public DateTime GetEpoch()
        {
            return _epoch;
        }
    }

}