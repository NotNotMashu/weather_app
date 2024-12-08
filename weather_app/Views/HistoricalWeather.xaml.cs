using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Windows.Shapes;
using weather_app.Services;

namespace weather_app.Views
{
    /// <summary>
    /// Interaction logic for HistoricalWeather.xaml
    /// </summary>
    public partial class HistoricalWeather : UserControl
    {
        private Coordinates _coordinates = new Coordinates();
        private XmlDataHandler _xmlDataHandler= new XmlDataHandler("weather_data.xml");

        public ObservableCollection<string> Records { get; set; } = new ObservableCollection<string>();

        public HistoricalWeather()
        {
            InitializeComponent();
            DataContext = this;
            LoadCoordinates();
            ResultsListView.ItemsSource = Records;
            //ShowAllRecords();
        }

        private void Refresh_List_Click(object sender, RoutedEventArgs e)
        {
            // Koordináták kiválasztása
            var startCoord = CoordStartBox.SelectedItem.ToString();
            var endCoord = CoordEndBox.SelectedItem.ToString();
            int startYear = Convert.ToInt16(((ComboBoxItem)StartYearComboBox.SelectedItem).Content);
            int endYear = Convert.ToInt16(((ComboBoxItem)EndYearComboBox.SelectedItem).Content);
            int startDay = Convert.ToInt16(((ComboBoxItem)DayStartBox.SelectedItem).Content);
            int endDay = Convert.ToInt16(((ComboBoxItem)DayEndBox.SelectedItem).Content);
            var stepFromBox = ScaleComboBox.SelectedItem.ToString();


            // Koordináták formátumának ellenőrzése és szétválasztása
            try
            {
                startCoord = startCoord.Replace('.', ',');
                endCoord = endCoord.Replace('.', ',');
                var startLatLong = startCoord.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var endLatLong = endCoord.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var stepLong = stepFromBox.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (startLatLong.Length != 2 || endLatLong.Length != 2)
                {
                    MessageBox.Show("Nem két érték van a koordinátákban.");
                    return;
                }

                String startLatitude = startLatLong[0];
                String endLatitude = endLatLong[0];
                String step = stepLong[1];

                // Rekordok szűrése
                List<WeatherData> coordinateFilteredRecords = _xmlDataHandler.GetFilteredRecords(startLatitude, endLatitude, startYear, endYear, startDay, endDay, int.Parse(step));

                // Rekordok megjelenítése a ListView-ban
                Records.Clear();
                foreach (var record in coordinateFilteredRecords)
                {
                    for (int i = 0; i < record.hourly.time.Count; i++)
                    {
                        string recordString = $"{record.latitude} {record.longitude} | " +
                                              $"Time: {record.hourly.time[i].Replace('T', ' ')} | " +
                                              $"Temperature: {record.hourly.temperature_2m[i]} | " +
                                              $"WindSpeed: {record.hourly.wind_speed_10m[i]} | " +
                                              $"Radiation: {record.hourly.direct_radiation[i]}";

                        // Hozzáadás a Records listához
                        Records.Add(recordString);
                    }
                }

                if (coordinateFilteredRecords.Count == 0)
                {
                    MessageBox.Show("Nincs megjeleníthető adat a megadott koordináták között.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt: {ex.Message}");
            }

        }


        //Koordináták feltöltése a spinnerekbe
        private void LoadCoordinates()
        {
            foreach (var coord in _coordinates.CoordinatePairs)
            {
                string coordString = $"{coord.Item1.ToString().Replace(',', '.')} {coord.Item2.ToString().Replace(',', '.')}";
                CoordStartBox.Items.Add(coordString);
                CoordEndBox.Items.Add(coordString);
            }

            if (CoordStartBox.Items.Count > 0)
            {
                CoordStartBox.SelectedIndex = 0;
                CoordEndBox.SelectedIndex = _coordinates.CoordinatePairs.Count-1;
            }
        }

        private void ShowAllRecords()
        {
            try
            {
                List<WeatherData> allRecords = _xmlDataHandler.GetAllRecords();

                Records.Clear();
                foreach (var record in allRecords)
                {
                    for (int i = 0; i < record.hourly.time.Count; i++)
                    {
                        string recordString = $"{record.latitude} {record.longitude} | " +
                                              $"Time: {record.hourly.time[i].Replace('T', ' ')} | " +
                                              $"Temperature: {record.hourly.temperature_2m[i]} | " +
                                              $"WindSpeed: {record.hourly.wind_speed_10m[i]} | " +
                                              $"Radiation: {record.hourly.direct_radiation[i]}";

                        Records.Add(recordString);
                    }
                }

                if (Records.Count > 0)
                {
                    MessageBox.Show($"Sikeresen betöltött {Records.Count} rekordot!");
                }
                else
                {
                    MessageBox.Show("Nincs megjeleníthető adat az XML-fájlban.");
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt: {ex.Message}");
            }
        }

    }
}
