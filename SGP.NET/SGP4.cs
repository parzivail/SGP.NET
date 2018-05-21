using System;

namespace SGPdotNET
{
    /// <summary>
    ///     The Simplified General Perturbations (Model 4) propagater
    /// </summary>
    public class Sgp4
    {
        private static readonly CommonConstants EmptyCommonConstants = new CommonConstants();
        private static readonly NearSpaceConstants EmptyNearSpaceConstants = new NearSpaceConstants();
        private static readonly DeepSpaceConstants EmptyDeepSpaceConstants = new DeepSpaceConstants();
        private static readonly IntegratorConstants EmptyIntegratorConstants = new IntegratorConstants();
        private static readonly IntegratorParams EmptyIntegratorParams = new IntegratorParams();

        private CommonConstants _commonConsts;
        private DeepSpaceConstants _deepspaceConsts;

        private OrbitalElements _elements;
        private IntegratorConstants _integratorConsts;
        private IntegratorParams _integratorParams;
        private NearSpaceConstants _nearspaceConsts;
        private bool _useDeepSpace;
        private bool _useSimpleModel;

        public Sgp4(Tle tle)
        {
            _elements = new OrbitalElements(tle);
            Initialize();
        }

        public void SetTle(Tle tle)
        {
            _elements = new OrbitalElements(tle);

            Initialize();
        }

        /// <summary>
        ///     Predicts the ECI position of a satellite at a time relative to the satellite's epoch
        /// </summary>
        /// <param name="tsince">Time since the satellite's epoch</param>
        /// <returns>The predicted position of the satellite at a time relative to the satellite's epoch</returns>
        public Eci FindPosition(double tsince)
        {
            return _useDeepSpace ? FindPositionSdp4(tsince) : FindPositionSgp4(tsince);
        }

        /// <summary>
        ///     Predicts the ECI position of a satellite at a specific date and time
        /// </summary>
        /// <param name="date">the date and time to predict</param>
        /// <returns>The predicted position of the satellite at a specific date and time</returns>
        public Eci FindPosition(DateTime date)
        {
            return FindPosition((date - _elements.Epoch).TotalMinutes);
        }

        private static double EvaluateCubicPolynomial(double x, double constant, double linear, double squared,
            double cubed)
        {
            return constant + x * (linear + x * (squared + x * cubed));
        }

        private void Initialize()
        {
            Reset();

            if (_elements.Eccentricity < 0.0 || _elements.Eccentricity > 0.999)
                throw new SatelliteException("GetEccentricity out of range");

            if (_elements.Inclination < 0.0 || _elements.Inclination > Math.PI)
                throw new SatelliteException("GetInclination out of range");

            _commonConsts.Cosio = Math.Cos(_elements.Inclination);
            _commonConsts.Sinio = Math.Sin(_elements.Inclination);
            var theta2 = _commonConsts.Cosio * _commonConsts.Cosio;
            _commonConsts.X3Thm1 = 3.0 * theta2 - 1.0;
            var eosq = _elements.Eccentricity * _elements.Eccentricity;
            var betao2 = 1.0 - eosq;
            var betao = Math.Sqrt(betao2);

            if (_elements.Period >= 225.0)
            {
                _useDeepSpace = true;
            }
            else
            {
                _useDeepSpace = false;

                /*
                 * for perigee less than 220 kilometers, the simple_model flag is set
                 * and the equations are truncated to linear variation in sqrt a and
                 * quadratic variation in mean anomly. also, the c3 term, the
                 * delta omega term and the delta m term are dropped
                 */
                _useSimpleModel = _elements.Perigee < 220.0;
            }

            /*
             * for perigee below 156km, the values of
             * s4 and qoms2t are altered
             */
            var s4 = SgpConstants.S;
            var qoms24 = SgpConstants.Qoms2T;
            if (_elements.Perigee < 156.0)
            {
                s4 = _elements.Perigee - 78.0;
                if (_elements.Perigee < 98.0)
                    s4 = 20.0;
                qoms24 = Math.Pow((120.0 - s4) * SgpConstants.DistanceUnitsPerEarthRadii / SgpConstants.EarthRadiusKm,
                    4.0);
                s4 = s4 / SgpConstants.EarthRadiusKm + SgpConstants.DistanceUnitsPerEarthRadii;
            }

            /*
             * generate constants
             */
            var pinvsq = 1.0
                         / (_elements.RecoveredSemiMajorAxis
                            * _elements.RecoveredSemiMajorAxis
                            * betao2 * betao2);
            var tsi = 1.0 / (_elements.RecoveredSemiMajorAxis - s4);
            _commonConsts.Eta = _elements.RecoveredSemiMajorAxis
                                * _elements.Eccentricity * tsi;
            var etasq = _commonConsts.Eta * _commonConsts.Eta;
            var eeta = _elements.Eccentricity * _commonConsts.Eta;
            var psisq = Math.Abs(1.0 - etasq);
            var coef = qoms24 * Math.Pow(tsi, 4.0);
            var coef1 = coef / Math.Pow(psisq, 3.5);
            var c2 = coef1 * _elements.RecoveredMeanMotion
                     * (_elements.RecoveredSemiMajorAxis
                        * (1.0 + 1.5 * etasq + eeta * (4.0 + etasq))
                        + 0.75 * SgpConstants.Ck2 * tsi / psisq * _commonConsts.X3Thm1
                        * (8.0 + 3.0 * etasq * (8.0 + etasq)));
            _commonConsts.C1 = _elements.BStar * c2;
            _commonConsts.A3Ovk2 = -SgpConstants.ZonalHarmonicJ3 / SgpConstants.Ck2 *
                                   SgpConstants.DistanceUnitsPerEarthRadii * SgpConstants.DistanceUnitsPerEarthRadii *
                                   SgpConstants.DistanceUnitsPerEarthRadii;
            _commonConsts.X1Mth2 = 1.0 - theta2;
            _commonConsts.C4 = 2.0 * _elements.RecoveredMeanMotion
                               * coef1 * _elements.RecoveredSemiMajorAxis * betao2
                               * (_commonConsts.Eta * (2.0 + 0.5 * etasq) + _elements.Eccentricity
                                  * (0.5 + 2.0 * etasq)
                                  - 2.0 * SgpConstants.Ck2 * tsi / (_elements.RecoveredSemiMajorAxis * psisq)
                                  * (-3.0 * _commonConsts.X3Thm1 * (1.0 - 2.0 * eeta + etasq
                                                                    * (1.5 - 0.5 * eeta))
                                     + 0.75 * _commonConsts.X1Mth2 * (2.0 * etasq - eeta *
                                                                      (1.0 + etasq)) *
                                     Math.Cos(2.0 * _elements.ArgumentPerigee)));
            var theta4 = theta2 * theta2;
            var temp1 = 3.0 * SgpConstants.Ck2 * pinvsq * _elements.RecoveredMeanMotion;
            var temp2 = temp1 * SgpConstants.Ck2 * pinvsq;
            var temp3 = 1.25 * SgpConstants.Ck4 * pinvsq * pinvsq * _elements.RecoveredMeanMotion;
            _commonConsts.Xmdot = _elements.RecoveredMeanMotion + 0.5 * temp1 * betao *
                                  _commonConsts.X3Thm1 + 0.0625 * temp2 * betao *
                                  (13.0 - 78.0 * theta2 + 137.0 * theta4);
            var x1M5Th = 1.0 - 5.0 * theta2;
            _commonConsts.Omgdot = -0.5 * temp1 * x1M5Th +
                                   0.0625 * temp2 * (7.0 - 114.0 * theta2 + 395.0 * theta4) +
                                   temp3 * (3.0 - 36.0 * theta2 + 49.0 * theta4);
            var xhdot1 = -temp1 * _commonConsts.Cosio;
            _commonConsts.Xnodot = xhdot1 + (0.5 * temp2 * (4.0 - 19.0 * theta2) + 2.0 * temp3 *
                                             (3.0 - 7.0 * theta2)) * _commonConsts.Cosio;
            _commonConsts.Xnodcf = 3.5 * betao2 * xhdot1 * _commonConsts.C1;
            _commonConsts.T2Cof = 1.5 * _commonConsts.C1;

            if (Math.Abs(_commonConsts.Cosio + 1.0) > 1.5e-12)
                _commonConsts.Xlcof = 0.125 * _commonConsts.A3Ovk2 * _commonConsts.Sinio *
                                      (3.0 + 5.0 * _commonConsts.Cosio) / (1.0 + _commonConsts.Cosio);
            else
                _commonConsts.Xlcof = 0.125 * _commonConsts.A3Ovk2 * _commonConsts.Sinio *
                                      (3.0 + 5.0 * _commonConsts.Cosio) / 1.5e-12;

            _commonConsts.Aycof = 0.25 * _commonConsts.A3Ovk2 * _commonConsts.Sinio;
            _commonConsts.X7Thm1 = 7.0 * theta2 - 1.0;

            if (_useDeepSpace)
            {
                _deepspaceConsts.Gsto = _elements.Epoch.ToGreenwichSiderealTime();

                DeepSpaceInitialise(eosq, _commonConsts.Sinio, _commonConsts.Cosio, betao,
                    theta2, betao2,
                    _commonConsts.Xmdot, _commonConsts.Omgdot, _commonConsts.Xnodot);
            }
            else
            {
                var c3 = 0.0;
                if (_elements.Eccentricity > 1.0e-4)
                    c3 = coef * tsi * _commonConsts.A3Ovk2 * _elements.RecoveredMeanMotion *
                         SgpConstants.DistanceUnitsPerEarthRadii *
                         _commonConsts.Sinio / _elements.Eccentricity;

                _nearspaceConsts.C5 = 2.0 * coef1 * _elements.RecoveredSemiMajorAxis * betao2 * (1.0 + 2.75 *
                                                                                                 (etasq + eeta) +
                                                                                                 eeta * etasq);
                _nearspaceConsts.Omgcof = _elements.BStar * c3 * Math.Cos(_elements.ArgumentPerigee);

                _nearspaceConsts.Xmcof = 0.0;
                if (_elements.Eccentricity > 1.0e-4)
                    _nearspaceConsts.Xmcof = -SgpConstants.TwoThirds * coef * _elements.BStar *
                                             SgpConstants.DistanceUnitsPerEarthRadii / eeta;

                _nearspaceConsts.Delmo = Math.Pow(1.0 + _commonConsts.Eta * Math.Cos(_elements.MeanAnomoly), 3.0);
                _nearspaceConsts.Sinmo = Math.Sin(_elements.MeanAnomoly);

                if (_useSimpleModel) return;

                var c1Sq = _commonConsts.C1 * _commonConsts.C1;
                _nearspaceConsts.D2 = 4.0 * _elements.RecoveredSemiMajorAxis * tsi * c1Sq;
                var temp = _nearspaceConsts.D2 * tsi * _commonConsts.C1 / 3.0;
                _nearspaceConsts.D3 = (17.0 * _elements.RecoveredSemiMajorAxis + s4) * temp;
                _nearspaceConsts.D4 = 0.5 * temp * _elements.RecoveredSemiMajorAxis *
                                      tsi * (221.0 * _elements.RecoveredSemiMajorAxis + 31.0 * s4) * _commonConsts.C1;
                _nearspaceConsts.T3Cof = _nearspaceConsts.D2 + 2.0 * c1Sq;
                _nearspaceConsts.T4Cof = 0.25 * (3.0 * _nearspaceConsts.D3 + _commonConsts.C1 *
                                                 (12.0 * _nearspaceConsts.D2 + 10.0 * c1Sq));
                _nearspaceConsts.T5Cof = 0.2 * (3.0 * _nearspaceConsts.D4 + 12.0 * _commonConsts.C1 *
                                                _nearspaceConsts.D3 + 6.0 * _nearspaceConsts.D2 * _nearspaceConsts.D2 +
                                                15.0 *
                                                c1Sq * (2.0 * _nearspaceConsts.D2 + c1Sq));
            }
        }

