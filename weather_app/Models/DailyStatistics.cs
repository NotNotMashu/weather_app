using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace weather_app.Models
{
    public class DailyStatistics
    {
        public string Date { get; set; }

        // Hőmérséklet statisztikák
        public double AverageTemperature { get; set; }
        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set; }

        // Szélsebesség statisztikák
        public double AverageWindSpeed { get; set; }
        public double MinWindSpeed { get; set; }
        public double MaxWindSpeed { get; set; }

        // Sugárzás statisztikák
        public double AverageDirectRadiation { get; set; }
        public double MaxDirectRadiation { get; set; }
        public double MinDirectRadiation { get; set; }

        // Szélirány statisztikák
        public double AverageWindDirection { get; set; }
        public double MinWindDirection { get; set; }
        public double MaxWindDirection { get; set; }

        // Diffúz sugárzás statisztikák
        public double AverageDiffuseRadiation { get; set; }
        public double MinDiffuseRadiation { get; set; }
        public double MaxDiffuseRadiation { get; set; }

        // Direkt normál irradiancia statisztikák
        public double AverageDirectNormalIrradiance { get; set; }
        public double MinDirectNormalIrradiance { get; set; }
        public double MaxDirectNormalIrradiance { get; set; }

        // Azonnali diffúz sugárzás statisztikák
        public double AverageDiffuseRadiationInstant { get; set; }
        public double MinDiffuseRadiationInstant { get; set; }
        public double MaxDiffuseRadiationInstant { get; set; }

        // Azonnali direkt normál irradiancia statisztikák
        public double AverageDirectNormalIrradianceInstant { get; set; }
        public double MinDirectNormalIrradianceInstant { get; set; }
        public double MaxDirectNormalIrradianceInstant { get; set; }
    }
}
