using System;
using System.Windows.Data;

namespace ArcGIS.WPF.Viewer.Converters
{
    // A converter class which negates a bool, i.e. true -> false.
    internal class BoolNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
