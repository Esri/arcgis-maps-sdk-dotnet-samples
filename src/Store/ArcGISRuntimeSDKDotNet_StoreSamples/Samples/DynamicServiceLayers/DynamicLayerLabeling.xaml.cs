using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates dynamic layer labeling using LayerDrawingOptions of a dyanmic layer.
    /// </summary>
    /// <title>Dynamic Layer Labeling</title>
    /// <category>Dynamic Service Layers</category>
    public sealed partial class DynamicLayerLabeling : Page
    {
        private ArcGISDynamicMapServiceLayer _usaLayer;

        public DynamicLayerLabeling()
        {
            this.InitializeComponent();

			MyMapView.Map.SpatialReference = SpatialReferences.WebMercator;
			MyMapView.Loaded += MyMapView_Loaded;

            _usaLayer = MyMapView.Map.Layers["USA"] as ArcGISDynamicMapServiceLayer;
            _usaLayer.VisibleLayers = new ObservableCollection<int>() { 0, 1, 2 };
        }

        private void MyMapView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Minor city label info
                DynamicLabelingInfo minorCityLabelInfo = new DynamicLabelingInfo();
                minorCityLabelInfo.LabelExpression = "[areaname]";
                minorCityLabelInfo.LabelPlacement = Esri.ArcGISRuntime.Layers.LabelPlacement.PointAboveRight;
                minorCityLabelInfo.Symbol = new TextSymbol()
                {
                    Color = Colors.Black,
                    Font = new SymbolFont("Arial", 10, SymbolFontStyle.Normal, SymbolTextDecoration.None, SymbolFontWeight.Normal)
                };
                minorCityLabelInfo.Where = "pop2000 <= 500000";
                minorCityLabelInfo.MaxScale = 0;
                minorCityLabelInfo.MinScale = 5000000;

                // Add minor city label info
                var labelInfos = _usaLayer.LayerDrawingOptions.First(ldo => ldo.LayerID == 0).LabelingInfos;
                labelInfos.Add(minorCityLabelInfo);
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog("Sample Error: " + ex.Message).ShowAsync();
            }
        }
    }
}
