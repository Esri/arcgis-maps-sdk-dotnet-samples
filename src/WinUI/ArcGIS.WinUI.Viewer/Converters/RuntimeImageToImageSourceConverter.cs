using Esri.ArcGISRuntime.UI;
using Microsoft.UI.Xaml.Data;
using System;

namespace ArcGIS.WinUI.Viewer.Converters
{
    internal class RuntimeImageToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is RuntimeImage runtimeImage)
            {
                return runtimeImage.ToImageSource();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
