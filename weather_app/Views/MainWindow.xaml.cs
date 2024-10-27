using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Channels;
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
using weather_app.Views;

namespace weather_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;
        //private readonly DbHandler _dbHandler;
        private readonly Coordinates _coordinates;
        private readonly LocationService _locationService;
        public MainViewModel ViewModel { get; set; }
        public ObservableCollection<CurrentWeatherData> CurrentWeatherData { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _coordinates = new Coordinates();
            _locationService = new LocationService();

            CurrentWeatherData = new ObservableCollection<CurrentWeatherData>();
            _locationService.GetLocation();


        }

        private async void Download_Data_Click(object sender, RoutedEventArgs e)
        {
            for (int year = 2014; year <= 2024; year++)
            {
                foreach (var coord in _coordinates.CoordinatePairs)
                {

                    double lat = coord.Item1;
                    double lon = coord.Item2;

                    _apiService.FetchAndStoreHistoricalWeatherData("open-meteo", lat, lon, year);
                    await Task.Delay(2000);
                }
            }
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Show_Historical_Window(object sender, RoutedEventArgs state)
        {
            MainContent.Content = new HistoricalWeather();
        }

        /*private async Task RefreshCurrentWeatherData()
         {
             CurrentWeatherData.Clear();

             foreach (string provider in new[] { "open-meteo"})
             {
                 string apiUrl = "https://archive-api.open-meteo.com/v1/archive?latitude=52.52&longitude=13.41&start_date=" + year + "-08-25&end_date=" + year + "-08-31&hourly=temperature_2m,wind_speed_10m,direct_radiation";
                 string jsonResponse = await _apiService.GetWeatherDataAsync(apiUrl);

                 if (!jsonResponse.StartsWith("Error:"))
                 {
                     //showing new data in specific field on screen
                 }
                 else
                 {
                     MessageBox.Show("Failed to retrieve weather data. Error at year " + year);
                 }
             }

             MessageBox.Show("Data reset complete.");
         }*/
    }
}