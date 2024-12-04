using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace weather_app.Models
{
    public class ComparisonData
    {
        public string Coordinate { get; set; }
        public List<ComparisonHourly> comparisonHourlies { get; set; }
    }

    public class ComparisonHourly
    {
        public string Time { get; set; }
        public DateTime DateTime => DateTime.Parse(Time);
        public double RecordedTemp { get; set; }
        public double RecordedWind { get; set; }
        public double RecordedRad { get; set; }
        public double ForecastTemp { get; set; }
        public double ForecastWind { get; set; }
        public double ForecastRad { get; set; }

        // Különbségek kiszámítása
        public double TempDiff => RecordedTemp - ForecastTemp; 
        public double WindDiff => RecordedWind - ForecastWind; 
        public double RadDiff => RecordedRad - ForecastRad;
    }
}
