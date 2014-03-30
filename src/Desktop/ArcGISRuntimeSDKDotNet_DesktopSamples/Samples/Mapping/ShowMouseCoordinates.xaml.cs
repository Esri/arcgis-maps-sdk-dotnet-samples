using Esri.ArcGISRuntime.Geometry;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample includes a Map and a single ArcGIS Server layer. MouseMove events on the Map are handled to return the mouse cursor location over the map. The location is displayed in pixels and map units.
    /// </summary>
    /// <title>Show Mouse Coordinates</title>
	/// <category>Mapping</category>
	public partial class ShowMouseCoordinates : UserControl
    {
        public ShowMouseCoordinates()
        {
            InitializeComponent();
        }

        private void mapView_MouseMove(object sender, MouseEventArgs e)
        {
            if (mapView.Extent == null)
                return;

            System.Windows.Point screenPoint = e.GetPosition(mapView);
            ScreenCoordsTextBlock.Text = string.Format("Screen Coords: X = {0}, Y = {1}",
                screenPoint.X, screenPoint.Y);

            MapPoint mapPoint = mapView.ScreenToLocation(screenPoint);
            if (mapView.WrapAround)
                mapPoint = GeometryEngine.NormalizeCentralMeridianOfGeometry(mapPoint) as MapPoint;
            MapCoordsTextBlock.Text = string.Format("Map Coords: X = {0}, Y = {1}",
                    Math.Round(mapPoint.X, 4), Math.Round(mapPoint.Y, 4));
        }
    }
}
