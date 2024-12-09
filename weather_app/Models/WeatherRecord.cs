namespace weather_app.ViewModels
{
    public class WeatherRecord
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<WeatherRecordHourly> HourlyDataList { get; set; }
    }

    public class WeatherRecordHourly
    {
        public string Time { get; set; }
        public double Temperature { get; set; }
        public double WindSpeed { get; set; }
        public double WindDirection { get; set; }
        public double DirectRadiation { get; set; }
        public double DiffuseRadiation { get; set; }
        public double DirectNormalIrradiance { get; set; }
        public double DiffuseRadiationInstant { get; set; }
        public double DirectNormalIrradianceInstant { get; set; }

        // Optional: Statistics properties
        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set; }
        public double MinWindSpeed { get; set; }
        public double MaxWindSpeed { get; set; }
        public double MinRadiation { get; set; }
        public double MaxRadiation { get; set; }
        public double MinWindDirection { get; set; }
        public double MaxWindDirection { get; set; }
        public double MinDiffuseRadiation { get; set; }
        public double MaxDiffuseRadiation { get; set; }
        public double MinDirectNormalIrradiance { get; set; }
        public double MaxDirectNormalIrradiance { get; set; }
        public double MinDiffuseRadiationInstant { get; set; }
        public double MaxDiffuseRadiationInstant { get; set; }
        public double MinDirectNormalIrradianceInstant { get; set; }
        public double MaxDirectNormalIrradianceInstant { get; set; }

    }
}
