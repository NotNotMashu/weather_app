using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                var filteredRecords = _xmlDataHandler.GetFilteredRecordsSeparated(selectedYear, selectedDay);
                var filteredRecordsOnlyByDay = _xmlDataHandler.GetFilteredRecordsSeparated(selectedDay);

                if (filteredRecords.Any())
                {
                    WeatherRecords = new ObservableCollection<WeatherRecord>(filteredRecords);
                    ResultListView.ItemsSource = WeatherRecords;

                    // Napi statisztikák kiszámítása, megjelenítése
                    var allTemperatures = WeatherRecords.SelectMany(wr => wr.HourlyDataList.Select(h => h.Temperature));
                    var allWindSpeeds = WeatherRecords.SelectMany(wr => wr.HourlyDataList.Select(h => h.WindSpeed));
                    var allRadiations = WeatherRecords.SelectMany(wr => wr.HourlyDataList.Select(h => h.Radiation));

                    var temperatureStats = DailyStatisticsCalculator.CalculateStatistics(allTemperatures);
                    var windSpeedStats = DailyStatisticsCalculator.CalculateStatistics(allWindSpeeds);
                    var radiationStats = DailyStatisticsCalculator.CalculateStatistics(allRadiations);

                    //Minden évre, kiválasztott napra:
                    var allTemperaturesAllYears = filteredRecordsOnlyByDay.SelectMany(wr => wr.HourlyDataList.Select(h => h.Temperature));
                    var allWindSpeedsAllYears = filteredRecordsOnlyByDay.SelectMany(wr => wr.HourlyDataList.Select(h => h.WindSpeed));
                    var allRadiationsAllYears = filteredRecordsOnlyByDay.SelectMany(wr => wr.HourlyDataList.Select(h => h.Radiation));

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
