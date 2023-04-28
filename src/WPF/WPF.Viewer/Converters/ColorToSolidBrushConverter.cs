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

            // Convert the input System.Drawing.Color to a System.Windows.Media.Color.
            WinMedia.Color outColor = WinMedia.Color.FromArgb(inColor.A, inColor.R, inColor.G, inColor.B);

            // Create a solid color brush using the color and return it.
            WinMedia.SolidColorBrush brush = new WinMedia.SolidColorBrush(outColor);
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
