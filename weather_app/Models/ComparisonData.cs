namespace weather_app.Models
{
    public class ComparisonData
    {
        //public string Coordinate { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<ComparisonHourly> comparisonHourlies { get; set; }
        public string Coordinate => $"{Latitude:F2} {Longitude:F2}";
    }

    public class ComparisonHourly
    {
        public string Time { get; set; }
        public DateTime DateTime => DateTime.Parse(Time);

        // Mért értékek
        public double RecordedTemperature { get; set; }
        public double RecordedWindSpeed { get; set; }
        public double RecordedDirectRadiation { get; set; }
        public double RecordedWindDirection { get; set; }
        public double RecordedDiffuseRadiation { get; set; }
        public double RecordedDirectNormalIrradiance { get; set; }
        public double RecordedDiffuseRadiationInstant { get; set; }
        public double RecordedDirectNormalIrradianceInstant { get; set; }

        // Előrejelzett értékek
        public double ForecastTemperature { get; set; }
        public double ForecastWindSpeed { get; set; }
        public double ForecastDirectRadiation { get; set; }
        public double ForecastWindDirection { get; set; }
        public double ForecastDiffuseRadiation { get; set; }
        public double ForecastDirectNormalIrradiance { get; set; }
        public double ForecastDiffuseRadiationInstant { get; set; }
        public double ForecastDirectNormalIrradianceInstant { get; set; }

        // Különbségek kiszámítása
        public double TempDiff => RecordedTemperature - ForecastTemperature;
        public double WindDiff => RecordedWindSpeed - ForecastWindSpeed;
        public double RadDiff => RecordedDirectRadiation - ForecastDirectRadiation;
        public double WindDirectionDiff => RecordedWindDirection - ForecastWindDirection;
        public double DiffuseRadiationDiff => RecordedDiffuseRadiation - ForecastDiffuseRadiation;
        public double DirectNormalIrradianceDiff => RecordedDirectNormalIrradiance - ForecastDirectNormalIrradiance;
        public double DiffuseRadiationInstantDiff => RecordedDiffuseRadiationInstant - ForecastDiffuseRadiationInstant;
        public double DirectNormalIrradianceInstantDiff => RecordedDirectNormalIrradianceInstant - ForecastDirectNormalIrradianceInstant;
    }
}
