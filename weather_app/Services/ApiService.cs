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

namespace weather_app.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly XmlDataHandler _xmlHandler;
        public ApiService()
        {
            _httpClient = new HttpClient();
            _xmlHandler = new XmlDataHandler("weather_data.xml");
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
            string apiUrl = $"https://archive-api.open-meteo.com/v1/archive?latitude={stringLat}&longitude={stringLong}&start_date={year}-08-25&end_date={year}-08-31&hourly=temperature_2m,wind_speed_10m,direct_radiation&timezone=Australia%2FSydney";
            string apiUrl2 = $"https://archive-api.open-meteo.com/v1/archive?latitude=" + stringLat + "&longitude=" + stringLong + "&start_date=" + year + "-08-25&end_date=" + year + "-08-31&hourly=temperature_2m,wind_speed_10m,direct_radiation&timezone=Australia%2FSydney";
            //MessageBox.Show(apiUrl2);
            try
            {
                string jsonResponse = await GetWeatherDataAsync(apiUrl);
                if (!jsonResponse.StartsWith("Error:"))
                {
                    // Deserialize the JSON into a single WeatherData object
                    WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(jsonResponse);

                    // Assuming you only need to append this single weather data
                    _xmlHandler.AppendHistoricalWeatherData(provider, latitude, longitude, weatherData);
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
        }

        public async Task CreateFileIfNeeded()
        {
            _xmlHandler.CreateOrReplaceXmlFile();
        }

            /*

                    public async Task<CurrentWeatherData> GetWeatherDataFromProviderAsync(string provider)
                    {
                        string apiUrl = string.Empty;

                        string locationUrl = "http://ip-api.com/json/";
                        using HttpClient client = new HttpClient();
                        string locationResponse = await client.GetStringAsync(locationUrl);

                        JObject locationData = JObject.Parse(locationResponse);
                        double latitude = (double)locationData["lat"];
                        double longitude = (double)locationData["lon"];

                        // Szolgáltató alapján más-más url
                        switch (provider.ToLower())
                        {
                            case "open-meteo":
                                apiUrl = "https://api.open-meteo.com/v1/forecast?latitude="+latitude+"&longitude="+longitude+"&current=temperature_2m,wind_speed_10m&hourly=temperature_2m,relative_humidity_2m,wind_speed_10m";
                                break;

                            case "provider2":
                                apiUrl = "https://api.provider2.com/current?lat=52.52&lon=13.41"; // Példa URL
                                break;

                            /*case "provider2":
                                apiUrl = "https://api.provider2.com/current?lat=52.52&lon=13.41"; // Példa URL
                                break;
                            default:
                                throw new ArgumentException("Ismeretlen szolgáltató: " + provider);
                        }

                        try
                        {
                            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                            response.EnsureSuccessStatusCode();

                            string jsonResponse = await response.Content.ReadAsStringAsync();
                            var data = JsonConvert.DeserializeObject<CurrentWeatherData>(jsonResponse);

                            return new CurrentWeatherData
                            {
                                Provider = provider,
                                Temperature = data.Temperature,
                                WindSpeed = data.WindSpeed,
                                Radiation = data.Radiation
                            };
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Hiba történt az API hívás során: {ex.Message}");
                            return new CurrentWeatherData(); // Üres
                        }

                    }*/
        }
}

