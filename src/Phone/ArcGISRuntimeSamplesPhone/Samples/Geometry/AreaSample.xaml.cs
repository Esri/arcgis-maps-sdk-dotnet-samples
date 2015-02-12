using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates how to calculate the area and perimeter of a polygon using the GeometryEngine.
	/// </summary>
	/// <title>Area</title>
	/// <category>Geometry</category>
	public sealed partial class AreaSample : Page
	{
		private const double toMilesConversion = 0.0006213700922;
		private const double toSqMilesConversion = 0.0000003861003;

		GraphicsOverlay _graphicsOverlay;
		public AreaSample()
		{
			InitializeComponent();

			MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-130, 20, -65, 55, SpatialReferences.Wgs84));
			_graphicsOverlay = MyMapView.GraphicsOverlays["Graphics"] as GraphicsOverlay;

			MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
		}

		async void MyMapView_SpatialReferenceChanged(object sender, System.EventArgs e)
		{
			MyMapView.SpatialReferenceChanged -= MyMapView_SpatialReferenceChanged;

			await doCalculateAreaAndLength();
		}

	 
		private async Task doCalculateAreaAndLength()
		{
			try
			{
				//Wait for user to draw
				var geom = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polygon);

				//show geometry on map
				_graphicsOverlay.Graphics.Clear();

				var g = new Graphic { Geometry = geom, Symbol = LayoutRoot.Resources["DefaultFillSymbol"] as Esri.ArcGISRuntime.Symbology.Symbol };
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
			catch (System.Threading.Tasks.TaskCanceledException)
			{
				var dlg = new MessageDialog("Current sketch has been canceled.", "Task Canceled!");
				var _x = dlg.ShowAsync();
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
			await doCalculateAreaAndLength();
 
		}

		private async void RestartButton_Click(object sender, RoutedEventArgs e)
		{
			ResetUI();
			await doCalculateAreaAndLength();
		}
	}
}
