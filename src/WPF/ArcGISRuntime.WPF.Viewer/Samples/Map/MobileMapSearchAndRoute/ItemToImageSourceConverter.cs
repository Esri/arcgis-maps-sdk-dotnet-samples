// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ArcGISRuntime.WPF.Samples.MobileMapSearchAndRoute
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
