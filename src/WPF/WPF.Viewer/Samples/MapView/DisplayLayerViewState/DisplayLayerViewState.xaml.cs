// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGIS.WPF.Samples.DisplayLayerViewState
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display layer view state",
        category: "MapView",
        description: "Determine if a layer is currently being viewed.",
        instructions: "Tap the *Load layer* button to add a feature layer to the map. The current view status of the layer will display on the map. Zoom in and out of the map and note the layer disappears when the map is scaled outside of its min and max scale range. Control the layer's visibility with the switch. If you disconnect your device from the network and pan around the map, a warning will display. Reconnect to the network to remove the warning. The layer's current view status will update accordingly as you carry out these actions.",
        tags: new[] { "layer", "load", "map", "status", "view", "visibility" })]
    public partial class DisplayLayerViewState
    {
        public DisplayLayerViewState()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic) { InitialViewpoint = new Viewpoint(new MapPoint(-11e6, 45e5, SpatialReferences.WebMercator), 40000000) };

            // Event for layer view state changed.
            MyMapView.LayerViewStateChanged += OnLayerViewStateChanged;
        }

        private void OnLayerViewStateChanged(object sender, LayerViewStateChangedEventArgs e)
        {
            // Check that the layer that changed is the feature layer added to the map.
            if (e.Layer == MyMapView.Map?.OperationalLayers.FirstOrDefault())
            {
                // Update the UI with the layer view status.
                LayerStatusLabel.Text = e.LayerViewState.Status.ToString();
            }
        }

        private void LoadButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadButton.Content = "Reload layer";
            _ = LoadLayer();
        }

        private async Task LoadLayer()
        {
            MyMapView.Map.OperationalLayers.Clear();
            LayerStatusLabel.Text = string.Empty;

            try
            {
                // Create a feature layer from a portal item.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri("https://runtime.maps.arcgis.com/"));
                PortalItem item = await PortalItem.CreateAsync(portal, "b8f4033069f141729ffb298b7418b653");
                var featureLayer = new FeatureLayer(item, 0) { MinScale = 40000000, MaxScale = 4000000 };

                // Load the layer and add it to the map.
                await featureLayer.LoadAsync();
                MyMapView.Map.OperationalLayers.Add(featureLayer);
                VisibilityCheckBox.IsChecked = featureLayer.IsVisible;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MyMapView.Map?.OperationalLayers?.FirstOrDefault() == null) return;

            MyMapView.Map.OperationalLayers.FirstOrDefault().IsVisible = VisibilityCheckBox.IsChecked == true;
        }
    }
}