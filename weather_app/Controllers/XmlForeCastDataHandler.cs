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

public class XmlForecastDataHandler
{

    private readonly string _filePath;

    public XmlForecastDataHandler(string filePath)
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

            var rootElement = new XElement("weather_forecast_data");
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), rootElement);
            doc.Save(_filePath); // xml létrehozása
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error while creating the XML file: {ex.Message}");
        }
    }
    public void AppendHistoricalForecastData(string provider, double latitude, double longitude, WeatherData weatherData)
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
                    new XElement("Radiation", weatherData.hourly.direct_radiation[i])
                );

                doc.Root.Add(recordElement);
            }
        }
        doc.Save(_filePath);
    }
}
