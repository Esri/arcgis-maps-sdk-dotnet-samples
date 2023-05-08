using System.Globalization;
using System.Reflection;

namespace ArcGIS.Converters
{
    internal class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(string)) { return null; }
            return Microsoft.Maui.Graphics.Color.FromArgb((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
