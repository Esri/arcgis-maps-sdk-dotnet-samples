using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI;

namespace ArcGISRuntime.Converters
{
    class ItemToImageSourceConverter : IValueConverter
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
