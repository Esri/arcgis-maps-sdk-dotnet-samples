using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
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


			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(-10863035.970, 3838021.340, -10744801.344, 3887145.299));
            InitializePictureMarkerSymbol();
            sms = LayoutRoot.Resources["MySimpleMarkerSymbol"] as SimpleMarkerSymbol;
            sfs = LayoutRoot.Resources["MySimpleFillSymbol"] as SimpleFillSymbol;
            graphicsLayer = mapView1.Map.Layers["MyGraphicsLayer"] as GraphicsLayer;

            mapView1.MapViewTapped += mapView1_MapViewTapped;
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
				var _ = dlg.ShowAsync();
            }
        }

  

        private async void InitializePictureMarkerSymbol()
        {
            try
            {
                var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/i_pushpin.png"));
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
