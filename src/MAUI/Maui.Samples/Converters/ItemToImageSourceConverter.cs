﻿using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Portal;
using System.Globalization;

namespace ArcGIS.Converters
{
    internal class ItemToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Item mapItem = value as Item;
            if (mapItem?.ThumbnailUri != null)
            {
                // Sometimes image URIs have a . appended to them...
                return ImageSource.FromUri(new Uri(mapItem.ThumbnailUri.OriginalString.TrimEnd('.')));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}