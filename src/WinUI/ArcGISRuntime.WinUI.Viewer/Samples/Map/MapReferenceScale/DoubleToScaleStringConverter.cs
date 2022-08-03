using System;
using Microsoft.UI.Xaml.Data;

namespace ArcGISRuntime.WinUI.Samples.MapReferenceScale
{
    class DoubleToScaleStringConverter : IValueConverter
    {
        // Converts a double to a string
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return $"1:{value:n0}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}