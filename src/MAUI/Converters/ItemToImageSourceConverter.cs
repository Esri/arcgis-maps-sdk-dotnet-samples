using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Portal;
using System;
using System.Globalization;

namespace ArcGISRuntimeMaui.Converters
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
                    // Sometimes image URIs have a . appended to them...
                    return ImageSource.FromUri(new Uri(mapItem.ThumbnailUri.OriginalString.TrimEnd('.')));
                }

                if (mapItem.Thumbnail != null &&
                    mapItem.Thumbnail.LoadStatus == LoadStatus.Loaded &&
                    mapItem.Thumbnail.Width > 0)
                {
                    return ImageSource.FromStream(() => mapItem.Thumbnail.GetEncodedBufferAsync().Result);
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
