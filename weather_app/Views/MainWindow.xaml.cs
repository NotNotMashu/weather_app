using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private readonly Coordinates _coordinates;
        private readonly LocationService _locationService;

        public MainViewModel ViewModel { get; set; }
        public ObservableCollection<CurrentWeatherData> CurrentWeatherData { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Initialize services and data collections
            _apiService = new ApiService();
            _coordinates = new Coordinates();
            _locationService = new LocationService();
            CurrentWeatherData = new ObservableCollection<CurrentWeatherData>();

            // Load current location (if required)
            _locationService.GetLocation();
        }

        private async void Download_Data_Click(object sender, RoutedEventArgs e)
        {
            // Download historical weather data for each coordinate and year
            for (int year = 2014; year <= 2024; year++)
            {
                foreach (var coord in _coordinates.CoordinatePairs)
                {
                    double lat = coord.Item1;
                    double lon = coord.Item2;

                    await _apiService.FetchAndStoreHistoricalWeatherData("open-meteo", lat, lon, year);
                    await Task.Delay(2000); // Avoid overwhelming the API with requests
                }
            }

            MessageBox.Show("Historical data download complete.");
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Show_Historical_Window(object sender, RoutedEventArgs e)
        {
            // Navigate to the HistoricalWeather user control
            MainContent.Content = new HistoricalWeather();
        }

        /* Uncomment and implement if needed
        private async Task RefreshCurrentWeatherData()
        {
            CurrentWeatherData.Clear();

            foreach (var coord in _coordinates.CoordinatePairs)
            {
                foreach (string provider in new[] { "open-meteo" })
                {
                    // Build API URL dynamically
                    string apiUrl = $"https://archive-api.open-meteo.com/v1/archive" +
                                    $"?latitude={coord.Item1}&longitude={coord.Item2}" +
                                    $"&start_date=2024-08-25&end_date=2024-08-31&hourly=temperature_2m,wind_speed_10m,direct_radiation";

                    string jsonResponse = await _apiService.GetWeatherDataAsync(apiUrl);

                    if (!jsonResponse.StartsWith("Error:"))
                    {
                        // Process the JSON response and update CurrentWeatherData
                    }
                    else
                    {
                        MessageBox.Show($"Failed to retrieve weather data for coordinates: {coord}");
                    }
                }
            }

            MessageBox.Show("Weather data refresh complete.");
        }
        */
    }
}
