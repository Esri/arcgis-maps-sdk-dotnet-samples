using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Common
{
    public class StringToShortConverter : IValueConverter
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
                Int16 shortValue = Int16.MinValue;
                if (Int16.TryParse(stringValue, out shortValue))
                    return shortValue;
            }
            return value;
        }
    }
}
