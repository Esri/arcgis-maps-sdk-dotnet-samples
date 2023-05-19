using System.Globalization;

namespace ArcGIS.Converters
{
    // Value converter class to return a color for the current load status
    public class LoadStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check the provided load status value.
            switch ((int)value)
            {
                // Green for loaded, red for not loaded (or failure to load), gray if still loading.
                case (int)Esri.ArcGISRuntime.LoadStatus.Loaded:
                    return Colors.Green;

                case (int)Esri.ArcGISRuntime.LoadStatus.Loading:
                    return Colors.Gray;

                case (int)Esri.ArcGISRuntime.LoadStatus.FailedToLoad:
                    return Colors.Red;

                case (int)Esri.ArcGISRuntime.LoadStatus.NotLoaded:
                    return Colors.Red;
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // No need to convert the other way.
            throw new NotImplementedException();
        }
    }
}
