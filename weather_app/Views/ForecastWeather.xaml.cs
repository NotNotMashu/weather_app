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
using Windows.Media.Devices;

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
                Task.Delay(5000).ContinueWith(_ =>
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
                    var windDirections = forecastData.hourly.wind_direction_10m;
                    var directRadiations = forecastData.hourly.direct_radiation;
                    var diffuseRadiations = forecastData.hourly.diffuse_radiation;
                    var directNormalIrradiances = forecastData.hourly.direct_normal_irradiance;
                    var diffuseRadiationInstants = forecastData.hourly.diffuse_radiation_instant;
                    var directNormalIrradianceInstants = forecastData.hourly.direct_normal_irradiance_instant;

                    var weatherRecord = new WeatherRecord
                    {
                        Latitude = coordpair.Item1,
                        Longitude = coordpair.Item2,
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
                                WindDirection = windDirections[i],
                                DirectRadiation = directRadiations[i],
                                DiffuseRadiation = diffuseRadiations[i],
                                DirectNormalIrradiance = directNormalIrradiances[i],
                                DiffuseRadiationInstant = diffuseRadiationInstants[i],
                                DirectNormalIrradianceInstant = directNormalIrradianceInstants[i]
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
                        WindDirection = Math.Round(group.Average(hourly => hourly.WindDirection), 2),
                        DirectRadiation = Math.Round(group.Average(hourly => hourly.DirectRadiation), 2),
                        DiffuseRadiation = Math.Round(group.Average(hourly => hourly.DiffuseRadiation), 2),
                        DirectNormalIrradiance = Math.Round(group.Average(hourly => hourly.DirectNormalIrradiance), 2),
                        DiffuseRadiationInstant = Math.Round(group.Average(hourly => hourly.DiffuseRadiationInstant), 2),
                        DirectNormalIrradianceInstant = Math.Round(group.Average(hourly => hourly.DirectNormalIrradianceInstant), 2),
                        MinTemperature = group.Min(hourly => hourly.Temperature),
                        MaxTemperature = group.Max(hourly => hourly.Temperature),
                        MinWindSpeed = group.Min(hourly => hourly.WindSpeed),
                        MaxWindSpeed = group.Max(hourly => hourly.WindSpeed),
                        MinWindDirection = group.Min(hourly => hourly.WindDirection),
                        MaxWindDirection = group.Max(hourly => hourly.WindDirection),
                        MinRadiation = group.Min(hourly => hourly.DirectRadiation),
                        MaxRadiation = group.Max(hourly => hourly.DirectRadiation),
                        MinDiffuseRadiation = group.Min(hourly => hourly.DiffuseRadiation),
                        MaxDiffuseRadiation = group.Max(hourly => hourly.DiffuseRadiation),
                        MinDirectNormalIrradiance = group.Min(hourly => hourly.DirectNormalIrradiance),
                        MaxDirectNormalIrradiance = group.Max(hourly => hourly.DirectNormalIrradiance),
                        MinDiffuseRadiationInstant = group.Min(hourly => hourly.DiffuseRadiationInstant),
                        MaxDiffuseRadiationInstant = group.Max(hourly => hourly.DiffuseRadiationInstant),
                        MinDirectNormalIrradianceInstant = group.Min(hourly => hourly.DirectNormalIrradianceInstant),
                        MaxDirectNormalIrradianceInstant = group.Max(hourly => hourly.DirectNormalIrradianceInstant)

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
                    .FirstOrDefault(record => $"{record.Latitude} {record.Longitude}" == SelectedCoordinate);

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
                        WindDirection = Math.Round(group.Average(hourly => hourly.WindDirection), 2),
                        DirectRadiation = Math.Round(group.Average(hourly => hourly.DirectRadiation), 2),
                        DiffuseRadiation = Math.Round(group.Average(hourly => hourly.DiffuseRadiation), 2),
                        DirectNormalIrradiance = Math.Round(group.Average(hourly => hourly.DirectNormalIrradiance), 2),
                        DiffuseRadiationInstant = Math.Round(group.Average(hourly => hourly.DiffuseRadiationInstant), 2),
                        DirectNormalIrradianceInstant = Math.Round(group.Average(hourly => hourly.DirectNormalIrradianceInstant), 2)
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
                if (_currentCoordinateIndex != -1 && $"{record.Latitude} {record.Longitude}" != SelectedCoordinate) continue; // nem az összes vagy a jelenleg kiválasztott esetén

                // Napokra bontás
                var dailyData = GroupByDay(record);

                foreach (var day in dailyData)
                {
                    string date = day.Key;
                    var hourlyDataList = day.Value;

                    var dailyTemperatures = hourlyDataList.Select(hourly => hourly.Temperature).ToList();
                    var dailyWindSpeeds = hourlyDataList.Select(hourly => hourly.WindSpeed).ToList();
                    var dailyRadiations = hourlyDataList.Select(hourly => hourly.DirectRadiation).ToList();
                    var dailyWindDirections = hourlyDataList.Select(hourly => hourly.WindDirection).ToList();
                    var dailyDiffuseRadiations = hourlyDataList.Select(hourly => hourly.DiffuseRadiation).ToList();
                    var dailyDirectNormalIrradiances = hourlyDataList.Select(hourly => hourly.DirectNormalIrradiance).ToList();
                    var dailyDiffuseRadiationInstants = hourlyDataList.Select(hourly => hourly.DiffuseRadiationInstant).ToList();
                    var dailyDirectNormalIrradianceInstants = hourlyDataList.Select(hourly => hourly.DirectNormalIrradianceInstant).ToList();

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

                        MinWindDirection = Math.Round(dailyWindDirections.Min(), 2),
                        MaxWindDirection = Math.Round(dailyWindDirections.Max(), 2),
                        AverageWindDirection = Math.Round(dailyWindDirections.Average(), 2),

                        MaxDirectRadiation = Math.Round(dailyRadiations.Max(), 2),
                        AverageDirectRadiation = Math.Round(dailyRadiations.Average(), 2),

                        MinDiffuseRadiation = Math.Round(dailyDiffuseRadiations.Min(), 2),
                        MaxDiffuseRadiation = Math.Round(dailyDiffuseRadiations.Max(), 2),
                        AverageDiffuseRadiation = Math.Round(dailyDiffuseRadiations.Average(), 2),

                        MinDirectNormalIrradiance = Math.Round(dailyDirectNormalIrradiances.Min(), 2),
                        MaxDirectNormalIrradiance = Math.Round(dailyDirectNormalIrradiances.Max(), 2),
                        AverageDirectNormalIrradiance = Math.Round(dailyDirectNormalIrradiances.Average(), 2),

                        MinDiffuseRadiationInstant = Math.Round(dailyDiffuseRadiationInstants.Min(), 2),
                        MaxDiffuseRadiationInstant = Math.Round(dailyDiffuseRadiationInstants.Max(), 2),
                        AverageDiffuseRadiationInstant = Math.Round(dailyDiffuseRadiationInstants.Average(), 2),

                        MinDirectNormalIrradianceInstant = Math.Round(dailyDirectNormalIrradianceInstants.Min(), 2),
                        MaxDirectNormalIrradianceInstant = Math.Round(dailyDirectNormalIrradianceInstants.Max(), 2),
                        AverageDirectNormalIrradianceInstant = Math.Round(dailyDirectNormalIrradianceInstants.Average(), 2)
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

                            MinWindDirection = Math.Round(statsForDate.Min(ds => ds.MinWindDirection), 2),
                            MaxWindDirection = Math.Round(statsForDate.Max(ds => ds.MaxWindDirection), 2),
                            AverageWindDirection = Math.Round(statsForDate.Average(ds => ds.AverageWindDirection), 2),

                            MinDirectRadiation = Math.Min(statsForDate.Min(ds => ds.MinDirectRadiation), 2),
                            MaxDirectRadiation = Math.Round(statsForDate.Max(ds => ds.MaxDirectRadiation), 2),
                            AverageDirectRadiation = Math.Round(statsForDate.Average(ds => ds.AverageDirectRadiation), 2),

                            MinDiffuseRadiation = Math.Round(statsForDate.Min(ds => ds.MinDiffuseRadiation), 2),
                            MaxDiffuseRadiation = Math.Round(statsForDate.Max(ds => ds.MaxDiffuseRadiation), 2),
                            AverageDiffuseRadiation = Math.Round(statsForDate.Average(ds => ds.AverageDiffuseRadiation), 2),

                            MinDirectNormalIrradiance = Math.Round(statsForDate.Min(ds => ds.MinDirectNormalIrradiance), 2),
                            MaxDirectNormalIrradiance = Math.Round(statsForDate.Max(ds => ds.MaxDirectNormalIrradiance), 2),
                            AverageDirectNormalIrradiance = Math.Round(statsForDate.Average(ds => ds.AverageDirectNormalIrradiance), 2),

                            MinDiffuseRadiationInstant = Math.Round(statsForDate.Min(ds => ds.MinDiffuseRadiationInstant), 2),
                            MaxDiffuseRadiationInstant = Math.Round(statsForDate.Max(ds => ds.MaxDiffuseRadiationInstant), 2),
                            AverageDiffuseRadiationInstant = Math.Round(statsForDate.Average(ds => ds.AverageDiffuseRadiationInstant), 2),

                            MinDirectNormalIrradianceInstant = Math.Round(statsForDate.Min(ds => ds.MinDirectNormalIrradianceInstant), 2),
                            MaxDirectNormalIrradianceInstant = Math.Round(statsForDate.Max(ds => ds.MaxDirectNormalIrradianceInstant), 2),
                            AverageDirectNormalIrradianceInstant = Math.Round(statsForDate.Average(ds => ds.AverageDirectNormalIrradianceInstant), 2)
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
                .FirstOrDefault(record => $"{record.Latitude} {record.Longitude}" == SelectedCoordinate);

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