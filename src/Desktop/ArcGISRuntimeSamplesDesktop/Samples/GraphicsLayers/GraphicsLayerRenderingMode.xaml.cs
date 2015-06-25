using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// This sample demonstrates the use of the GraphicsLayer.RenderingMode property to control how a GraphicsOverlay draws its graphics. Zooming and Panning the map in each of the rendering modes will show the differences between them. Rendering mode differences will be more pronounced with higher numbers of graphics in the graphic layer.
	/// </summary>
	/// <title>Rendering Mode</title>
	/// <category>Layers</category>
	/// <subcategory>Graphics Layers</subcategory>
	public partial class GraphicsLayerRenderingMode : UserControl
	{
		private Random _random = new Random();

		private Envelope _maxExtent;
		private List<Graphic> _graphics = new List<Graphic>();

		/// <summary>Construct Rendering Mode sample control</summary>
		public GraphicsLayerRenderingMode()
		{
			InitializeComponent();

			graphicCountSlider.Value = 1000;
			renderingModeCombo.SelectedIndex = 0;

			// Create the minimum set of graphics
			MyMapView.NavigationCompleted += MyMapView_NavigationCompleted;
		}

		private async void MyMapView_NavigationCompleted(object sender, EventArgs e)
		{
			MyMapView.NavigationCompleted -= MyMapView_NavigationCompleted;
			try
			{
				await CreateGraphicsAsync(1000);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to create graphics. Error = " + ex.ToString(), "Sample Error"); 
			}
		}

		// Creates a new graphics z with the specified graphics count and rendering mode
		private async void CreateGraphicsLayerButton_Click(object sender, RoutedEventArgs e)
		{
            if (MyMapView.Map.Layers.Count() > 1)
                MyMapView.Map.Layers.RemoveAt(1);

			var graphicsLayer = new GraphicsLayer() { 
				RenderingMode = (GraphicsRenderingMode)renderingModeCombo.SelectedValue 
			};
			MyMapView.Map.Layers.Add(graphicsLayer);

			// Add new graphics if needed
			var numGraphics = (int)graphicCountSlider.Value;
			if (_graphics.Count < numGraphics)
				await CreateGraphicsAsync(numGraphics - _graphics.Count);

			graphicsLayer.Graphics.AddRange(_graphics.Take(numGraphics));
		}

		// Add new random graphics to the graphics layer
		private async Task CreateGraphicsAsync(int numGraphics)
		{
			await MyMapView.LayersLoadedAsync();

			if (_maxExtent == null)
				_maxExtent = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry)
                    .TargetGeometry.Extent;

			for (int n = 0; n < numGraphics; ++n)
				_graphics.Add(CreateRandomGraphic());
		}

		// Create a random graphic
		private Graphic CreateRandomGraphic()
		{
			return new Graphic()
			{
				Geometry = GetRandomMapPoint(),
				Symbol = new SimpleMarkerSymbol()
				{
					Style = (SimpleMarkerStyle)_random.Next(0, 6),
					Color = GetRandomColor(),
					Size = 12
				}
			};
		}

		// Utility: Generate a random MapPoint within the current extent
		private MapPoint GetRandomMapPoint()
		{
			double x = _maxExtent.XMin + (_random.NextDouble() * _maxExtent.Width);
			double y = _maxExtent.YMin + (_random.NextDouble() * _maxExtent.Height);
			return new MapPoint(x, y, MyMapView.SpatialReference);
		}

		// Utility: Generate a random System.Windows.Media.Color
		private Color GetRandomColor()
		{
			var colorBytes = new byte[3];
			_random.NextBytes(colorBytes);
			return Color.FromRgb(colorBytes[0], colorBytes[1], colorBytes[2]);
		}
	}
}
