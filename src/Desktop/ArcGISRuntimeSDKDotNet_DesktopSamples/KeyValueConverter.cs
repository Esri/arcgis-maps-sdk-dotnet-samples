using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples
{
    public class KeyValueConverter : IValueConverter
    {
        private bool AreEqual(object firstObject, object secondObject)
        {
            if (firstObject is short)
            {
                var first = (short)firstObject;
                var second = System.Convert.ToInt16(secondObject, CultureInfo.InvariantCulture);
                return Int16.Equals(first, second);
            }
            if (firstObject is int)
            {
                var first = (int)firstObject;
                var second = System.Convert.ToInt32(secondObject, CultureInfo.InvariantCulture);
                return Int32.Equals(first, second);
            }
            if (firstObject is string)
            {
                var first = (string)firstObject;
                var second = System.Convert.ToString(secondObject, CultureInfo.InvariantCulture);
                return string.Equals(first, second);
            }
            return object.Equals(firstObject, secondObject);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is IEnumerable<KeyValuePair<object, string>>)
            {
                var lookup = (IEnumerable<KeyValuePair<object, string>>)parameter;
                return lookup.FirstOrDefault(item => AreEqual(value, item.Key));
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is KeyValuePair<object, string>)
            {
                var kvp = (KeyValuePair<object, string>)value;
                return kvp.Key;
            }
            return value;
        }
    }
}