        private Eci FindPositionSdp4(double tsince)
        {
            /*
             * update for secular gravity and atmospheric drag
             */
            var xmdf = _elements.MeanAnomoly
                       + _commonConsts.Xmdot * tsince;
            var omgadf = _elements.ArgumentPerigee
                         + _commonConsts.Omgdot * tsince;
            var xnoddf = _elements.AscendingNode
                         + _commonConsts.Xnodot * tsince;

            var tsq = tsince * tsince;
            var xnode = xnoddf + _commonConsts.Xnodcf * tsq;
            var tempa = 1.0 - _commonConsts.C1 * tsince;
            var tempe = _elements.BStar * _commonConsts.C4 * tsince;
            var templ = _commonConsts.T2Cof * tsq;

            var xn = _elements.RecoveredMeanMotion;
            var e = _elements.Eccentricity;
            var xincl = _elements.Inclination;

            DeepSpaceSecular(tsince, ref xmdf, omgadf, xnode, ref e, ref xincl, ref xn);

            if (xn <= 0.0)
                throw new SatelliteException("Error: (xn <= 0.0)");

            var a = Math.Pow(SgpConstants.ReciprocalOfMinutesPerTimeUnit / xn, SgpConstants.TwoThirds) * tempa * tempa;
            e -= tempe;
            var xmam = xmdf + _elements.RecoveredMeanMotion * templ;

            DeepSpacePeriodics(tsince, ref e, ref xincl, ref omgadf, ref xnode, ref xmam);

            /*
             * keeping xincl positive important unless you need to display xincl
             * and dislike negative inclinations
             */
            if (xincl < 0.0)
            {
                xincl = -xincl;
                xnode += Math.PI;
                omgadf -= Math.PI;
            }

            var xl = xmam + omgadf + xnode;
            var omega = omgadf;

            /*
             * fix tolerance for error recognition
             */
            if (e <= -0.001)
                throw new SatelliteException("Error: (e <= -0.001)");

            if (e < 1.0e-6)
                e = 1.0e-6;
            else if (e > 1.0 - 1.0e-6)
                e = 1.0 - 1.0e-6;

            /*
             * re-compute the perturbed values
             */
            var perturbedSinio = Math.Sin(xincl);
            var perturbedCosio = Math.Cos(xincl);

            var perturbedTheta2 = perturbedCosio * perturbedCosio;

            var perturbedX3Thm1 = 3.0 * perturbedTheta2 - 1.0;
            var perturbedX1Mth2 = 1.0 - perturbedTheta2;
            var perturbedX7Thm1 = 7.0 * perturbedTheta2 - 1.0;

            double perturbedXlcof;
            if (Math.Abs(perturbedCosio + 1.0) > 1.5e-12)
                perturbedXlcof = 0.125 * _commonConsts.A3Ovk2 * perturbedSinio
                                 * (3.0 + 5.0 * perturbedCosio) / (1.0 + perturbedCosio);
            else
                perturbedXlcof = 0.125 * _commonConsts.A3Ovk2 * perturbedSinio
                                 * (3.0 + 5.0 * perturbedCosio) / 1.5e-12;

            var perturbedAycof = 0.25 * _commonConsts.A3Ovk2
                                 * perturbedSinio;

            /*
             * using calculated values, find position and velocity
             */
            return CalculateFinalPositionVelocity(tsince, e,
                a, omega, xl, xnode,
                xincl, perturbedXlcof, perturbedAycof,
                perturbedX3Thm1, perturbedX1Mth2, perturbedX7Thm1,
                perturbedCosio, perturbedSinio);
        }

