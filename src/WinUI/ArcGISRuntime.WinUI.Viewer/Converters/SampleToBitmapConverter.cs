// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.IO;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace ArcGISRuntime.WinUI.Viewer.Converters
{
    public class SampleToBitmapConverter : IValueConverter  
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var sampleImageName = value.ToString();

            try
            {
                var idx = sampleImageName.IndexOf("\\ArcGISRuntime.WinUI.Viewer\\");
                sampleImageName = sampleImageName.Substring(idx + 28).Replace('\\', '/');
                return new BitmapImage(
                    new Uri(
                        string.Format("ms-appx:///{0}", sampleImageName)));
            }
            catch (Exception)
            {
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
