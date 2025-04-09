using System.Windows.Data;
using System.Windows;

namespace ArcGIS.WPF.Viewer.Converters
{
    internal class BoolToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool? isVisible = value as bool?;
            if (parameter != null && isVisible.HasValue)
                isVisible = !isVisible;
            if (isVisible.HasValue && isVisible.Value == true)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
