using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace weather_app.Models
{
    class DailyStatistics
    {
        public string Summary { get; set; }
        public double AverageTemperature { get; set; }
        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set; }

        public double AverageWindSpeed { get; set; }
        public double MinWindSpeed { get; set; }
        public double MaxWindSpeed { get; set; }

        public double AverageRadiation { get; set; }
        public double MinRadiation { get; set; }
        public double MaxRadiation { get; set; }
    }
}
