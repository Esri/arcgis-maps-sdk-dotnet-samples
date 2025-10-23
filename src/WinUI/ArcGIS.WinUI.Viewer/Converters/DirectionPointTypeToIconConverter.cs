using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using Esri.Calcite.WinUI;

namespace ArcGIS.WinUI.Viewer.Converters
{
    internal class DirectionPointTypeToIconConverter : IValueConverter
    {
        // Create a dictionary of direction point type and corresponding icons.
        private Dictionary<int, char> _directionIcons = new()
        {
            {51, (char)CalciteIcon.RouteFrom}, {52, (char)CalciteIcon.Straight}, {301, (char)CalciteIcon.ForkRight}, {200, (char)CalciteIcon.ForkLeft}, {305, (char)CalciteIcon.Right},
            {205, (char)CalciteIcon.Left}, {50, (char)CalciteIcon.RouteTo}
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