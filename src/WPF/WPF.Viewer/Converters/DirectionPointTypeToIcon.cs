using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ArcGIS.Converters
{
    internal class DirectionPointTypeToIconConverter : IValueConverter
    {
        // Create a dictionary of direction point type IDs and the corresponding file.
        private Dictionary<int, string> _directionIcons = new Dictionary<int, string>()
        {
            {51, "startpoint.png"}, {52, "straight.png"}, {301, "forkright.png"}, {200, "forkleft.png"}, {305, "right.png"}, {205, "left"},
            {50, "endpoint.png"}
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _directionIcons[(int)value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
