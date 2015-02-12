using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates use of the GeometryEngine.ConvexHull method to create a convex hull polygon for three or more points.  A convex hull is the smallest polygon that completely encases a set (i.e. locus) of points. To use the sample, add three or more points on the map and click the Generate Convex Hull button. A polygon containing all the points entered will be returned.
    /// </summary>
    /// <title>Convex Hull</title>
	/// <category>Geometry</category>
	public partial class ConvexHull : UserControl
    {
		private GraphicsOverlay _inputGraphicsOverlay;
		private GraphicsOverlay _convexHullGraphicsOverlay;
        private Symbol _pointSymbol;
        private Symbol _polygonSymbol;

        /// <summary>Construct Convex Hull sample control</summary>
        public ConvexHull()
        {
            InitializeComponent();

			_inputGraphicsOverlay = MyMapView.GraphicsOverlays["inputGraphicsOverlay"];
			_convexHullGraphicsOverlay = MyMapView.GraphicsOverlays["convexHullGraphicsOverlay"];
            _pointSymbol = (Symbol)layoutGrid.Resources["PointSymbol"];
            _polygonSymbol = (Symbol)layoutGrid.Resources["ConvexHullSymbol"];

			MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
        }

		void MyMapView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			var x = DrawPoints();
		}

        // Continuosly accepts new points from the user
		private async Task DrawPoints()
        {
			try
			{
				await MyMapView.LayersLoadedAsync();

				var point = await MyMapView.Editor.RequestPointAsync();

				// reset graphics layers if we've already created a convex hull polygon
				if (_convexHullGraphicsOverlay.Graphics.Count > 0)
				{
					_inputGraphicsOverlay.Graphics.Clear();
					_convexHullGraphicsOverlay.Graphics.Clear();
				}

				_inputGraphicsOverlay.Graphics.Add(new Graphic(point, _pointSymbol));

				if (_inputGraphicsOverlay.Graphics.Count > 2)
					btnConvexHull.IsEnabled = true;
					
				await DrawPoints();
			}
			catch (TaskCanceledException) { }
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
                var convexHull = GeometryEngine.ConvexHull(_inputGraphicsOverlay.Graphics.Select(g => g.Geometry));
				_convexHullGraphicsOverlay.Graphics.Add(new Graphic(convexHull, _polygonSymbol));

                btnConvexHull.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error calculating convex hull: " + ex.Message, "Convex Hull Sample");
            }
        }
    }
}
