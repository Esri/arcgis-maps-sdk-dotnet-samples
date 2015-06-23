using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Demonstrates use of the GeometryEngine to calculate a buffer.
	/// </summary>
	/// <title>Buffer</title>
	/// <category>Geometry</category>
	public sealed partial class BufferSample : Page
	{
		private const double milesToMetersConversion = 1609.34;

		private GraphicsOverlay _graphicsOverlay;
		private PictureMarkerSymbol _pms;
		private SimpleFillSymbol _sfs;

		public BufferSample()
		{
			InitializeComponent();

			SetupSymbols();
			_sfs = LayoutRoot.Resources["MySimpleFillSymbol"] as SimpleFillSymbol;
			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicOverlay"];

			MyMapView.MapViewTapped += MyMapView_MapViewTapped;
		}

		void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
		{           
			try
			{
				_graphicsOverlay.Graphics.Clear();
				
				var pointGeom = e.Location;
				var bufferGeom = GeometryEngine.Buffer(pointGeom, 5 * milesToMetersConversion);

				//show geometries on map
				if (_graphicsOverlay != null)
				{
					var pointGraphic = new Graphic { Geometry = pointGeom, Symbol = _pms };
					_graphicsOverlay.Graphics.Add(pointGraphic);

					var bufferGraphic = new Graphic { Geometry = bufferGeom, Symbol = _sfs };
					_graphicsOverlay.Graphics.Add(bufferGraphic);
				}
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Geometry Engine Failed!").ShowAsync();
			}
		}

		private async void SetupSymbols()
		{
			try
			{
				_pms = LayoutRoot.Resources["MyPictureMarkerSymbol"] as PictureMarkerSymbol;
				await _pms.SetSourceAsync(new Uri("ms-appx:///ArcGISRuntimeSamplesStore/Assets/RedStickPin.png"));
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Buffer Sample").ShowAsync();
			}
		}
	}
}
