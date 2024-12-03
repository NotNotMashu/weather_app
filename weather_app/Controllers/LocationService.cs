using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Devices.Geolocation;

namespace weather_app.Services
{
    public class LocationService
    {
        public async Task<(double Latitude, double Longitude)> GetCurrentLocationAsync()
        {
            try
            {
                var geolocator = new Geolocator { DesiredAccuracy = PositionAccuracy.High };

                // Request access to location services
                var accessStatus = await Geolocator.RequestAccessAsync();

                if (accessStatus == GeolocationAccessStatus.Allowed)
                {
                    // Get the current position
                    var position = await geolocator.GetGeopositionAsync();

                    // Extract latitude and longitude
                    double latitude = position.Coordinate.Point.Position.Latitude;
                    double longitude = position.Coordinate.Point.Position.Longitude;

                    return (latitude, longitude);
                }
                else
                {
                    throw new Exception("Access to location is denied.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get location: {ex.Message}");
            }
        }

        /*public async void GetLocation()
        {
            try
            {
                var locationService = new LocationService();
                var (latitude, longitude) = await locationService.GetCurrentLocationAsync();

                //MessageBox.Show($"Your current location is:\nLatitude: {latitude}, Longitude: {longitude}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }*/
    }
}
