﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI;

namespace ArcGISRuntime.Converters
{
    class ItemToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
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

            return new BitmapImage();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
