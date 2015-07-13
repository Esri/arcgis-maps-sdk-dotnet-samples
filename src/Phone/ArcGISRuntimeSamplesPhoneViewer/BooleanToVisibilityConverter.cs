using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ArcGISRuntime.Samples.PhoneViewer
{
    /// <summary>Convert between boolean and visibility</summary>
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>Convert bool or bool? to Visibility</summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool bValue = false;
            if (value is bool)
            {
                bValue = (bool)value;
            }
            else if (value is Nullable<bool>)
            {
                Nullable<bool> tmp = (Nullable<bool>)value;
                bValue = tmp.HasValue ? tmp.Value : false;
            }
            return (bValue) ? Visibility.Visible : Visibility.Collapsed;
        }
 
        /// <summary>Convert Visibility to boolean</summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility)
            {
                return (Visibility)value == Visibility.Visible;
            }
            else
            {
                return false;
            }
        }
    }
}
