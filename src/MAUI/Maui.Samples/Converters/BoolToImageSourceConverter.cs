using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGIS.Converters
{
    public class BoolToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Determine if the OS is in light or dark theme.
            AppTheme currentTheme = Application.Current.RequestedTheme;

            // Check if the sample is a favorite then change icon according to OS theme.
            ImageSource imageSource;
            if ((bool)value)
            {
                imageSource = ImageSource.FromFile("starfilled.png");
            }
            else
            {
                imageSource = currentTheme == AppTheme.Light ? ImageSource.FromFile("star.png") : ImageSource.FromFile("stardark.png");
            }

            return imageSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
