using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using Esri.Calcite.WPF;

namespace ArcGIS.WPF.Viewer.Converters
{
    internal class DirectionPointTypeToIconConverter : IValueConverter
    {
        // Create a dictionary of direction point type and corresponding icons.
        private Dictionary<int, char> _directionIcons = new Dictionary<int, char>
        {
            {51, (char)CalciteIcon.RouteFrom}, {52, (char)CalciteIcon.Straight}, {301, (char)CalciteIcon.ForkRight}, {200, (char)CalciteIcon.ForkLeft}, {305, (char)CalciteIcon.Right},
            {205, (char)CalciteIcon.Left}, {50, (char)CalciteIcon.RouteTo}
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
