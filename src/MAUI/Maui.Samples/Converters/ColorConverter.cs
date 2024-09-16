using System.Globalization;
using System.Reflection;

namespace ArcGIS.Converters
{
    internal class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            // Determine the object type.
            Type type = value.GetType();

            if (type == typeof(string))
            {
                string colorString = value as string;

                // Color needs to be in hex format or else it will throw.
                return Color.FromArgb(colorString);
            }
            else if (type == typeof(System.Drawing.Color))
            {
                var color = (System.Drawing.Color)value;

                return new Color(color.R, color.G, color.B, color.A);
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
