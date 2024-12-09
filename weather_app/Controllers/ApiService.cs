using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Windows;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.WebRequestMethods;
using System.Collections;
using System.Diagnostics;
using Windows.Services.Maps;
using Microsoft.Extensions.Configuration;
using System.IO;
using weather_app.Models;
using System.Security.Policy;

namespace weather_app.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly XmlDataHandler _xmlHandler;
        private readonly XmlForecastDataHandler _xmlForecastDataHandler;
        private string apiForecastPart1 = "https://api.open-meteo.com/v1/forecast?latitude=";
        private string apiForecastPart2 = "&longitude=";
        private string apiForecastPart3 = "&hourly=temperature_2m,wind_speed_10m,wind_direction_10m,direct_radiation,diffuse_radiation,direct_normal_irradiance,diffuse_radiation_instant,direct_normal_irradiance_instant&timezone=auto&forecast_days=15";
        string openWeatherMapKey, weatherStackKey, weatherApiKey;
        private string _weatherIconUrl;
        public string WeatherIconUrl
        {
            get => _weatherIconUrl;
            set
            {
                _weatherIconUrl = value;
            }
        }

        public ApiService()
        {
            _httpClient = new HttpClient();
            _xmlHandler = new XmlDataHandler("weather_data.xml");
            _xmlForecastDataHandler = new XmlForecastDataHandler("weather_forecast_data.xml");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            openWeatherMapKey = configuration["OpenWeatherMap:ApiKey"];
            weatherStackKey = configuration["WeatherStack:ApiKey"];
            weatherApiKey = configuration["WeatherAPI:ApiKey"];
        }

        public async Task<List<CurrentWeatherResponse>> CallAllCurrentProviders(string lat, string lon)
        {
            List<CurrentWeatherResponse> responses = new List<CurrentWeatherResponse>();

            string apiUrlOpenWeathermap = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={openWeatherMapKey}&units=metric";
            //string apiUrl2 = $"https://api.openweathermap.org/data/2.5/solar_radiation?lat={latitude}&lon={longitude}&appid={apiKey}"; // Ingyenesen nem elérhető
            var openWeatherResponse = await CallCurrentWeather(apiUrlOpenWeathermap);
            if (openWeatherResponse != null)
            {
                responses.Add(openWeatherResponse);
            }

            string apiUrlWeatherStack = $"https://api.weatherstack.com/current?access_key={weatherStackKey}&query={lat},{lon}";
            var weatherStackResponse = await CallCurrentWeather(apiUrlWeatherStack);
            if (weatherStackResponse != null)
            {
                responses.Add(weatherStackResponse);
            }

            string apiUrlWeatherApi = $"http://api.weatherapi.com/v1/current.json?key={weatherApiKey}&q={lat},{lon}&aqi=no";
            var weatherApiResponse = await CallCurrentWeather(apiUrlWeatherApi);
            if (weatherApiResponse != null)
            {
                responses.Add(weatherApiResponse);
            }

            return responses;
        }
        public async Task<CurrentWeatherResponse> CallCurrentWeather(string url)
        {
            try
            {
                string jsonResponse = await GetWeatherDataAsync(url);
                Debug.WriteLine("Response: " + jsonResponse);

                var weatherData = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                string provider = "Ismeretlen";
                double temperature = 0.0;
                double windSpeed = 0.0;

                // OpenWeatherMap
                if (weatherData.main != null && weatherData.wind != null)
                {
                    provider = "OpenWeather";
                    temperature = weatherData.main.temp ?? 0.0;
                    windSpeed = (weatherData.wind.speed ?? 0.0);
                }
                // WeatherStack
                else if (weatherData.current != null && weatherData.current.wind_speed != null)
                {
                    provider = "WeatherStack";
                    temperature = weatherData.current.temperature ?? 0.0;
                    windSpeed = weatherData.current.wind_speed ?? 0.0;
                }
                // WeatherAPI
                else if (weatherData.current != null && weatherData.current.wind_kph != null)
                {
                    provider = "WeatherAPI";
                    temperature = weatherData.current.temp_c ?? 0.0;
                    windSpeed = weatherData.current.wind_kph ?? 0.0;

                    _weatherIconUrl = "https:" + weatherData.current.condition.icon;
                }

                Debug.WriteLine($"Temperature: {temperature}°C");
                Debug.WriteLine($"Wind Speed: {windSpeed} km/h");

                return new CurrentWeatherResponse
                {
                    Provider = provider,
                    Temperature = temperature,
                    WindSpeed = windSpeed
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public class WeatherDataCollection
        {
            public List<WeatherData> WeatherDataList { get; set; }
        }

        public async Task<string> GetWeatherDataAsync(string url)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                //MessageBox.Show($"Request error: {e.Message} url: {url}");
                return "Error: Unable to retrieve weather data. Please check the connection.";
            }
        }

        public async Task FetchAndStoreHistoricalWeatherData(string provider, double latitude, double longitude, int year)
        {
            string stringLat = latitude.ToString().Replace(',', '.');
            string stringLong = longitude.ToString().Replace(",", ".");
            string apiUrl = $"https://archive-api.open-meteo.com/v1/archive?latitude={stringLat}&longitude={stringLong}&start_date={year}-08-25&end_date={year}-08-31&hourly=temperature_2m,wind_speed_10m,wind_direction_10m,direct_radiation,diffuse_radiation,direct_normal_irradiance,diffuse_radiation_instant,direct_normal_irradiance_instant&timezone=auto";
            string apiUrlForecast = $"https://historical-forecast-api.open-meteo.com/v1/forecast?latitude={stringLat}&longitude={stringLong}&start_date={year}-08-25&end_date={year}-08-31&hourly=temperature_2m,wind_speed_10m,wind_direction_10m,direct_radiation,diffuse_radiation,direct_normal_irradiance,diffuse_radiation_instant,direct_normal_irradiance_instant&timezone=auto";
            try
            {
                string jsonResponse = await GetWeatherDataAsync(apiUrl);
                string jsonResponseForecast = await GetWeatherDataAsync(apiUrlForecast);

                if (!jsonResponse.StartsWith("Error:"))
                {
                    WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(jsonResponse);

                    _xmlHandler.AppendHistoricalWeatherData(provider, latitude, longitude, weatherData);
                }
                else
                {
                    Debug.WriteLine("Error in fetching historical weather data.");
                }
                if (year >= 2022)
                {
                    if (!jsonResponseForecast.StartsWith("Error:"))
                    {
                        WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(jsonResponseForecast);

                        _xmlForecastDataHandler.AppendHistoricalForecastData(provider, latitude, longitude, weatherData);
                    }
                    else
                    {
                        Debug.WriteLine("Error in fetching historical forecast data.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing historical forecast API response: {ex.Message}");
            }
        }

        public async Task<WeatherData> ReturnForecastData(Tuple<double, double> coordpair)
        {
            try
            {
                string url = String.Concat(apiForecastPart1, coordpair.Item1.ToString().Replace(',', '.'), apiForecastPart2, coordpair.Item2.ToString().Replace(',', '.'), apiForecastPart3);
                //string url = $"{apiForecastPart1}{coordpair.Item1.ToString().Replace(',', '.')}&{apiForecastPart2}{coordpair.Item2.ToString().Replace(',', '.')}{apiForecastPart3}";
                string jsonResponse = await GetWeatherDataAsync(url);
                //MessageBox.Show(jsonResponse);

                if (!jsonResponse.StartsWith("Error:"))
                {
                    WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(jsonResponse);
                    return weatherData;
                }
                else
                {
                    Debug.WriteLine("Error in fetching historical weather data.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing historical API response: {ex.Message}");
            }
            return null;
        }


        public async Task CreateFileIfNeeded()
        {
            _xmlHandler.CreateOrReplaceXmlFile();
            _xmlForecastDataHandler.CreateOrReplaceXmlFile();
        }
    }
}

