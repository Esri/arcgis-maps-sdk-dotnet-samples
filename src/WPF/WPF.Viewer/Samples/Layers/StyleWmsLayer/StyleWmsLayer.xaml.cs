// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.StyleWmsLayer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Style WMS layers",
        category: "Layers",
        description: "Change the style of a Web Map Service (WMS) layer.",
        instructions: "Once the layer loads, the toggle button will be enabled. Click it to toggle between the first and second styles of the WMS layer.",
        tags: new[] { "WMS", "imagery", "styles", "visualization" })]
    public partial class StyleWmsLayer
    {
        // Hold the URL to the service, which has satellite imagery covering the state of Minnesota.
        private Uri _wmsUrl = new Uri("https://imageserver.gisdata.mn.gov/cgi-bin/mncomp?SERVICE=WMS&VERSION=1.3.0&REQUEST=GetCapabilities");

        // Hold a list of uniquely-identifying WMS layer names to display.
        private List<String> _wmsLayerNames = new List<string> { "mncomp" };

        // Hold a reference to the layer to enable re-styling.
        private WmsLayer _mnWmsLayer;

        public StyleWmsLayer()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Create a map with spatial reference appropriate for the service.
                Map myMap = new Map(SpatialReference.Create(26915)) { MinScale = 7000000.0 };

                // Create a new WMS layer displaying the specified layers from the service.
                // The default styles are chosen by default.
                _mnWmsLayer = new WmsLayer(_wmsUrl, _wmsLayerNames);

                // Wait for the layer to load.
                await _mnWmsLayer.LoadAsync();

                // Center the map on the layer's contents.
                myMap.InitialViewpoint = new Viewpoint(_mnWmsLayer.FullExtent);

                // Add the layer to the map.
                myMap.OperationalLayers.Add(_mnWmsLayer);

                // Add the map to the view.
                MyMapView.Map = myMap;

                // Enable the buttons.
                FirstStyleButton.IsEnabled = true;
                SecondStyleButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private void FirstStyleButton_Clicked(object sender, RoutedEventArgs e)
        {
            // Get the available styles from the first sublayer.
            IReadOnlyList<string> styles = _mnWmsLayer.Sublayers[0].SublayerInfo.Styles;

            // Apply the first style to the first sublayer.
            _mnWmsLayer.Sublayers[0].CurrentStyle = styles[0];
        }

        private void SecondStyleButton_Clicked(object sender, RoutedEventArgs e)
        {
            // Get the available styles from the first sublayer.
            IReadOnlyList<string> styles = _mnWmsLayer.Sublayers[0].SublayerInfo.Styles;

            // Apply the second style to the first sublayer.
            _mnWmsLayer.Sublayers[0].CurrentStyle = styles[1];
        }
    }
}