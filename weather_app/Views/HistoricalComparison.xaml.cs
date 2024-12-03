using System;
using System.Collections.Generic;
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

namespace weather_app.Views
{
    public partial class HistoricalComparison : UserControl
    {
        public HistoricalComparison()
        {
            InitializeComponent();

            string weatherDataFilePath = "weather_data.xml";
            string weatherForecastDataFilePath = "weather_forecast_data.xml";

            WeatherDataComparer comparer = new WeatherDataComparer(weatherDataFilePath, weatherForecastDataFilePath);
            List<ComparisonData> comparisonData = comparer.CompareData();

        }

        private async void LoadComparisonData(WeatherDataComparer comparer)
        {
            List<ComparisonData> comparisonData = await Task.Run(() => comparer.CompareData());

            // Itt dolgozhatsz a comparisonData listával, például beállíthatod a UI-t
            // Példa: egy ListBox-ba való megjelenítés
            MyListBox.ItemsSource = comparisonData; // MyListBox az adott ListBox neve a UI-ban
        }
    }
}
