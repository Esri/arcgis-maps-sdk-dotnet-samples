using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates adding an ArcGIS dynamic map service layer to a map
    /// </summary>
    /// <title>ArcGIS Dynamic Map Service Layer</title>
    /// <category>Dynamic Service Layers</category>
    public sealed partial class ArcGISDynamicMapServiceLayerSample : Page
    {
        private ArcGISDynamicMapServiceLayer _usaLayer;

        public ArcGISDynamicMapServiceLayerSample()
        {
            this.InitializeComponent();

			MyMapView.Map.SpatialReference = SpatialReferences.WebMercator;
			_usaLayer = MyMapView.Map.Layers["USA"] as ArcGISDynamicMapServiceLayer;
			MyMapView.LayerLoaded += MyMapView_LayerLoaded;
        }

        private void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.LoadError != null)
            {
                var _x =  new MessageDialog(e.LoadError.Message, "Layer Error").ShowAsync();
                return;
            }

            if (e.Layer == _usaLayer)
            {
                if (_usaLayer.DynamicLayerInfos == null)
                    _usaLayer.DynamicLayerInfos = _usaLayer.CreateDynamicLayerInfosFromLayerInfos();

                _usaLayer.VisibleLayers = new ObservableCollection<int>(_usaLayer.DynamicLayerInfos
                    .Where(info => info.DefaultVisibility == true)
                    .Select((info, idx) => idx));

                visibleLayers.ItemsSource = _usaLayer.DynamicLayerInfos
                    .Select((info, idx) => new Tuple<string, int, bool>(info.Name, idx, info.DefaultVisibility));
            }
        }

        private void LayerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = e.OriginalSource as CheckBox;
            if (checkBox != null)
            {
                int layerIndex = ((Tuple<string, int, bool>)checkBox.Tag).Item2;

                if (checkBox.IsChecked == true)
                    _usaLayer.VisibleLayers.Add(layerIndex);
                else
                    _usaLayer.VisibleLayers.Remove(layerIndex);
            }
        }
    }
}
