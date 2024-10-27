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

namespace weather_app.Services
{
    public class XmlDataHandler
    {
        private readonly string _filePath;

        public XmlDataHandler(string filePath)
        {
            _filePath = filePath;

            if (!File.Exists(_filePath))
            {
                var rootElement = new XElement("WeatherData");
                var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), rootElement);
                doc.Save(_filePath);
            }
        }

        public void AppendHistoricalWeatherData(string provider, double latitude, double longitude, WeatherData weatherData)
        {
            XDocument doc = XDocument.Load(_filePath);
            // Új record
            for (int i = 0; i < weatherData.hourly.time.Count; i++)
            {
                XElement recordElement = new XElement("Record",
                    new XElement("Provider", provider),
                    new XElement("Latitude", latitude),
                    new XElement("Longitude", longitude),
                    new XElement("Time", weatherData.hourly.time[i]),
                    new XElement("Temperature", weatherData.hourly.temperature_2m[i]),
                    new XElement("WindSpeed", weatherData.hourly.wind_speed_10m[i]),
                    new XElement("Radiation", weatherData.hourly.direct_radiation[i])
                );

                doc.Root.Add(recordElement);
            }

            doc.Save(_filePath);
        }

        public List<string> GetAllRecords()
        {
            List<string> records = new List<string>();

            if (!File.Exists(_filePath))
                return records;

            XDocument doc = XDocument.Load(_filePath);
            string fileContent = File.ReadAllText(_filePath);
            MessageBox.Show($"XML tartalom:\n{fileContent}");

            foreach (var record in doc.Root.Elements("Record"))
            {
                string provider = record.Element("Provider")?.Value ?? "Unknown";
                string latitude = record.Element("Latitude")?.Value ?? "Unknown";
                string longitude = record.Element("Longitude")?.Value ?? "Unknown";
                string time = record.Element("Time")?.Value ?? "Unknown";
                string temperature = record.Element("Temperature")?.Value ?? "Unknown";
                string windSpeed = record.Element("WindSpeed")?.Value ?? "Unknown";
                string radiation = record.Element("Radiation")?.Value ?? "Unknown";

                string recordString = $"{latitude}, {longitude} | {time} | Temperature: {temperature}, WindSpeed: {windSpeed}, Radiation: {radiation}";
                records.Add(recordString);
            }

            return records;
        }

        public List<string> GetFilteredRecords(string startLatitude, string endLatitude)
        {
            List<string> filteredRecords = new List<string>();

            try
            {
                var allRecords = GetAllRecords();

                if (allRecords.Count == 0)
                {
                    MessageBox.Show("No records available in the source list.");
                    return filteredRecords;
                }

                // Ensure consistent parsing of start and end latitude values
                double startLat = double.Parse(startLatitude.Replace(',', '.'), CultureInfo.InvariantCulture);
                double endLat = double.Parse(endLatitude.Replace(',', '.'), CultureInfo.InvariantCulture);

                //MessageBox.Show($"startLat: {startLat}, endLat: {endLat}");

                filteredRecords = allRecords.Where(record =>
                {
                    //MessageBox.Show($"Processing Record: {record}");

                    var latitude = GetLatitudeFromRecord(record);

                    //MessageBox.Show($"Parsed latitude: {latitude}, startLat: {startLat}, endLat: {endLat}");

                    //bool isInRange = (latitude > startLat || latitude.Equals(startLat)) && (latitude < endLat || latitude.Equals(endLat));

                    //bool isInRange = latitude >= startLat && latitude <= endLat;
                    /*if (AreDoublesEqual(latitude, startLat))
                    {
                        MessageBox.Show("The latitude is approximately equal to the start latitude.");
                    }*/
                    return AreDoublesEqual(latitude, startLat) || AreDoublesEqual(latitude, endLat) || (latitude > startLat && latitude < endLat);
                    //MessageBox.Show($"Is in range: {isInRange}");
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred during filtering: {ex.Message}");
            }

            MessageBox.Show($"Filtered Records Count: {filteredRecords.Count}");
            return filteredRecords;
        }

        private double GetLatitudeFromRecord(string record)
        {
            try
            {
                // Regular expression to match the first number before the comma
                var match = Regex.Match(record, @"^(-?\d+(\.\d+)?)");
                //MessageBox.Show("match to , and to double:" + Convert.ToDouble(match.Groups[1].Value.ToString().Replace('.',',')));

                if (match.Success)
                {
                    // Parse the latitude value from the matched group
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

            return 0; // Return 0 if no valid latitude was found
        }

        private bool AreDoublesEqual(double value1, double value2, double tolerance = 0.0001)
        {
            return Math.Abs(value1 - value2) < tolerance;
        }

    }
}