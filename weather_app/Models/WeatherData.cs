using System.Collections.Generic;

public class HourlyData
{
    public List<string> time { get; set; }
    public List<double> temperature_2m { get; set; }
    public List<double> wind_speed_10m { get; set; }
    public List<double> wind_direction_10m { get; set; }
    public List<double> direct_radiation { get; set; }
    public List<double> diffuse_radiation { get; set; } // DHI
    public List<double> direct_normal_irradiance { get; set; } // DNI
    public List<double> diffuse_radiation_instant { get; set; } // DHI Instant
    public List<double> direct_normal_irradiance_instant { get; set; } // DNI Instant
}

public class WeatherData
{
    public double latitude { get; set; }
    public double longitude { get; set; }
    public HourlyData hourly { get; set; }

    public string coordinate => $"{latitude:F2}, {longitude:F2}";
}
