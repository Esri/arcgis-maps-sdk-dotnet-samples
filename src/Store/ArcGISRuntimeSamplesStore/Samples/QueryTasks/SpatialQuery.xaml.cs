using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Demonstrates how to spatially search your data using a QueryTask with its geometry attribute set.
	/// </summary>
	/// <title>Spatial Query</title>
	/// <category>Query Tasks</category>
	public sealed partial class SpatialQuery : Page
	{
		public SpatialQuery()
		{
			this.InitializeComponent();

			InitializePictureMarkerSymbol();
		}

		private async void InitializePictureMarkerSymbol()
		{
			try
			{
				var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///ArcGISRuntimeSamplesStore/Assets/RedStickpin.png"));
				var imageSource = await imageFile.OpenReadAsync();
				var pms = LayoutRoot.Resources["DefaultMarkerSymbol"] as PictureMarkerSymbol;
				await pms.SetSourceAsync(imageSource);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		private async void MyMapView_Tapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
		{
			try
			{
				var graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
				if (!(graphicsOverlay.Graphics.Count == 0))
					graphicsOverlay.Graphics.Clear();

				graphicsOverlay.Graphics.Add(new Graphic() { Geometry = e.Location });

				var bufferOverlay = MyMapView.GraphicsOverlays["bufferOverlay"];
				if (!(bufferOverlay.Graphics.Count == 0))
					bufferOverlay.Graphics.Clear();

				var bufferResult = GeometryEngine.Buffer(e.Location, 100);
				bufferOverlay.Graphics.Add(new Graphic() { Geometry = bufferResult });

				var queryTask = new QueryTask(
					new Uri("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/BloomfieldHillsMichigan/Parcels/MapServer/2"));
				var query = new Query("1=1")
				{
					ReturnGeometry = true,
					OutSpatialReference = MyMapView.SpatialReference,
					Geometry = bufferResult
				};
				query.OutFields.Add("OWNERNME1");

				var queryResult = await queryTask.ExecuteAsync(query);
				if (queryResult != null && queryResult.FeatureSet != null)
				{
					var resultOverlay = MyMapView.GraphicsOverlays["parcelOverlay"];
					if (!(resultOverlay.Graphics.Count == 0))
						resultOverlay.Graphics.Clear();

					resultOverlay.Graphics.AddRange(queryResult.FeatureSet.Features.OfType<Graphic>());
				}
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}
	}
}
