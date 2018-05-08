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
  public class Global
  {
    public const double kAE = 1.0;
    public const  double kQ0 = 120.0;
    public const double kS0 = 78.0;
    public const double kMU = 398600.8;
    public const double kXKMPER = 6378.135;
    public const double kXJ2 = 1.082616e-3;
    public const double kXJ3 = -2.53881e-6;
    public const double kXJ4 = -1.65597e-6;

    /*
 * alternative XKE
 * affects final results
 * aiaa-2006-6573
 * const double kXKE = 60.0 / sqrt(kXKMPER * kXKMPER * kXKMPER / kMU);
 * dundee
 * const double kXKE = 7.43669161331734132e-2;
 */
    public static double kXKE = 60.0 / Math.Sqrt(kXKMPER * kXKMPER * kXKMPER / kMU);
    public const double kCK2 = 0.5 * kXJ2 * kAE * kAE;
    public const double kCK4 = -0.375 * kXJ4 * kAE * kAE * kAE * kAE;

    /*
 * alternative QOMS2T
 * affects final results
 * aiaa-2006-6573
 * #define QOMS2T   (pow(((Q0 - S0) / XKMPER), 4.0))
 * dundee
 * #define QOMS2T   (1.880279159015270643865e-9)
 */
    public static double kQOMS2T = Math.Pow(((kQ0 - kS0) / kXKMPER), 4.0);

    public const double kS = kAE * (1.0 + kS0 / kXKMPER);
    public const double kPI = 3.14159265358979323846264338327950288419716939937510582;
    public const double kTWOPI = 2.0 * kPI;
    public const double kTWOTHIRD = 2.0 / 3.0;
    public const double kTHDT = 4.37526908801129966e-3;
    /*
 * earth flattening
 */
    public const double kF = 1.0 / 298.26;
    /*
 * earth rotation per sideral day
 */
    public const double kOMEGA_E = 1.00273790934;
    public const double kAU = 1.49597870691e8;

    public const double kSECONDS_PER_DAY = 86400.0;
    public const double kMINUTES_PER_DAY = 1440.0;
    public const double kHOURS_PER_DAY = 24.0;

    // Jan 1.0 1900 = Jan 1 1900 00h UTC
    public const double kEPOCH_JAN1_00H_1900 = 2415019.5;

    // Jan 1.5 1900 = Jan 1 1900 12h UTC
    public const double kEPOCH_JAN1_12H_1900 = 2415020.0;

    // Jan 1.5 2000 = Jan 1 2000 12h UTC
    public const double kEPOCH_JAN1_12H_2000 = 2451545.0;
  }
}

