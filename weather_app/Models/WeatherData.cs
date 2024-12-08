using System.Collections.Generic;

public class HourlyData
{
    public List<string> time { get; set; }
    public List<double> temperature_2m { get; set; }
    public List<double> wind_speed_10m { get; set; }
    public List<double> direct_radiation { get; set; }
}

public class WeatherData
{
    public double latitude { get; set; }
    public double longitude { get; set; }
    public HourlyData hourly { get; set; }

    public string coordinate => $"{latitude}, {longitude}";
}
