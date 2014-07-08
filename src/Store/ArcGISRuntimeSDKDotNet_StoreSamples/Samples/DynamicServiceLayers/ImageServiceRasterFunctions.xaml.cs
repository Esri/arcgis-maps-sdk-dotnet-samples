using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// Demonstrates applying raster functions to an image service layer.
	/// </summary>
    /// <title>Image Service Raster Functions</title>
    /// <category>Dynamic Service Layers</category>
	public sealed partial class ImageServiceRasterFunctions : Page
    {
        public ImageServiceRasterFunctions()
        {
            this.InitializeComponent();
            mapView.Map.InitialViewpoint = new Viewpoint(new Envelope(1445440, 540657, 1452348, 544407, new SpatialReference(2264)));
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
