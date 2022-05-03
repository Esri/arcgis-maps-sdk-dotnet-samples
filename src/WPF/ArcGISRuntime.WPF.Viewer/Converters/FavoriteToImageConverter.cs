using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ArcGISRuntime.WPF.Viewer.Converters
{
    public class FavoriteToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                {
                    return "resources/favoriteStar.png";
                }
            }

            return "resources/whiteStar.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
