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
                if (value != null && File.Exists(value.ToString()))
                {
                    ImageSource image;
                    image = ImageSource.FromFile(value.ToString());

                    if (image != null)
                    {
                        return image;
                    };
                }
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
