using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace weather_app.Models
{
    public class ComparisonData
    {
        public string coordinate { get; set; }
        public string time { get; set; }
        public double recordedTemp { get; set; }
        public double recordedWind { get; set; }
        public double recordedRad {  get; set; }
        public double forecastTemp { get; set; }
        public double forecastWind { get; set; }
        public double forecastRad { get; set; }
    }
}
