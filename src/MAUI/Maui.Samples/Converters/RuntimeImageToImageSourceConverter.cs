using System.Globalization;
using Esri.ArcGISRuntime.UI;
using Microsoft.Maui.Controls;

namespace ArcGIS.Converters;

internal class RuntimeImageToImageSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is RuntimeImage runtimeImage)
        {
            return Esri.ArcGISRuntime.Maui.RuntimeImageExtensions.ToImageSource(runtimeImage);
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
