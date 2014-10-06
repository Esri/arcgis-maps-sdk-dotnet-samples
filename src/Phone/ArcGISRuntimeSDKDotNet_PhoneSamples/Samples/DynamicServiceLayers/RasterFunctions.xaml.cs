using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Dynamic Service Layers</category>
	public sealed partial class RasterFunctions : Page
    {
        public RasterFunctions()
        {
            this.InitializeComponent();
			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(1445440, 540657, 1452348, 544407, SpatialReference.Create(2264)));
        }

        private void RasterFunctionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ArcGISImageServiceLayer imageLayer = mapView1.Map.Layers["MyImageLayer"] as ArcGISImageServiceLayer;
            var rasterFunction = (sender as ComboBox).SelectedItem as RasterFunctionInfo;
            if (rasterFunction != null)
            {
                RenderingRule renderingRule = new RenderingRule() { RasterFunctionName = rasterFunction.FunctionName };
                imageLayer.RenderingRule = renderingRule;
            }
        }

        private void ArcGISImageServiceLayer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
           
        }

        private void mapView1_LayerLoaded(object sender, Esri.ArcGISRuntime.Controls.LayerLoadedEventArgs e)
        {
            if (e.Layer.ID == "MyImageLayer")
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
