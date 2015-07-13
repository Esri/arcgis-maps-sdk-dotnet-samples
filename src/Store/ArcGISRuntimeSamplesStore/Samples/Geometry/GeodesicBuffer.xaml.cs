using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using Windows.UI.Popups;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Demonstrates use of the GeometryEngine to calculate a geodesic buffer.
	/// </summary>
	/// <title>Geodesic Buffer</title>
	/// <category>Geometry</category>
	public partial class GeodesicBuffer : Windows.UI.Xaml.Controls.Page
	{
		private PictureMarkerSymbol _pinSymbol;
		private SimpleFillSymbol _bufferSymbol;
		private GraphicsOverlay _graphicsOverlay;

		/// <summary>Construct Geodesic Buffer sample control</summary>
		public GeodesicBuffer()
		{
			InitializeComponent();

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
			SetupSymbols();
		}

		private void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
		{
			try
			{
				_graphicsOverlay.Graphics.Clear();

				var point = e.Location;
				var buffer = GeometryEngine.GeodesicBuffer(
					GeometryEngine.NormalizeCentralMeridian(point), //Normalize in case we we're too far west/east of the world bounds
					500, LinearUnits.Miles);

				var pointGraphic = new Graphic { Geometry = point, Symbol = _pinSymbol };
				_graphicsOverlay.Graphics.Add(pointGraphic);

				var bufferGraphic = new Graphic { Geometry = buffer, Symbol = _bufferSymbol };
				_graphicsOverlay.Graphics.Add(bufferGraphic);
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}

		private async void SetupSymbols()
		{
			try
			{
				_pinSymbol = new PictureMarkerSymbol() { Width = 24, Height = 24, YOffset = 12 };
				await _pinSymbol.SetSourceAsync(new Uri("ms-appx:///ArcGISRuntimeSamplesStore/Assets/RedStickPin.png"));

				_bufferSymbol = LayoutRoot.Resources["BufferSymbol"] as SimpleFillSymbol;

			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}
	}
}
