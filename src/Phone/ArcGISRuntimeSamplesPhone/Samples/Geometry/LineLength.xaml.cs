using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates use of the GeometryEngine.GeodesicLength method to calculate the length of a line.
	/// </summary>
	/// <title>Line Length</title>
	/// <category>Geometry</category>
	public sealed partial class LineLength : Page
	{
		GraphicsLayer myGraphicsLayer;
		Geometry inputGeom;
		public LineLength()
		{
			InitializeComponent();

			MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-13149423, 3997267, -12992880, 4062214, SpatialReferences.WebMercator));
			myGraphicsLayer = MyMapView.Map.Layers["MyGraphicsLayer"] as GraphicsLayer;
		}

		private async Task DoGeodesicLength()
		{
			ResetButton.IsEnabled = false;

			try
			{
				if (MyMapView.Editor.IsActive)
					MyMapView.Editor.Cancel.Execute(null);

				//Get the input polygon geometry from the user
				inputGeom = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polyline);

				if (inputGeom != null)
				{
					//Add the polygon drawn by the user
					var g = new Graphic
					{
						Geometry = inputGeom,
					};
					myGraphicsLayer.Graphics.Add(g);


					//Get the label point for the input geometry
					var length = GeometryEngine.GeodesicLength(inputGeom);
					LineLengthTextBlock.Text = length.ToString("N2") + " m";
					LineLengthTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
				}
			}
			catch (Exception)
			{

			}
			ResetButton.IsEnabled = true;

		}

		private async void ResetButton_Click(object sender, RoutedEventArgs e)
		{
			LineLengthTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

			myGraphicsLayer.Graphics.Clear();
			await DoGeodesicLength();

		}


		private async void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.Layer.ID == "MyGraphicsLayer")
			{
				await DoGeodesicLength();
			}
		}
	}
}
