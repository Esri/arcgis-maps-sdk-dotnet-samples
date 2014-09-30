using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples
{
    public class StringToDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                var stringValue = (string)value;
                DateTime dateTime = DateTime.MinValue;
                if (DateTime.TryParse(stringValue, out dateTime))
                    return dateTime;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
