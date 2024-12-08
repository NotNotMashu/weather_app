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
        public ObservableCollection<WeatherData> WeatherDataList { get; set; } = new ObservableCollection<WeatherData>();
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
                    WeatherDataList = new ObservableCollection<WeatherData>(filteredRecords);
                    ResultListView.ItemsSource = WeatherDataList;

                        //csoportosítás koordináták szerint, majd adatok óránkénti kiírása
                    //TODO



                    // Napi statisztikák kiszámítása és megjelenítése
                    var allTemperatures = WeatherDataList.SelectMany(wd => wd.hourly.temperature_2m);
                    var allWindSpeeds = WeatherDataList.SelectMany(wd => wd.hourly.wind_speed_10m);
                    var allRadiations = WeatherDataList.SelectMany(wd => wd.hourly.direct_radiation);

                    var temperatureStats = DailyStatisticsCalculator.CalculateStatistics(allTemperatures);
                    var windSpeedStats = DailyStatisticsCalculator.CalculateStatistics(allWindSpeeds);
                    var radiationStats = DailyStatisticsCalculator.CalculateStatistics(allRadiations);

                    // Minden évre, kiválasztott napra:
                    var allTemperaturesAllYears = filteredRecordsOnlyByDay.SelectMany(wd => wd.hourly.temperature_2m);
                    var allWindSpeedsAllYears = filteredRecordsOnlyByDay.SelectMany(wd => wd.hourly.wind_speed_10m);
                    var allRadiationsAllYears = filteredRecordsOnlyByDay.SelectMany(wd => wd.hourly.direct_radiation);

                    var overallTemperatureStats = DailyStatisticsCalculator.CalculateStatistics(allTemperaturesAllYears);
                    var overallWindSpeedStats = DailyStatisticsCalculator.CalculateStatistics(allWindSpeedsAllYears);
                    var overallRadiationStats = DailyStatisticsCalculator.CalculateStatistics(allRadiationsAllYears);

                    var dailyStats = new ObservableCollection<string>
                    {
                        "Adott naphoz tartozó számítások:",
                        "Hőmérséklet (°C):",
                        $"Minimum: {temperatureStats.Min:F2}   Maximum: {temperatureStats.Max:F2}   Átlag: {temperatureStats.Avg:F2}",
                        "Szélsebesség (km/h):",
                        $"Minimum: {windSpeedStats.Min:F2}   Maximum: {windSpeedStats.Max:F2}   Átlag: {windSpeedStats.Avg:F2}",
                        "Sugárzás (W/m²):",
                        $"Minimum: {radiationStats.Min:F2}   Maximum: {radiationStats.Max:F2}   Átlag: {radiationStats.Avg:F2}",
                        "",
                        "Összes év adott naphoz tartozó számítások:",
                        "Hőmérséklet (°C):",
                        $"Minimum: {overallTemperatureStats.Min:F2}   Maximum: {overallTemperatureStats.Max:F2}   Átlag: {overallTemperatureStats.Avg:F2}",
                        "Szélsebesség (km/h):",
                        $"Minimum: {overallWindSpeedStats.Min:F2}   Maximum: {overallWindSpeedStats.Max:F2}   Átlag: {overallWindSpeedStats.Avg:F2}",
                        "Sugárzás (W/m²):",
                        $"Minimum: {overallRadiationStats.Min:F2}   Maximum: {overallRadiationStats.Max:F2}   Átlag: {overallRadiationStats.Avg:F2}"
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
