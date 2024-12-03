using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace weather_app.Models
{
    public class CurrentWeatherResponse
    {
        public string Provider { get; set; }
        public double Temperature { get; set; }
        public double WindSpeed { get; set; }
    }
}
