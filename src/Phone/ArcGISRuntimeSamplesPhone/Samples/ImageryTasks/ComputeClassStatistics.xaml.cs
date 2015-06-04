using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Imagery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// Demonstrates how to compute class statistics for an image layer and display the results with the Maximum Likelihood Classification renderer rule.
	/// </summary>
	/// <title>Compute Class Statistics</title>
	/// <category>Imagery Tasks</category>
	public partial class ComputeClassStatistics : Page
	{
		private ArcGISImageServiceLayer _imageLayer;
		private GraphicsOverlay _graphicsOverlay;

		/// <summary>Construct compute class statistics sample control</summary>
		public ComputeClassStatistics()
		{
			InitializeComponent();

			_graphicsOverlay = MyMapView.GraphicsOverlays.First();

			MyMapView.LayerLoaded += MyMapView_LayerLoaded;
		}

		// Zooms to the image layer and starts accepting user points
		private async void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.Layer is ArcGISImageServiceLayer)
			{
				if (e.Layer.FullExtent != null)
					await MyMapView.SetViewAsync(e.Layer.FullExtent);

				_imageLayer = (ArcGISImageServiceLayer)e.Layer;
				await AcceptClassPointsAsync();
			}
		}

		// Computes Class Statistics for the image layer using user input graphics as class definition polygons
		private async void ComputeClassStatisticsButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (_graphicsOverlay.Graphics.Count < 2)
					throw new ArgumentException("Before computing statistics, enter two or more class definition areas by clicking the image on the map.");

				progress.Visibility = Visibility.Visible;
				if (MyMapView.Editor.IsActive)
					MyMapView.Editor.Cancel.Execute(null);

				var statsTask = new ComputeClassStatisticsTask(new Uri(_imageLayer.ServiceUri));

				var statsParam = new ComputeClassStatisticsParameters();
				statsParam.ClassDescriptions = _graphicsOverlay.Graphics
					.Select((g, idx) => new ClassDescription(idx, idx.ToString(), g.Geometry as Polygon)).ToList();

				var result = await statsTask.ComputeClassStatisticsAsync(statsParam);

				_imageLayer.RenderingRule = new RenderingRule()
				{
					RasterFunctionName = "MLClassify",
					VariableName = "Raster",
					RasterFunctionArguments = new Dictionary<string, object> { { "SignatureFile", result.GSG } },
				};
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
			finally
			{
				progress.Visibility = Visibility.Collapsed;
			}
		}

		// Reset the rendering rule for the image layer and restart accepting loop
		private async void ResetButton_Click(object sender, RoutedEventArgs e)
		{
			_graphicsOverlay.Graphics.Clear();
			_imageLayer.RenderingRule = null;
			await AcceptClassPointsAsync();
		}

		// Continually accepts user-entered points
		// - Buffered polygons are created from the points and added to the graphics layer
		private async Task AcceptClassPointsAsync()
        { 
            // Get current viewpoints extent from the MapView
            var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
            var viewpointExtent = currentViewpoint.TargetGeometry.Extent;
			
            try
			{
                while (true)
				{
					var point = await MyMapView.Editor.RequestPointAsync();
					var polygon = GeometryEngine.Buffer(point, viewpointExtent.Width * .01);
					var attr = new Dictionary<string, object>() { { "ID", _graphicsOverlay.Graphics.Count + 1 } };
					_graphicsOverlay.Graphics.Add(new Graphic(polygon, attr));
				}
			}
			catch (TaskCanceledException)
			{
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}
	}
}