        private Eci FindPositionSgp4(double tsince)
        {
            /*
             * update for secular gravity and atmospheric drag
             */
            var xmdf = _elements.MeanAnomoly
                       + _commonConsts.Xmdot * tsince;
            var omgadf = _elements.ArgumentPerigee
                         + _commonConsts.Omgdot * tsince;
            var xnoddf = _elements.AscendingNode
                         + _commonConsts.Xnodot * tsince;

            var tsq = tsince * tsince;
            var xnode = xnoddf + _commonConsts.Xnodcf * tsq;
            var tempa = 1.0 - _commonConsts.C1 * tsince;
            var tempe = _elements.BStar * _commonConsts.C4 * tsince;
            var templ = _commonConsts.T2Cof * tsq;

            var xincl = _elements.Inclination;
            var omega = omgadf;
            var xmp = xmdf;

            if (!_useSimpleModel)
            {
                var delomg = _nearspaceConsts.Omgcof * tsince;
                var delm = _nearspaceConsts.Xmcof
                           * (Math.Pow(1.0 + _commonConsts.Eta * Math.Cos(xmdf), 3.0)
                              * -_nearspaceConsts.Delmo);
                var temp = delomg + delm;

                xmp += temp;
                omega -= temp;

                var tcube = tsq * tsince;
                var tfour = tsince * tcube;

                tempa = tempa - _nearspaceConsts.D2 * tsq - _nearspaceConsts.D3
                        * tcube - _nearspaceConsts.D4 * tfour;
                tempe += _elements.BStar * _nearspaceConsts.C5
                         * (Math.Sin(xmp) - _nearspaceConsts.Sinmo);
                templ += _nearspaceConsts.T3Cof * tcube + tfour
                         * (_nearspaceConsts.T4Cof + tsince * _nearspaceConsts.T5Cof);
            }

            var a = _elements.RecoveredSemiMajorAxis * tempa * tempa;
            var e = _elements.Eccentricity - tempe;
            var xl = xmp + omega + xnode + _elements.RecoveredMeanMotion * templ;

            /*
             * fix tolerance for error recognition
             */
            if (e <= -0.001)
                throw new SatelliteException("Error: (e <= -0.001)");
            if (e < 1.0e-6)
                e = 1.0e-6;
            else if (e > 1.0 - 1.0e-6)
                e = 1.0 - 1.0e-6;

            /*
             * using calculated values, find position and velocity
             * we can pass in constants from Initialise() as these dont change
             */
            return CalculateFinalPositionVelocity(tsince, e,
                a, omega, xl, xnode,
                xincl, _commonConsts.Xlcof, _commonConsts.Aycof,
                _commonConsts.X3Thm1, _commonConsts.X1Mth2, _commonConsts.X7Thm1,
                _commonConsts.Cosio, _commonConsts.Sinio);
        }

        private Eci CalculateFinalPositionVelocity(
            double tsince,
            double e,
            double a,
            double omega,
            double xl,
            double xnode,
            double xincl,
            double xlcof,
            double aycof,
            double x3Thm1,
            double x1Mth2,
            double x7Thm1,
            double cosio,
            double sinio)
        {
            var beta2 = 1.0 - e * e;
            var xn = SgpConstants.ReciprocalOfMinutesPerTimeUnit / Math.Pow(a, 1.5);
            /*
             * long period periodics
             */
            var axn = e * Math.Cos(omega);
            var temp11 = 1.0 / (a * beta2);
            var xll = temp11 * xlcof * axn;
            var aynl = temp11 * aycof;
            var xlt = xl + xll;
            var ayn = e * Math.Sin(omega) + aynl;
            var elsq = axn * axn + ayn * ayn;

            if (elsq >= 1.0)
                throw new SatelliteException("Error: (elsq >= 1.0)");

            /*
             * solve keplers equation
             * - solve using Newton-Raphson root solving
             * - here capu is almost the mean anomoly
             * - initialise the eccentric anomaly term epw
             * - The fmod saves reduction of angle to +/-2pi in sin/cos() and prevents
             * convergence problems.
             */
            var capu = (xlt - xnode) % SgpConstants.TwoPi;
            var epw = capu;

            var sinepw = 0.0;
            var cosepw = 0.0;
            var ecose = 0.0;
            var esine = 0.0;

            /*
           * sensibility check for N-R correction
           */
            var maxNewtonNaphson = 1.25 * Math.Abs(Math.Sqrt(elsq));

            var keplerRunning = true;

            for (var i = 0; i < 10 && keplerRunning; i++)
            {
                sinepw = Math.Sin(epw);
                cosepw = Math.Cos(epw);
                ecose = axn * cosepw + ayn * sinepw;
                esine = axn * sinepw - ayn * cosepw;

                var f = capu - epw + esine;

                if (Math.Abs(f) < 1.0e-12)
                {
                    keplerRunning = false;
                }
                else
                {
                    /*
                     * 1st order Newton-Raphson correction
                     */
                    var fdot = 1.0 - ecose;
                    var deltaEpw = f / fdot;

                    /*
                     * 2nd order Newton-Raphson correction.
                     * f / (fdot - 0.5 * d2f * f/fdot)
                     */
                    if (i == 0)
                    {
                        if (deltaEpw > maxNewtonNaphson)
                            deltaEpw = maxNewtonNaphson;
                        else if (deltaEpw < -maxNewtonNaphson)
                            deltaEpw = -maxNewtonNaphson;
                    }
                    else
                    {
                        deltaEpw = f / (fdot + 0.5 * esine * deltaEpw);
                    }

                    /*
                     * Newton-Raphson correction of -F/DF
                     */
                    epw += deltaEpw;
                }
            }

            /*
             * short period preliminary quantities
             */
            var temp21 = 1.0 - elsq;
            var pl = a * temp21;

            if (pl < 0.0)
                throw new SatelliteException("Error: (pl < 0.0)");

            var r = a * (1.0 - ecose);
            var temp31 = 1.0 / r;
            var rdot = SgpConstants.ReciprocalOfMinutesPerTimeUnit * Math.Sqrt(a) * esine * temp31;
            var rfdot = SgpConstants.ReciprocalOfMinutesPerTimeUnit * Math.Sqrt(pl) * temp31;
            var temp32 = a * temp31;
            var betal = Math.Sqrt(temp21);
            var temp33 = 1.0 / (1.0 + betal);
            var cosu = temp32 * (cosepw - axn + ayn * esine * temp33);
            var sinu = temp32 * (sinepw - ayn - axn * esine * temp33);
            var u = Math.Atan2(sinu, cosu);
            var sin2U = 2.0 * sinu * cosu;
            var cos2U = 2.0 * cosu * cosu - 1.0;

            /*
             * update for short periodics
             */
            var temp41 = 1.0 / pl;
            var temp42 = SgpConstants.Ck2 * temp41;
            var temp43 = temp42 * temp41;

            var rk = r * (1.0 - 1.5 * temp43 * betal * x3Thm1)
                     + 0.5 * temp42 * x1Mth2 * cos2U;
            var uk = u - 0.25 * temp43 * x7Thm1 * sin2U;
            var xnodek = xnode + 1.5 * temp43 * cosio * sin2U;
            var xinck = xincl + 1.5 * temp43 * cosio * sinio * cos2U;
            var rdotk = rdot - xn * temp42 * x1Mth2 * sin2U;
            var rfdotk = rfdot + xn * temp42 * (x1Mth2 * cos2U + 1.5 * x3Thm1);

            /*
             * orientation vectors
             */
            var sinuk = Math.Sin(uk);
            var cosuk = Math.Cos(uk);
            var sinik = Math.Sin(xinck);
            var cosik = Math.Cos(xinck);
            var sinnok = Math.Sin(xnodek);
            var cosnok = Math.Cos(xnodek);
            var xmx = -sinnok * cosik;
            var xmy = cosnok * cosik;
            var ux = xmx * sinuk + cosnok * cosuk;
            var uy = xmy * sinuk + sinnok * cosuk;
            var uz = sinik * sinuk;
            var vx = xmx * cosuk - cosnok * sinuk;
            var vy = xmy * cosuk - sinnok * sinuk;
            var vz = sinik * cosuk;
            /*
             * position and velocity
             */
            var x = rk * ux * SgpConstants.EarthRadiusKm;
            var y = rk * uy * SgpConstants.EarthRadiusKm;
            var z = rk * uz * SgpConstants.EarthRadiusKm;
            var position = new Vector3(x, y, z);
            var xdot = (rdotk * ux + rfdotk * vx) * SgpConstants.EarthRadiusKm / 60.0;
            var ydot = (rdotk * uy + rfdotk * vy) * SgpConstants.EarthRadiusKm / 60.0;
            var zdot = (rdotk * uz + rfdotk * vz) * SgpConstants.EarthRadiusKm / 60.0;
            var velocity = new Vector3(xdot, ydot, zdot);

            if (rk < 1.0)
                throw new DecayedException(
                    _elements.Epoch.AddMinutes(tsince),
                    position,
                    velocity);

            return new Eci(_elements.Epoch.AddMinutes(tsince), position, velocity);
        }

