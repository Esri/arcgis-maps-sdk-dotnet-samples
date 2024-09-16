using System;
using System.Drawing;
using System.Windows.Data;
using WinMedia = System.Windows.Media;

namespace ArcGIS.WPF.Viewer.Converters
{
    // A class that converts a System.Drawing.Color object to a solid brush for setting background color for UI controls.
    internal class ColorToSolidBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Get the input value as a System.Drawing.Color.
            Color inColor = (Color)value;

            // Create a solid color brush using the color and return it.
            return Convert(inColor);
        }

        public static WinMedia.SolidColorBrush Convert(Color color)
        {  
            // Convert the input System.Drawing.Color to a System.Windows.Media.Color.
            WinMedia.Color outColor = WinMedia.Color.FromArgb(color.A, color.R, color.G, color.B);

            // Create a solid color brush using the color and return it.
            return new WinMedia.SolidColorBrush(outColor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static Color ConvertBack(WinMedia.SolidColorBrush brush)
        {
            // Convert the input System.Windows.Media.Color to a System.Drawing.Color.
            return Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
        }
    }
}
