using System;

namespace SGP4_Sharp
{
    /// <summary>
    ///     Container for the extracted orbital elements used by the SGP4 propagator.
    /// </summary>
    public class OrbitalElements
    {
        public OrbitalElements(Tle tle)
        {
            // extract and format tle data
            MeanAnomoly = tle.GetMeanAnomaly(false);
            AscendingNode = tle.GetRightAscendingNode(false);
            ArgumentPerigee = tle.GetArgumentPerigee(false);
            Eccentricity = tle.Eccentricity;
            Inclination = tle.GetInclination(false);
            MeanMotion = tle.MeanMotionRevPerDay * Global.KTwopi / Global.KMinutesPerDay;
            BStar = tle.BStarDragTerm;
            Epoch = tle.Epoch;

            // recover original mean motion (xnodp) and semimajor axis (aodp) from input elements
            var a1 = Math.Pow(Global.KXke / MeanMotion, Global.KTwothird);
            var cosio = Math.Cos(Inclination);
            var theta2 = cosio * cosio;
            var x3Thm1 = 3.0 * theta2 - 1.0;
            var eosq = Eccentricity * Eccentricity;
            var betao2 = 1.0 - eosq;
            var betao = Math.Sqrt(betao2);
            var temp = 1.5 * Global.KCk2 * x3Thm1 / (betao * betao2);
            var del1 = temp / (a1 * a1);
            var a0 = a1 * (1.0 - del1 * (1.0 / 3.0 + del1 * (1.0 + del1 * 134.0 / 81.0)));
            var del0 = temp / (a0 * a0);

            RecoveredMeanMotion = MeanMotion / (1.0 + del0);
            RecoveredSemiMajorAxis = a0 / (1.0 - del0);

            // find perigee and period
            Perigee = (RecoveredSemiMajorAxis * (1.0 - Eccentricity) - Global.KAe) * Global.EarthRadiusKm;
            Period = Global.KTwopi / RecoveredMeanMotion;
        }

        /// <summary>
        ///     XMO
        /// </summary>
        public double MeanAnomoly { get; }

        /// <summary>
        ///     XNODEO
        /// </summary>
        public double AscendingNode { get; }

        /// <summary>
        ///     OMEGAO
        /// </summary>
        public double ArgumentPerigee { get; }

        /// <summary>
        ///     XNO
        /// </summary>
        public double MeanMotion { get; }

        /// <summary>
        ///     AODP
        /// </summary>
        public double RecoveredSemiMajorAxis { get; }

        /// <summary>
        ///     XNODP
        /// </summary>
        public double RecoveredMeanMotion { get; }

        public double Perigee { get; }
        public double Period { get; }
        public DateTime Epoch { get; }
        public double BStar { get; }
        public double Eccentricity { get; }
        public double Inclination { get; }

        public override bool Equals(object obj)
        {
            return obj is OrbitalElements elements &&
                   Equals(MeanAnomoly, elements.MeanAnomoly) &&
                   Equals(AscendingNode, elements.AscendingNode) &&
                   Equals(ArgumentPerigee, elements.ArgumentPerigee) &&
                   Equals(MeanMotion, elements.MeanMotion) &&
                   Equals(RecoveredSemiMajorAxis, elements.RecoveredSemiMajorAxis) &&
                   Equals(RecoveredMeanMotion, elements.RecoveredMeanMotion) &&
                   Equals(Perigee, elements.Perigee) &&
                   Equals(Period, elements.Period) &&
                   Equals(Epoch, elements.Epoch) &&
                   Equals(BStar, elements.BStar) &&
                   Equals(Eccentricity, elements.Eccentricity) &&
                   Equals(Inclination, elements.Inclination);
        }

        public override int GetHashCode()
        {
            var hashCode = 410639991;
            hashCode = hashCode * -1521134295 + MeanAnomoly.GetHashCode();
            hashCode = hashCode * -1521134295 + AscendingNode.GetHashCode();
            hashCode = hashCode * -1521134295 + ArgumentPerigee.GetHashCode();
            hashCode = hashCode * -1521134295 + MeanMotion.GetHashCode();
            hashCode = hashCode * -1521134295 + RecoveredSemiMajorAxis.GetHashCode();
            hashCode = hashCode * -1521134295 + RecoveredMeanMotion.GetHashCode();
            hashCode = hashCode * -1521134295 + Perigee.GetHashCode();
            hashCode = hashCode * -1521134295 + Period.GetHashCode();
            hashCode = hashCode * -1521134295 + Epoch.GetHashCode();
            hashCode = hashCode * -1521134295 + BStar.GetHashCode();
            hashCode = hashCode * -1521134295 + Eccentricity.GetHashCode();
            hashCode = hashCode * -1521134295 + Inclination.GetHashCode();
            return hashCode;
        }
    }
}