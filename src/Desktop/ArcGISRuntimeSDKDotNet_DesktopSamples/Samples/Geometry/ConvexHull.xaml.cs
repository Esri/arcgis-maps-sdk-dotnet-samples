using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates use of the GeometryEngine.ConvexHull method to create a convex hull polygon for three or more points.  A convex hull is the smallest polygon that completely encases a set (i.e. locus) of points. To use the sample, add three or more points on the map and click the Generate Convex Hull button. A polygon containing all the points entered will be returned.
    /// </summary>
    /// <title>Convex Hull</title>
	/// <category>Geometry</category>
	public partial class ConvexHull : UserControl
    {
        private Symbol _pointSymbol;
        private Symbol _polygonSymbol;

        /// <summary>Construct Convex Hull sample control</summary>
        public ConvexHull()
        {
            InitializeComponent();

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
                    if (convexHullGraphics.Graphics.Count > 0)
                    {
                        inputGraphics.Graphics.Clear();
                        convexHullGraphics.Graphics.Clear();
                    }

                    inputGraphics.Graphics.Add(new Graphic(point, _pointSymbol));

                    if (inputGraphics.Graphics.Count > 2)
                        btnConvexHull.IsEnabled = true;
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding points: " + ex.Message, "Convex Hull Sample");
            }
        }

        // Creates a convex hull polygon from the input point graphics
        private void ConvexHullButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var convexHull = GeometryEngine.ConvexHull(inputGraphics.Graphics.Select(g => g.Geometry));
                convexHullGraphics.Graphics.Add(new Graphic(convexHull, _polygonSymbol));

                btnConvexHull.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error calculating convex hull: " + ex.Message, "Convex Hull Sample");
            }
        }
    }
}
