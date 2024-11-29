using System;
using System.Collections.Generic;
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
    public partial class ForecastWeather : UserControl
    {
        ApiService _apiService = new ApiService();
        Coordinates _coordinates = new Coordinates();
        public ForecastWeather()
        {
            InitializeComponent();
            DataContext = this;
            LoadForecastData();

        }

        public class WeatherViewModel
        {
            public List<WeatherRecord> FilteredRecords { get; set; }
        }

        private async Task<List<WeatherRecord>> LoadForecastData()
        {
            var weatherRecords = new List<WeatherRecord>();
            foreach (var coordpair in _coordinates.CoordinatePairs)
            {
                WeatherData forecastData = await _apiService.ReturnForecastData(coordpair);

                if (forecastData != null)
                {
                    Debug.WriteLine("Forecast data loaded successfully.");

                    if (forecastData.hourly != null)
                    {
                        var times = forecastData.hourly.time;
                        var temperatures = forecastData.hourly.temperature_2m;
                        var windSpeeds = forecastData.hourly.wind_speed_10m;
                        var radiations = forecastData.hourly.direct_radiation;
                        string coordinate = coordpair.Item1 +" "+coordpair.Item2;

                        var weatherRecord = new WeatherRecord
                        {
                            Coordinate = coordinate,
                            HourlyDataList = new List<WeatherRecordHourly>()
                        };

                        for (int i = 0; i < times.Count; i++)
                        {
                            DateTime timestamp = DateTime.Parse(times[i]);
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
                                //Debug.WriteLine($"{coordinate} {times[i].Split('T')[0]} {hour} óra, Hőmérséklet: {temperatures[i]} °C, Szélsebesség: {windSpeeds[i]} km/h, Sugárzás: {radiations[i]} W/m²");
                            }
                        }
                        weatherRecords.Add(weatherRecord);
                    }
                    else
                    {
                        Debug.WriteLine("Hourly data is missing in the forecast data.");
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to load forecast data.");
                }
            }
            return weatherRecords;
        }
    }
}
