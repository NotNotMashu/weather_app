using System.Collections.ObjectModel;
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

        // Összehasonlítjuk a rekordokat az azonos koordináták és idő alapján
        var matchingRecords = from weatherData in weatherDataRecords
                              join forecastData in weatherForecastDataRecords
                              on new { weatherData.latitude, weatherData.longitude }
                              equals new { forecastData.latitude, forecastData.longitude }
                              let weatherTimes = weatherData.hourly.time.Select(t => DateTime.Parse(t)).ToList()
                              let forecastTimes = forecastData.hourly.time.Select(t => DateTime.Parse(t)).ToList()
                              where weatherTimes.Intersect(forecastTimes).Any() // Csak ha van közös időpont
                              select new
                              {
                                  Coordinate = $"{weatherData.latitude:F2} {weatherData.longitude:F6}",
                                  HourlyData = weatherTimes
                                      .Select((time, index) => new ComparisonHourly
                                      {
                                          Time = weatherData.hourly.time[index], // Tároljuk a Time stringet
                                          RecordedTemp = weatherData.hourly.temperature_2m[index],
                                          RecordedWind = weatherData.hourly.wind_speed_10m[index],
                                          RecordedRad = weatherData.hourly.direct_radiation[index],
                                          ForecastTemp = forecastData.hourly.temperature_2m[index],
                                          ForecastWind = forecastData.hourly.wind_speed_10m[index],
                                          ForecastRad = forecastData.hourly.direct_radiation[index]
                                      })
                                      .Where(hourly => forecastTimes.Contains(hourly.DateTime)) // Csak egyező időpontok
                                      .ToList()
                              };

        // Létrehozzuk a ComparisonData listát
        List<ComparisonData> comparisonData = new List<ComparisonData>();

        foreach (var record in matchingRecords)
        {
            comparisonData.Add(new ComparisonData
            {
                Coordinate = record.Coordinate,
                comparisonHourlies = record.HourlyData
            });
        }

        return comparisonData;
    }

    public ObservableCollection<ComparisonData> GroupComparisonData(List<ComparisonData> comparisonData)
    {
        // Itt csoportosítjuk a koordinátákat
        var groupedData = new ObservableCollection<ComparisonData>();

        // Csoportosítás koordináták alapján
        var groupedCoordinates = comparisonData
            .GroupBy(d => d.Coordinate)
            .ToList();

        foreach (var group in groupedCoordinates)
        {
            // Az egyes koordinátákhoz hozzáadjuk az óránkénti adatokat
            var comparisonDataForCoordinate = new ComparisonData
            {
                Coordinate = group.Key,
                comparisonHourlies = group.SelectMany(d => d.comparisonHourlies).ToList()
            };

            groupedData.Add(comparisonDataForCoordinate);
        }

        return groupedData;
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
