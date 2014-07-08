using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Mapping</category>
	public sealed partial class LayerList : Page
    {
        public LayerList()
        {
            this.InitializeComponent();

			mapView1.Map.InitialViewpoint = new Envelope(-13279585.9811197, 4010136.34579502,
                -12786146.5545795, 4280849.94238526, SpatialReferences.WebMercator);
        }

        private void RemoveLayerButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = (sender as FrameworkElement).DataContext as Layer;
            mapView1.Map.Layers.Remove(layer);
        }
    }

    // Value converter that translates true to <see cref="Visibility.Visible"/> and false to <see cref="Visibility.Collapsed"/>.
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(Visibility))
                return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
            else if (targetType == typeof(bool))
                return (value is Visibility && ((Visibility)value == Visibility.Visible)) ? true : false;
            else return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool) //convert to visibility
                return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
            else //convert to bool
                return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
