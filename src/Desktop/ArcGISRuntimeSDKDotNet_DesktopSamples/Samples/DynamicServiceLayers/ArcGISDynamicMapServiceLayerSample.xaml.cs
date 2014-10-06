using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates adding an ArcGIS dynamic map service layer to a Map in XAML. 
    /// </summary>
    /// <title>ArcGIS Dynamic Map Service Layer</title>
	/// <category>Layers</category>
	/// <subcategory>Dynamic Service Layers</subcategory>
	public partial class ArcGISDynamicMapServiceLayerSample : UserControl
    {
        private ArcGISDynamicMapServiceLayer _usaLayer;

		public ArcGISDynamicMapServiceLayerSample()
		{
			InitializeComponent();

			MyMapView.Map.SpatialReference = SpatialReferences.WebMercator;
			_usaLayer = MyMapView.Map.Layers["USA"] as ArcGISDynamicMapServiceLayer;
			MyMapView.LayerLoaded += MyMapView_LayerLoaded;
		}

        private void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.LoadError != null)
            {
                MessageBox.Show(e.LoadError.Message, "Layer Error");
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
