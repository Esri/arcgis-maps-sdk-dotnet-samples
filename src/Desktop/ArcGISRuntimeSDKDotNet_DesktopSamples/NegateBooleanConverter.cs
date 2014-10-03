using System;
using System.Globalization;
using System.Windows.Data;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples
{
    public class NegateBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                var booleanValue = (bool)value;
                return !booleanValue;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}