// Copyright 2026 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using System;
using Microsoft.UI.Xaml;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGIS.WinUI.Samples.ShowPopup
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Show popup",
        category: "Data",
        description: "Show predefined popups from a web map.",
        instructions: "Tap on the features to prompt a popup that displays information about the feature.",
        tags: new[] { "feature", "feature layer", "popup", "web map" })]
    public partial class ShowPopup
    {
        private FeatureLayer _featureLayer;
        private bool _isIdentifying;

        public ShowPopup()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Create and load the web map that contains predefined popups.
                Map map = new Map(new Uri("https://arcgisruntime.maps.arcgis.com/home/item.html?id=9f3a674e998f461580006e626611f9ad"));
                await map.LoadAsync();

                // Get the layer whose features define the popups.
                _featureLayer = map.OperationalLayers.OfType<FeatureLayer>().First();

                // Listen for taps after the map and its operational layer are ready.
                MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
                MyMapView.Map = map;
            }
            catch (Exception ex)
            {
                await new MessageDialog2(ex.Message, "Error").ShowAsync();
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            if (_featureLayer == null || _isIdentifying) return;

            _isIdentifying = true;
            try
            {
                // Identify the first feature at the tapped location and request its predefined popup.
                _featureLayer.ClearSelection();
                IdentifyLayerResult result = await MyMapView.IdentifyLayerAsync(_featureLayer, e.Position, 12, false);

                if (result.GeoElements.FirstOrDefault() is not Feature feature ||
                    result.Popups.FirstOrDefault() is not Popup popup)
                {
                    return;
                }

                // Select the identified feature and display its popup.
                _featureLayer.SelectFeature(feature);
                PopupViewer.Popup = popup;
                PopupBackground.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                await new MessageDialog2(ex.Message, "Error").ShowAsync();
            }
            finally
            {
                _isIdentifying = false;
            }
        }

        private void ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            PopupBackground.Visibility = Visibility.Collapsed;
            PopupViewer.Popup = null;
            _featureLayer?.ClearSelection();
        }
    }
}