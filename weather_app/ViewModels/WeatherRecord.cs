using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace weather_app.ViewModels
{
        public class WeatherRecord
        {
            public string Coordinate { get; set; }
            public List<WeatherRecordHourly> HourlyDataList { get; set; }
        }

        public class WeatherRecordHourly
        {
            public string Time { get; set; }
            public string Temperature { get; set; }
            public string WindSpeed { get; set; }
            public string Radiation { get; set; }
        }
}
