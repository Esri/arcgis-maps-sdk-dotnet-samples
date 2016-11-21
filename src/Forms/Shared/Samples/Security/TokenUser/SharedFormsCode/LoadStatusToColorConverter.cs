using System;
using System.Globalization;
using Xamarin.Forms;

namespace SharedFormsCode
{
    // Value converter class to return a color for the current load status
    // Note: to make this class accessible as a static resource in the shared form (TokenKnownUserPage.xaml)
    //       this class was added to a PCL project rather than the shared project (the shared project is still 
    //       needed for the shared ArcGIS Runtime code).
    public class LoadStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Default to gray as the text color
            Color statusColor = Color.Gray;

            // Check the provided load status value
            switch ((int)value)
            {
                // Green for loaded
                // Red for not loaded or failure to load
                // Gray if still loading
                case 0: // == Esri.ArcGISRuntime.LoadStatus.Loaded
                    statusColor = Color.Green;
                    break;
                case 1: // == Esri.ArcGISRuntime.LoadStatus.Loading
                    statusColor = Color.Gray;
                    break;
                case 2: // == Esri.ArcGISRuntime.LoadStatus.FailedToLoad
                    statusColor = Color.Red;
                    break;
                case 3: // == Esri.ArcGISRuntime.LoadStatus.NotLoaded
                    statusColor = Color.Red;
                    break;
            }

            return statusColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // No need to convert the other way
            throw new NotImplementedException();
        }

    }
}
