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
using static weather_app.Views.DetailedHistoricalWeather;
using weather_app.Services;
using weather_app.ViewModels;
using System.Diagnostics;

namespace weather_app.Views
{
    public partial class HistoricalComparison : UserControl
    {
        List<ComparisonData> comparisonData = new List<ComparisonData>();
        ObservableCollection<ComparisonData> comparisons;
        public HistoricalComparison()
        {
            InitializeComponent();
            DataContext = this;

            string weatherDataFilePath = "weather_data.xml";
            string weatherForecastDataFilePath = "weather_forecast_data.xml";

            WeatherDataComparer comparer = new WeatherDataComparer(weatherDataFilePath, weatherForecastDataFilePath);
            comparisonData = comparer.CompareData();

        }



        private async void LoadComparisonData(List<ComparisonData> comparisonData)
        {
            if (int.TryParse(DayBox.Text, out int selectedDay) &&
                int.TryParse(YearBox.Text, out int selectedYear))
            {
                var groupedData = comparisonData
    .Select(data => new ComparisonData
    {
        Latitude = data.Latitude, 
        Longitude = data.Longitude,
        comparisonHourlies = data.comparisonHourlies
            .Where(hourly => hourly.DateTime.Day == selectedDay && hourly.DateTime.Year == selectedYear)
            .ToList()
    })
    .Where(data => data.comparisonHourlies.Any())
    .GroupBy(data => new { data.Latitude, data.Longitude }) // Csoportosítás Latitude és Longitude alapján
    .Select(group => new ComparisonData
    {
        Latitude = group.Key.Latitude,
        Longitude = group.Key.Longitude,
        comparisonHourlies = group.SelectMany(g => g.comparisonHourlies).ToList()
    })
    .ToList();


                if (groupedData.Any())
                {
                    comparisons = new ObservableCollection<ComparisonData>(groupedData);
                    ResultListView.ItemsSource = comparisons;
                }
                else
                {
                    MessageBox.Show("No data found for the selected criteria.");
                }
            }
            else
            {
                MessageBox.Show("Invalid input for day and year.");
            }
        }



        private void Search_Click(object sender, RoutedEventArgs e)
        {
            LoadComparisonData(comparisonData);
        }
    }
}
