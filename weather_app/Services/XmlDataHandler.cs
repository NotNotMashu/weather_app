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

        public List<string> GetFilteredRecords(
            string startLatitude,
            string endLatitude,
            int startYear,
            int endYear,
            int startDay,
            int endDay,
            int step)
        {
            List<string> filteredRecords = new List<string>();

            try
            {
                var allRecords = GetAllRecords();

                // Szűrés koordinátákra
                var coordinateFilteredRecords = allRecords
                    .Where(record => IsWithinCoordinates(record, startLatitude, endLatitude))
                    .ToList();

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

        private bool IsWithinCoordinates(string record, string startLat, string endLat)
        {
            var latitude = GetLatitudeFromRecord(record);
            double startLatitude = double.Parse(startLat.Replace(',', '.'), CultureInfo.InvariantCulture);
            double endLatitude = double.Parse(endLat.Replace(',', '.'), CultureInfo.InvariantCulture);
            //MessageBox.Show("double startlat: " + startLatitude + " endlat: "+ endLatitude);

            double minLatitude = Math.Min(startLatitude, endLatitude);
            double maxLatitude = Math.Max(startLatitude, endLatitude);

            // Ellenőrzés
            return Math.Abs(latitude - startLatitude) < 0.001 ||  // Azonos érték esetén
                   (latitude >= minLatitude && latitude <= maxLatitude);
        }

        private bool IsWithinYear(string record, int startYear, int endYear, int step)
        {
            var yearRegex = new Regex(@"\b(\d{4})\b"); // Év kinyerése
            var match = yearRegex.Match(record);

            if (match.Success && int.TryParse(match.Value, out int year))
            {
                return (year >= startYear && year <= endYear && ((year - startYear) % step == 0)) || year == endYear;
            }

            return false;
        }

        private bool IsWithinDays(string record, int startDay, int endDay)
        {
            var dateRegex = new Regex(@"\d{4}-\d{2}-(\d{2})"); // Nap kinyerése
            var match = dateRegex.Match(record);

            if (match.Success && int.TryParse(match.Groups[1].Value, out int day))
            {
                return day >= startDay && day <= endDay;
            }

            return false;
        }

    }
}