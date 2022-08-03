using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections;

namespace ArcGISRuntime.WinUI.Samples.MobileMapSearchAndRoute
{
    public class NullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null || String.IsNullOrEmpty(value.ToString()))
                return Visibility.Collapsed;

            IList listValue = value as IList;
            if (listValue != null && listValue.Count > 0)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}