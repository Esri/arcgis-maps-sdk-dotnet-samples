using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Shows how to create a ClassBreaksRenderer for a graphics layer.
	/// Earthquake data points are pulled from an online source and rendered using the GraphicsLayer ClassBreaksRenderer.
	/// </summary>
	/// <title>Class Breaks Renderer</title>
	/// <category>Symbology</category>
	public partial class ClassBreaksRendererSample : Windows.UI.Xaml.Controls.Page
	{
		private Random _random = new Random();
		private GraphicsOverlay _cities;

		/// <summary>Construct Class Breaks Renderer sample control</summary>
		public ClassBreaksRendererSample()
		{
			InitializeComponent();

			_cities = MyMapView.GraphicsOverlays["cities"];

			MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
		}

		// Load data
		private async void MyMapView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			try
			{
				MyMapView.SpatialReferenceChanged -= MyMapView_SpatialReferenceChanged;
				await LoadDataAsync();
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("Error loading sample data: " + ex.Message, "Sample Error").ShowAsync();
			}
		}

	  
		// Change the graphics layer renderer to a new ClassBreaksRenderer
		private void ChangeRendererButton_Click(object sender, RoutedEventArgs e)
		{
			SimpleMarkerStyle style = (SimpleMarkerStyle)_random.Next(0, 6);

			_cities.Renderer = new ClassBreaksRenderer()
			{
				Field = "pop2000",
				Infos = new ClassBreakInfoCollection() 
				{ 
					new ClassBreakInfo() { Minimum = 0, Maximum = 50000, Symbol = GetRandomSymbol(style) },
					new ClassBreakInfo() { Minimum = 50000, Maximum = 100000, Symbol = GetRandomSymbol(style) },
					new ClassBreakInfo() { Minimum = 100000, Maximum = 250000, Symbol = GetRandomSymbol(style) },
					new ClassBreakInfo() { Minimum = 250000, Maximum = 500000, Symbol = GetRandomSymbol(style) },
					new ClassBreakInfo() { Minimum = 500000, Maximum = 1000000, Symbol = GetRandomSymbol(style) },
					new ClassBreakInfo() { Minimum = 1000000, Maximum = 5000000, Symbol = GetRandomSymbol(style) },
				}
			};
		}

		// Load map service
		private async Task LoadDataAsync()
		{
			var queryTask = new QueryTask(
				new Uri("http://sampleserver6.arcgisonline.com/ArcGIS/rest/services/USA/MapServer/0"));

			// Get current viewpoints extent from the MapView
			var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
			var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

			var query = new Query(viewpointExtent)
			{
				ReturnGeometry = true,
				OutSpatialReference = MyMapView.SpatialReference,
				Where = "pop2000 > 50000",
				OutFields = new OutFields(new string[] { "pop2000" })
			};
			var result = await queryTask.ExecuteAsync(query);

			_cities.Graphics.Clear();
			_cities.Graphics.AddRange(result.FeatureSet.Features.OfType<Graphic>());
		}

		// Utility: Generate a random simple marker symbol
		private SimpleMarkerSymbol GetRandomSymbol(SimpleMarkerStyle style)
		{
			return new SimpleMarkerSymbol()
			{
				Size = 12,
				Color = GetRandomColor(),
				Style = style
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
