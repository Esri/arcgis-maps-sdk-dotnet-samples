using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates applying raster functions to an image service layer.
    /// </summary>
    /// <title>Image Service Raster Functions</title>
	/// <category>Layers</category>
	/// <subcategory>Dynamic Service Layers</subcategory>
	public partial class ImageServiceRasterFunctions : UserControl
    {
        public ImageServiceRasterFunctions()
        {
            InitializeComponent();

			mapView.Map.InitialViewpoint = new Viewpoint(new Envelope(1445440, 540657, 1452348, 544407, SpatialReference.Create(2264)));
			mapView.Map.SpatialReference = SpatialReference.Create(2264);
            mapView.LayerLoaded += mapView_LayerLoaded;
        }

        private void RasterFunctionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ArcGISImageServiceLayer imageLayer = mapView.Map.Layers["ImageLayer"] as ArcGISImageServiceLayer;
            var rasterFunction = (sender as ComboBox).SelectedItem as RasterFunctionInfo;
            if (rasterFunction != null)
            {
                RenderingRule renderingRule = new RenderingRule() { RasterFunctionName = rasterFunction.FunctionName };
                imageLayer.RenderingRule = renderingRule;
            }
        }

        private void mapView_LayerLoaded(object sender, Esri.ArcGISRuntime.Controls.LayerLoadedEventArgs e)
        {
            if (e.Layer.ID == "ImageLayer")
            {
                ArcGISImageServiceLayer imageLayer = e.Layer as ArcGISImageServiceLayer;
                if (e.LoadError == null)
                {
                    RasterFunctionsComboBox.ItemsSource = imageLayer.ServiceInfo.RasterFunctionInfos;
                    RasterFunctionsComboBox.SelectedIndex = 0;
                }
            }
        }
    }
}
