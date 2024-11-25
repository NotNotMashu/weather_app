using System.Globalization;
using System.Windows.Data;

namespace weather_app.Views
{
    public class RemoveCommaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            string valueString = value.ToString();
            if (double.TryParse(valueString, out double result))
            {
                return result.ToString("F1", CultureInfo.InvariantCulture); // Eltávolítja a vesszőket
            }

            return valueString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
