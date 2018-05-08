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
  /**
 * @mainpage
 *
 * This documents the SGP4 tracking library.
 */

  /**
 * @brief The simplified perturbations model 4 propagater.
 */
  public class SGP4
  {

    public SGP4(Tle tle)
    {
      elements_ = new OrbitalElements(tle);
      Initialise();
    }

    public void SetTle(Tle tle)
    {
      /*
     * extract and format tle data
     */
      elements_ = new OrbitalElements(tle);

      Initialise();
    }

    public Eci FindPosition(double tsince)
    {     
      if (use_deep_space_)
      {
        return FindPositionSDP4(tsince);
      }
      else
      {
        return FindPositionSGP4(tsince);
      }
    }

    public Eci FindPosition(DateTime date)
    {      
      return FindPosition((date - elements_.Epoch()).TotalMinutes());
    }

    /**
 * @param[in] x
 * @param[in] constant
 * @param[in] linear
 * @param[in] squared
 * @param[in] cubed
 * @returns result
 */
    public static double EvaluateCubicPolynomial(
      double x,
      double constant,
      double linear,
      double squared,
      double cubed)
    {
      return constant + x * (linear + x * (squared + x * cubed));
    }

    private struct CommonConstants
    {
      public double cosio;
      public double sinio;
      public double eta;
      public double t2cof;
      public double a3ovk2;
      public double x1mth2;
      public double x3thm1;
      public double x7thm1;
      public double aycof;
      public double xlcof;
      public double xnodcf;
      public double c1;
      public double c4;
      public double omgdot;
      // secular rate of omega (radians/sec)
      public double xnodot;
      // secular rate of xnode (radians/sec)
      public double xmdot;
      // secular rate of xmo   (radians/sec)
    };

    private struct NearSpaceConstants
    {
      public double c5;
      public double omgcof;
      public double xmcof;
      public double delmo;
      public double sinmo;
      public double d2;
      public double d3;
      public double d4;
      public double t3cof;
      public double t4cof;
      public double t5cof;
    };

    private struct DeepSpaceConstants
    {
      public double gsto;
      public double zmol;
      public double zmos;
      /*
         * whether the deep space orbit is
         * geopotential resonance for 12 hour orbits
         */
      public bool resonance_flag;
      /*
         * whether the deep space orbit is
         * 24h synchronous resonance
         */
      public bool synchronous_flag;
      /*
         * lunar / solar ants for epoch
         * applied during DeepSpaceSecular()
         */
      public double sse;
      public double ssi;
      public double ssl;
      public double ssg;
      public double ssh;
      /*
         * lunar / solar ants
         * used during DeepSpaceCalculateLunarSolarTerms()
         */
      public double se2;
      public double si2;
      public double sl2;
      public double sgh2;
      public double sh2;
      public double se3;
      public double si3;
      public double sl3;
      public double sgh3;
      public double sh3;
      public double sl4;
      public double sgh4;
      public double ee2;
      public double e3;
      public double xi2;
      public double xi3;
      public double xl2;
      public double xl3;
      public double xl4;
      public double xgh2;
      public double xgh3;
      public double xgh4;
      public double xh2;
      public double xh3;
      /*
         * used during DeepSpaceCalcDotTerms()
         */
      public double d2201;
      public double d2211;
      public double d3210;
      public double d3222;
      public double d4410;
      public double d4422;
      public double d5220;
      public double d5232;
      public double d5421;
      public double d5433;
      public double del1;
      public double del2;
      public double del3;
    };

    private struct IntegratorValues
    {
      public double xndot;
      public double xnddt;
      public double xldot;
    };

    private struct IntegratorConstants
    {
      /*
         * integrator ants
         */
      public double xfact;
      public double xlamo;

      /*
         * integrator values for epoch
         */
      public IntegratorValues values_0;
    };

    private struct IntegratorParams
    {
      /*
         * integrator values
         */
      public double xli;
      public double xni;
      public double atime;
      /*
         * itegrator values for current d_atime_
         */
      public IntegratorValues values_t;
    };

    private void Initialise()
    {
      /*
     * reset all constants etc
     */
      Reset();

      /*
     * error checks
     */
      if (elements_.Eccentricity() < 0.0 || elements_.Eccentricity() > 0.999)
      {
        throw new SatelliteException("Eccentricity out of range");
      }

      if (elements_.Inclination() < 0.0 || elements_.Inclination() > Global.kPI)
      {
        throw new SatelliteException("Inclination out of range");
      }

      common_consts_.cosio = Math.Cos(elements_.Inclination());
      common_consts_.sinio = Math.Sin(elements_.Inclination());
      double theta2 = common_consts_.cosio * common_consts_.cosio;
      common_consts_.x3thm1 = 3.0 * theta2 - 1.0;
      double eosq = elements_.Eccentricity() * elements_.Eccentricity();
      double betao2 = 1.0 - eosq;
      double betao = Math.Sqrt(betao2);

      if (elements_.Period() >= 225.0)
      {
        use_deep_space_ = true;
      }
      else
      {
        use_deep_space_ = false;
        use_simple_model_ = false;
        /*
         * for perigee less than 220 kilometers, the simple_model flag is set
         * and the equations are truncated to linear variation in sqrt a and
         * quadratic variation in mean anomly. also, the c3 term, the
         * delta omega term and the delta m term are dropped
         */
        if (elements_.Perigee() < 220.0)
        {
          use_simple_model_ = true;
        }
      }

      /*
     * for perigee below 156km, the values of
     * s4 and qoms2t are altered
     */
      double s4 = Global.kS;
      double qoms24 = Global.kQOMS2T;
      if (elements_.Perigee() < 156.0)
      {
        s4 = elements_.Perigee() - 78.0;
        if (elements_.Perigee() < 98.0)
        {
          s4 = 20.0;
        }
        qoms24 = Math.Pow((120.0 - s4) * Global.kAE / Global.kXKMPER, 4.0);
        s4 = s4 / Global.kXKMPER + Global.kAE;
      }

      /*
     * generate constants
     */
      double pinvsq = 1.0
                      / (elements_.RecoveredSemiMajorAxis()
                      * elements_.RecoveredSemiMajorAxis()
                      * betao2 * betao2);
      double tsi = 1.0 / (elements_.RecoveredSemiMajorAxis() - s4);
      common_consts_.eta = elements_.RecoveredSemiMajorAxis()
      * elements_.Eccentricity() * tsi;
      double etasq = common_consts_.eta * common_consts_.eta;
      double eeta = elements_.Eccentricity() * common_consts_.eta;
      double psisq = Math.Abs(1.0 - etasq);
      double coef = qoms24 * Math.Pow(tsi, 4.0);
      double coef1 = coef / Math.Pow(psisq, 3.5);
      double c2 = coef1 * elements_.RecoveredMeanMotion()
                  * (elements_.RecoveredSemiMajorAxis()
                  * (1.0 + 1.5 * etasq + eeta * (4.0 + etasq))
                  + 0.75 * Global.kCK2 * tsi / psisq * common_consts_.x3thm1
                  * (8.0 + 3.0 * etasq * (8.0 + etasq)));
      common_consts_.c1 = elements_.BStar() * c2;
      common_consts_.a3ovk2 = -Global.kXJ3 / Global.kCK2 * Global.kAE * Global.kAE * Global.kAE;
      common_consts_.x1mth2 = 1.0 - theta2;
      common_consts_.c4 = 2.0 * elements_.RecoveredMeanMotion()
      * coef1 * elements_.RecoveredSemiMajorAxis() * betao2
      * (common_consts_.eta * (2.0 + 0.5 * etasq) + elements_.Eccentricity()
      * (0.5 + 2.0 * etasq)
      - 2.0 * Global.kCK2 * tsi / (elements_.RecoveredSemiMajorAxis() * psisq)
      * (-3.0 * common_consts_.x3thm1 * (1.0 - 2.0 * eeta + etasq
      * (1.5 - 0.5 * eeta))
      + 0.75 * common_consts_.x1mth2 * (2.0 * etasq - eeta *
      (1.0 + etasq)) * Math.Cos(2.0 * elements_.ArgumentPerigee())));
      double theta4 = theta2 * theta2;
      double temp1 = 3.0 * Global.kCK2 * pinvsq * elements_.RecoveredMeanMotion();
      double temp2 = temp1 * Global.kCK2 * pinvsq;
      double temp3 = 1.25 * Global.kCK4 * pinvsq * pinvsq * elements_.RecoveredMeanMotion();
      common_consts_.xmdot = elements_.RecoveredMeanMotion() + 0.5 * temp1 * betao *
      common_consts_.x3thm1 + 0.0625 * temp2 * betao *
      (13.0 - 78.0 * theta2 + 137.0 * theta4);
      double x1m5th = 1.0 - 5.0 * theta2;
      common_consts_.omgdot = -0.5 * temp1 * x1m5th +
      0.0625 * temp2 * (7.0 - 114.0 * theta2 + 395.0 * theta4) +
      temp3 * (3.0 - 36.0 * theta2 + 49.0 * theta4);
      double xhdot1 = -temp1 * common_consts_.cosio;
      common_consts_.xnodot = xhdot1 + (0.5 * temp2 * (4.0 - 19.0 * theta2) + 2.0 * temp3 *
      (3.0 - 7.0 * theta2)) * common_consts_.cosio;
      common_consts_.xnodcf = 3.5 * betao2 * xhdot1 * common_consts_.c1;
      common_consts_.t2cof = 1.5 * common_consts_.c1;

      if (Math.Abs(common_consts_.cosio + 1.0) > 1.5e-12)
      {
        common_consts_.xlcof = 0.125 * common_consts_.a3ovk2 * common_consts_.sinio * (3.0 + 5.0 * common_consts_.cosio) / (1.0 + common_consts_.cosio);
      }
      else
      {
        common_consts_.xlcof = 0.125 * common_consts_.a3ovk2 * common_consts_.sinio * (3.0 + 5.0 * common_consts_.cosio) / 1.5e-12;
      }

      common_consts_.aycof = 0.25 * common_consts_.a3ovk2 * common_consts_.sinio;
      common_consts_.x7thm1 = 7.0 * theta2 - 1.0;

      if (use_deep_space_)
      {
        deepspace_consts_.gsto = elements_.Epoch().ToGreenwichSiderealTime();

        DeepSpaceInitialise(eosq, common_consts_.sinio, common_consts_.cosio, betao,
          theta2, betao2,
          common_consts_.xmdot, common_consts_.omgdot, common_consts_.xnodot);
      }
      else
      {
        double c3 = 0.0;
        if (elements_.Eccentricity() > 1.0e-4)
        {
          c3 = coef * tsi * common_consts_.a3ovk2 * elements_.RecoveredMeanMotion() * Global.kAE *
          common_consts_.sinio / elements_.Eccentricity();
        }

        nearspace_consts_.c5 = 2.0 * coef1 * elements_.RecoveredSemiMajorAxis() * betao2 * (1.0 + 2.75 *
        (etasq + eeta) + eeta * etasq);
        nearspace_consts_.omgcof = elements_.BStar() * c3 * Math.Cos(elements_.ArgumentPerigee());

        nearspace_consts_.xmcof = 0.0;
        if (elements_.Eccentricity() > 1.0e-4)
        {
          nearspace_consts_.xmcof = -Global.kTWOTHIRD * coef * elements_.BStar() * Global.kAE / eeta;
        }

        nearspace_consts_.delmo = Math.Pow(1.0 + common_consts_.eta * (Math.Cos(elements_.MeanAnomoly())), 3.0);
        nearspace_consts_.sinmo = Math.Sin(elements_.MeanAnomoly());

        if (!use_simple_model_)
        {
          double c1sq = common_consts_.c1 * common_consts_.c1;
          nearspace_consts_.d2 = 4.0 * elements_.RecoveredSemiMajorAxis() * tsi * c1sq;
          double temp = nearspace_consts_.d2 * tsi * common_consts_.c1 / 3.0;
          nearspace_consts_.d3 = (17.0 * elements_.RecoveredSemiMajorAxis() + s4) * temp;
          nearspace_consts_.d4 = 0.5 * temp * elements_.RecoveredSemiMajorAxis() *
          tsi * (221.0 * elements_.RecoveredSemiMajorAxis() + 31.0 * s4) * common_consts_.c1;
          nearspace_consts_.t3cof = nearspace_consts_.d2 + 2.0 * c1sq;
          nearspace_consts_.t4cof = 0.25 * (3.0 * nearspace_consts_.d3 + common_consts_.c1 *
          (12.0 * nearspace_consts_.d2 + 10.0 * c1sq));
          nearspace_consts_.t5cof = 0.2 * (3.0 * nearspace_consts_.d4 + 12.0 * common_consts_.c1 *
          nearspace_consts_.d3 + 6.0 * nearspace_consts_.d2 * nearspace_consts_.d2 + 15.0 *
          c1sq * (2.0 * nearspace_consts_.d2 + c1sq));
        }
      }
    }

    private Eci FindPositionSDP4(double tsince)
    {
      /*
     * the final values
     */
      double e;
      double a;
      double omega;
      double xl;
      double xnode;
      double xincl;

      /*
     * update for secular gravity and atmospheric drag
     */
      double xmdf = elements_.MeanAnomoly()
                    + common_consts_.xmdot * tsince;
      double omgadf = elements_.ArgumentPerigee()
                      + common_consts_.omgdot * tsince;
      double xnoddf = elements_.AscendingNode()
                      + common_consts_.xnodot * tsince;

      double tsq = tsince * tsince;
      xnode = xnoddf + common_consts_.xnodcf * tsq;
      double tempa = 1.0 - common_consts_.c1 * tsince;
      double tempe = elements_.BStar() * common_consts_.c4 * tsince;
      double templ = common_consts_.t2cof * tsq;

      double xn = elements_.RecoveredMeanMotion();
      e = elements_.Eccentricity();
      xincl = elements_.Inclination();

      DeepSpaceSecular(tsince, xmdf, omgadf, xnode, e, xincl, xn);

      if (xn <= 0.0)
      {
        throw new SatelliteException("Error: (xn <= 0.0)");
      }

      a = Math.Pow(Global.kXKE / xn, Global.kTWOTHIRD) * tempa * tempa;
      e -= tempe;
      double xmam = xmdf + elements_.RecoveredMeanMotion() * templ;

      DeepSpacePeriodics(tsince, e, xincl, omgadf, xnode, xmam);

      /*
     * keeping xincl positive important unless you need to display xincl
     * and dislike negative inclinations
     */
      if (xincl < 0.0)
      {
        xincl = -xincl;
        xnode += Global.kPI;
        omgadf -= Global.kPI;
      }

      xl = xmam + omgadf + xnode;
      omega = omgadf;

      /*
     * fix tolerance for error recognition
     */
      if (e <= -0.001)
      {
        throw new SatelliteException("Error: (e <= -0.001)");
      }
      else if (e < 1.0e-6)
      {
        e = 1.0e-6;
      }
      else if (e > (1.0 - 1.0e-6))
      {
        e = 1.0 - 1.0e-6;
      }

      /*
     * re-compute the perturbed values
     */
      double perturbed_sinio = Math.Sin(xincl);
      double perturbed_cosio = Math.Cos(xincl);

      double perturbed_theta2 = perturbed_cosio * perturbed_cosio;

      double perturbed_x3thm1 = 3.0 * perturbed_theta2 - 1.0;
      double perturbed_x1mth2 = 1.0 - perturbed_theta2;
      double perturbed_x7thm1 = 7.0 * perturbed_theta2 - 1.0;

      double perturbed_xlcof;
      if (Math.Abs(perturbed_cosio + 1.0) > 1.5e-12)
      {
        perturbed_xlcof = 0.125 * common_consts_.a3ovk2 * perturbed_sinio
        * (3.0 + 5.0 * perturbed_cosio) / (1.0 + perturbed_cosio);
      }
      else
      {
        perturbed_xlcof = 0.125 * common_consts_.a3ovk2 * perturbed_sinio
        * (3.0 + 5.0 * perturbed_cosio) / 1.5e-12;
      }

      double perturbed_aycof = 0.25 * common_consts_.a3ovk2
                               * perturbed_sinio;

      /*
     * using calculated values, find position and velocity
     */
      return CalculateFinalPositionVelocity(tsince, e,
        a, omega, xl, xnode,
        xincl, perturbed_xlcof, perturbed_aycof,
        perturbed_x3thm1, perturbed_x1mth2, perturbed_x7thm1,
        perturbed_cosio, perturbed_sinio);

    }

    private Eci FindPositionSGP4(double tsince)
    {
      /*
     * the final values
     */
      double e;
      double a;
      double omega;
      double xl;
      double xnode;
      double xincl;

      /*
     * update for secular gravity and atmospheric drag
     */
      double xmdf = elements_.MeanAnomoly()
                    + common_consts_.xmdot * tsince;
      double omgadf = elements_.ArgumentPerigee()
                      + common_consts_.omgdot * tsince;
      double xnoddf = elements_.AscendingNode()
                      + common_consts_.xnodot * tsince;

      double tsq = tsince * tsince;
      xnode = xnoddf + common_consts_.xnodcf * tsq;
      double tempa = 1.0 - common_consts_.c1 * tsince;
      double tempe = elements_.BStar() * common_consts_.c4 * tsince;
      double templ = common_consts_.t2cof * tsq;

      xincl = elements_.Inclination();
      omega = omgadf;
      double xmp = xmdf;

      if (!use_simple_model_)
      {
        double delomg = nearspace_consts_.omgcof * tsince;
        double delm = nearspace_consts_.xmcof
                      * (Math.Pow(1.0 + common_consts_.eta * Math.Cos(xmdf), 3.0)
                      * -nearspace_consts_.delmo);
        double temp = delomg + delm;

        xmp += temp;
        omega -= temp;

        double tcube = tsq * tsince;
        double tfour = tsince * tcube;

        tempa = tempa - nearspace_consts_.d2 * tsq - nearspace_consts_.d3
        * tcube - nearspace_consts_.d4 * tfour;
        tempe += elements_.BStar() * nearspace_consts_.c5
        * (Math.Sin(xmp) - nearspace_consts_.sinmo);
        templ += nearspace_consts_.t3cof * tcube + tfour
        * (nearspace_consts_.t4cof + tsince * nearspace_consts_.t5cof);
      }

      a = elements_.RecoveredSemiMajorAxis() * tempa * tempa;
      e = elements_.Eccentricity() - tempe;
      xl = xmp + omega + xnode + elements_.RecoveredMeanMotion() * templ;

      /*
     * fix tolerance for error recognition
     */
      if (e <= -0.001)
      {
        throw new SatelliteException("Error: (e <= -0.001)");
      }
      else if (e < 1.0e-6)
      {
        e = 1.0e-6;
      }
      else if (e > (1.0 - 1.0e-6))
      {
        e = 1.0 - 1.0e-6;
      }

      /*
     * using calculated values, find position and velocity
     * we can pass in constants from Initialise() as these dont change
     */
      return CalculateFinalPositionVelocity(tsince, e,
        a, omega, xl, xnode,
        xincl, common_consts_.xlcof, common_consts_.aycof,
        common_consts_.x3thm1, common_consts_.x1mth2, common_consts_.x7thm1,
        common_consts_.cosio, common_consts_.sinio);
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
      double x3thm1,
      double x1mth2,
      double x7thm1,
      double cosio,
      double sinio)
    {
      double beta2 = 1.0 - e * e;
      double xn = Global.kXKE / Math.Pow(a, 1.5);
      /*
     * long period periodics
     */
      double axn = e * Math.Cos(omega);
      double temp11 = 1.0 / (a * beta2);
      double xll = temp11 * xlcof * axn;
      double aynl = temp11 * aycof;
      double xlt = xl + xll;
      double ayn = e * Math.Sin(omega) + aynl;
      double elsq = axn * axn + ayn * ayn;

      if (elsq >= 1.0)
      {
        throw new SatelliteException("Error: (elsq >= 1.0)");
      }

      /*
     * solve keplers equation
     * - solve using Newton-Raphson root solving
     * - here capu is almost the mean anomoly
     * - initialise the eccentric anomaly term epw
     * - The fmod saves reduction of angle to +/-2pi in sin/cos() and prevents
     * convergence problems.
     */
      double capu = (xlt - xnode) % Global.kTWOPI;
      double epw = capu;

      double sinepw = 0.0;
      double cosepw = 0.0;
      double ecose = 0.0;
      double esine = 0.0;

      /*
     * sensibility check for N-R correction
     */
      double max_newton_naphson = 1.25 * Math.Abs(Math.Sqrt(elsq));

      bool kepler_running = true;

      for (int i = 0; i < 10 && kepler_running; i++)
      {
        sinepw = Math.Sin(epw);
        cosepw = Math.Cos(epw);
        ecose = axn * cosepw + ayn * sinepw;
        esine = axn * sinepw - ayn * cosepw;

        double f = capu - epw + esine;

        if (Math.Abs(f) < 1.0e-12)
        {
          kepler_running = false;
        }
        else
        {
          /*
             * 1st order Newton-Raphson correction
             */
          double fdot = 1.0 - ecose;
          double delta_epw = f / fdot;

          /*
             * 2nd order Newton-Raphson correction.
             * f / (fdot - 0.5 * d2f * f/fdot)
             */
          if (i == 0)
          {
            if (delta_epw > max_newton_naphson)
            {
              delta_epw = max_newton_naphson;
            }
            else if (delta_epw < -max_newton_naphson)
            {
              delta_epw = -max_newton_naphson;
            }
          }
          else
          {
            delta_epw = f / (fdot + 0.5 * esine * delta_epw);
          }

          /*
             * Newton-Raphson correction of -F/DF
             */
          epw += delta_epw;
        }
      }
      /*
     * short period preliminary quantities
     */
      double temp21 = 1.0 - elsq;
      double pl = a * temp21;

      if (pl < 0.0)
      {
        throw new SatelliteException("Error: (pl < 0.0)");
      }

      double r = a * (1.0 - ecose);
      double temp31 = 1.0 / r;
      double rdot = Global.kXKE * Math.Sqrt(a) * esine * temp31;
      double rfdot = Global.kXKE * Math.Sqrt(pl) * temp31;
      double temp32 = a * temp31;
      double betal = Math.Sqrt(temp21);
      double temp33 = 1.0 / (1.0 + betal);
      double cosu = temp32 * (cosepw - axn + ayn * esine * temp33);
      double sinu = temp32 * (sinepw - ayn - axn * esine * temp33);
      double u = Math.Atan2(sinu, cosu);
      double sin2u = 2.0 * sinu * cosu;
      double cos2u = 2.0 * cosu * cosu - 1.0;

      /*
     * update for short periodics
     */
      double temp41 = 1.0 / pl;
      double temp42 = Global.kCK2 * temp41;
      double temp43 = temp42 * temp41;

      double rk = r * (1.0 - 1.5 * temp43 * betal * x3thm1)
                  + 0.5 * temp42 * x1mth2 * cos2u;
      double uk = u - 0.25 * temp43 * x7thm1 * sin2u;
      double xnodek = xnode + 1.5 * temp43 * cosio * sin2u;
      double xinck = xincl + 1.5 * temp43 * cosio * sinio * cos2u;
      double rdotk = rdot - xn * temp42 * x1mth2 * sin2u;
      double rfdotk = rfdot + xn * temp42 * (x1mth2 * cos2u + 1.5 * x3thm1);

      /*
     * orientation vectors
     */
      double sinuk = Math.Sin(uk);
      double cosuk = Math.Cos(uk);
      double sinik = Math.Sin(xinck);
      double cosik = Math.Cos(xinck);
      double sinnok = Math.Sin(xnodek);
      double cosnok = Math.Cos(xnodek);
      double xmx = -sinnok * cosik;
      double xmy = cosnok * cosik;
      double ux = xmx * sinuk + cosnok * cosuk;
      double uy = xmy * sinuk + sinnok * cosuk;
      double uz = sinik * sinuk;
      double vx = xmx * cosuk - cosnok * sinuk;
      double vy = xmy * cosuk - sinnok * sinuk;
      double vz = sinik * cosuk;
      /*
     * position and velocity
     */
      double x = rk * ux * Global.kXKMPER;
      double y = rk * uy * Global.kXKMPER;
      double z = rk * uz * Global.kXKMPER;
      Vector position = new Vector(x, y, z);
      double xdot = (rdotk * ux + rfdotk * vx) * Global.kXKMPER / 60.0;
      double ydot = (rdotk * uy + rfdotk * vy) * Global.kXKMPER / 60.0;
      double zdot = (rdotk * uz + rfdotk * vz) * Global.kXKMPER / 60.0;
      Vector velocity = new Vector(xdot, ydot, zdot);

      if (rk < 1.0)
      {
        throw new DecayedException(
          elements_.Epoch().AddMinutes(tsince),
          position,
          velocity);
      }

      return new Eci(elements_.Epoch().AddMinutes(tsince), position, velocity);
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
      double se = 0.0;
      double si = 0.0;
      double sl = 0.0;
      double sgh = 0.0;
      double shdq = 0.0;

      double bfact = 0.0;

      const double ZNS = 1.19459E-5;
      const double C1SS = 2.9864797E-6;
      const double ZES = 0.01675;
      const double ZNL = 1.5835218E-4;
      const double C1L = 4.7968065E-7;
      const double ZEL = 0.05490;
      const double ZCOSIS = 0.91744867;
      const double ZSINI = 0.39785416;
      const double ZSINGS = -0.98088458;
      const double ZCOSGS = 0.1945905;
      const double Q22 = 1.7891679E-6;
      const double Q31 = 2.1460748E-6;
      const double Q33 = 2.2123015E-7;
      const double ROOT22 = 1.7891679E-6;
      const double ROOT32 = 3.7393792E-7;
      const double ROOT44 = 7.3636953E-9;
      const double ROOT52 = 1.1428639E-7;
      const double ROOT54 = 2.1765803E-9;

      double aqnv = 1.0 / elements_.RecoveredSemiMajorAxis();
      double xpidot = omgdot + xnodot;
      double sinq = Math.Sin(elements_.AscendingNode());
      double cosq = Math.Cos(elements_.AscendingNode());
      double sing = Math.Sin(elements_.ArgumentPerigee());
      double cosg = Math.Cos(elements_.ArgumentPerigee());

      /*
     * initialize lunar / solar terms
     */
      double jday = elements_.Epoch().ToJulian() - Global.kEPOCH_JAN1_12H_2000;

      double xnodce = 4.5236020 - 9.2422029e-4 * jday;
      double xnodce_temp = xnodce % Global.kTWOPI;
      double stem = Math.Sin(xnodce_temp);
      double ctem = Math.Cos(xnodce_temp);
      double zcosil = 0.91375164 - 0.03568096 * ctem;
      double zsinil = Math.Sqrt(1.0 - zcosil * zcosil);
      double zsinhl = 0.089683511 * stem / zsinil;
      double zcoshl = Math.Sqrt(1.0 - zsinhl * zsinhl);
      double c = 4.7199672 + 0.22997150 * jday;
      double gam = 5.8351514 + 0.0019443680 * jday;
      deepspace_consts_.zmol = Util.WrapTwoPI(c - gam);
      double zx = 0.39785416 * stem / zsinil;
      double zy = zcoshl * ctem + 0.91744867 * zsinhl * stem;
      zx = Math.Atan2(zx, zy);
      zx = (gam + zx - xnodce) % Global.kTWOPI;

      double zcosgl = Math.Cos(zx);
      double zsingl = Math.Sin(zx);
      deepspace_consts_.zmos = Util.WrapTwoPI(6.2565837 + 0.017201977 * jday);

      /*
     * do solar terms
     */
      double zcosg = ZCOSGS;
      double zsing = ZSINGS;
      double zcosi = ZCOSIS;
      double zsini = ZSINI;
      double zcosh = cosq;
      double zsinh = sinq;
      double cc = C1SS;
      double zn = ZNS;
      double ze = ZES;
      double xnoi = 1.0 / elements_.RecoveredMeanMotion();

      for (int cnt = 0; cnt < 2; cnt++)
      {
        /*
         * solar terms are done a second time after lunar terms are done
         */
        double a1 = zcosg * zcosh + zsing * zcosi * zsinh;
        double a3 = -zsing * zcosh + zcosg * zcosi * zsinh;
        double a7 = -zcosg * zsinh + zsing * zcosi * zcosh;
        double a8 = zsing * zsini;
        double a9 = zsing * zsinh + zcosg * zcosi * zcosh;
        double a10 = zcosg * zsini;
        double a2 = cosio * a7 + sinio * a8;
        double a4 = cosio * a9 + sinio * a10;
        double a5 = -sinio * a7 + cosio * a8;
        double a6 = -sinio * a9 + cosio * a10;
        double x1 = a1 * cosg + a2 * sing;
        double x2 = a3 * cosg + a4 * sing;
        double x3 = -a1 * sing + a2 * cosg;
        double x4 = -a3 * sing + a4 * cosg;
        double x5 = a5 * sing;
        double x6 = a6 * sing;
        double x7 = a5 * cosg;
        double x8 = a6 * cosg;
        double z31 = 12.0 * x1 * x1 - 3.0 * x3 * x3;
        double z32 = 24.0 * x1 * x2 - 6.0 * x3 * x4;
        double z33 = 12.0 * x2 * x2 - 3.0 * x4 * x4;
        double z1 = 3.0 * (a1 * a1 + a2 * a2) + z31 * eosq;
        double z2 = 6.0 * (a1 * a3 + a2 * a4) + z32 * eosq;
        double z3 = 3.0 * (a3 * a3 + a4 * a4) + z33 * eosq;

        double z11 = -6.0 * a1 * a5
                     + eosq * (-24.0 * x1 * x7 - 6.0 * x3 * x5);
        double z12 = -6.0 * (a1 * a6 + a3 * a5)
                     + eosq * (-24.0 * (x2 * x7 + x1 * x8) - 6.0 * (x3 * x6 + x4 * x5));
        double z13 = -6.0 * a3 * a6
                     + eosq * (-24.0 * x2 * x8 - 6.0 * x4 * x6);
        double z21 = 6.0 * a2 * a5
                     + eosq * (24.0 * x1 * x5 - 6.0 * x3 * x7);
        double z22 = 6.0 * (a4 * a5 + a2 * a6)
                     + eosq * (24.0 * (x2 * x5 + x1 * x6) - 6.0 * (x4 * x7 + x3 * x8));
        double z23 = 6.0 * a4 * a6
                     + eosq * (24.0 * x2 * x6 - 6.0 * x4 * x8);

        z1 = z1 + z1 + betao2 * z31;
        z2 = z2 + z2 + betao2 * z32;
        z3 = z3 + z3 + betao2 * z33;

        double s3 = cc * xnoi;
        double s2 = -0.5 * s3 / betao;
        double s4 = s3 * betao;
        double s1 = -15.0 * elements_.Eccentricity() * s4;
        double s5 = x1 * x3 + x2 * x4;
        double s6 = x2 * x3 + x1 * x4;
        double s7 = x2 * x4 - x1 * x3;

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
        if (elements_.Inclination() < 5.2359877e-2
            || elements_.Inclination() > Global.kPI - 5.2359877e-2)
        {
          shdq = 0.0;
        }
        else
        {
          shdq = (-zn * s2 * (z21 + z23)) / sinio;
        }

        deepspace_consts_.ee2 = 2.0 * s1 * s6;
        deepspace_consts_.e3 = 2.0 * s1 * s7;
        deepspace_consts_.xi2 = 2.0 * s2 * z12;
        deepspace_consts_.xi3 = 2.0 * s2 * (z13 - z11);
        deepspace_consts_.xl2 = -2.0 * s3 * z2;
        deepspace_consts_.xl3 = -2.0 * s3 * (z3 - z1);
        deepspace_consts_.xl4 = -2.0 * s3 * (-21.0 - 9.0 * eosq) * ze;
        deepspace_consts_.xgh2 = 2.0 * s4 * z32;
        deepspace_consts_.xgh3 = 2.0 * s4 * (z33 - z31);
        deepspace_consts_.xgh4 = -18.0 * s4 * ze;
        deepspace_consts_.xh2 = -2.0 * s2 * z22;
        deepspace_consts_.xh3 = -2.0 * s2 * (z23 - z21);

        if (cnt == 1)
        {
          break;
        }
        /*
         * do lunar terms
         */
        deepspace_consts_.sse = se;
        deepspace_consts_.ssi = si;
        deepspace_consts_.ssl = sl;
        deepspace_consts_.ssh = shdq;
        deepspace_consts_.ssg = sgh - cosio * deepspace_consts_.ssh;
        deepspace_consts_.se2 = deepspace_consts_.ee2;
        deepspace_consts_.si2 = deepspace_consts_.xi2;
        deepspace_consts_.sl2 = deepspace_consts_.xl2;
        deepspace_consts_.sgh2 = deepspace_consts_.xgh2;
        deepspace_consts_.sh2 = deepspace_consts_.xh2;
        deepspace_consts_.se3 = deepspace_consts_.e3;
        deepspace_consts_.si3 = deepspace_consts_.xi3;
        deepspace_consts_.sl3 = deepspace_consts_.xl3;
        deepspace_consts_.sgh3 = deepspace_consts_.xgh3;
        deepspace_consts_.sh3 = deepspace_consts_.xh3;
        deepspace_consts_.sl4 = deepspace_consts_.xl4;
        deepspace_consts_.sgh4 = deepspace_consts_.xgh4;
        zcosg = zcosgl;
        zsing = zsingl;
        zcosi = zcosil;
        zsini = zsinil;
        zcosh = zcoshl * cosq + zsinhl * sinq;
        zsinh = sinq * zcoshl - cosq * zsinhl;
        zn = ZNL;
        cc = C1L;
        ze = ZEL;
      }

      deepspace_consts_.sse += se;
      deepspace_consts_.ssi += si;
      deepspace_consts_.ssl += sl;
      deepspace_consts_.ssg += sgh - cosio * shdq;
      deepspace_consts_.ssh += shdq;

      deepspace_consts_.resonance_flag = false;
      deepspace_consts_.synchronous_flag = false;
      bool initialise_integrator = true;

      if (elements_.RecoveredMeanMotion() < 0.0052359877
          && elements_.RecoveredMeanMotion() > 0.0034906585)
      {
        /*
         * 24h synchronous resonance terms initialisation
         */
        deepspace_consts_.resonance_flag = true;
        deepspace_consts_.synchronous_flag = true;

        double g200 = 1.0 + eosq * (-2.5 + 0.8125 * eosq);
        double g310 = 1.0 + 2.0 * eosq;
        double g300 = 1.0 + eosq * (-6.0 + 6.60937 * eosq);
        double f220 = 0.75 * (1.0 + cosio) * (1.0 + cosio);
        double f311 = 0.9375 * sinio * sinio * (1.0 + 3.0 * cosio)
                      - 0.75 * (1.0 + cosio);
        double f330 = 1.0 + cosio;
        f330 = 1.875 * f330 * f330 * f330;
        deepspace_consts_.del1 = 3.0 * elements_.RecoveredMeanMotion()
        * elements_.RecoveredMeanMotion()
        * aqnv * aqnv;
        deepspace_consts_.del2 = 2.0 * deepspace_consts_.del1
        * f220 * g200 * Q22;
        deepspace_consts_.del3 = 3.0 * deepspace_consts_.del1
        * f330 * g300 * Q33 * aqnv;
        deepspace_consts_.del1 = deepspace_consts_.del1
        * f311 * g310 * Q31 * aqnv;

        integrator_consts_.xlamo = elements_.MeanAnomoly()
        + elements_.AscendingNode()
        + elements_.ArgumentPerigee()
        - deepspace_consts_.gsto;
        bfact = xmdot + xpidot - Global.kTHDT;
        bfact += deepspace_consts_.ssl
        + deepspace_consts_.ssg
        + deepspace_consts_.ssh;
      }
      else if (elements_.RecoveredMeanMotion() < 8.26e-3
               || elements_.RecoveredMeanMotion() > 9.24e-3
               || elements_.Eccentricity() < 0.5)
      {
        initialise_integrator = false;
      }
      else
      {
        /*
         * geopotential resonance initialisation for 12 hour orbits
         */
        deepspace_consts_.resonance_flag = true;

        double g211;
        double g310;
        double g322;
        double g410;
        double g422;
        double g520;

        double g201 = -0.306 - (elements_.Eccentricity() - 0.64) * 0.440;

        if (elements_.Eccentricity() <= 0.65)
        {
          g211 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            3.616, -13.247, +16.290, 0.0);
          g310 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            -19.302, 117.390, -228.419, 156.591);
          g322 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            -18.9068, 109.7927, -214.6334, 146.5816);
          g410 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            -41.122, 242.694, -471.094, 313.953);
          g422 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            -146.407, 841.880, -1629.014, 1083.435);
          g520 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            -532.114, 3017.977, -5740.032, 3708.276);
        }
        else
        {
          g211 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            -72.099, 331.819, -508.738, 266.724);
          g310 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            -346.844, 1582.851, -2415.925, 1246.113);
          g322 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            -342.585, 1554.908, -2366.899, 1215.972);
          g410 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            -1052.797, 4758.686, -7193.992, 3651.957);
          g422 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            -3581.69, 16178.11, -24462.77, 12422.52);

          if (elements_.Eccentricity() <= 0.715)
          {
            g520 = EvaluateCubicPolynomial(elements_.Eccentricity(),
              1464.74, -4664.75, 3763.64, 0.0);
          }
          else
          {
            g520 = EvaluateCubicPolynomial(elements_.Eccentricity(),
              -5149.66, 29936.92, -54087.36, 31324.56);
          }
        }

        double g533;
        double g521;
        double g532;

        if (elements_.Eccentricity() < 0.7)
        {
          g533 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            -919.2277, 4988.61, -9064.77, 5542.21);
          g521 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            -822.71072, 4568.6173, -8491.4146, 5337.524);
          g532 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            -853.666, 4690.25, -8624.77, 5341.4);
        }
        else
        {
          g533 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            -37995.78, 161616.52, -229838.2, 109377.94);
          g521 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            -51752.104, 218913.95, -309468.16, 146349.42);
          g532 = EvaluateCubicPolynomial(elements_.Eccentricity(),
            -40023.88, 170470.89, -242699.48, 115605.82);
        }

        double sini2 = sinio * sinio;
        double f220 = 0.75 * (1.0 + 2.0 * cosio + theta2);
        double f221 = 1.5 * sini2;
        double f321 = 1.875 * sinio * (1.0 - 2.0 * cosio - 3.0 * theta2);
        double f322 = -1.875 * sinio * (1.0 + 2.0 * cosio - 3.0 * theta2);
        double f441 = 35.0 * sini2 * f220;
        double f442 = 39.3750 * sini2 * sini2;
        double f522 = 9.84375 * sinio
                      * (sini2 * (1.0 - 2.0 * cosio - 5.0 * theta2)
                      + 0.33333333 * (-2.0 + 4.0 * cosio + 6.0 * theta2));
        double f523 = sinio
                      * (4.92187512 * sini2 * (-2.0 - 4.0 * cosio + 10.0 * theta2)
                      + 6.56250012 * (1.0 + 2.0 * cosio - 3.0 * theta2));
        double f542 = 29.53125 * sinio * (2.0 - 8.0 * cosio + theta2 *
                      (-12.0 + 8.0 * cosio + 10.0 * theta2));
        double f543 = 29.53125 * sinio * (-2.0 - 8.0 * cosio + theta2 *
                      (12.0 + 8.0 * cosio - 10.0 * theta2));

        double xno2 = elements_.RecoveredMeanMotion()
                      * elements_.RecoveredMeanMotion();
        double ainv2 = aqnv * aqnv;

        double temp1 = 3.0 * xno2 * ainv2;
        double temp = temp1 * ROOT22;
        deepspace_consts_.d2201 = temp * f220 * g201;
        deepspace_consts_.d2211 = temp * f221 * g211;
        temp1 = temp1 * aqnv;
        temp = temp1 * ROOT32;
        deepspace_consts_.d3210 = temp * f321 * g310;
        deepspace_consts_.d3222 = temp * f322 * g322;
        temp1 = temp1 * aqnv;
        temp = 2.0 * temp1 * ROOT44;
        deepspace_consts_.d4410 = temp * f441 * g410;
        deepspace_consts_.d4422 = temp * f442 * g422;
        temp1 = temp1 * aqnv;
        temp = temp1 * ROOT52;
        deepspace_consts_.d5220 = temp * f522 * g520;
        deepspace_consts_.d5232 = temp * f523 * g532;
        temp = 2.0 * temp1 * ROOT54;
        deepspace_consts_.d5421 = temp * f542 * g521;
        deepspace_consts_.d5433 = temp * f543 * g533;

        integrator_consts_.xlamo = elements_.MeanAnomoly()
        + elements_.AscendingNode()
        + elements_.AscendingNode()
        - deepspace_consts_.gsto
        - deepspace_consts_.gsto;
        bfact = xmdot
        + xnodot + xnodot
        - Global.kTHDT - Global.kTHDT;
        bfact = bfact + deepspace_consts_.ssl
        + deepspace_consts_.ssh
        + deepspace_consts_.ssh;
      }

      if (initialise_integrator)
      {
        /*
         * initialise integrator
         */
        integrator_consts_.xfact = bfact - elements_.RecoveredMeanMotion();
        integrator_params_.atime = 0.0;
        integrator_params_.xni = elements_.RecoveredMeanMotion();
        integrator_params_.xli = integrator_consts_.xlamo;
        /*
         * precompute dot terms for epoch
         */
        DeepSpaceCalcDotTerms(integrator_consts_.values_0);
      }
    }

    private void DeepSpaceCalculateLunarSolarTerms(
      double tsince,
      double pe,
      double pinc,
      double pl,
      double pgh,
      double ph)
    {
      const double ZES = 0.01675;
      const double ZNS = 1.19459E-5;
      const double ZNL = 1.5835218E-4;
      const double ZEL = 0.05490;

      /*
     * calculate solar terms for time tsince
     */
      double zm = deepspace_consts_.zmos + ZNS * tsince;
      double zf = zm + 2.0 * ZES * Math.Sin(zm);
      double sinzf = Math.Sin(zf);
      double f2 = 0.5 * sinzf * sinzf - 0.25;
      double f3 = -0.5 * sinzf * Math.Cos(zf);

      double ses = deepspace_consts_.se2 * f2
                   + deepspace_consts_.se3 * f3;
      double sis = deepspace_consts_.si2 * f2
                   + deepspace_consts_.si3 * f3;
      double sls = deepspace_consts_.sl2 * f2
                   + deepspace_consts_.sl3 * f3
                   + deepspace_consts_.sl4 * sinzf;
      double sghs = deepspace_consts_.sgh2 * f2
                    + deepspace_consts_.sgh3 * f3
                    + deepspace_consts_.sgh4 * sinzf;
      double shs = deepspace_consts_.sh2 * f2
                   + deepspace_consts_.sh3 * f3;

      /*
     * calculate lunar terms for time tsince
     */
      zm = deepspace_consts_.zmol + ZNL * tsince;
      zf = zm + 2.0 * ZEL * Math.Sin(zm);
      sinzf = Math.Sin(zf);
      f2 = 0.5 * sinzf * sinzf - 0.25;
      f3 = -0.5 * sinzf * Math.Cos(zf);

      double sel = deepspace_consts_.ee2 * f2
                   + deepspace_consts_.e3 * f3;
      double sil = deepspace_consts_.xi2 * f2
                   + deepspace_consts_.xi3 * f3;
      double sll = deepspace_consts_.xl2 * f2
                   + deepspace_consts_.xl3 * f3
                   + deepspace_consts_.xl4 * sinzf;
      double sghl = deepspace_consts_.xgh2 * f2
                    + deepspace_consts_.xgh3 * f3
                    + deepspace_consts_.xgh4 * sinzf;
      double shl = deepspace_consts_.xh2 * f2
                   + deepspace_consts_.xh3 * f3;

      /*
     * merge calculated values
     */
      pe = ses + sel;
      pinc = sis + sil;
      pl = sls + sll;
      pgh = sghs + sghl;
      ph = shs + shl;
    }

    private void DeepSpacePeriodics(
      double tsince,
      double em,
      double xinc,
      double omgasm,
      double xnodes,
      double xll)
    {
      /*
     * storage for lunar / solar terms
     * set by DeepSpaceCalculateLunarSolarTerms()
     */
      double pe = 0.0;
      double pinc = 0.0;
      double pl = 0.0;
      double pgh = 0.0;
      double ph = 0.0;

      /*
     * calculate lunar / solar terms for current time
     */
      DeepSpaceCalculateLunarSolarTerms(tsince, pe, pinc, pl, pgh, ph);

      xinc += pinc;
      em += pe;

      /* Spacetrack report #3 has sin/cos from before perturbations
     * added to xinc (oldxinc), but apparently report # 6 has then
     * from after they are added.
     * use for strn3
     * if (elements_.Inclination() >= 0.2)
     * use for gsfc
     * if (xinc >= 0.2)
     * (moved from start of function)
     */
      double sinis = Math.Sin(xinc);
      double cosis = Math.Cos(xinc);

      if (xinc >= 0.2)
      {
        /*
         * apply periodics directly
         */
        double tmp_ph = ph / sinis;

        omgasm += pgh - cosis * tmp_ph;
        xnodes += tmp_ph;
        xll += pl;
      }
      else
      {
        /*
         * apply periodics with lyddane modification
         */
        double sinok = Math.Sin(xnodes);
        double cosok = Math.Cos(xnodes);
        double alfdp = sinis * sinok;
        double betdp = sinis * cosok;
        double dalf = ph * cosok + pinc * cosis * sinok;
        double dbet = -ph * sinok + pinc * cosis * cosok;

        alfdp += dalf;
        betdp += dbet;

        xnodes = Util.WrapTwoPI(xnodes);

        double xls = xll + omgasm + cosis * xnodes;
        double dls = pl + pgh - pinc * xnodes * sinis;
        xls += dls;

        /*
         * save old xnodes value
         */
        double oldxnodes = xnodes;

        xnodes = Math.Atan2(alfdp, betdp);
        if (xnodes < 0.0)
        {
          xnodes += Global.kTWOPI;
        }

        /*
         * Get perturbed xnodes in to same quadrant as original.
         * RAAN is in the range of 0 to 360 degrees
         * atan2 is in the range of -180 to 180 degrees
         */
        if (Math.Abs(oldxnodes - xnodes) > Global.kPI)
        {
          if (xnodes < oldxnodes)
          {
            xnodes += Global.kTWOPI;
          }
          else
          {
            xnodes -= Global.kTWOPI;
          }
        }

        xll += pl;
        omgasm = xls - xll - cosis * xnodes;
      }
    }

    private void DeepSpaceSecular(
      double tsince,
      double xll,
      double omgasm,
      double xnodes,
      double em,
      double xinc,
      double xn)
    {
      const double STEP = 720.0;
      const double STEP2 = 259200.0;

      xll += deepspace_consts_.ssl * tsince;
      omgasm += deepspace_consts_.ssg * tsince;
      xnodes += deepspace_consts_.ssh * tsince;
      em += deepspace_consts_.sse * tsince;
      xinc += deepspace_consts_.ssi * tsince;

      if (deepspace_consts_.resonance_flag)
      {
        /*
         * 1st condition (if tsince is less than one time step from epoch)
         * 2nd condition (if integrator_params_.atime and
         *     tsince are of opposite signs, so zero crossing required)
         * 3rd condition (if tsince is closer to zero than 
         *     integrator_params_.atime, only integrate away from zero)
         */
        if (Math.Abs(tsince) < STEP ||
            tsince * integrator_params_.atime <= 0.0 ||
            Math.Abs(tsince) < Math.Abs(integrator_params_.atime))
        {
          /*
             * restart from epoch
             */
          integrator_params_.atime = 0.0;
          integrator_params_.xni = elements_.RecoveredMeanMotion();
          integrator_params_.xli = integrator_consts_.xlamo;

          /*
             * restore precomputed values for epoch
             */
          integrator_params_.values_t = integrator_consts_.values_0;
        }

        double ft = tsince - integrator_params_.atime;

        /*
         * if time difference (ft) is greater than the time step (720.0)
         * loop around until integrator_params_.atime is within one time step of
         * tsince
         */
        if (Math.Abs(ft) >= STEP)
        {
          /*
             * calculate step direction to allow integrator_params_.atime
             * to catch up with tsince
             */
          double delt = -STEP;
          if (ft >= 0.0)
          {
            delt = STEP;
          }

          do
          {
            /*
                 * integrate using current dot terms
                 */
            DeepSpaceIntegrator(delt, STEP2, integrator_params_.values_t);

            /*
                 * calculate dot terms for next integration
                 */
            DeepSpaceCalcDotTerms(integrator_params_.values_t);

            ft = tsince - integrator_params_.atime;
          } while (Math.Abs(ft) >= STEP);
        }

        /*
         * integrator
         */
        xn = integrator_params_.xni
        + integrator_params_.values_t.xndot * ft
        + integrator_params_.values_t.xnddt * ft * ft * 0.5;
        double xl = integrator_params_.xli
                    + integrator_params_.values_t.xldot * ft
                    + integrator_params_.values_t.xndot * ft * ft * 0.5;
        double temp = -xnodes + deepspace_consts_.gsto + tsince * Global.kTHDT;

        if (deepspace_consts_.synchronous_flag)
        {
          xll = xl + temp - omgasm;
        }
        else
        {
          xll = xl + temp + temp;
        }
      }
    }

    private void DeepSpaceCalcDotTerms(IntegratorValues values)
    {
      const double G22 = 5.7686396;
      const double G32 = 0.95240898;
      const double G44 = 1.8014998;
      const double G52 = 1.0508330;
      const double G54 = 4.4108898;
      const double FASX2 = 0.13130908;
      const double FASX4 = 2.8843198;
      const double FASX6 = 0.37448087;

      if (deepspace_consts_.synchronous_flag)
      {

        values.xndot = deepspace_consts_.del1
        * Math.Sin(integrator_params_.xli - FASX2)
        + deepspace_consts_.del2
        * Math.Sin(2.0 * (integrator_params_.xli - FASX4))
        + deepspace_consts_.del3
        * Math.Sin(3.0 * (integrator_params_.xli - FASX6));
        values.xnddt = deepspace_consts_.del1
        * Math.Cos(integrator_params_.xli - FASX2)
        + 2.0 * deepspace_consts_.del2
        * Math.Cos(2.0 * (integrator_params_.xli - FASX4))
        + 3.0 * deepspace_consts_.del3
        * Math.Cos(3.0 * (integrator_params_.xli - FASX6));
      }
      else
      {
        double xomi = elements_.ArgumentPerigee()
                      + common_consts_.omgdot * integrator_params_.atime;
        double x2omi = xomi + xomi;
        double x2li = integrator_params_.xli + integrator_params_.xli;

        values.xndot = deepspace_consts_.d2201
        * Math.Sin(x2omi + integrator_params_.xli - G22)
        * +deepspace_consts_.d2211
        * Math.Sin(integrator_params_.xli - G22)
        + deepspace_consts_.d3210
        * Math.Sin(xomi + integrator_params_.xli - G32)
        + deepspace_consts_.d3222
        * Math.Sin(-xomi + integrator_params_.xli - G32)
        + deepspace_consts_.d4410
        * Math.Sin(x2omi + x2li - G44)
        + deepspace_consts_.d4422
        * Math.Sin(x2li - G44)
        + deepspace_consts_.d5220
        * Math.Sin(xomi + integrator_params_.xli - G52)
        + deepspace_consts_.d5232
        * Math.Sin(-xomi + integrator_params_.xli - G52)
        + deepspace_consts_.d5421
        * Math.Sin(xomi + x2li - G54)
        + deepspace_consts_.d5433
        * Math.Sin(-xomi + x2li - G54);
        values.xnddt = deepspace_consts_.d2201
        * Math.Cos(x2omi + integrator_params_.xli - G22)
        + deepspace_consts_.d2211
        * Math.Cos(integrator_params_.xli - G22)
        + deepspace_consts_.d3210
        * Math.Cos(xomi + integrator_params_.xli - G32)
        + deepspace_consts_.d3222
        * Math.Cos(-xomi + integrator_params_.xli - G32)
        + deepspace_consts_.d5220
        * Math.Cos(xomi + integrator_params_.xli - G52)
        + deepspace_consts_.d5232
        * Math.Cos(-xomi + integrator_params_.xli - G52)
        + 2.0 * (deepspace_consts_.d4410 * Math.Cos(x2omi + x2li - G44)
        + deepspace_consts_.d4422
        * Math.Cos(x2li - G44)
        + deepspace_consts_.d5421
        * Math.Cos(xomi + x2li - G54)
        + deepspace_consts_.d5433
        * Math.Cos(-xomi + x2li - G54));
      }

      values.xldot = integrator_params_.xni + integrator_consts_.xfact;
      values.xnddt *= values.xldot;
    }

    private void DeepSpaceIntegrator(
      double delt,
      double step2,
      IntegratorValues values)
    {
      /*
     * integrator
     */
      integrator_params_.xli += values.xldot * delt + values.xndot * step2;
      integrator_params_.xni += values.xndot * delt + values.xnddt * step2;

      /*
     * increment integrator time
     */
      integrator_params_.atime += delt;
    }

    private void Reset()
    {
      use_simple_model_ = false;
      use_deep_space_ = false;

      common_consts_ = Empty_CommonConstants;
      nearspace_consts_ = Empty_NearSpaceConstants;
      deepspace_consts_ = Empty_DeepSpaceConstants;
      integrator_consts_ = Empty_IntegratorConstants;
      integrator_params_ = Empty_IntegratorParams;
    }

    /*
     * flags
     */
    private bool use_simple_model_;
    private bool use_deep_space_;

    /*
     * the ants used
     */
    private CommonConstants common_consts_;
    private NearSpaceConstants nearspace_consts_;
    private DeepSpaceConstants deepspace_consts_;
    private IntegratorConstants integrator_consts_;
    private IntegratorParams integrator_params_;

    /*
     * the orbit data
     */
    private OrbitalElements elements_;

    private static SGP4.CommonConstants Empty_CommonConstants = new CommonConstants();
    private static SGP4.NearSpaceConstants Empty_NearSpaceConstants = new NearSpaceConstants();
    private static SGP4.DeepSpaceConstants Empty_DeepSpaceConstants = new DeepSpaceConstants();
    private static SGP4.IntegratorConstants Empty_IntegratorConstants = new IntegratorConstants();
    private static SGP4.IntegratorParams Empty_IntegratorParams = new IntegratorParams();
  }
}
