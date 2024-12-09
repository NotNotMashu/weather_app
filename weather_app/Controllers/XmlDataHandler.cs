using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data.Common;
using System.Windows;
using System.Text.RegularExpressions;
using System.Globalization;
using weather_app.ViewModels;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace weather_app.Services
{
    public class XmlDataHandler
    {
        private readonly string _filePath;

        public XmlDataHandler(string filePath)
        {
            _filePath = filePath;
        }

        public void CreateOrReplaceXmlFile()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    File.Delete(_filePath); // Törlés, ha már létezik
                }

                var rootElement = new XElement("weather_data");
                var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), rootElement);
                doc.Save(_filePath); // xml létrehozása
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while creating the XML file: {ex.Message}");
            }
        }

        public void AppendHistoricalWeatherData(string provider, double latitude, double longitude, WeatherData weatherData)
        {
            XDocument doc = XDocument.Load(_filePath);

            for (int i = 0; i < weatherData.hourly.time.Count; i++)
            {
                DateTime timestamp = DateTime.Parse(weatherData.hourly.time[i]);
                int hour = timestamp.Hour;

                // Csak 6:00, 12:00 és 18:00
                if (hour == 6 || hour == 12 || hour == 18)
                {
                    XElement recordElement = new XElement("Record",
                    new XElement("Provider", provider),
                    new XElement("Latitude", latitude),
                    new XElement("Longitude", longitude),
                    new XElement("Time", weatherData.hourly.time[i]),
                    new XElement("Temperature", weatherData.hourly.temperature_2m[i]),
                    new XElement("WindSpeed", weatherData.hourly.wind_speed_10m[i]),
                    new XElement("WindDirection", weatherData.hourly.wind_direction_10m[i]),
                    new XElement("DirectRadiation", weatherData.hourly.direct_radiation[i]),
                    new XElement("DiffuseRadiation", weatherData.hourly.diffuse_radiation[i]), // DHI
                    new XElement("DirectNormalIrradiance", weatherData.hourly.direct_normal_irradiance[i]), // DNI
                    new XElement("DiffuseRadiationInstant", weatherData.hourly.diffuse_radiation_instant[i]), // DHI Instant
                    new XElement("DirectNormalIrradianceInstant", weatherData.hourly.direct_normal_irradiance_instant[i]) //DNI Instant
                );

                    doc.Root.Add(recordElement);
                }
            }

            doc.Save(_filePath);
        }

        public List<WeatherData> GetAllRecords()
        {
            List<WeatherData> weatherDataList = new List<WeatherData>();

            if (!File.Exists(_filePath))
                return weatherDataList;

            XDocument doc = XDocument.Load(_filePath);

            foreach (var record in doc.Root.Elements("Record"))
            {
                string latitudeStr = record.Element("Latitude")?.Value ?? "0";
                string longitudeStr = record.Element("Longitude")?.Value ?? "0";
                double latitude = double.TryParse(latitudeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat) ? lat : 0;
                double longitude = double.TryParse(longitudeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double lon) ? lon : 0;

                string time = record.Element("Time")?.Value ?? "Unknown";
                string temperatureStr = record.Element("Temperature")?.Value ?? "0";
                string windSpeedStr = record.Element("WindSpeed")?.Value ?? "0";
                string windDirectionStr = record.Element("WindDirection")?.Value ?? "0";
                string directRadiationStr = record.Element("DirectRadiation")?.Value ?? "0";
                string diffuseRadiationStr = record.Element("DiffuseRadiation")?.Value ?? "0";
                string directNormalIrradianceStr = record.Element("DirectNormalIrradiance")?.Value ?? "0";
                string diffuseRadiationInstantStr = record.Element("DiffuseRadiationInstant")?.Value ?? "0";
                string directNormalIrradianceInstantStr = record.Element("DirectNormalIrradianceInstant")?.Value ?? "0";

                double temperature = double.TryParse(temperatureStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double temp) ? temp : 0;
                double windSpeed = double.TryParse(windSpeedStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double wind) ? wind : 0;
                double windDirection = double.TryParse(windDirectionStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double direction) ? direction : 0;
                double directRadiation = double.TryParse(directRadiationStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double directRad) ? directRad : 0;
                double diffuseRadiation = double.TryParse(diffuseRadiationStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double diffuseRad) ? diffuseRad : 0;
                double directNormalIrradiance = double.TryParse(directNormalIrradianceStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double dni) ? dni : 0;
                double diffuseRadiationInstant = double.TryParse(diffuseRadiationInstantStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double diffuseInstant) ? diffuseInstant : 0;
                double directNormalIrradianceInstant = double.TryParse(directNormalIrradianceInstantStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double dniInstant) ? dniInstant : 0;

                HourlyData hourlyData = new HourlyData
                {
                    time = new List<string> { time },
                    temperature_2m = new List<double> { temperature },
                    wind_speed_10m = new List<double> { windSpeed },
                    wind_direction_10m = new List<double> { windDirection },
                    direct_radiation = new List<double> { directRadiation },
                    diffuse_radiation = new List<double> { diffuseRadiation },
                    direct_normal_irradiance = new List<double> { directNormalIrradiance },
                    diffuse_radiation_instant = new List<double> { diffuseRadiationInstant },
                    direct_normal_irradiance_instant = new List<double> { directNormalIrradianceInstant }
                };

                WeatherData weatherData = new WeatherData
                {
                    latitude = latitude,
                    longitude = longitude,
                    hourly = hourlyData
                };

                weatherDataList.Add(weatherData);
            }

            return weatherDataList;
        }

        public List<WeatherData> GetFilteredRecords(
            string startLatitude,
            string endLatitude,
            int startYear,
            int endYear,
            int startDay,
            int endDay,
            int step)
        {
            List<WeatherData> filteredRecords = new List<WeatherData>();

            try
            {
                var allRecords = GetAllRecords();

                // Szűrés koordinátákra
                var coordinateFilteredRecords = (!startLatitude.Equals("skip"))
                    ? allRecords
                        .Where(record => IsWithinCoordinates(record, startLatitude, endLatitude))
                        .ToList()
                    : allRecords;

                // Szűrés évekre (csak ha nem a teljes tartomány van kiválasztva)
                var yearFilteredRecords = (startYear != 2014 || endYear != 2024)
                    ? coordinateFilteredRecords
                        .Where(record => IsWithinYear(record, startYear, endYear, step))
                        .ToList()
                    : coordinateFilteredRecords;

                // Szűrés napokra (csak ha nem a teljes tartomány van kiválasztva)
                filteredRecords = (startDay != 25 || endDay != 31)
                    ? yearFilteredRecords
                        .Where(record => IsWithinDays(record, startDay, endDay))
                        .ToList()
                    : yearFilteredRecords;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a szűrés során: {ex.Message}");
            }

            return filteredRecords;
        }

        /*private double GetLatitudeFromRecord(string record)
        {
            try
            {
                var match = Regex.Match(record, @"^(-?\d+(\.\d+)?)");
                if (match.Success)
                {
                    if (double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude))
                    {
                        //MessageBox.Show("parsed latitude: " + latitude); //-12,47
                        return latitude;
                    }
                }
                else
                {
                    MessageBox.Show("Latitude not found in the record.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing latitude: {ex.Message}");
            }

            return 0; 
        }*/

        /*private bool AreDoublesEqual(double value1, double value2, double tolerance = 0.0001)
        {
            return Math.Abs(value1 - value2) < tolerance;
        }*/

        private bool IsWithinCoordinates(WeatherData record, string startLat, string endLat)
        {
            var latitude = record.latitude;
            double startLatitude = double.Parse(startLat.Replace(',', '.'), CultureInfo.InvariantCulture);
            double endLatitude = double.Parse(endLat.Replace(',', '.'), CultureInfo.InvariantCulture);

            double minLatitude = Math.Min(startLatitude, endLatitude);
            double maxLatitude = Math.Max(startLatitude, endLatitude);

            bool isWithin = Math.Abs(latitude - startLatitude) < 0.001 || (latitude >= minLatitude && latitude <= maxLatitude);
            return isWithin;
        }

        private bool IsWithinYear(WeatherData record, int startYear, int endYear, int step)
        {
            var time = DateTime.Parse(record.hourly.time[0]);
            int year = time.Year;

            return year >= startYear && year <= endYear && ((year - startYear) % step == 0) || year == endYear;
        }

        private bool IsWithinDays(WeatherData record, int startDay, int endDay)
        {
            var time = DateTime.Parse(record.hourly.time[0]);
            int day = time.Day;

            return day >= startDay && day <= endDay;
        }


        /*public List<WeatherRecord> ParseRecords(List<string> recordStrings)
        {
            List<WeatherRecord> weatherRecords = new List<WeatherRecord>();

            var groupedByCoordinate = recordStrings
                .GroupBy(record => record.Split('|')[0])  // Csoportosítás koordináta alapján
                .ToList();

            foreach (var group in groupedByCoordinate)
            {
                WeatherRecord weatherRecord = new WeatherRecord
                {
                    Coordinate = group.Key,
                    HourlyDataList = new List<WeatherRecordHourly>()
                };

                foreach (var recordString in group)
                {
                    var parts = recordString.Split('|');
                    if (parts.Length < 2)
                        continue;

                    var time = parts[1].Split('T')[1].Trim();
                    var dataParts = parts[2].Split(new string[] { "Temperature:", "WindSpeed:", "Radiation:" }, StringSplitOptions.None);

                    if (dataParts.Length < 4)
                        continue;

                    var temperature = dataParts[1].Trim().Split(',');
                    var windSpeed = dataParts[2].Trim().Split(',');
                    var radiation = dataParts[3].Trim().Split(',');

                    weatherRecord.HourlyDataList.Add(new WeatherRecordHourly
                    {
                        Time = time,
                        Temperature = double.Parse(temperature[0], System.Globalization.CultureInfo.InvariantCulture),
                        WindSpeed = double.Parse(windSpeed[0], System.Globalization.CultureInfo.InvariantCulture),
                        Radiation = double.Parse(radiation[0], System.Globalization.CultureInfo.InvariantCulture)
                    });
                }

                weatherRecords.Add(weatherRecord);
            }

            return weatherRecords;
        }*/

        public List<WeatherData> GetFilteredRecordsSeparated(int year, int day)
        {
            List<WeatherData> filteredRecords = GetFilteredRecords("skip", "skip", year, year, day, day, 1);

            return filteredRecords;
        }

        public List<WeatherData> GetFilteredRecordsSeparated(int day)
        {
            List<WeatherData> weatherRecords = new List<WeatherData>();
            for (int year = 2014; year <= 2024; year++)
            {
                var filteredRecords = GetFilteredRecords("skip", "skip", year, year, day, day, 1);

                if (filteredRecords != null)
                {
                    weatherRecords.AddRange(filteredRecords);
                }
            }
            return weatherRecords;
        }
    }
}