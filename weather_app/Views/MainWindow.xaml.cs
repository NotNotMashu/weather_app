﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using weather_app.Models;
using weather_app.Services;
using weather_app.ViewModels;
using weather_app.Views;
using Windows.Services.Maps;

namespace weather_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ApiService _apiService;
        private readonly Coordinates _coordinates;
        private readonly XmlDataHandler _xmlDataHandler;
        private readonly XmlForecastDataHandler _xmlForecastDataHandler;
        private readonly LocationService _locationService;
        private double _latitude;
        private double _longitude;
        private DispatcherTimer _weatherUpdateTimer;
        public ObservableCollection<CurrentWeatherResponse> WeatherDataList { get; set; } = new ObservableCollection<CurrentWeatherResponse>();
        private string _iconUrl;
        public string IconUrl
        {
            get => _iconUrl;
            set
            {
                if (_iconUrl != value)
                {
                    _iconUrl = value;
                    OnPropertyChanged(nameof(IconUrl));
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _apiService = new ApiService();
            _coordinates = new Coordinates();
            _locationService = new LocationService();

            Loaded += MainWindow_Loaded;

            // Időzítő beállítása
            _weatherUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(20)
            };
            _weatherUpdateTimer.Tick += WeatherUpdateTimer_Tick;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task FetchWeatherDataAsync()
        {
            try
            {
                var (latitude, longitude) = await _locationService.GetCurrentLocationAsync();
                _latitude = latitude;
                _longitude = longitude;

                var weatherResponses = await _apiService.CallAllCurrentProviders(_latitude.ToString().Replace(',','.'), _longitude.ToString().Replace(',','.'));

                WeatherDataList.Clear();

                // Adatok hozzáadása a listához
                foreach (var response in weatherResponses)
                {
                    WeatherDataList.Add(new CurrentWeatherResponse
                    {
                        Provider = response.Provider,
                        Temperature = response.Temperature,
                        WindSpeed = response.WindSpeed
                    });
                }

                // Átlag
                // Átlag hozzáadása, figyelmen kívül hagyva az "Ismeretlen" Provider-t
                if (weatherResponses.Any())
                {
                    // Szűrés: kizárjuk az "Ismeretlen" Provider-t
                    var validResponses = weatherResponses.Where(r => r.Provider != "Ismeretlen").ToList();

                    // Ha marad érvényes adat
                    if (validResponses.Any())
                    {
                        double avgTemp = validResponses.Average(r => r.Temperature);
                        double avgWind = validResponses.Average(r => r.WindSpeed);

                        WeatherDataList.Add(new CurrentWeatherResponse
                        {
                            Provider = "Átlag",
                            Temperature = avgTemp,
                            WindSpeed = avgWind
                        });

                        IconUrl = _apiService.WeatherIconUrl;
                    }
                    else
                    {
                        MessageBox.Show("Nincs érvényes adat a jelenlegi időjárás átlagának számításához.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }


        private async void Download_Data_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Kérem várjon, ez a folyamat egy kis időt vesz igénybe.");
            _apiService.CreateFileIfNeeded();
            for (int year = 2014; year <= 2024; year++)
            {
                foreach (var coord in _coordinates.CoordinatePairs)
                {
                    double lat = coord.Item1;
                    double lon = coord.Item2;

                    await _apiService.FetchAndStoreHistoricalWeatherData("open-meteo", lat, lon, year);
                }
            }
            MessageBox.Show("Letöltés vége.");
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Show_Historical_Window(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new HistoricalWeather();
        }

        private void Show_Detailed_Historical_Window(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DetailedHistoricalWeather();
        }

        private void Show_Forecast_Window(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ForecastWeather();
        }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Első lekérdezés
            await FetchWeatherDataAsync();

            // Időzítő indítása
            _weatherUpdateTimer.Start();
        }
        private async void WeatherUpdateTimer_Tick(object sender, EventArgs e)
        {
            await FetchWeatherDataAsync();
        }

        private void Show_Comparison_Window(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new HistoricalComparison();
        }
    }
}
