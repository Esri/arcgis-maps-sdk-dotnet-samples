using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// This sample demonstrates the GraphicsLayer.GraphicsSource property.
	/// </summary>
	/// <title>Graphics Source</title>
	/// <category>Graphics Layers</category>
	public sealed partial class GraphicsSourceSample : Page
	{
		private Random _random = new Random();
		private GraphicsLayer _graphicsLayer;
		private List<List<Graphic>> _graphicsSources;
		private int _graphicSourceIndex;

		public GraphicsSourceSample()
		{
			this.InitializeComponent();

			_graphicsLayer = MyMapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;
			CreateGraphics();
		}

		// Switch between pre-created graphics lists
		private void SwitchGraphicSourceButton_Click(object sender, RoutedEventArgs e)
		{
			++_graphicSourceIndex;
			if (_graphicSourceIndex == _graphicsSources.Count)
				_graphicSourceIndex = 0;

			_graphicsLayer.GraphicsSource = _graphicsSources[_graphicSourceIndex];
		}

		// Create three List<Graphic> objects with random graphics to serve as layer GraphicsSources
		private async void CreateGraphics()
		{
			try
			{
				await MyMapView.LayersLoadedAsync();

				_graphicsSources = new List<List<Graphic>>()
				{
					new List<Graphic>(),
					new List<Graphic>(),
					new List<Graphic>()
				};

				foreach (var graphicList in _graphicsSources)
				{
					for (int n = 0; n < 10; ++n)
					{
						graphicList.Add(CreateRandomGraphic());
					}
				}

				_graphicSourceIndex = 0;
				_graphicsLayer.GraphicsSource = _graphicsSources[_graphicSourceIndex];
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("Exception occurred : " + ex.ToString(), "Sample error").ShowAsync();
			}
		}

		// Create a random graphic
		private Graphic CreateRandomGraphic()
		{
			return new Graphic()
			{
				Geometry = GetRandomMapPoint(),
				Symbol = new SimpleMarkerSymbol() { Color = GetRandomColor(), Size = 15, Style = GetRandomMarkerStyle() }
			};
		}

		// Utility: Generate a random MapPoint within the current extent
		private MapPoint GetRandomMapPoint()
		{
            // Get current viewpoints extent from the MapView
            var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
            var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

            double x = viewpointExtent.XMin + (_random.NextDouble() * viewpointExtent.Width);
            double y = viewpointExtent.YMin + (_random.NextDouble() * viewpointExtent.Height);
			return new MapPoint(x, y, MyMapView.SpatialReference);
		}

		// Utility: Generate a random System.Windows.Media.Color
		private Color GetRandomColor()
		{
			var colorBytes = new byte[3];
			_random.NextBytes(colorBytes);
			return Color.FromArgb(0xFF, colorBytes[0], colorBytes[1], colorBytes[2]);
		}

		// Utility: Generate a random marker style
		private SimpleMarkerStyle GetRandomMarkerStyle()
		{
			return (SimpleMarkerStyle)_random.Next(0, 6);
		}
	}
}
