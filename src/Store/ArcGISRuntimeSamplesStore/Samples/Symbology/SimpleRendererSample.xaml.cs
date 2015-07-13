using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Shows how to create a SimpleRenderer for a graphics layer in XAML and code.
	/// </summary>
	/// <title>Simple Renderer</title>
	/// <category>Symbology</category>
	public partial class SimpleRendererSample : Windows.UI.Xaml.Controls.Page
	{
		private Random _random = new Random();
		private GraphicsOverlay _graphicsOverlay;

		/// <summary>Construct Simple Renderer sample control</summary>
		public SimpleRendererSample()
		{
			InitializeComponent();

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];

			MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
		}

		// Start map interaction
		private async void MyMapView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			MyMapView.SpatialReferenceChanged -= MyMapView_SpatialReferenceChanged;
			await AcceptPointsAsync();
		}

		// Change the graphics layer renderer to a new SimpleRenderer
		private void ChangeRendererButton_Click(object sender, RoutedEventArgs e)
		{
			_graphicsOverlay.Renderer = new SimpleRenderer() { Symbol = GetRandomSymbol() };
		}

		// Accept user map clicks and add points to the graphics layer (use the default symbol from renderer)
		private async Task AcceptPointsAsync()
		{
			try
			{
				while (MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry.Extent != null)
				{
					var point = await MyMapView.Editor.RequestPointAsync();
					_graphicsOverlay.Graphics.Add(new Graphic(point));
				}
			}
			catch (TaskCanceledException)
			{
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}

		// Utility: Generate a random simple marker symbol
		private SimpleMarkerSymbol GetRandomSymbol()
		{
			return new SimpleMarkerSymbol()
			{
				Size = 15,
				Color = GetRandomColor(),
				Style = (SimpleMarkerStyle)_random.Next(0, 6)
			};
		}
		
		// Utility function: Generate a random System.Windows.Media.Color
		private Color GetRandomColor()
		{
			var colorBytes = new byte[3];
			_random.NextBytes(colorBytes);
			return Color.FromArgb(0xFF, colorBytes[0], colorBytes[1], colorBytes[2]);
		}
	}
}
