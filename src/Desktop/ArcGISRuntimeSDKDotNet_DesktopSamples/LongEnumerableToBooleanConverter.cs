using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples
{
    public class LongEnumerableToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<long>)
            {
                var enumerable = (IEnumerable<long>)value;
                if (parameter != null)
                {
                    var requiredCount = parameter as string;
                    if (!string.IsNullOrWhiteSpace(requiredCount))
                    {
                        int count = 0;
                        if (int.TryParse(requiredCount, out count))
                            return enumerable.Count() >= count;
                    }
                }
                return enumerable != null && enumerable.Any();
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}