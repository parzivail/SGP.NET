using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SGP4_Sharp;
using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            //var tle = new Tle(
            //    "1 33591U 09005A   18126.90753522  .00000083  00000-0  70028-4 0  9998",
            //    "2 33591  99.1390 104.1221 0015038  83.2147 277.0734 14.12275499476086"
            //    );

            //var sgp4 = new SGP4(tle);
            
            //var end = DateTime.Now + TimeSpan.FromSeconds(10);
            //long count = 0;
            //while (DateTime.Now <= end)
            //{
            //    var eci = sgp4.FindPosition(SGP4_Sharp.DateTime.Now());
            //    var geo = eci.ToGeodetic();

            //    //Console.WriteLine($"{geo.latitude / Math.PI * 180}\t{geo.longitude / Math.PI * 180}\t{geo.altitude}");
            //    count++;
            //}
            //Console.WriteLine($"{count / 10}op/s");
            //Console.WriteLine($"{10d / count}s/op");
            //Console.ReadKey();

            Application.EnableVisualStyles();
            Application.Run(new FormLauncher());
        }
    }
}
