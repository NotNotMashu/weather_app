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

                if (filteredRecords.Any())
                {
                    WeatherRecords = new ObservableCollection<WeatherRecord>(filteredRecords);
                    ResultListView.ItemsSource = WeatherRecords;
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


    }
}
