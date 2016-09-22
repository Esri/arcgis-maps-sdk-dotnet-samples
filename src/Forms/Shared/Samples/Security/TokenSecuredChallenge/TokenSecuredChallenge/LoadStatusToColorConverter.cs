using System;
using System.Globalization;
using Xamarin.Forms;

namespace TokenSecuredChallenge
{
    // Value converter class to return a color for the current load status
    // Note: to make this class accessible as a static resource in the shared form (TokenChallengePage.xaml)
    //       the assembly name for each platform had to be changed to the same value ("TokenChallengeForms")
    //       in order to provide a consistent XML namespace value. Another option would be to place such code in
    //       a PCL project rather than a shared project (the shared project would still be needed for the ArcGIS 
    //       Runtime code).
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
                case (int)Esri.ArcGISRuntime.LoadStatus.Loaded:
                    statusColor = Color.Green;
                    break;
                case (int)Esri.ArcGISRuntime.LoadStatus.Loading:
                    statusColor = Color.Gray;
                    break;
                case (int)Esri.ArcGISRuntime.LoadStatus.FailedToLoad:
                    statusColor = Color.Red;
                    break;
                case (int)Esri.ArcGISRuntime.LoadStatus.NotLoaded:
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
