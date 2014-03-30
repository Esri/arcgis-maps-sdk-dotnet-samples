using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstrates use of the GeometryEngine to calculate a geodesic buffer. To use the sample, click a point on the map. The click point and a geodesic buffer of 500 miles around the point will be shown.
    /// </summary>
    /// <title>Geodesic Buffer</title>
	/// <category>Geometry</category>
	public partial class GeodesicBuffer : UserControl
    {
        private PictureMarkerSymbol _pinSymbol;
        private SimpleFillSymbol _bufferSymbol;

        /// <summary>Construct Geodesic Buffer sample control</summary>
        public GeodesicBuffer()
        {
            InitializeComponent();
            var task = SetupSymbols();
        }


		private void mapView_MouseMove(object sender, MouseEventArgs e)
		{
			try {

                // Convert screen point to map point
                var point = mapView.ScreenToLocation(e.GetPosition(mapView));
				if (point == null)
					return;

				var buffer = GeometryEngine.GeodesicBuffer(
					GeometryEngine.NormalizeCentralMeridianOfGeometry(point), //Normalize in case we we're too far west/east of the world bounds
					500, LinearUnits.Miles);

				Graphic bufferGraphic = null;
                //show geometries on map
                if (graphicsLayer.Graphics.Count == 0) {
					bufferGraphic = new Graphic { Geometry = buffer, Symbol = _bufferSymbol };
					graphicsLayer.Graphics.Add(bufferGraphic);
				}
				else
					bufferGraphic = graphicsLayer.Graphics[0];
				bufferGraphic.Geometry = buffer;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Geometry Engine Failed!");
            }
        }

        private async Task SetupSymbols()
        {
            _pinSymbol = new PictureMarkerSymbol() { Width = 48, Height = 48, YOffset = 24 };
            await _pinSymbol.SetSourceAsync(new Uri("pack://application:,,,/ArcGISRuntimeSDKDotNet_DesktopSamples;component/Assets/RedStickpin.png"));

            _bufferSymbol = layoutGrid.Resources["BufferSymbol"] as SimpleFillSymbol;
        }
    }
}
