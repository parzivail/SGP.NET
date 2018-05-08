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
 * @brief The extracted orbital elements used by the SGP4 propagator.
 */
  public class OrbitalElements
  {
    public OrbitalElements(Tle tle)
    {
      /*
     * extract and format tle data
     */
      mean_anomoly_ = tle.MeanAnomaly(false);
      ascending_node_ = tle.RightAscendingNode(false);
      argument_perigee_ = tle.ArgumentPerigee(false);
      eccentricity_ = tle.Eccentricity();
      inclination_ = tle.Inclination(false);
      mean_motion_ = tle.MeanMotion() * Global.kTWOPI / Global.kMINUTES_PER_DAY;
      bstar_ = tle.BStar();
      epoch_ = tle.Epoch();

      /*
     * recover original mean motion (xnodp) and semimajor axis (aodp)
     * from input elements
     */
      double a1 = Math.Pow(Global.kXKE / MeanMotion(), Global.kTWOTHIRD);
      double cosio = Math.Cos(Inclination());
      double theta2 = cosio * cosio;
      double x3thm1 = 3.0 * theta2 - 1.0;
      double eosq = Eccentricity() * Eccentricity();
      double betao2 = 1.0 - eosq;
      double betao = Math.Sqrt(betao2);
      double temp = (1.5 * Global.kCK2) * x3thm1 / (betao * betao2);
      double del1 = temp / (a1 * a1);
      double a0 = a1 * (1.0 - del1 * (1.0 / 3.0 + del1 * (1.0 + del1 * 134.0 / 81.0)));
      double del0 = temp / (a0 * a0);

      recovered_mean_motion_ = MeanMotion() / (1.0 + del0);
      /*
     * alternative way to calculate
     * doesnt affect final results
     * recovered_semi_major_axis_ = pow(XKE / RecoveredMeanMotion(), TWOTHIRD);
     */
      recovered_semi_major_axis_ = a0 / (1.0 - del0);

      /*
     * find perigee and period
     */
      perigee_ = (RecoveredSemiMajorAxis() * (1.0 - Eccentricity()) - Global.kAE) * Global.kXKMPER;
      period_ = Global.kTWOPI / RecoveredMeanMotion();
    }

    /*
     * XMO
     */
    public double MeanAnomoly()
    {
      return mean_anomoly_;
    }

    /*
     * XNODEO
     */
    public double AscendingNode()
    {
      return ascending_node_;
    }

    /*
     * OMEGAO
     */
    public double ArgumentPerigee()
    {
      return argument_perigee_;
    }

    /*
     * EO
     */
    public double Eccentricity()
    {
      return eccentricity_;
    }

    /*
     * XINCL
     */
    public double Inclination()
    {
      return inclination_;
    }

    /*
     * XNO
     */
    public double MeanMotion()
    {
      return mean_motion_;
    }

    /*
     * BSTAR
     */
    public double BStar()
    {
      return bstar_;
    }

    /*
     * AODP
     */
    public double RecoveredSemiMajorAxis()
    {
      return recovered_semi_major_axis_;
    }

    /*
     * XNODP
     */
    public double RecoveredMeanMotion()
    {
      return recovered_mean_motion_;
    }

    /*
     * PERIGE
     */
    public double Perigee()
    {
      return perigee_;
    }

    /*
     * Period in minutes
     */
    public double Period()
    {
      return period_;
    }

    /*
     * EPOCH
     */
    public DateTime Epoch()
    {
      return epoch_;
    }

    private double mean_anomoly_;
    private double ascending_node_;
    private double argument_perigee_;
    private double eccentricity_;
    private double inclination_;
    private double mean_motion_;
    private double bstar_;
    private double recovered_semi_major_axis_;
    private double recovered_mean_motion_;
    private double perigee_;
    private double period_;
    private DateTime epoch_;
  }

}