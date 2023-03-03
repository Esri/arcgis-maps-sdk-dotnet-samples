using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGIS.WinUI.Viewer.Converters
{
    internal class DirectionPointTypeToIconConverter : IValueConverter
    {

        // Create a dictionary of direction point type and corresponding icons.
        private Dictionary<int, string> _directionIcons = new Dictionary<int, string>()
        {
            {51, "&#xe259;"}, {52, "calcite_glyphicon_Straight"}, {301, "calcite_glyphicon_ForkRight"},
            {200, "calcite_glyphicon_ForkLeft"}, {305, "calcite_glyphicon_Right"}, {205, "calcite_glyphicon_Left"},
            {50, "calcite_glyphicon_RouteTo"}
        };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return _directionIcons[(int)value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
