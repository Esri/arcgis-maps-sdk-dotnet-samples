using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates the use of the GeometryEngine to calculate a buffer.
	/// </summary>
	/// <title>Buffer</title>
	/// <category>Geometry</category>
	public sealed partial class BufferPoint : Page
	{
		GraphicsLayer graphicsLayer;
		PictureMarkerSymbol pms;

		SimpleMarkerSymbol sms;
		SimpleFillSymbol sfs;
		private const double milesToMetersConversion = 1609.34;

		public BufferPoint()
		{
			InitializeComponent();

			MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-10863035.970, 3838021.340, -10744801.344, 3887145.299));
			InitializePictureMarkerSymbol();
			sms = LayoutRoot.Resources["MySimpleMarkerSymbol"] as SimpleMarkerSymbol;
			sfs = LayoutRoot.Resources["MySimpleFillSymbol"] as SimpleFillSymbol;
			graphicsLayer = MyMapView.Map.Layers["MyGraphicsLayer"] as GraphicsLayer;

			MyMapView.MapViewTapped += mapView1_MapViewTapped;
		}

		void mapView1_MapViewTapped(object sender, MapViewInputEventArgs e)
		{
			graphicsLayer.Graphics.Clear();
			try
			{

				var pointGeom = e.Location;

				var bufferGeom = GeometryEngine.Buffer(pointGeom, 5 * milesToMetersConversion);

				//show geometries on map
				if (graphicsLayer != null)
				{
					var pointGraphic = new Graphic { Geometry = pointGeom, Symbol = pms };
					graphicsLayer.Graphics.Add(pointGraphic);

					var bufferGraphic = new Graphic { Geometry = bufferGeom, Symbol = sfs };
					graphicsLayer.Graphics.Add(bufferGraphic);
				}
			}
			catch (Exception ex)
			{
				var dlg = new MessageDialog(ex.Message, "Geometry Engine Failed!");
				var _x = dlg.ShowAsync();
			}
		}


		private async void InitializePictureMarkerSymbol()
		{
			try
			{
				var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///ArcGISRuntimeSamplesPhone/Assets/i_pushpin.png"));
				var imageSource = await imageFile.OpenReadAsync();
				pms = LayoutRoot.Resources["MyPictureMarkerSymbol"] as PictureMarkerSymbol;
				await pms.SetSourceAsync(imageSource);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

	}
}
