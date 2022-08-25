using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ArcGISRuntimeMaui.Resources
{
    /// <summary>
    /// Converts a double to a Thickness, where the <c>Top</c> is the value, all other properties set to 0.
    /// Used for binding the mapview inset to the height of the form, to ensure all map content is rendered.
    /// </summary>
    class InsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if ((double)value < 0)
                {
                    return new Thickness(0);
                }
                return new Thickness(0, (double)value, 0, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new Thickness(0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
