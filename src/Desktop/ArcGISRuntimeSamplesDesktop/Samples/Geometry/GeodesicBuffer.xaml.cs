using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates use of the GeometryEngine to calculate a geodesic buffer. To use the sample, click a point on the map. The click point and a geodesic buffer of 500 miles around the point will be shown.
    /// </summary>
    /// <title>Geodesic Buffer</title>
    /// <category>Geometry</category>
    public partial class GeodesicBuffer : UserControl
    {
        private SimpleFillSymbol _bufferSymbol;
		private GraphicsOverlay _graphicsOverlay;

        /// <summary>Construct Geodesic Buffer sample control</summary>
        public GeodesicBuffer()
        {
            InitializeComponent();
			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
            _bufferSymbol = layoutGrid.Resources["BufferSymbol"] as SimpleFillSymbol;
        }

        private void MyMapView_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // Convert screen point to map point
                var point = MyMapView.ScreenToLocation(e.GetPosition(MyMapView));
                if (point == null)
                    return;

                var buffer = GeometryEngine.GeodesicBuffer(
					GeometryEngine.NormalizeCentralMeridian(point), //Normalize in case we we're too far west/east of the world bounds
                    500, LinearUnits.Miles);

                Graphic bufferGraphic = null;
				if (_graphicsOverlay.Graphics.Count == 0)
                {
                    bufferGraphic = new Graphic { Geometry = buffer, Symbol = _bufferSymbol };
					_graphicsOverlay.Graphics.Add(bufferGraphic);
                }
                else
					bufferGraphic = _graphicsOverlay.Graphics[0];
                bufferGraphic.Geometry = buffer;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Geometry Engine Failed!");
            }
        }
    }
}
