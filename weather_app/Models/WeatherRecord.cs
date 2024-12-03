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
        public double Temperature { get; set; }
        public double WindSpeed { get; set; }
        public double Radiation { get; set; }

        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set; }
        public double MinWindSpeed { get; set; }
        public double MaxWindSpeed { get; set; }
        public double MinRadiation { get; set; }
        public double MaxRadiation { get; set; }
    }
}
