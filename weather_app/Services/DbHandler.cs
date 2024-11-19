using Npgsql;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Windows;
using Newtonsoft.Json;

namespace weather_app.Services
{
    public class DbHandler
    {
        private readonly string _connectionString;

        public DbHandler(string host, string database, string username, string password, string port)
        {
            _connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }

        // open,return connection
        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        // make table
        public void MakeTable(string tableName)
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                // SQL query to create the table
                string query = $@"
                CREATE TABLE IF NOT EXISTS {tableName} (
                provider VARCHAR(50),
                longitude DOUBLE PRECISION,
                latitude DOUBLE PRECISION,
                date TIMESTAMP,
                temperature DOUBLE PRECISION,
                wind DOUBLE PRECISION,
                radiation DOUBLE PRECISION
            )";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // check if table is empty
        public bool IsTableEmpty(string tableName)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = $"SELECT COUNT(*) FROM {tableName}";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count == 0;
                }
            }
        }

        public async Task InsertDataToTable(string jsonString, string tableName, string provider)
        {
            // Deserialize the JSON string to a WeatherData object
            WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(jsonString);

            if (weatherData != null)
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string query = $@"
                INSERT INTO {tableName} (provider, latitude, longitude, date, temperature, wind, radiation) 
                VALUES (@provider, @latitude, @longitude, @date, @temperature, @wind, @radiation)";

                    // Loop through each data point
                    for (int i = 0; i < weatherData.hourly.time.Count; i++)
                    {
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            // static parameters
                            command.Parameters.AddWithValue("@provider", provider);
                            command.Parameters.AddWithValue("@longitude", weatherData.longitude);
                            command.Parameters.AddWithValue("@latitude", weatherData.latitude);

                            // Set the parameters for each hourly data point
                            command.Parameters.AddWithValue("@date", DateTime.Parse(weatherData.hourly.time[i]));
                            command.Parameters.AddWithValue("@temperature", weatherData.hourly.temperature_2m[i]);
                            command.Parameters.AddWithValue("@wind", weatherData.hourly.wind_speed_10m[i]);
                            command.Parameters.AddWithValue("@radiation", weatherData.hourly.direct_radiation[i]);

                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Failed to parse the weather data. Please check the input JSON.");
            }
        }

        public bool HasDataFromProvider(string provider)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = $"SELECT COUNT(*) FROM weather_data WHERE provider = @provider";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@provider", provider);

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }
    }
}
