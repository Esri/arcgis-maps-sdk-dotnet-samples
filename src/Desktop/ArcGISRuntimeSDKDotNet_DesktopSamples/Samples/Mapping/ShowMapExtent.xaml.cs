using Esri.ArcGISRuntime.Geometry;
using System;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample includes a Map and a ArcGISTiledMapServiceLayer and demonstrates how to handle the ExtentChanged event on the MapView.  The extent parameters of the map are displayed in a textblock as they change at runtime.  Since the Map is in wrap-around mode, absolute values represent the map extent on an infinite continuous grid where values on the X axis increase or decrease as you pan.  Normalized values represent the map extent in the real-world and take the dateline (or current central meridian) into account.
    /// </summary>
    /// <title>Show Map Extent</title>
	/// <category>Mapping</category>
	public partial class ShowMapExtent : UserControl
    {
        public ShowMapExtent()
        {
            InitializeComponent();
        }

        private void MapView_ExtentChanged(object sender, EventArgs e)
        {
            Envelope newExtent = null;

            if (mapView.WrapAround)
            {
                Geometry normalizedExtent = GeometryEngine.NormalizeCentralMeridianOfGeometry(mapView.Extent);
                if (normalizedExtent is Polygon)
                {
                    var normalizedPolygon = (Polygon)normalizedExtent;

					if (normalizedPolygon.Parts.Count == 1)
                        newExtent = normalizedPolygon.Extent;
                    else
                    {
                        newExtent = new Envelope();

						foreach (var p in normalizedPolygon.Parts[0])
                        {
                            if (p.X < newExtent.XMin || double.IsNaN(newExtent.XMin))
                                newExtent.XMin = p.X;
                            if (p.Y < newExtent.YMin || double.IsNaN(newExtent.YMin))
                                newExtent.YMin = p.Y;
                        }

						foreach (var p in normalizedPolygon.Parts[1])
                        {
                            if (p.X > newExtent.XMax || double.IsNaN(newExtent.XMax))
                                newExtent.XMax = p.X;
                            if (p.Y > newExtent.YMax || double.IsNaN(newExtent.YMax))
                                newExtent.YMax = p.Y;
                        }
                    }
                }
                else if (normalizedExtent is Envelope)
                    newExtent = normalizedExtent as Envelope;
            }
            else
                newExtent = mapView.Extent;

            MinXNormalized.Text = newExtent.XMin.ToString("0.000");
            MinYNormalized.Text = newExtent.YMin.ToString("0.000");
            MaxXNormalized.Text = newExtent.XMax.ToString("0.000");
            MaxYNormalized.Text = newExtent.YMax.ToString("0.000");
        }
    }
}
