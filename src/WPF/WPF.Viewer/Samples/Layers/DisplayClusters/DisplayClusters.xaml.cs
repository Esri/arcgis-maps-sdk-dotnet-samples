// Copyright 2023 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Popups;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Reduction;
using Esri.ArcGISRuntime.UI.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGIS.WPF.Samples.DisplayClusters
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display clusters",
        category: "Layers",
        description: "Display a web map with a point feature layer that has feature reduction enabled to aggregate points into clusters.",
        instructions: "Pan and zoom the map to view how clustering is dynamically updated. Toggle clustering off to view the original point features that make up the clustered elements. When clustering is on, you can click on a clustered geoelement to view aggregated information and summary statistics for that cluster. When clustering is toggled off and you click on the original feature you get access to information about individual power plant features.",
        tags: new[] { "aggregate", "bin", "cluster", "group", "merge", "normalize", "reduce", "summarize" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class DisplayClusters
    {
        // Hold a reference to the feature layer.
        private FeatureLayer _layer;

        public DisplayClusters()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Get the power plants web map from the default portal.
            var portal = await ArcGISPortal.CreateAsync();
            PortalItem portalItem = await PortalItem.CreateAsync(portal, "8916d50c44c746c1aafae001552bad23");

            // Create a new map from the web map.
            MyMapView.Map = new Map(portalItem);

            // Get the power plant feature layer once the map has finished loading.
            await MyMapView.Map.LoadAsync();
            _layer = (FeatureLayer)MyMapView.Map.OperationalLayers.First();
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Clear any previously selected features or clusters.
            _layer.ClearSelection();

            // Identify the tapped observation.
            var results = await MyMapView.IdentifyLayerAsync(_layer, e.Position, 3, false);

            // Return if no popups are found.
            if (results.GeoElements.Count == 0 || results.Popups.Count == 0) return;

            if (results.Popups.FirstOrDefault() is Popup popup)
            {
                // Set the popup and make it visible.
                PopupViewer.Popup = popup;
                PopupBackground.Visibility = Visibility.Visible;
            }

            // If the tapped observation is an AggregateGeoElement then select it.
            if (results.GeoElements.FirstOrDefault() is AggregateGeoElement aggregateGeoElement)
            {
                // Select the AggregateGeoElement.
                aggregateGeoElement.IsSelected = true;

                // Get the contained GeoElements.
                IReadOnlyList<GeoElement> geoElements = await aggregateGeoElement.GetGeoElementsAsync();

                // Set the geoelements as an items source and set the visibility.
                GeoElementsGrid.ItemsSource = geoElements;
                GeoElementsPanel.Visibility = Visibility.Visible;
            }
            else if (results.GeoElements.FirstOrDefault() is ArcGISFeature feature)
            {
                // If the tapped observation is not an AggregateGeoElement select the feature.
                _layer.SelectFeature(feature);
            }
        }

        // Enable clustering feature reduction if the checkbox has been checked, disable otherwise.
        private void EnableClusteringCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            // This event is raised when sample is initially loaded when layer is null.
            if (_layer == null) return;

            _layer.FeatureReduction.IsEnabled = (bool)(sender as CheckBox).IsChecked;
        }

        // Hide and nullify the opened popup when user left clicks.
        private void PopupBackground_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PopupBackground.Visibility = Visibility.Collapsed;
            GeoElementsPanel.Visibility = Visibility.Collapsed;
            PopupViewer.Popup = null;
            GeoElementsGrid.ItemsSource = null;
        }
    }
}