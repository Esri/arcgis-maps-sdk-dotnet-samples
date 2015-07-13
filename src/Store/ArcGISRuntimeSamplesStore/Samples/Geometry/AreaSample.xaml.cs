using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates how to calculate the area and perimeter of a polygon using the GeometryEngine.
    /// </summary>
    /// <title>Area</title>
    /// <category>Geometry</category>
    public sealed partial class AreaSample : Page
    {
        private const double toMilesConversion = 0.0006213700922;
        private const double toSqMilesConversion = 0.0000003861003;

		private GraphicsOverlay _graphicsOverlay;

		public AreaSample()
		{
			InitializeComponent();

			_graphicsOverlay = MyMapView.GraphicsOverlays["AreaOverlay"];
			MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
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
				//Wait for user to draw
				var geom = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polygon);

				//show geometry on map
				_graphicsOverlay.Graphics.Clear();

				var g = new Graphic 
				{
					Geometry = geom, 
					Symbol = LayoutRoot.Resources["DefaultFillSymbol"] as Esri.ArcGISRuntime.Symbology.Symbol 
				};
				_graphicsOverlay.Graphics.Add(g);

				//Calculate results
				var areaPlanar = GeometryEngine.Area(geom);
				ResultsAreaPlanar.Text = string.Format("{0} sq. miles", (areaPlanar * toSqMilesConversion).ToString("n3"));

				var perimPlanar = GeometryEngine.Length(geom);
				ResultsPerimeterPlanar.Text = string.Format("{0} miles", (perimPlanar * toMilesConversion).ToString("n3"));

				var areaGeodesic = GeometryEngine.GeodesicArea(geom);
				ResultsAreaGeodesic.Text = string.Format("{0} sq. miles", (areaGeodesic * toSqMilesConversion).ToString("n3"));

				var perimGeodesic = GeometryEngine.GeodesicLength(geom);
				ResultsPerimeterGeodesic.Text = string.Format("{0} miles", (perimGeodesic * toMilesConversion).ToString("n3"));

				Instructions.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
				Results.Visibility = Windows.UI.Xaml.Visibility.Visible;
			}
			catch (TaskCanceledException)
			{
				var _x = new MessageDialog("Current sketch has been canceled.", "Task Canceled!").ShowAsync();
			}
		}

		private void ResetUI()
		{
			_graphicsOverlay.Graphics.Clear();
            Instructions.Visibility = Visibility.Visible;
            Results.Visibility = Visibility.Collapsed;
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
    }
}
