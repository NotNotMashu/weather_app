using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace weather_app.Services
{
    public class Coordinates
    {
        public List<Tuple<double, double>> CoordinatePairs { get; private set; }

        public Coordinates()
        {
            InitializeCoordinates();
        }

        private void InitializeCoordinates()
        {
            CoordinatePairs = new List<Tuple<double, double>>
            {
                new Tuple<double, double>(-12.47 ,130.84), // Start
                new Tuple<double, double>(-13.15, 131.11),
                new Tuple<double, double>(-13.69, 131.69),
                new Tuple<double, double>(-14.40, 132.18),
                new Tuple<double, double>(-14.82, 132.94),
                new Tuple<double, double>(-15.68, 133.30), //500
                new Tuple<double, double>(-16.47, 133.39),
                new Tuple<double, double>(-17.34, 133.44),
                new Tuple<double, double>(-18.16, 133.72),
                new Tuple<double, double>(-18.95, 134.13), 
                new Tuple<double, double>(-19.81, 134.20), //1000
                new Tuple<double, double>(-20.71, 134.24),
                new Tuple<double, double>(-21.51, 133.90),
                new Tuple<double, double>(-22.21, 133.43),
                new Tuple<double, double>(-23.03, 133.61),
                new Tuple<double, double>(-23.80, 133.86), //1500
                new Tuple<double, double>(-24.48, 133.28),
                new Tuple<double, double>(-25.34, 133.19),
                new Tuple<double, double>(-26.19, 133.19),
                new Tuple<double, double>(-27.03, 133.44),
                new Tuple<double, double>(-27.76, 134.00), //2000 
                new Tuple<double, double>(-28.59, 134.29),
                new Tuple<double, double>(-29.21, 134.96),
                new Tuple<double, double>(-30.05, 135.20),
                new Tuple<double, double>(-30.84, 135.57),
                new Tuple<double, double>(-31.23, 136.47), //2500
                new Tuple<double, double>(-31.70, 137.22),
                new Tuple<double, double>(-32.45, 137.71),
                new Tuple<double, double>(-33.23, 138.11),
                new Tuple<double, double>(-34.09, 138.20),
                new Tuple<double, double>(-34.93, 138.60) //3000 
            };
        }
    }
}
