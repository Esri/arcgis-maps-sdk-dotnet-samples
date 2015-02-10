using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates how to calculate the area and perimeter of a polygon using the GeometryEngine.
    /// </summary>
    /// <title>Area</title>
	/// <category>Geometry</category>
	public partial class AreaSample : UserControl
    {
        private const double toMilesConversion = 0.0006213700922;
        private const double toSqMilesConversion = 0.0000003861003;

		private GraphicsOverlay _graphicsOverlay;

        /// <summary>Construct Area sample control</summary>
        public AreaSample()
        {
            InitializeComponent();
			MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
			_graphicsOverlay = MyMapView.GraphicsOverlays["AreaOverlay"];
        }

		private async void MyMapView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			MyMapView.SpatialReferenceChanged -= MyMapView_SpatialReferenceChanged;
			await DoCalculateAreaAndLengthAsync();
		}

        private async Task DoCalculateAreaAndLengthAsync()
        {
            try
            {
                // Wait for user to draw
                var geom = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polygon);

                // show geometry on map
				_graphicsOverlay.Graphics.Clear();

                var graphic = new Graphic 
				{ 
					Geometry = geom, 
					Symbol = LayoutRoot.Resources["DefaultFillSymbol"] as Symbol 
				};
				_graphicsOverlay.Graphics.Add(graphic);

                // Calculate results
                var areaPlanar = GeometryEngine.Area(geom);
                ResultsAreaPlanar.Text = string.Format("{0} sq. miles", (areaPlanar * toSqMilesConversion).ToString("n3"));

                var perimPlanar = GeometryEngine.Length(geom);
                ResultsPerimeterPlanar.Text = string.Format("{0} miles", (perimPlanar * toMilesConversion).ToString("n3"));

                var areaGeodesic = GeometryEngine.GeodesicArea(geom);
                ResultsAreaGeodesic.Text = string.Format("{0} sq. miles", (areaGeodesic * toSqMilesConversion).ToString("n3"));

                var perimGeodesic = GeometryEngine.GeodesicLength(geom);
                ResultsPerimeterGeodesic.Text = string.Format("{0} miles", (perimGeodesic * toMilesConversion).ToString("n3"));

                Instructions.Visibility = Visibility.Collapsed;
                Results.Visibility = Visibility.Visible;
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show("Current sketch has been canceled.", "Task Canceled!");
            }
        }

        private async void CancelCurrent_Click(object sender, RoutedEventArgs e)
        {
            MyMapView.Editor.Cancel.Execute(null);
            ResetUI();
			await DoCalculateAreaAndLengthAsync();
        }

        private async void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            ResetUI();
			await DoCalculateAreaAndLengthAsync();
        }

        private void ResetUI()
        {
			_graphicsOverlay.Graphics.Clear();
            Instructions.Visibility = Visibility.Visible;
            Results.Visibility = Visibility.Collapsed;
        }
    }
}
