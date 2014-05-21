using Esri.ArcGISRuntime.Geometry;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Shows how to handle the ExtentChanged event on the MapView and display extent properties at runtime.
    /// Since the Map is in wrap-around mode, absolute values represent the map extent on an infinite continuous grid where values on the X axis increase or decrease as you pan.
    /// Normalized values represent the map extent in the real-world and take the dateline (or current central meridian) into account.
    /// </summary>
    /// <title>Show Map Extent</title>
    /// <category>Mapping</category>
	public sealed partial class ShowMapExtent : Page
    {
        public ShowMapExtent()
        {
            this.InitializeComponent();

            mapView.ExtentChanged += MapView_ExtentChanged;
        }

        private void MapView_ExtentChanged(object sender, System.EventArgs e)
        {
            Envelope newExtent = null;

            if (mapView.WrapAround)
            {
                Geometry normalizedExtent = GeometryEngine.NormalizeCentralMeridianOfGeometry(mapView.Extent);
                if (normalizedExtent is Polygon)
                {
                    var normalizedPolygon = (Polygon)normalizedExtent;

                    if (normalizedPolygon.Rings.Count == 1)
                        newExtent = normalizedPolygon.Extent;
                    else
                    {
                        newExtent = new Envelope();

                        foreach (var p in normalizedPolygon.Rings[0])
                        {
                            if (p.X < newExtent.XMin || double.IsNaN(newExtent.XMin))
                                newExtent.XMin = p.X;
                            if (p.Y < newExtent.YMin || double.IsNaN(newExtent.YMin))
                                newExtent.YMin = p.Y;
                        }

                        foreach (var p in normalizedPolygon.Rings[1])
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

            MinXAbsolute.Text = mapView.Extent.XMin.ToString("0.000");
            MinYAbsolute.Text = mapView.Extent.YMin.ToString("0.000");
            MaxXAbsolute.Text = mapView.Extent.XMax.ToString("0.000");
            MaxYAbsolute.Text = mapView.Extent.YMax.ToString("0.000");
        }
    }
}
