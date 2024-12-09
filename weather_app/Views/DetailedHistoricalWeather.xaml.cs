using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using weather_app.Models;
using weather_app.Services;
using weather_app.ViewModels;

namespace weather_app.Views
{
    public partial class DetailedHistoricalWeather : UserControl
    {
        public ObservableCollection<WeatherRecord> WeatherRecords { get; set; } = new ObservableCollection<WeatherRecord>();
        XmlDataHandler _xmlDataHandler = new XmlDataHandler("weather_data.xml");
        public DetailedHistoricalWeather()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(DayBox.Text, out int selectedDay) &&
                int.TryParse(YearBox.Text, out int selectedYear))
            {
                // Szűrés a kívánt év és nap szerint
                var filteredRecords = _xmlDataHandler.GetFilteredRecordsSeparated(selectedYear, selectedDay);
                var filteredRecordsOnlyByDay = _xmlDataHandler.GetFilteredRecordsSeparated(selectedDay);

                if (filteredRecords.Any())
                {
                    // Binding miatt
                    // Átalakítás WeatherData listából WeatherRecord listába
                    var weatherRecords = filteredRecords.Select(wd => new WeatherRecord
                    {
                        Latitude = wd.latitude,
                        Longitude = wd.longitude,
                        HourlyDataList = wd.hourly.time.Select((t, index) => new WeatherRecordHourly
                        {
                            Time = DateTime.Parse(t).ToString("HH:mm"),
                            Temperature = wd.hourly.temperature_2m[index],
                            WindSpeed = wd.hourly.wind_speed_10m[index],
                            WindDirection = wd.hourly.wind_direction_10m[index],
                            DirectRadiation = wd.hourly.direct_radiation[index],
                            DiffuseRadiation = wd.hourly.diffuse_radiation[index],
                            DirectNormalIrradiance = wd.hourly.direct_normal_irradiance[index],
                            DiffuseRadiationInstant = wd.hourly.diffuse_radiation_instant[index],
                            DirectNormalIrradianceInstant = wd.hourly.direct_normal_irradiance_instant[index],
                        }).ToList()
                    }).ToList();


                    var groupedByCoordinates = weatherRecords
                        .GroupBy(wr => new { wr.Latitude, wr.Longitude })
                        .Select(group => new WeatherRecord
                        {
                            Latitude = group.Key.Latitude,
                            Longitude = group.Key.Longitude,
                            HourlyDataList = group.SelectMany(wr => wr.HourlyDataList).ToList()
                        })
                        .ToList();

                    // Frissítsd a WeatherRecords-t az átalakított listával
                    WeatherRecords = new ObservableCollection<WeatherRecord>(groupedByCoordinates);
                    ResultListView.ItemsSource = WeatherRecords;


                    // Napi statisztikák kiszámítása, megjelenítése
                    var allTemperatures = WeatherRecords.SelectMany(wr => wr.HourlyDataList.Select(h => h.Temperature));
                    var allWindSpeeds = WeatherRecords.SelectMany(wr => wr.HourlyDataList.Select(h => h.WindSpeed));
                    var allRadiations = WeatherRecords.SelectMany(wr => wr.HourlyDataList.Select(h => h.DirectRadiation));
                    var allWindDirections = WeatherRecords.SelectMany(wr => wr.HourlyDataList.Select(h => h.WindDirection));
                    var allDiffuseRadiations = WeatherRecords.SelectMany(wr => wr.HourlyDataList.Select(h => h.DiffuseRadiation));
                    var allDirectNormalIrradiances = WeatherRecords.SelectMany(wr => wr.HourlyDataList.Select(h => h.DirectNormalIrradiance));
                    var allDiffuseRadiationInstants = WeatherRecords.SelectMany(wr => wr.HourlyDataList.Select(h => h.DiffuseRadiationInstant));
                    var allDirectNormalIrradianceInstants = WeatherRecords.SelectMany(wr => wr.HourlyDataList.Select(h => h.DirectNormalIrradianceInstant));

                    var temperatureStats = DailyStatisticsCalculator.CalculateStatistics(allTemperatures);
                    var windSpeedStats = DailyStatisticsCalculator.CalculateStatistics(allWindSpeeds);
                    var radiationStats = DailyStatisticsCalculator.CalculateStatistics(allRadiations);
                    var windDirectionStats = DailyStatisticsCalculator.CalculateStatistics(allWindDirections);
                    var diffuseRadiationStats = DailyStatisticsCalculator.CalculateStatistics(allDiffuseRadiations);
                    var directNormalIrradianceStats = DailyStatisticsCalculator.CalculateStatistics(allDirectNormalIrradiances);
                    var diffuseRadiationInstantStats = DailyStatisticsCalculator.CalculateStatistics(allDiffuseRadiationInstants);
                    var directNormalIrradianceInstantStats = DailyStatisticsCalculator.CalculateStatistics(allDirectNormalIrradianceInstants);

                    // Minden évre, kiválasztott napra:
                    var allTemperaturesAllYears = filteredRecordsOnlyByDay.SelectMany(wd => wd.hourly.temperature_2m);
                    var allWindSpeedsAllYears = filteredRecordsOnlyByDay.SelectMany(wd => wd.hourly.wind_speed_10m);
                    var allRadiationsAllYears = filteredRecordsOnlyByDay.SelectMany(wd => wd.hourly.direct_radiation);
                    var allWindDirectionsAllYears = filteredRecordsOnlyByDay.SelectMany(wd => wd.hourly.wind_direction_10m);
                    var allDiffuseRadiationsAllYears = filteredRecordsOnlyByDay.SelectMany(wd => wd.hourly.diffuse_radiation);
                    var allDirectNormalIrradiancesAllYears = filteredRecordsOnlyByDay.SelectMany(wd => wd.hourly.direct_normal_irradiance);
                    var allDiffuseRadiationInstantsAllYears = filteredRecordsOnlyByDay.SelectMany(wd => wd.hourly.diffuse_radiation_instant);
                    var allDirectNormalIrradianceInstantsAllYears = filteredRecordsOnlyByDay.SelectMany(wd => wd.hourly.direct_normal_irradiance_instant);

                    var overallTemperatureStats = DailyStatisticsCalculator.CalculateStatistics(allTemperaturesAllYears);
                    var overallWindSpeedStats = DailyStatisticsCalculator.CalculateStatistics(allWindSpeedsAllYears);
                    var overallRadiationStats = DailyStatisticsCalculator.CalculateStatistics(allRadiationsAllYears);
                    var overallWindDirectionStats = DailyStatisticsCalculator.CalculateStatistics(allWindDirectionsAllYears);
                    var overallDiffuseRadiationStats = DailyStatisticsCalculator.CalculateStatistics(allDiffuseRadiationsAllYears);
                    var overallDirectNormalIrradianceStats = DailyStatisticsCalculator.CalculateStatistics(allDirectNormalIrradiancesAllYears);
                    var overallDiffuseRadiationInstantStats = DailyStatisticsCalculator.CalculateStatistics(allDiffuseRadiationInstantsAllYears);
                    var overallDirectNormalIrradianceInstantStats = DailyStatisticsCalculator.CalculateStatistics(allDirectNormalIrradianceInstantsAllYears);

                    var dailyStats = new ObservableCollection<string>
{
    "Adott naphoz tartozó számítások:",
    "Hőmérséklet (°C):",
    $"Minimum: {temperatureStats.Min:F2}   Maximum: {temperatureStats.Max:F2}   Átlag: {temperatureStats.Avg:F2}",
    "Szélsebesség (km/h):",
    $"Minimum: {windSpeedStats.Min:F2}   Maximum: {windSpeedStats.Max:F2}   Átlag: {windSpeedStats.Avg:F2}",
    "Szélirány (°):",
    $"Minimum: {windDirectionStats.Min:F2}   Maximum: {windDirectionStats.Max:F2}   Átlag: {windDirectionStats.Avg:F2}",
    "Sugárzás (W/m²):",
    $"Minimum: {radiationStats.Min:F2}   Maximum: {radiationStats.Max:F2}   Átlag: {radiationStats.Avg:F2}",
    "Diffúz sugárzás (W/m²):",
    $"Minimum: {diffuseRadiationStats.Min:F2}   Maximum: {diffuseRadiationStats.Max:F2}   Átlag: {diffuseRadiationStats.Avg:F2}",
    "Direkt normál besugárzás (W/m²):",
    $"Minimum: {directNormalIrradianceStats.Min:F2}   Maximum: {directNormalIrradianceStats.Max:F2}   Átlag: {directNormalIrradianceStats.Avg:F2}",
    "Diffúz sugárzás pillanatnyi (W/m²):",
    $"Minimum: {diffuseRadiationInstantStats.Min:F2}   Maximum: {diffuseRadiationInstantStats.Max:F2}   Átlag: {diffuseRadiationInstantStats.Avg:F2}",
    "Direkt normál pillanatnyi besugárzás (W/m²):",
    $"Minimum: {directNormalIrradianceInstantStats.Min:F2}   Maximum: {directNormalIrradianceInstantStats.Max:F2}   Átlag: {directNormalIrradianceInstantStats.Avg:F2}",
    "",
    "Összes év adott naphoz tartozó számítások:",
    "Hőmérséklet (°C):",
    $"Minimum: {overallTemperatureStats.Min:F2}   Maximum: {overallTemperatureStats.Max:F2}   Átlag: {overallTemperatureStats.Avg:F2}",
    "Szélsebesség (km/h):",
    $"Minimum: {overallWindSpeedStats.Min:F2}   Maximum: {overallWindSpeedStats.Max:F2}   Átlag: {overallWindSpeedStats.Avg:F2}",
    "Szélirány (°):",
    $"Minimum: {overallWindDirectionStats.Min:F2}   Maximum: {overallWindDirectionStats.Max:F2}   Átlag: {overallWindDirectionStats.Avg:F2}",
    "Sugárzás (W/m²):",
    $"Minimum: {overallRadiationStats.Min:F2}   Maximum: {overallRadiationStats.Max:F2}   Átlag: {overallRadiationStats.Avg:F2}",
    "Diffúz sugárzás (W/m²):",
    $"Minimum: {overallDiffuseRadiationStats.Min:F2}   Maximum: {overallDiffuseRadiationStats.Max:F2}   Átlag: {overallDiffuseRadiationStats.Avg:F2}",
    "Direkt normál besugárzás (W/m²):",
    $"Minimum: {overallDirectNormalIrradianceStats.Min:F2}   Maximum: {overallDirectNormalIrradianceStats.Max:F2}   Átlag: {overallDirectNormalIrradianceStats.Avg:F2}",
    "Diffúz sugárzás pillanatnyi (W/m²):",
    $"Minimum: {overallDiffuseRadiationInstantStats.Min:F2}   Maximum: {overallDiffuseRadiationInstantStats.Max:F2}   Átlag: {overallDiffuseRadiationInstantStats.Avg:F2}",
    "Direkt normál pillanatnyi besugárzás (W/m²):",
    $"Minimum: {overallDirectNormalIrradianceInstantStats.Min:F2}   Maximum: {overallDirectNormalIrradianceInstantStats.Max:F2}   Átlag: {overallDirectNormalIrradianceInstantStats.Avg:F2}"
};


                    AvgDetailsListView.ItemsSource = dailyStats;
                }
                else
                {
                    MessageBox.Show("Nincs adat a szűrési kritériumoknak megfelelően.");
                }
            }
            else
            {
                MessageBox.Show("Hiba a nap és az év kezelésében.");
            }
        }
        public static class DailyStatisticsCalculator
        {
            public static (double Min, double Max, double Avg) CalculateStatistics(IEnumerable<double> values)
            {
                if (!values.Any()) return (0, 0, 0);

                double min = values.Min();
                double max = values.Max();
                double avg = values.Average();

                return (min, max, avg);
            }
        }
    }
}
