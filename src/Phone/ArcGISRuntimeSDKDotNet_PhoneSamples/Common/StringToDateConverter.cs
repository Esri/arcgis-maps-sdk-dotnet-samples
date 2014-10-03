using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Common
{
    public class StringToDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
                return System.Convert.ToString(value, CultureInfo.InvariantCulture);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                var stringValue = (string)value;
                DateTime dateValue = DateTime.MinValue;
                if (DateTime.TryParse(stringValue, out dateValue))
                    return dateValue;
            }
            return value;
        }
    }
}
