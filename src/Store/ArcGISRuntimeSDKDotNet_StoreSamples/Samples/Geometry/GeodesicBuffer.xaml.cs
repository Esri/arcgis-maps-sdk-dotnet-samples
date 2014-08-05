using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
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
        private GraphicsLayer _graphicsLayer;

        /// <summary>Construct Geodesic Buffer sample control</summary>
        public GeodesicBuffer()
        {
            InitializeComponent();

            _graphicsLayer = MyMapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;
            var task = SetupSymbols();
        }

        private void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            try
            {
                _graphicsLayer.Graphics.Clear();

                var point = e.Location;
                var buffer = GeometryEngine.GeodesicBuffer(
                    GeometryEngine.NormalizeCentralMeridianOfGeometry(point), //Normalize in case we we're too far west/east of the world bounds
                    500, LinearUnits.Miles);

                var pointGraphic = new Graphic { Geometry = point, Symbol = _pinSymbol };
                _graphicsLayer.Graphics.Add(pointGraphic);

                var bufferGraphic = new Graphic { Geometry = buffer, Symbol = _bufferSymbol };
                _graphicsLayer.Graphics.Add(bufferGraphic);
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        private async Task SetupSymbols()
        {
            _pinSymbol = new PictureMarkerSymbol() { Width = 24, Height = 24, YOffset = 12 };
            await _pinSymbol.SetSourceAsync(new Uri("ms-appx:///Assets/RedStickPin.png"));

            _bufferSymbol = LayoutRoot.Resources["BufferSymbol"] as SimpleFillSymbol;
        }
    }
}