        private void DeepSpaceInitialise(
            double eosq,
            double sinio,
            double cosio,
            double betao,
            double theta2,
            double betao2,
            double xmdot,
            double omgdot,
            double xnodot)
        {
            var se = 0.0;
            var si = 0.0;
            var sl = 0.0;
            var sgh = 0.0;
            var shdq = 0.0;

            var bfact = 0.0;

            const double zns = 1.19459E-5;
            const double c1Ss = 2.9864797E-6;
            const double zes = 0.01675;
            const double znl = 1.5835218E-4;
            const double c1L = 4.7968065E-7;
            const double zel = 0.05490;
            const double zcosis = 0.91744867;
            const double cZsini = 0.39785416;
            const double zsings = -0.98088458;
            const double zcosgs = 0.1945905;
            const double q22 = 1.7891679E-6;
            const double q31 = 2.1460748E-6;
            const double q33 = 2.2123015E-7;
            const double root22 = 1.7891679E-6;
            const double root32 = 3.7393792E-7;
            const double root44 = 7.3636953E-9;
            const double root52 = 1.1428639E-7;
            const double root54 = 2.1765803E-9;

            var aqnv = 1.0 / _elements.RecoveredSemiMajorAxis;
            var xpidot = omgdot + xnodot;
            var sinq = Math.Sin(_elements.AscendingNode);
            var cosq = Math.Cos(_elements.AscendingNode);
            var sing = Math.Sin(_elements.ArgumentPerigee);
            var cosg = Math.Cos(_elements.ArgumentPerigee);

            /*
             * initialize lunar / solar terms
             */
            var jday = _elements.Epoch.ToJulian() - SgpConstants.EpochJan112H2000;

            var xnodce = 4.5236020 - 9.2422029e-4 * jday;
            var xnodceTemp = xnodce % SgpConstants.TwoPi;
            var stem = Math.Sin(xnodceTemp);
            var ctem = Math.Cos(xnodceTemp);
            var zcosil = 0.91375164 - 0.03568096 * ctem;
            var zsinil = Math.Sqrt(1.0 - zcosil * zcosil);
            var zsinhl = 0.089683511 * stem / zsinil;
            var zcoshl = Math.Sqrt(1.0 - zsinhl * zsinhl);
            var c = 4.7199672 + 0.22997150 * jday;
            var gam = 5.8351514 + 0.0019443680 * jday;
            _deepspaceConsts.Zmol = Util.WrapTwoPi(c - gam);
            var zx = 0.39785416 * stem / zsinil;
            var zy = zcoshl * ctem + 0.91744867 * zsinhl * stem;
            zx = Math.Atan2(zx, zy);
            zx = (gam + zx - xnodce) % SgpConstants.TwoPi;

            var zcosgl = Math.Cos(zx);
            var zsingl = Math.Sin(zx);
            _deepspaceConsts.Zmos = Util.WrapTwoPi(6.2565837 + 0.017201977 * jday);

            /*
             * do solar terms
             */
            var zcosg = zcosgs;
            var zsing = zsings;
            var zcosi = zcosis;
            var zsini = cZsini;
            var zcosh = cosq;
            var zsinh = sinq;
            var cc = c1Ss;
            var zn = zns;
            var ze = zes;
            var xnoi = 1.0 / _elements.RecoveredMeanMotion;

            for (var cnt = 0; cnt < 2; cnt++)
            {
                /*
                 * solar terms are done a second time after lunar terms are done
                 */
                var a1 = zcosg * zcosh + zsing * zcosi * zsinh;
                var a3 = -zsing * zcosh + zcosg * zcosi * zsinh;
                var a7 = -zcosg * zsinh + zsing * zcosi * zcosh;
                var a8 = zsing * zsini;
                var a9 = zsing * zsinh + zcosg * zcosi * zcosh;
                var a10 = zcosg * zsini;
                var a2 = cosio * a7 + sinio * a8;
                var a4 = cosio * a9 + sinio * a10;
                var a5 = -sinio * a7 + cosio * a8;
                var a6 = -sinio * a9 + cosio * a10;
                var x1 = a1 * cosg + a2 * sing;
                var x2 = a3 * cosg + a4 * sing;
                var x3 = -a1 * sing + a2 * cosg;
                var x4 = -a3 * sing + a4 * cosg;
                var x5 = a5 * sing;
                var x6 = a6 * sing;
                var x7 = a5 * cosg;
                var x8 = a6 * cosg;
                var z31 = 12.0 * x1 * x1 - 3.0 * x3 * x3;
                var z32 = 24.0 * x1 * x2 - 6.0 * x3 * x4;
                var z33 = 12.0 * x2 * x2 - 3.0 * x4 * x4;
                var z1 = 3.0 * (a1 * a1 + a2 * a2) + z31 * eosq;
                var z2 = 6.0 * (a1 * a3 + a2 * a4) + z32 * eosq;
                var z3 = 3.0 * (a3 * a3 + a4 * a4) + z33 * eosq;

                var z11 = -6.0 * a1 * a5
                          + eosq * (-24.0 * x1 * x7 - 6.0 * x3 * x5);
                var z12 = -6.0 * (a1 * a6 + a3 * a5)
                          + eosq * (-24.0 * (x2 * x7 + x1 * x8) - 6.0 * (x3 * x6 + x4 * x5));
                var z13 = -6.0 * a3 * a6
                          + eosq * (-24.0 * x2 * x8 - 6.0 * x4 * x6);
                var z21 = 6.0 * a2 * a5
                          + eosq * (24.0 * x1 * x5 - 6.0 * x3 * x7);
                var z22 = 6.0 * (a4 * a5 + a2 * a6)
                          + eosq * (24.0 * (x2 * x5 + x1 * x6) - 6.0 * (x4 * x7 + x3 * x8));
                var z23 = 6.0 * a4 * a6
                          + eosq * (24.0 * x2 * x6 - 6.0 * x4 * x8);

                z1 = z1 + z1 + betao2 * z31;
                z2 = z2 + z2 + betao2 * z32;
                z3 = z3 + z3 + betao2 * z33;

                var s3 = cc * xnoi;
                var s2 = -0.5 * s3 / betao;
                var s4 = s3 * betao;
                var s1 = -15.0 * _elements.Eccentricity * s4;
                var s5 = x1 * x3 + x2 * x4;
                var s6 = x2 * x3 + x1 * x4;
                var s7 = x2 * x4 - x1 * x3;

                se = s1 * zn * s5;
                si = s2 * zn * (z11 + z13);
                sl = -zn * s3 * (z1 + z3 - 14.0 - 6.0 * eosq);
                sgh = s4 * zn * (z31 + z33 - 6.0);

                /*
                 * replaced
                 * sh = -zn * s2 * (z21 + z23
                 * with
                 * shdq = (-zn * s2 * (z21 + z23)) / sinio
                 */
                if (_elements.Inclination < 5.2359877e-2
                    || _elements.Inclination > Math.PI - 5.2359877e-2)
                    shdq = 0.0;
                else
                    shdq = -zn * s2 * (z21 + z23) / sinio;

                _deepspaceConsts.Ee2 = 2.0 * s1 * s6;
                _deepspaceConsts.E3 = 2.0 * s1 * s7;
                _deepspaceConsts.Xi2 = 2.0 * s2 * z12;
                _deepspaceConsts.Xi3 = 2.0 * s2 * (z13 - z11);
                _deepspaceConsts.Xl2 = -2.0 * s3 * z2;
                _deepspaceConsts.Xl3 = -2.0 * s3 * (z3 - z1);
                _deepspaceConsts.Xl4 = -2.0 * s3 * (-21.0 - 9.0 * eosq) * ze;
                _deepspaceConsts.Xgh2 = 2.0 * s4 * z32;
                _deepspaceConsts.Xgh3 = 2.0 * s4 * (z33 - z31);
                _deepspaceConsts.Xgh4 = -18.0 * s4 * ze;
                _deepspaceConsts.Xh2 = -2.0 * s2 * z22;
                _deepspaceConsts.Xh3 = -2.0 * s2 * (z23 - z21);

                if (cnt == 1)
                    break;
                /*
                 * do lunar terms
                 */
                _deepspaceConsts.Sse = se;
                _deepspaceConsts.Ssi = si;
                _deepspaceConsts.Ssl = sl;
                _deepspaceConsts.Ssh = shdq;
                _deepspaceConsts.Ssg = sgh - cosio * _deepspaceConsts.Ssh;
                _deepspaceConsts.Se2 = _deepspaceConsts.Ee2;
                _deepspaceConsts.Si2 = _deepspaceConsts.Xi2;
                _deepspaceConsts.Sl2 = _deepspaceConsts.Xl2;
                _deepspaceConsts.Sgh2 = _deepspaceConsts.Xgh2;
                _deepspaceConsts.Sh2 = _deepspaceConsts.Xh2;
                _deepspaceConsts.Se3 = _deepspaceConsts.E3;
                _deepspaceConsts.Si3 = _deepspaceConsts.Xi3;
                _deepspaceConsts.Sl3 = _deepspaceConsts.Xl3;
                _deepspaceConsts.Sgh3 = _deepspaceConsts.Xgh3;
                _deepspaceConsts.Sh3 = _deepspaceConsts.Xh3;
                _deepspaceConsts.Sl4 = _deepspaceConsts.Xl4;
                _deepspaceConsts.Sgh4 = _deepspaceConsts.Xgh4;
                zcosg = zcosgl;
                zsing = zsingl;
                zcosi = zcosil;
                zsini = zsinil;
                zcosh = zcoshl * cosq + zsinhl * sinq;
                zsinh = sinq * zcoshl - cosq * zsinhl;
                zn = znl;
                cc = c1L;
                ze = zel;
            }

            _deepspaceConsts.Sse += se;
            _deepspaceConsts.Ssi += si;
            _deepspaceConsts.Ssl += sl;
            _deepspaceConsts.Ssg += sgh - cosio * shdq;
            _deepspaceConsts.Ssh += shdq;

            _deepspaceConsts.ResonanceFlag = false;
            _deepspaceConsts.SynchronousFlag = false;
            var initialiseIntegrator = true;

            if (_elements.RecoveredMeanMotion < 0.0052359877
                && _elements.RecoveredMeanMotion > 0.0034906585)
            {
                /*
                 * 24h synchronous resonance terms initialisation
                 */
                _deepspaceConsts.ResonanceFlag = true;
                _deepspaceConsts.SynchronousFlag = true;

                var g200 = 1.0 + eosq * (-2.5 + 0.8125 * eosq);
                var g310 = 1.0 + 2.0 * eosq;
                var g300 = 1.0 + eosq * (-6.0 + 6.60937 * eosq);
                var f220 = 0.75 * (1.0 + cosio) * (1.0 + cosio);
                var f311 = 0.9375 * sinio * sinio * (1.0 + 3.0 * cosio)
                           - 0.75 * (1.0 + cosio);
                var f330 = 1.0 + cosio;
                f330 = 1.875 * f330 * f330 * f330;
                _deepspaceConsts.Del1 = 3.0 * _elements.RecoveredMeanMotion
                                        * _elements.RecoveredMeanMotion
                                        * aqnv * aqnv;
                _deepspaceConsts.Del2 = 2.0 * _deepspaceConsts.Del1
                                        * f220 * g200 * q22;
                _deepspaceConsts.Del3 = 3.0 * _deepspaceConsts.Del1
                                        * f330 * g300 * q33 * aqnv;
                _deepspaceConsts.Del1 = _deepspaceConsts.Del1
                                        * f311 * g310 * q31 * aqnv;

                _integratorConsts.Xlamo = _elements.MeanAnomoly
                                          + _elements.AscendingNode
                                          + _elements.ArgumentPerigee
                                          - _deepspaceConsts.Gsto;
                bfact = xmdot + xpidot - SgpConstants.EarthRotationPerMinRad;
                bfact += _deepspaceConsts.Ssl
                         + _deepspaceConsts.Ssg
                         + _deepspaceConsts.Ssh;
            }
            else if (_elements.RecoveredMeanMotion < 8.26e-3
                     || _elements.RecoveredMeanMotion > 9.24e-3
                     || _elements.Eccentricity < 0.5)
            {
                initialiseIntegrator = false;
            }
            else
            {
                /*
                 * geopotential resonance initialisation for 12 hour orbits
                 */
                _deepspaceConsts.ResonanceFlag = true;

                double g211;
                double g310;
                double g322;
                double g410;
                double g422;
                double g520;

                var g201 = -0.306 - (_elements.Eccentricity - 0.64) * 0.440;

                if (_elements.Eccentricity <= 0.65)
                {
                    g211 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        3.616, -13.247, +16.290, 0.0);
                    g310 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        -19.302, 117.390, -228.419, 156.591);
                    g322 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        -18.9068, 109.7927, -214.6334, 146.5816);
                    g410 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        -41.122, 242.694, -471.094, 313.953);
                    g422 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        -146.407, 841.880, -1629.014, 1083.435);
                    g520 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        -532.114, 3017.977, -5740.032, 3708.276);
                }
                else
                {
                    g211 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        -72.099, 331.819, -508.738, 266.724);
                    g310 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        -346.844, 1582.851, -2415.925, 1246.113);
                    g322 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        -342.585, 1554.908, -2366.899, 1215.972);
                    g410 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        -1052.797, 4758.686, -7193.992, 3651.957);
                    g422 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        -3581.69, 16178.11, -24462.77, 12422.52);

                    if (_elements.Eccentricity <= 0.715)
                        g520 = EvaluateCubicPolynomial(_elements.Eccentricity,
                            1464.74, -4664.75, 3763.64, 0.0);
                    else
                        g520 = EvaluateCubicPolynomial(_elements.Eccentricity,
                            -5149.66, 29936.92, -54087.36, 31324.56);
                }

                double g533;
                double g521;
                double g532;

                if (_elements.Eccentricity < 0.7)
                {
                    g533 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        -919.2277, 4988.61, -9064.77, 5542.21);
                    g521 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        -822.71072, 4568.6173, -8491.4146, 5337.524);
                    g532 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        -853.666, 4690.25, -8624.77, 5341.4);
                }
                else
                {
                    g533 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        -37995.78, 161616.52, -229838.2, 109377.94);
                    g521 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        -51752.104, 218913.95, -309468.16, 146349.42);
                    g532 = EvaluateCubicPolynomial(_elements.Eccentricity,
                        -40023.88, 170470.89, -242699.48, 115605.82);
                }

                var sini2 = sinio * sinio;
                var f220 = 0.75 * (1.0 + 2.0 * cosio + theta2);
                var f221 = 1.5 * sini2;
                var f321 = 1.875 * sinio * (1.0 - 2.0 * cosio - 3.0 * theta2);
                var f322 = -1.875 * sinio * (1.0 + 2.0 * cosio - 3.0 * theta2);
                var f441 = 35.0 * sini2 * f220;
                var f442 = 39.3750 * sini2 * sini2;
                var f522 = 9.84375 * sinio
                           * (sini2 * (1.0 - 2.0 * cosio - 5.0 * theta2)
                              + 0.33333333 * (-2.0 + 4.0 * cosio + 6.0 * theta2));
                var f523 = sinio
                           * (4.92187512 * sini2 * (-2.0 - 4.0 * cosio + 10.0 * theta2)
                              + 6.56250012 * (1.0 + 2.0 * cosio - 3.0 * theta2));
                var f542 = 29.53125 * sinio * (2.0 - 8.0 * cosio + theta2 *
                                               (-12.0 + 8.0 * cosio + 10.0 * theta2));
                var f543 = 29.53125 * sinio * (-2.0 - 8.0 * cosio + theta2 *
                                               (12.0 + 8.0 * cosio - 10.0 * theta2));

                var xno2 = _elements.RecoveredMeanMotion
                           * _elements.RecoveredMeanMotion;
                var ainv2 = aqnv * aqnv;

                var temp1 = 3.0 * xno2 * ainv2;
                var temp = temp1 * root22;
                _deepspaceConsts.D2201 = temp * f220 * g201;
                _deepspaceConsts.D2211 = temp * f221 * g211;
                temp1 = temp1 * aqnv;
                temp = temp1 * root32;
                _deepspaceConsts.D3210 = temp * f321 * g310;
                _deepspaceConsts.D3222 = temp * f322 * g322;
                temp1 = temp1 * aqnv;
                temp = 2.0 * temp1 * root44;
                _deepspaceConsts.D4410 = temp * f441 * g410;
                _deepspaceConsts.D4422 = temp * f442 * g422;
                temp1 = temp1 * aqnv;
                temp = temp1 * root52;
                _deepspaceConsts.D5220 = temp * f522 * g520;
                _deepspaceConsts.D5232 = temp * f523 * g532;
                temp = 2.0 * temp1 * root54;
                _deepspaceConsts.D5421 = temp * f542 * g521;
                _deepspaceConsts.D5433 = temp * f543 * g533;

                _integratorConsts.Xlamo = _elements.MeanAnomoly
                                          + _elements.AscendingNode
                                          + _elements.AscendingNode
                                          - _deepspaceConsts.Gsto
                                          - _deepspaceConsts.Gsto;
                bfact = xmdot
                        + xnodot + xnodot
                        - SgpConstants.EarthRotationPerMinRad - SgpConstants.EarthRotationPerMinRad;
                bfact = bfact + _deepspaceConsts.Ssl
                        + _deepspaceConsts.Ssh
                        + _deepspaceConsts.Ssh;
            }

            if (initialiseIntegrator)
            {
                /*
                 * initialise integrator
                 */
                _integratorConsts.Xfact = bfact - _elements.RecoveredMeanMotion;
                _integratorParams.Atime = 0.0;
                _integratorParams.Xni = _elements.RecoveredMeanMotion;
                _integratorParams.Xli = _integratorConsts.Xlamo;
                /*
                 * precompute dot terms for epoch
                 */
                DeepSpaceCalcDotTerms(_integratorConsts.Values0);
            }
        }

        private void DeepSpaceCalculateLunarSolarTerms(double tsince, ref double pe, ref double pinc, ref double pl,
            ref double pgh, ref double ph)
        {
            const double zes = 0.01675;
            const double zns = 1.19459E-5;
            const double znl = 1.5835218E-4;
            const double zel = 0.05490;

            /*
             * calculate solar terms for time tsince
             */
            var zm = _deepspaceConsts.Zmos + zns * tsince;
            var zf = zm + 2.0 * zes * Math.Sin(zm);
            var sinzf = Math.Sin(zf);
            var f2 = 0.5 * sinzf * sinzf - 0.25;
            var f3 = -0.5 * sinzf * Math.Cos(zf);

            var ses = _deepspaceConsts.Se2 * f2
                      + _deepspaceConsts.Se3 * f3;
            var sis = _deepspaceConsts.Si2 * f2
                      + _deepspaceConsts.Si3 * f3;
            var sls = _deepspaceConsts.Sl2 * f2
                      + _deepspaceConsts.Sl3 * f3
                      + _deepspaceConsts.Sl4 * sinzf;
            var sghs = _deepspaceConsts.Sgh2 * f2
                       + _deepspaceConsts.Sgh3 * f3
                       + _deepspaceConsts.Sgh4 * sinzf;
            var shs = _deepspaceConsts.Sh2 * f2
                      + _deepspaceConsts.Sh3 * f3;

            /*
             * calculate lunar terms for time tsince
             */
            zm = _deepspaceConsts.Zmol + znl * tsince;
            zf = zm + 2.0 * zel * Math.Sin(zm);
            sinzf = Math.Sin(zf);
            f2 = 0.5 * sinzf * sinzf - 0.25;
            f3 = -0.5 * sinzf * Math.Cos(zf);

            var sel = _deepspaceConsts.Ee2 * f2
                      + _deepspaceConsts.E3 * f3;
            var sil = _deepspaceConsts.Xi2 * f2
                      + _deepspaceConsts.Xi3 * f3;
            var sll = _deepspaceConsts.Xl2 * f2
                      + _deepspaceConsts.Xl3 * f3
                      + _deepspaceConsts.Xl4 * sinzf;
            var sghl = _deepspaceConsts.Xgh2 * f2
                       + _deepspaceConsts.Xgh3 * f3
                       + _deepspaceConsts.Xgh4 * sinzf;
            var shl = _deepspaceConsts.Xh2 * f2
                      + _deepspaceConsts.Xh3 * f3;

            /*
             * merge calculated values
             */
            pe = ses + sel;
            pinc = sis + sil;
            pl = sls + sll;
            pgh = sghs + sghl;
            ph = shs + shl;
        }

        private void DeepSpacePeriodics(double tsince, ref double em, ref double xinc, ref double omgasm,
            ref double xnodes, ref double xll)
        {
            /*
             * storage for lunar / solar terms
             * set by DeepSpaceCalculateLunarSolarTerms()
             */
            var pe = 0.0;
            var pinc = 0.0;
            var pl = 0.0;
            var pgh = 0.0;
            var ph = 0.0;

            /*
             * calculate lunar / solar terms for current time
             */
            DeepSpaceCalculateLunarSolarTerms(tsince, ref pe, ref pinc, ref pl, ref pgh, ref ph);

            xinc += pinc;
            em += pe;

            /* Spacetrack report #3 has sin/cos from before perturbations
             * added to xinc (oldxinc), but apparently report # 6 has then
             * from after they are added.
             * use for strn3
             * if (elements_.GetInclination() >= 0.2)
             * use for gsfc
             * if (xinc >= 0.2)
             * (moved from start of function)
             */
            var sinis = Math.Sin(xinc);
            var cosis = Math.Cos(xinc);

            if (xinc >= 0.2)
            {
                /*
                 * apply periodics directly
                 */
                var tmpPh = ph / sinis;

                omgasm += pgh - cosis * tmpPh;
                xnodes += tmpPh;
                xll += pl;
            }
            else
            {
                /*
                 * apply periodics with lyddane modification
                 */
                var sinok = Math.Sin(xnodes);
                var cosok = Math.Cos(xnodes);
                var alfdp = sinis * sinok;
                var betdp = sinis * cosok;
                var dalf = ph * cosok + pinc * cosis * sinok;
                var dbet = -ph * sinok + pinc * cosis * cosok;

                alfdp += dalf;
                betdp += dbet;

                xnodes = Util.WrapTwoPi(xnodes);

                var xls = xll + omgasm + cosis * xnodes;
                var dls = pl + pgh - pinc * xnodes * sinis;
                xls += dls;

                /*
                 * save old xnodes value
                 */
                var oldxnodes = xnodes;

                xnodes = Math.Atan2(alfdp, betdp);
                if (xnodes < 0.0)
                    xnodes += SgpConstants.TwoPi;

                /*
                 * Get perturbed xnodes in to same quadrant as original.
                 * RAAN is in the range of 0 to 360 degrees
                 * atan2 is in the range of -180 to 180 degrees
                 */
                if (Math.Abs(oldxnodes - xnodes) > Math.PI)
                    if (xnodes < oldxnodes)
                        xnodes += SgpConstants.TwoPi;
                    else
                        xnodes -= SgpConstants.TwoPi;

                xll += pl;
                omgasm = xls - xll - cosis * xnodes;
            }
        }

        private void DeepSpaceSecular(double tsince, ref double xll, double omgasm, double xnodes, ref double em,
            ref double xinc, ref double xn)
        {
            const double step = 720.0;
            const double step2 = 259200.0;

            xll += _deepspaceConsts.Ssl * tsince;
            omgasm += _deepspaceConsts.Ssg * tsince;
            xnodes += _deepspaceConsts.Ssh * tsince;
            em += _deepspaceConsts.Sse * tsince;
            xinc += _deepspaceConsts.Ssi * tsince;

            if (_deepspaceConsts.ResonanceFlag)
            {
                /*
                 * 1st condition (if tsince is less than one time step from epoch)
                 * 2nd condition (if integrator_params_.atime and
                 *     tsince are of opposite signs, so zero crossing required)
                 * 3rd condition (if tsince is closer to zero than 
                 *     integrator_params_.atime, only integrate away from zero)
                 */
                if (Math.Abs(tsince) < step ||
                    tsince * _integratorParams.Atime <= 0.0 ||
                    Math.Abs(tsince) < Math.Abs(_integratorParams.Atime))
                {
                    /*
                     * restart from epoch
                     */
                    _integratorParams.Atime = 0.0;
                    _integratorParams.Xni = _elements.RecoveredMeanMotion;
                    _integratorParams.Xli = _integratorConsts.Xlamo;

                    /*
                     * restore precomputed values for epoch
                     */
                    _integratorParams.ValuesT = _integratorConsts.Values0;
                }

                var ft = tsince - _integratorParams.Atime;

                /*
                 * if time difference (ft) is greater than the time step (720.0)
                 * loop around until integrator_params_.atime is within one time step of
                 * tsince
                 */
                if (Math.Abs(ft) >= step)
                {
                    /*
                     * calculate step direction to allow integrator_params_.atime
                     * to catch up with tsince
                     */
                    var delt = -step;
                    if (ft >= 0.0)
                        delt = step;

                    do
                    {
                        /*
                         * integrate using current dot terms
                         */
                        DeepSpaceIntegrator(delt, step2, _integratorParams.ValuesT);

                        /*
                         * calculate dot terms for next integration
                         */
                        DeepSpaceCalcDotTerms(_integratorParams.ValuesT);

                        ft = tsince - _integratorParams.Atime;
                    } while (Math.Abs(ft) >= step);
                }

                /*
                 * integrator
                 */
                xn = _integratorParams.Xni
                     + _integratorParams.ValuesT.Xndot * ft
                     + _integratorParams.ValuesT.Xnddt * ft * ft * 0.5;
                var xl = _integratorParams.Xli
                         + _integratorParams.ValuesT.Xldot * ft
                         + _integratorParams.ValuesT.Xndot * ft * ft * 0.5;
                var temp = -xnodes + _deepspaceConsts.Gsto + tsince * SgpConstants.EarthRotationPerMinRad;

                if (_deepspaceConsts.SynchronousFlag)
                    xll = xl + temp - omgasm;
                else
                    xll = xl + temp + temp;
            }
        }

        private void DeepSpaceCalcDotTerms(IntegratorValues values)
        {
            const double g22 = 5.7686396;
            const double g32 = 0.95240898;
            const double g44 = 1.8014998;
            const double g52 = 1.0508330;
            const double g54 = 4.4108898;
            const double fasx2 = 0.13130908;
            const double fasx4 = 2.8843198;
            const double fasx6 = 0.37448087;

            if (_deepspaceConsts.SynchronousFlag)
            {
                values.Xndot = _deepspaceConsts.Del1
                               * Math.Sin(_integratorParams.Xli - fasx2)
                               + _deepspaceConsts.Del2
                               * Math.Sin(2.0 * (_integratorParams.Xli - fasx4))
                               + _deepspaceConsts.Del3
                               * Math.Sin(3.0 * (_integratorParams.Xli - fasx6));
                values.Xnddt = _deepspaceConsts.Del1
                               * Math.Cos(_integratorParams.Xli - fasx2)
                               + 2.0 * _deepspaceConsts.Del2
                               * Math.Cos(2.0 * (_integratorParams.Xli - fasx4))
                               + 3.0 * _deepspaceConsts.Del3
                               * Math.Cos(3.0 * (_integratorParams.Xli - fasx6));
            }
            else
            {
                var xomi = _elements.ArgumentPerigee
                           + _commonConsts.Omgdot * _integratorParams.Atime;
                var x2Omi = xomi + xomi;
                var x2Li = _integratorParams.Xli + _integratorParams.Xli;

                values.Xndot = _deepspaceConsts.D2201
                               * Math.Sin(x2Omi + _integratorParams.Xli - g22)
                               * +_deepspaceConsts.D2211
                               * Math.Sin(_integratorParams.Xli - g22)
                               + _deepspaceConsts.D3210
                               * Math.Sin(xomi + _integratorParams.Xli - g32)
                               + _deepspaceConsts.D3222
                               * Math.Sin(-xomi + _integratorParams.Xli - g32)
                               + _deepspaceConsts.D4410
                               * Math.Sin(x2Omi + x2Li - g44)
                               + _deepspaceConsts.D4422
                               * Math.Sin(x2Li - g44)
                               + _deepspaceConsts.D5220
                               * Math.Sin(xomi + _integratorParams.Xli - g52)
                               + _deepspaceConsts.D5232
                               * Math.Sin(-xomi + _integratorParams.Xli - g52)
                               + _deepspaceConsts.D5421
                               * Math.Sin(xomi + x2Li - g54)
                               + _deepspaceConsts.D5433
                               * Math.Sin(-xomi + x2Li - g54);
                values.Xnddt = _deepspaceConsts.D2201
                               * Math.Cos(x2Omi + _integratorParams.Xli - g22)
                               + _deepspaceConsts.D2211
                               * Math.Cos(_integratorParams.Xli - g22)
                               + _deepspaceConsts.D3210
                               * Math.Cos(xomi + _integratorParams.Xli - g32)
                               + _deepspaceConsts.D3222
                               * Math.Cos(-xomi + _integratorParams.Xli - g32)
                               + _deepspaceConsts.D5220
                               * Math.Cos(xomi + _integratorParams.Xli - g52)
                               + _deepspaceConsts.D5232
                               * Math.Cos(-xomi + _integratorParams.Xli - g52)
                               + 2.0 * (_deepspaceConsts.D4410 * Math.Cos(x2Omi + x2Li - g44)
                                        + _deepspaceConsts.D4422
                                        * Math.Cos(x2Li - g44)
                                        + _deepspaceConsts.D5421
                                        * Math.Cos(xomi + x2Li - g54)
                                        + _deepspaceConsts.D5433
                                        * Math.Cos(-xomi + x2Li - g54));
            }

            values.Xldot = _integratorParams.Xni + _integratorConsts.Xfact;
            values.Xnddt *= values.Xldot;
        }

        private void DeepSpaceIntegrator(double delt, double step2, IntegratorValues values)
        {
            /*
           * integrator
           */
            _integratorParams.Xli += values.Xldot * delt + values.Xndot * step2;
            _integratorParams.Xni += values.Xndot * delt + values.Xnddt * step2;

            /*
           * increment integrator time
           */
            _integratorParams.Atime += delt;
        }

        private void Reset()
        {
            _useSimpleModel = false;
            _useDeepSpace = false;

            _commonConsts = EmptyCommonConstants;
            _nearspaceConsts = EmptyNearSpaceConstants;
            _deepspaceConsts = EmptyDeepSpaceConstants;
            _integratorConsts = EmptyIntegratorConstants;
            _integratorParams = EmptyIntegratorParams;
        }

        private struct CommonConstants
        {
            public double Cosio;
            public double Sinio;
            public double Eta;
            public double T2Cof;
            public double A3Ovk2;
            public double X1Mth2;
            public double X3Thm1;
            public double X7Thm1;
            public double Aycof;
            public double Xlcof;
            public double Xnodcf;
            public double C1;
            public double C4;

            /// <summary>
            ///     secular rate of omega (radians/sec)
            /// </summary>
            public double Omgdot;

            /// <summary>
            ///     secular rate of xnode (radians/sec)
            /// </summary>
            public double Xnodot;

            /// <summary>
            ///     ecular rate of xnode (radians/sec)
            /// </summary>
            public double Xmdot;
        }

        private struct NearSpaceConstants
        {
            public double C5;
            public double Omgcof;
            public double Xmcof;
            public double Delmo;
            public double Sinmo;
            public double D2;
            public double D3;
            public double D4;
            public double T3Cof;
            public double T4Cof;
            public double T5Cof;
        }

        private struct DeepSpaceConstants
        {
            public double Gsto;
            public double Zmol;
            public double Zmos;

            /// <summary>
            ///     whether the deep space orbit is geopotential resonance for 12 hour orbits
            /// </summary>
            public bool ResonanceFlag;

            /// <summary>
            ///     whether the deep space orbit is 24h synchronous resonance
            /// </summary>
            public bool SynchronousFlag;

            // lunar / solar constants for epoch applied during DeepSpaceSecular()

            public double Sse;
            public double Ssi;
            public double Ssl;
            public double Ssg;
            public double Ssh;

            // lunar / solar constants used during DeepSpaceCalculateLunarSolarTerms()

            public double Se2;
            public double Si2;
            public double Sl2;
            public double Sgh2;
            public double Sh2;
            public double Se3;
            public double Si3;
            public double Sl3;
            public double Sgh3;
            public double Sh3;
            public double Sl4;
            public double Sgh4;
            public double Ee2;
            public double E3;
            public double Xi2;
            public double Xi3;
            public double Xl2;
            public double Xl3;
            public double Xl4;
            public double Xgh2;
            public double Xgh3;
            public double Xgh4;
            public double Xh2;
            public double Xh3;

            // used during DeepSpaceCalcDotTerms()

            public double D2201;
            public double D2211;
            public double D3210;
            public double D3222;
            public double D4410;
            public double D4422;
            public double D5220;
            public double D5232;
            public double D5421;
            public double D5433;
            public double Del1;
            public double Del2;
            public double Del3;
        }

        private struct IntegratorValues
        {
            public double Xndot;
            public double Xnddt;
            public double Xldot;
        }

        private struct IntegratorConstants
        {
            // integrator constants
            public double Xfact;
            public double Xlamo;

            // integrator values for epoch
            public IntegratorValues Values0;
        }

        private struct IntegratorParams
        {
            // integrator values
            public double Xli;
            public double Xni;
            public double Atime;

            // integrator values for current d_atime_
            public IntegratorValues ValuesT;
        }
    }
}