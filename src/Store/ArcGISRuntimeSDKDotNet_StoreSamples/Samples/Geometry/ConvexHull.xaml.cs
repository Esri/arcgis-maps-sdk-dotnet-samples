using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates use of the GeometryEngine.ConvexHull method to create a convex hull polygon for three or more points.
    /// A convex hull is the smallest polygon that completely encases a set (i.e. locus) of points.
    /// </summary>
    /// <title>Convex Hull</title>
	/// <category>Geometry</category>
	public partial class ConvexHull : Windows.UI.Xaml.Controls.Page
    {
        private GraphicsLayer _inputGraphics;
        private GraphicsLayer _convexHullGraphics;

        private Symbol _pointSymbol;
        private Symbol _polygonSymbol;

        /// <summary>Construct Convex Hull sample control</summary>
        public ConvexHull()
        {
            InitializeComponent();

            _inputGraphics = mapView.Map.Layers["InputGraphics"] as GraphicsLayer;
            _convexHullGraphics = mapView.Map.Layers["ConvexHullGraphics"] as GraphicsLayer;
            _pointSymbol = (Symbol)layoutGrid.Resources["PointSymbol"];
            _polygonSymbol = (Symbol)layoutGrid.Resources["ConvexHullSymbol"];

            DrawPoints();
        }

        // Continuosly accepts new points from the user
        private async void DrawPoints()
        {
            try
            {
                await mapView.LayersLoadedAsync();

                while (mapView.Extent != null)
                {
                    var point = await mapView.Editor.RequestPointAsync();

                    // reset graphics layers if we've already created a convex hull polygon
                    if (_convexHullGraphics.Graphics.Count > 0)
                    {
                        _inputGraphics.Graphics.Clear();
                        _convexHullGraphics.Graphics.Clear();
                    }

                    _inputGraphics.Graphics.Add(new Graphic(point, _pointSymbol));

                    if (_inputGraphics.Graphics.Count > 2)
                        btnConvexHull.IsEnabled = true;
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog("Error adding points: " + ex.Message, "Sample Error").ShowAsync();
            }
        }

        // Creates a convex hull polygon from the input point graphics
        private void ConvexHullButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var convexHull = GeometryEngine.ConvexHull(_inputGraphics.Graphics.Select(g => g.Geometry));
                _convexHullGraphics.Graphics.Add(new Graphic(convexHull, _polygonSymbol));

                btnConvexHull.IsEnabled = false;
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog("Error calculating convex hull: " + ex.Message, "Sample Error").ShowAsync();
            }
        }
    }
}
