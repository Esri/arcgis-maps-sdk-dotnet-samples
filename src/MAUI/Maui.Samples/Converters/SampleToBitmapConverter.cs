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
                platform = "_windows";
#elif ANDROID
                platform = "_android";
#elif MACCATALYST
                platform = "_maccatalyst";
#elif IOS
                platform = "_ios";
#endif

                ImageSource image = null;

#if !ANDROID
                if (value != null && File.Exists(value.ToString()))
                {
                    image = ImageSource.FromFile(value.ToString());
                }
                else
                {
                    image = ImageSource.FromFile($@"Resources\Thumbnails\placeholder{platform}.jpg");

                }
#else
                string imagePath = value.ToString();
                List<string> folders = imagePath.Split('/').ToList();
                string fileName = folders.Last();

                var assembly = Assembly.GetExecutingAssembly();
                string imageResource = assembly.GetManifestResourceNames().Single(n => n.EndsWith(fileName));
                var sourceStream = assembly.GetManifestResourceStream(imageResource);
                var memoryStream = new MemoryStream();
                sourceStream.CopyTo(memoryStream);
                byte[] bytes = memoryStream.ToArray();
                memoryStream.Close();

                if (value != null)
                {
                    image = ImageSource.FromStream(() =>
                    { 
                        return new MemoryStream(bytes);
                    });
                }
                else
                {
                    image = ImageSource.FromFile($@"Resources/Thumbnails/placeholder{platform}.jpg");
                }
#endif
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
