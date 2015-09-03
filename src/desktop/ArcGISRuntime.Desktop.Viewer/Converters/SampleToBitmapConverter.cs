using ArcGISRuntime.Desktop.Viewer.Managers;
using System;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Globalization;

namespace ArcGISRuntime.Desktop.Viewer.Converters
{
    public class SampleToBitmapConverter : IValueConverter  
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sampleImageName = value.ToString();

            //var path = Path.Combine(
            //    ApplicationManager.Current.FullDirectoryName,
            //    sampleImageName);

            try
            {
                if (File.Exists(sampleImageName))
                    return BitmapFrame.Create(
                        new Uri(sampleImageName, UriKind.Absolute),
                        BitmapCreateOptions.DelayCreation,
                        BitmapCacheOption.OnLoad);
            }
            catch (Exception)
            {
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
