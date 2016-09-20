using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TokenSecuredKnownUser
{
    class LoadStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Color statusColor;

            switch ((int)value)
            {
                case (int)Esri.ArcGISRuntime.LoadStatus.Loaded:
                    statusColor = Colors.Green;
                    break;
                case (int)Esri.ArcGISRuntime.LoadStatus.Loading:
                    statusColor = Colors.Gray;
                    break;
                case (int)Esri.ArcGISRuntime.LoadStatus.FailedToLoad:
                    statusColor = Colors.Red;
                    break;
                case (int)Esri.ArcGISRuntime.LoadStatus.NotLoaded:
                    statusColor = Colors.Red;
                    break;
                default:
                    statusColor = Colors.Gray;
                    break;
            }

            return new SolidColorBrush(statusColor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
