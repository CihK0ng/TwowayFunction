using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwowayFunction
{
    internal class DataBasicHandle
    {
        public class data
        {
            public double Z_tail { get; set; }
            public double Z_res { get; set; }
            public double H_headloss { get; set; }

            public double H_huuich { get; set; }
            public double H_max { get; set; }

           
        }

        

        public double Interpolate(double Pio, double Ho)
        {
            data data = new data();

            data.H_huuich = data.Z_res - data.Z_tail - data.H_headloss; 
            return 0;
        }
    }
}
