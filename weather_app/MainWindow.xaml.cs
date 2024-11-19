using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using weather_app.Services;
using weather_app.ViewModels;

namespace weather_app
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly DbHandler _dbHandler;
        public MainViewModel ViewModel { get; set; }
        public ObservableCollection<CurrentWeatherData> CurrentWeatherData { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            _apiService = new ApiService();
            _dbHandler = new DbHandler("127.0.0.1", "weather_db", "postgres", "admin", "5433");
            _dbHandler.MakeTable("weather_data");
            _dbHandler.MakeTable("forecast_data");

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(5)
            };
            timer.Tick += async (sender, e) => await RefreshCurrentWeatherData();
            timer.Start();

            Loaded += async (s, e) => await RefreshCurrentWeatherData();

            /*bool isEmpty = _dbHandler.IsTableEmpty("weather_data");
            MessageBox.Show(isEmpty ? "The table is empty." : "The table has data.");*/
        }

        private async void Refresh_Data_Click(object sender, RoutedEventArgs e)
        {
            _apiService.CallApi(_dbHandler, "weather_data");   
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async Task RefreshCurrentWeatherData()
        {
            CurrentWeatherData.Clear();

            foreach (string provider in new[] { "open-meteo", "Provider2" })
            {
                var weather = await _apiService.GetWeatherDataFromProviderAsync(provider);
                CurrentWeatherData.Add(new CurrentWeatherData
                {
                    Provider = provider,
                    Temperature = weather.Temperature,
                    WindSpeed = weather.WindSpeed,
                    Radiation = weather.Radiation
                });
            }
        }
    }
}