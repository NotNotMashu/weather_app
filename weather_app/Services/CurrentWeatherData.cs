using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace weather_app.Services
{
    public class CurrentWeatherData
    {
        public DateTime Time { get; set; }
        public double Temperature { get; set; }
        public double WindSpeed { get; set; }
        public double Radiation { get; set; }
        public string Provider { get; set; }
    }
}
