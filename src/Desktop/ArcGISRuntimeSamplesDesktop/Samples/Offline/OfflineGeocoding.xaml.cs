using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Demonstrates performing a geocode by submitting values for multiple address fields to a local locator.
	/// </summary>
	/// <title>Geocoding</title>
	/// <category>Offline</category>
	public partial class OfflineGeocoding : UserControl
	{
		private const string LOCATOR_PATH = @"..\..\..\samples-data\locators\san-diego\san-diego-locator.loc";

		private LocalLocatorTask _locatorTask;
		private GraphicsOverlay _graphicsOverlay;

		/// <summary>Construct Offline Geocoding sample control</summary>
		public OfflineGeocoding()
		{
			InitializeComponent();

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
			SetupRendererSymbols();
		}

		// Setup marker symbol and renderer
		private async void SetupRendererSymbols()
		{
			try
			{
				var markerSymbol = new PictureMarkerSymbol() { Width = 48, Height = 48, YOffset = 24 };
				await markerSymbol.SetSourceAsync(
					new Uri("pack://application:,,,/ArcGISRuntimeSamplesDesktop;component/Assets/RedStickpin.png"));
				_graphicsOverlay.Renderer = new SimpleRenderer() { Symbol = markerSymbol, };
			}
			catch(Exception ex)
			{
				MessageBox.Show("Error occurred : " + ex.Message, "Sample error");
			}
		}

		// Geocode input address and add result graphics to the map
		private async void FindButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				progress.Visibility = Visibility.Visible;
				listResults.Visibility = Visibility.Collapsed;
				_graphicsOverlay.GraphicsSource = null;

				// Street, City, State, ZIP
				Dictionary<string, string> address = new Dictionary<string, string>();
				if (!string.IsNullOrEmpty(InputAddress.Text))
					address.Add("Street", InputAddress.Text);
				if (!string.IsNullOrEmpty(City.Text))
					address.Add("City", City.Text);
				if (!string.IsNullOrEmpty(State.Text))
					address.Add("State", State.Text);
				if (!string.IsNullOrEmpty(Zip.Text))
					address.Add("ZIP", Zip.Text);

				if (_locatorTask == null)
					_locatorTask = await Task.Run<LocalLocatorTask>(() => new LocalLocatorTask(LOCATOR_PATH));

				var candidateResults = await _locatorTask.GeocodeAsync(
					address, new List<string> { "Match_addr" }, MyMapView.SpatialReference, CancellationToken.None);

				_graphicsOverlay.GraphicsSource = candidateResults
					.Select(result => new Graphic(result.Location, new Dictionary<string, object> { { "Locator", result } }));

				await MyMapView.SetViewAsync(ExtentFromGraphics().Expand(2));
			}
			catch (AggregateException ex)
			{
				var innermostExceptions = ex.Flatten().InnerExceptions;
				if (innermostExceptions != null && innermostExceptions.Count > 0)
					MessageBox.Show(string.Join(" > ", innermostExceptions.Select(i => i.Message).ToArray()));
				else
					MessageBox.Show(ex.Message);
			}
			catch (System.Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
				progress.Visibility = Visibility.Collapsed;
				if (_graphicsOverlay.GraphicsSource != null)
					listResults.Visibility = Visibility.Visible;
			}
		}

		// Helper method to retrieve an extent from graphics in the graphics layer
		private Envelope ExtentFromGraphics()
		{
			var graphics = _graphicsOverlay.GraphicsSource;
			if (graphics == null || graphics.Count() == 0)
				return MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry.Extent;

			var extent = graphics.First().Geometry.Extent;
			foreach (var graphic in graphics)
			{
				if (graphic == null || graphic.Geometry == null)
					continue;
				extent = extent.Union(graphic.Geometry.Extent);
				MapPoint point = graphic.Geometry as MapPoint;
			}

			return extent;
		}
	}
}
