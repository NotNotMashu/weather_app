using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using weather_app.Models;
using weather_app.ViewModels;

public class WeatherDataComparer
{
    private readonly string _weatherDataFilePath;
    private readonly string _weatherForecastDataFilePath;

    public WeatherDataComparer(string weatherDataFilePath, string weatherForecastDataFilePath)
    {
        _weatherDataFilePath = weatherDataFilePath;
        _weatherForecastDataFilePath = weatherForecastDataFilePath;
    }

    public List<ComparisonData> CompareData()
    {
        // Kiolvassuk a két fájlt
        List<WeatherData> weatherDataRecords = ReadWeatherDataFromXml(_weatherDataFilePath);
        List<WeatherData> weatherForecastDataRecords = ReadWeatherDataFromXml(_weatherForecastDataFilePath);

        // Összehasonlítjuk a rekordokat az azonos koordináták és időpontok alapján
        var matchingRecords = from weatherData in weatherDataRecords
                              join forecastData in weatherForecastDataRecords
                              on new { weatherData.latitude, weatherData.longitude } equals new { forecastData.latitude, forecastData.longitude }
                              from timeIndex in Enumerable.Range(0, weatherData.hourly.time.Count)
                              where weatherData.hourly.time[timeIndex] == forecastData.hourly.time[timeIndex]
                              select new
                              {
                                  weatherData.latitude,
                                  weatherData.longitude,
                                  time = weatherData.hourly.time[timeIndex],
                                  WeatherTemperature = weatherData.hourly.temperature_2m[timeIndex],
                                  ForecastTemperature = forecastData.hourly.temperature_2m[timeIndex],
                                  WeatherWindSpeed = weatherData.hourly.wind_speed_10m[timeIndex],
                                  ForecastWindSpeed = forecastData.hourly.wind_speed_10m[timeIndex],
                                  WeatherRadiation = weatherData.hourly.direct_radiation[timeIndex],
                                  ForecastRadiation = forecastData.hourly.direct_radiation[timeIndex]
                              };

        List<ComparisonData> comparisonData = new List<ComparisonData>();
        foreach (var record in matchingRecords)
        {
            comparisonData.Add(new ComparisonData
            {
                coordinate = record.latitude.ToString("F2") + " " + record.longitude.ToString("F6"),
                time = record.time,
                recordedTemp = record.WeatherTemperature,
                recordedWind = record.WeatherWindSpeed,
                recordedRad = record.WeatherRadiation,
                forecastTemp = record.ForecastTemperature,
                forecastWind = record.ForecastWindSpeed,
                forecastRad = record.ForecastRadiation
            });
        }

        return comparisonData;
    }


    private List<WeatherData> ReadWeatherDataFromXml(string filePath)
    {
        List<WeatherData> weatherDataRecords = new List<WeatherData>();

        try
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("XML file not found.");
                return weatherDataRecords;
            }

            XDocument doc = XDocument.Load(filePath);

            foreach (var record in doc.Root.Elements("Record"))
            {
                var weatherData = new WeatherData
                {
                    latitude = double.Parse(record.Element("Latitude")?.Value ?? "0", CultureInfo.InvariantCulture),
                    longitude = double.Parse(record.Element("Longitude")?.Value ?? "0", CultureInfo.InvariantCulture),
                    hourly = new HourlyData
                    {
                        time = record.Elements("Time").Select(x => x.Value).ToList(),
                        temperature_2m = record.Elements("Temperature").Select(x => double.Parse(x.Value, CultureInfo.InvariantCulture)).ToList(),
                        wind_speed_10m = record.Elements("WindSpeed").Select(x => double.Parse(x.Value, CultureInfo.InvariantCulture)).ToList(),
                        direct_radiation = record.Elements("Radiation").Select(x => double.Parse(x.Value, CultureInfo.InvariantCulture)).ToList()
                    }
                };

                weatherDataRecords.Add(weatherData);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error while reading the XML file: {ex.Message}");
        }

        return weatherDataRecords;
    }
}
