﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGIS.Converters
{
    internal class DirectionPointTypeToIconConverter : IValueConverter
    {
        // Create a dictionary of direction point type and corresponding icons.
        private Dictionary<int, string> _directionIcons = new()
        {
            {51, "\xe259"}, {52, "\xe2aa"}, {301, "\xe124"}, {200, "\xe122"}, {305, "\xe24e"},
            {205, "\xe199"}, {50, "\xe25a"}
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
