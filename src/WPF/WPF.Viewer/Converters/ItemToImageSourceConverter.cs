using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ArcGIS.WPF.Viewer.Converters
{
    internal class ItemToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Item mapItem = value as Item;
            if (mapItem != null)
            {
                if (mapItem.ThumbnailUri != null)
                {
                    // Sometimes image URIs have a . appended to them... BitmapImage doesn't like that.
                    return new BitmapImage(new Uri(mapItem.ThumbnailUri.OriginalString.TrimEnd('.')));
                }

                if (mapItem.Thumbnail != null &&
                    mapItem.Thumbnail.LoadStatus == LoadStatus.Loaded &&
                    mapItem.Thumbnail.Width > 0)
                {
                    return mapItem.Thumbnail.ToImageSourceAsync().Result;
                }
            }

            return new BitmapImage(new Uri("/Resources/files-48.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}