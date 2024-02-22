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
using Esri.ArcGISRuntime.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGIS.WinUI.Samples.DisplayClusters
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

            PopupBackground.Tapped += (sender, args) =>
            {
                PopupBackground.Visibility = Visibility.Collapsed;
                PopupViewer.PopupManager = null;
            };
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Identify the tapped observation.
            IdentifyLayerResult results = await MyMapView.IdentifyLayerAsync(_layer, e.Position, 3, true);

            // Return if no popups are found.
            if (results.Popups.Count == 0) return;

            // Set the popup and make it visible.
            PopupViewer.PopupManager = new PopupManager(results.Popups.FirstOrDefault());
            PopupBackground.Visibility = Visibility.Visible;
        }

        // Enable clustering feature reduction if the checkbox has been checked, disable otherwise.
        private void CheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            // This event is raised when sample is initially loaded when layer is null.
            if (_layer == null) return;

            _layer.FeatureReduction.IsEnabled = (bool)(sender as CheckBox).IsChecked;
        }
    }
}