using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Shows mouse and device coordinates when the user taps the device.
    /// </summary>
    /// <title>Show Mouse Coordinates</title>
    /// <category>Mapping</category>
	public sealed partial class ShowMouseCoordinates : Page
    {
        public ShowMouseCoordinates()
        {
            this.InitializeComponent();
        }

        private void mapView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (mapView.Extent == null)
                return;

            var pointerPoint = e.GetCurrentPoint(mapView);

            var position = pointerPoint.Position;
            
            MapPoint location = null;
            if (!pointerPoint.IsInContact)
                location = mapView.ScreenToLocation(position);

            UpdateDisplayPoints(position, location);
        }

        private void UpdateDisplayPoints(Point position, MapPoint location)
        {
            txtScreenCoords.Text = string.Format("Screen Coords: {0:0}, {1:0}", position.X, position.Y);

            if (location != null)
            {
                MapPoint mapPoint = location;
                if (mapView.WrapAround)
                    mapPoint = GeometryEngine.NormalizeCentralMeridianOfGeometry(mapPoint) as MapPoint;
                txtMapCoords.Text = string.Format("Map Coords: {0:0.000}, {1:0.000}", mapPoint.X, mapPoint.Y);
            }
        }
    }
}
