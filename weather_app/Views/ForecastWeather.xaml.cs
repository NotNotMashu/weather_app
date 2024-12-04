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
using weather_app.Models;
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
        public ObservableCollection<DailyStatistics> WeeklyWeather { get; set; } = new ObservableCollection<DailyStatistics>();
        private List<WeatherRecord> _loadedWeatherRecords = new List<WeatherRecord>();
        public ObservableCollection<WeatherRecordHourly> DailyWeather { get; set; } = new ObservableCollection<WeatherRecordHourly>();
        public ObservableCollection<string> UniqueDates { get; set; } = new ObservableCollection<string>();

        private int _currentCoordinateIndex = 0;
        private bool _isrunningFirstTime = true;


        public ForecastWeather()
        {
            InitializeComponent();
            DataContext = this;
            UpdateSelectedCoordinate();
            InitializeForecastWeather();

            if (UniqueDates.Count > 0)
            {
                DateBox.SelectedIndex = 0;
            }

            if (_isrunningFirstTime == false)
            {
                Task.Delay(1000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => ShowSelectedDayData());
                });
            }
        }
        public class WeatherViewModel
        {
            public List<WeatherRecord> FilteredRecords { get; set; }
        }

        private async void InitializeForecastWeather()
        {
            List<WeatherRecord> weatherRecords = await LoadForecastData();
            _loadedWeatherRecords = weatherRecords;

            CalculateDailyStats(_loadedWeatherRecords);
        }

        private async Task<List<WeatherRecord>> LoadForecastData()
        {
            var weatherRecords = new List<WeatherRecord>();
            var uniqueDates = new HashSet<string>();

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
                        if (hour == 6 || hour == 12 || hour == 18 || hour == 8 || hour == 14 || hour == 10 || hour == 16)
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

            CalculateDailyStats(weatherRecords);
            return weatherRecords;
        }


        private void PreviousCoordinate()
        {
            if (_coordinates.CoordinatePairs.Count == 0) return;

            if (_currentCoordinateIndex > 0)
            {
                _currentCoordinateIndex--;
            }
            else
            {
                _currentCoordinateIndex = _coordinates.CoordinatePairs.Count - 1;
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
                _currentCoordinateIndex = 0;
            }

            UpdateSelectedCoordinate();
        }


        public string SelectedCoordinate
        {
            get
            {
                if (_currentCoordinateIndex == -1)
                {
                    return "Összes koordináta";
                }

                return _coordinates.CoordinatePairs.Count > 0
                    ? $"{_coordinates.CoordinatePairs[_currentCoordinateIndex].Item1} {_coordinates.CoordinatePairs[_currentCoordinateIndex].Item2}"
                    : string.Empty;
            }
        }


        public void UpdateSelectedCoordinate()
        {
            OnPropertyChanged(nameof(SelectedCoordinate));
            RecalculateStatsForSelectedCoordinate();
            ShowSelectedDayData();
        }


        private void RecalculateStatsForSelectedCoordinate()
        {
            if (_loadedWeatherRecords != null)
            {
                CalculateDailyStats(_loadedWeatherRecords);
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            PreviousCoordinate();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NextCoordinate();
        }

        private void Day_Filter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                string tag = clickedButton.Tag as string;

                _isrunningFirstTime = false;
                ShowSelectedDayData();
            }
        }

        private void ShowSelectedDayData()
        {
            

            if(_isrunningFirstTime == true)
            {
                _isrunningFirstTime = false;
                return;
            }

            string selectedDate = DateBox.SelectedItem.ToString();

            DailyWeather.Clear();

            IEnumerable<WeatherRecordHourly> selectedData;

            if (_currentCoordinateIndex == -1) // Összes koordináta kiválasztva
            {
                selectedData = _loadedWeatherRecords
                    .SelectMany(record => record.HourlyDataList)
                    .Where(hourly => DateTime.Parse(hourly.Time).ToString("yyyy-MM-dd") == selectedDate);

                // Részletes táblázat megjelenítése, ha az összes koordinátra van szükség
                var groupedByCoordinate = selectedData
                    .GroupBy(hourly => DateTime.Parse(hourly.Time).Hour)
                    .Select(group => new WeatherRecordHourly
                    {
                        Time = $"{group.Key}:00",
                        Temperature = Math.Round(group.Average(hourly => hourly.Temperature), 2),
                        WindSpeed = Math.Round(group.Average(hourly => hourly.WindSpeed), 2),
                        Radiation = Math.Round(group.Average(hourly => hourly.Radiation), 2),
                        MinTemperature = group.Min(hourly => hourly.Temperature),
                        MaxTemperature = group.Max(hourly => hourly.Temperature),
                        MaxWindSpeed = group.Max(hourly => hourly.WindSpeed),
                        MinRadiation = group.Min(hourly => hourly.Radiation),
                        MaxRadiation = group.Max(hourly => hourly.Radiation)
                    }).ToList();

                if (!groupedByCoordinate.Any())
                {
                    MessageBox.Show("Nincs adat az adott napra!");
                    return;
                }

                foreach (var hourlyData in groupedByCoordinate)
                {
                    DailyWeather.Add(hourlyData);
                }

                DefaultDataGrid.Visibility = Visibility.Collapsed; // Az egyszerű táblázatot elrejtjük
                DetailedDataGrid.Visibility = Visibility.Visible; // A részletes táblázatot mutatjuk
                DetailedDataGrid.ItemsSource = groupedByCoordinate; // Beállítjuk a részletes adatokat
            }
            else
            {
                var selectedRecord = _loadedWeatherRecords
                    .FirstOrDefault(record => record.Coordinate == SelectedCoordinate);

                if (selectedRecord == null)
                {
                    MessageBox.Show("Nincs adat az adott koordinátához!");
                    return;
                }

                selectedData = selectedRecord.HourlyDataList
                    .Where(hourly => DateTime.Parse(hourly.Time).ToString("yyyy-MM-dd") == selectedDate);

                // Egyszerű táblázat megjelenítése
                var groupedData = selectedData
                    .GroupBy(hourly => DateTime.Parse(hourly.Time).Hour)
                    .Select(group => new WeatherRecordHourly
                    {
                        Time = $"{group.Key}:00",
                        Temperature = Math.Round(group.Average(hourly => hourly.Temperature), 2),
                        WindSpeed = Math.Round(group.Average(hourly => hourly.WindSpeed), 2),
                        Radiation = Math.Round(group.Average(hourly => hourly.Radiation), 2),
                        MinTemperature = group.Min(hourly => hourly.Temperature),
                        MaxTemperature = group.Max(hourly => hourly.Temperature),
                        MaxWindSpeed = group.Max(hourly => hourly.WindSpeed),
                        MinRadiation = group.Min(hourly => hourly.Radiation),
                        MaxRadiation = group.Max(hourly => hourly.Radiation)
                    }).ToList();

                if (!groupedData.Any())
                {
                    MessageBox.Show("Nincs adat az adott napra!");
                    return;
                }

                foreach (var hourlyData in groupedData)
                {
                    DailyWeather.Add(hourlyData);
                }

                DefaultDataGrid.Visibility = Visibility.Visible; // Az egyszerű táblázatot mutatjuk
                DetailedDataGrid.Visibility = Visibility.Collapsed; // A részletes táblázatot elrejtjük
                DefaultDataGrid.ItemsSource = groupedData; // Beállítjuk az egyszerű táblázat adatforrását
            }
        }



        private void CalculateDailyStats(List<WeatherRecord> weatherRecords)
        {
            var dailyStatsList = new List<DailyStatistics>();

            foreach (var record in weatherRecords)
            {
                if (_currentCoordinateIndex != -1 && record.Coordinate != SelectedCoordinate) continue; // nem az összes vagy a jelenleg kiválasztott esetén

                // Napokra bontás
                var dailyData = GroupByDay(record);

                foreach (var day in dailyData)
                {
                    string date = day.Key;
                    var hourlyDataList = day.Value;

                    // Napi adatok
                    var dailyTemperatures = hourlyDataList.Select(hourly => hourly.Temperature).ToList();
                    var dailyWindSpeeds = hourlyDataList.Select(hourly => hourly.WindSpeed).ToList();
                    var dailyRadiations = hourlyDataList.Select(hourly => hourly.Radiation).ToList();
                    // Statisztika
                    dailyStatsList.Add(new DailyStatistics
                    {
                        Date = date,
                        MinTemperature = Math.Round(dailyTemperatures.Min(), 2),
                        MaxTemperature = Math.Round(dailyTemperatures.Max(), 2),
                        AverageTemperature = Math.Round(dailyTemperatures.Average(), 2),
                        MinWindSpeed = Math.Round(dailyWindSpeeds.Min(), 2),
                        MaxWindSpeed = Math.Round(dailyWindSpeeds.Max(), 2),
                        AverageWindSpeed = Math.Round(dailyWindSpeeds.Average(), 2),
                        MaxRadiation = Math.Round(dailyRadiations.Max(), 2),
                        AverageRadiation = Math.Round(dailyRadiations.Average(), 2)
                    });
                }
            }

            WeeklyWeather.Clear();
            if (_currentCoordinateIndex != -1)
            {
                foreach (var stat in dailyStatsList)
                {
                    // Lista hozzáfűzése, ha nem minden koordinátára történik számítás
                    WeeklyWeather.Add(stat);
                }
            }
            else
            {
                if (dailyStatsList.Any())
                {
                    var allCoordDailyStatsList = new List<DailyStatistics>();
                    var groupedByDate = dailyStatsList
                        .GroupBy(ds => ds.Date)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    foreach (var dateGroup in groupedByDate)
                    {
                        string date = dateGroup.Key;
                        var statsForDate = dateGroup.Value;

                        // Napi statisztikák kiszámítása az összes koordinátából
                        allCoordDailyStatsList.Add(new DailyStatistics
                        {
                            Date = date,
                            MinTemperature = Math.Round(statsForDate.Min(ds => ds.MinTemperature), 2),
                            MaxTemperature = Math.Round(statsForDate.Max(ds => ds.MaxTemperature), 2),
                            AverageTemperature = Math.Round(statsForDate.Average(ds => ds.AverageTemperature), 2),
                            MinWindSpeed = Math.Round(statsForDate.Min(ds => ds.MinWindSpeed), 2),
                            MaxWindSpeed = Math.Round(statsForDate.Max(ds => ds.MaxWindSpeed), 2),
                            AverageWindSpeed = Math.Round(statsForDate.Average(ds => ds.AverageWindSpeed), 2),
                            MaxRadiation = Math.Round(statsForDate.Max(ds => ds.MaxRadiation), 2),
                            AverageRadiation = Math.Round(statsForDate.Average(ds => ds.AverageRadiation), 2)
                        });
                    }

                    foreach (var stat in allCoordDailyStatsList)
                    {
                        WeeklyWeather.Add(stat);
                    }
                }
            }
        }

        private Dictionary<string, List<WeatherRecordHourly>> GroupByDay(WeatherRecord record)
        {
            return record.HourlyDataList
                .GroupBy(hourly => DateTime.Parse(hourly.Time).ToString("yyyy-MM-dd"))
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private void RefreshDailyWeather()
        {
            DailyWeather.Clear();

            var selectedRecord = _loadedWeatherRecords
                .FirstOrDefault(record => record.Coordinate == SelectedCoordinate);

            if (selectedRecord == null)
            {
                MessageBox.Show("Nincs adat az adott koordinátához!");
                return;
            }

            foreach (var hourlyData in selectedRecord.HourlyDataList)
            {
                DailyWeather.Add(hourlyData);
            }
        }

        private void All_Coordinates_Click(object sender, RoutedEventArgs e)
        {
            _currentCoordinateIndex = -1;
            UpdateSelectedCoordinate();
            OnPropertyChanged(nameof(SelectedCoordinate));
        }
    }
}