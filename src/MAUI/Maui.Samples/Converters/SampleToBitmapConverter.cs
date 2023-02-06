using System.Globalization;
using System.Reflection;

namespace ArcGIS.Converters
{
    public class SampleToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string platform = string.Empty;
#if WINDOWS
                platform = "_Windows";
#elif ANDROID
                platform = "_Android";
#elif MACCATALYST
                platform = "_MacCatalyst";
#elif IOS
                platform = "_iOS";
#endif

                ImageSource image = null;

                if (value != null && File.Exists(value.ToString() + platform))
                {
                    image = ImageSource.FromFile(value.ToString() + platform);
                }
                else
                {
                    image = ImageSource.FromFile($@"Resources\Thumbnails\Placeholder{platform}.jpg");
                }

                if (image != null)
                {
                    return image;
                };
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
