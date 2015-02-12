using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates using the GeometryEngine.DistanceFromGeometry method to calculate the linear distance of the shortest length between two geometries.
    /// </summary>
    /// <title>Distance From Geometry</title>
    /// <category>Geometry</category>
    public partial class DistanceFromGeometry : Windows.UI.Xaml.Controls.Page
    {
        private const double METERS_TO_MILES = 0.0006213700922;

        private Symbol _lineSymbol;
        private Symbol _pointSymbol;
        private GraphicsOverlay _graphicsOverlay;

        /// <summary>Construct Distance From Geometry sample control</summary>
        public DistanceFromGeometry()
        {
            InitializeComponent();

            _lineSymbol = LayoutRoot.Resources["LineSymbol"] as Symbol;
            _pointSymbol = LayoutRoot.Resources["PointSymbol"] as Symbol;
			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
        }

        // Calculates the linear distance between two user-defined geometries
        private async void DistanceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtResults.Visibility = Visibility.Collapsed;
                _graphicsOverlay.Graphics.Clear();

                // wait for user to draw a polyline
                var line = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polyline, _lineSymbol);
                _graphicsOverlay.Graphics.Add(new Graphic(line, _lineSymbol));

                // wait for user to draw a point
                var point = await MyMapView.Editor.RequestPointAsync();
                _graphicsOverlay.Graphics.Add(new Graphic(point, _pointSymbol));

                // Calculate distance between line and point
                double distance = GeometryEngine.Distance(line, point) * METERS_TO_MILES;
                txtResults.Text = string.Format("Distance between geometries: {0:0.000} miles", distance);
                txtResults.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog("Distance Calculation Error: " + ex.Message, "Sample Error").ShowAsync();
                txtResults.Visibility = Visibility.Collapsed;
                _graphicsOverlay.Graphics.Clear();
            }
        }
    }
}
