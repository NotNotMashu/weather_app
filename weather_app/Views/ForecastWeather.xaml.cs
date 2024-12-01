using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using weather_app.Services;
using weather_app.ViewModels;

namespace weather_app.Views
{
    public partial class ForecastWeather : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        ApiService _apiService = new ApiService();
        Coordinates _coordinates = new Coordinates();

        public ObservableCollection<WeatherRecord> WeeklyWeather { get; set; } = new ObservableCollection<WeatherRecord>();

        public ObservableCollection<WeatherRecord> DailyWeather { get; set; } = new ObservableCollection<WeatherRecord>();
        public ObservableCollection<string> UniqueDates { get; set; } = new ObservableCollection<string>();

        public ForecastWeather()
        {
            InitializeComponent();
            DataContext = this;
            UpdateSelectedCoordinate();
            InitializeForecastWeather();
        }
        public class WeatherViewModel
        {
            public List<WeatherRecord> FilteredRecords { get; set; }
        }


        private async void InitializeForecastWeather()
        {
            List<WeatherRecord> weatherRecords = await LoadForecastData();
            // TODO rest
        }

        private async Task<List<WeatherRecord>> LoadForecastData()
        {
            var weatherRecords = new List<WeatherRecord>();
            var uniqueDates = new HashSet<string>();

            // Párhuzamos API-hívások indítása
            var tasks = _coordinates.CoordinatePairs.Select(async coordpair =>
            {
                WeatherData forecastData = await _apiService.ReturnForecastData(coordpair);

                if (forecastData != null && forecastData.hourly != null)
                {
                    var times = forecastData.hourly.time;
                    var temperatures = forecastData.hourly.temperature_2m;
                    var windSpeeds = forecastData.hourly.wind_speed_10m;
                    var radiations = forecastData.hourly.direct_radiation;
                    string coordinate = $"{coordpair.Item1} {coordpair.Item2}";

                    var weatherRecord = new WeatherRecord
                    {
                        Coordinate = coordinate,
                        HourlyDataList = new List<WeatherRecordHourly>()
                    };

                    for (int i = 0; i < times.Count; i++)
                    {
                        DateTime timestamp = DateTime.Parse(times[i]);
                        string dateOnly = timestamp.ToString("yyyy-MM-dd");
                        lock (uniqueDates)
                        {
                            uniqueDates.Add(dateOnly);
                        }

                        int hour = timestamp.Hour;
                        if (hour == 6 || hour == 12 || hour == 18 || hour == 3 || hour == 9 || hour == 15)
                        {
                            var hourlyData = new WeatherRecordHourly
                            {
                                Time = times[i],
                                Temperature = temperatures[i],
                                WindSpeed = windSpeeds[i],
                                Radiation = radiations[i]
                            };
                            weatherRecord.HourlyDataList.Add(hourlyData);
                        }
                    }

                    lock (weatherRecords)
                    {
                        weatherRecords.Add(weatherRecord);
                    }
                }
            }).ToArray();

            await Task.WhenAll(tasks);

            var uniqueDatesList = uniqueDates.ToList();
            foreach (string date in uniqueDatesList)
            {
                UniqueDates.Add(date);
            }

            UniqueDates.Insert(0, "Minden nap");
            CalculateDailyStats(weatherRecords);
            return weatherRecords;
        }
        /*private void LoadCoordinates()
        {
            foreach (var coord in _coordinates.CoordinatePairs)
            {
                string coordString = $"{coord.Item1.ToString().Replace(',', '.')} {coord.Item2.ToString().Replace(',', '.')}";
            }
        }*/

        private void PreviousCoordinate()
        {
            if (_coordinates.CoordinatePairs.Count == 0) return;

            if (_currentCoordinateIndex > 0)
            {
                _currentCoordinateIndex--;
            }
            else
            {
                _currentCoordinateIndex = _coordinates.CoordinatePairs.Count - 1; // Ugrás az utolsóra
            }

            UpdateSelectedCoordinate();
        }

        private void NextCoordinate()
        {
            if (_coordinates.CoordinatePairs.Count == 0) return;

            if (_currentCoordinateIndex < _coordinates.CoordinatePairs.Count - 1)
            {
                _currentCoordinateIndex++;
            }
            else
            {
                _currentCoordinateIndex = 0; // Ugrás az elsőre
            }

            UpdateSelectedCoordinate();
        }

        private int _currentCoordinateIndex = 0;

        public string SelectedCoordinate
        {
            get => _coordinates.CoordinatePairs.Count > 0
                ? $"{_coordinates.CoordinatePairs[_currentCoordinateIndex].Item1} {_coordinates.CoordinatePairs[_currentCoordinateIndex].Item2}"
                : string.Empty;
        }

        public void UpdateSelectedCoordinate()
        {
            OnPropertyChanged(nameof(SelectedCoordinate));
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            PreviousCoordinate();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NextCoordinate();
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CalculateDailyStats(List<WeatherRecord> weatherRecords)
        {
            foreach (var record in weatherRecords)
            {
                // Napokra bontás
                var dailyData = GroupByDay(record);

                foreach (var day in dailyData)
                {
                    string date = day.Key;
                    var hourlyDataList = day.Value;

                    // Adatok gyűjtése az adott napra
                    var dailyTemperatures = hourlyDataList.Select(hourly => hourly.Temperature).ToList();
                    var dailyWindSpeeds = hourlyDataList.Select(hourly => hourly.WindSpeed).ToList();
                    var dailyRadiations = hourlyDataList.Select(hourly => hourly.Radiation).ToList();

                    // Napi minimum és maximum értékek
                    double minTemperature = dailyTemperatures.Min();
                    double maxTemperature = dailyTemperatures.Max();
                    double minWindSpeed = dailyWindSpeeds.Min();
                    double maxWindSpeed = dailyWindSpeeds.Max();
                    double maxRadiation = dailyRadiations.Max();

                    // Debug kiírás
                    Debug.WriteLine($"Coordinate: {record.Coordinate}");
                    Debug.WriteLine($"Date: {date}");
                    Debug.WriteLine($"Min Temperature: {minTemperature}°C");
                    Debug.WriteLine($"Max Temperature: {maxTemperature}°C");
                    Debug.WriteLine($"Min Wind Speed: {minWindSpeed} m/s");
                    Debug.WriteLine($"Max Wind Speed: {maxWindSpeed} m/s");
                    Debug.WriteLine($"Max Radiation: {maxRadiation} W/m²");
                    Debug.WriteLine("------------------------------------------------------");
                }
            }
        }

        private Dictionary<string, List<WeatherRecordHourly>> GroupByDay(WeatherRecord record)
        {
            return record.HourlyDataList
                .GroupBy(hourly => DateTime.Parse(hourly.Time).ToString("yyyy-MM-dd"))
                .ToDictionary(g => g.Key, g => g.ToList());
        }


    }
}
