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
using Esri.ArcGISRuntime.Portal;

namespace ArcGIS.Samples.DisplayPointsUsingClusteringFeatureReduction
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        "Display points using clustering feature reduction",
        "Layers",
        "Display a web map with a point feature layer that has feature reduction enabled to aggregate points into clusters.",
        "")]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class DisplayPointsUsingClusteringFeatureReduction
    {
        private FeatureLayer _layer;

        public DisplayPointsUsingClusteringFeatureReduction()
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

            // Hide and nullify an opened popup when user taps screen.
            PopupBackground.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    PopupBackground.IsVisible = false;
                    PopupViewer.Popup = null;
                })
            });
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            // Identify the tapped observation.
            IdentifyLayerResult results = await MyMapView.IdentifyLayerAsync(_layer, e.Position, 3, true);

            // Return if no observations were found.
            if (results.Popups.Count == 0) return;

            // Set the popup and make it visible.
            PopupViewer.Popup = results.Popups.FirstOrDefault();
            PopupBackground.IsVisible = true;
        }

        // Enable clustering feature reduction if the checkbox has been checked, disable otherwise.
        private void CheckBox_CheckChanged(object sender, CheckedChangedEventArgs e)
        {
            // This event is raised when sample is initially loaded when layer is null.
            if (_layer == null) return;

            _layer.FeatureReduction.IsEnabled = (bool)(sender as CheckBox).IsChecked;
        }
    }
}