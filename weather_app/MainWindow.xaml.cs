using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using weather_app.Services;

namespace weather_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly DbHandler _dbHandler;
        public MainWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _dbHandler = new DbHandler("127.0.0.1", "weather_db", "postgres", "admin", "5433");
            _dbHandler.MakeTable("weather_data");
            _dbHandler.MakeTable("forecast_data");

            /*bool isEmpty = _dbHandler.IsTableEmpty("weather_data");
            MessageBox.Show(isEmpty ? "The table is empty." : "The table has data.");*/
        }

        private async void Refresh_Data_Click(object sender, RoutedEventArgs e)
        {
            for (int year = 2010; year < 2025; year++)
            {
                string apiUrl = "https://archive-api.open-meteo.com/v1/archive?latitude=52.52&longitude=13.41&start_date=" + year +"-08-25&end_date="+year+"-08-31&hourly=temperature_2m,wind_speed_10m,direct_radiation";
                string jsonResponse = await _apiService.GetWeatherDataAsync(apiUrl);

                if (!jsonResponse.StartsWith("Error:"))
                {
                    // inserting data to db
                    await _dbHandler.InsertDataToTable(jsonResponse, "weather_data", "open-meteo");
                }
                else
                {
                    MessageBox.Show("Failed to retrieve weather data. Error at year "+year);
                }
            }
            MessageBox.Show("Data reset complete.");
        }
    }
}