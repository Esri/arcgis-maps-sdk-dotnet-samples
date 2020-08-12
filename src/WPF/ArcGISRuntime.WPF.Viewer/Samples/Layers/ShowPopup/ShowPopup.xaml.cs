﻿// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Popups;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Diagnostics;
using System.Linq;

namespace ArcGISRuntime.WPF.Samples.ShowPopup
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Show popup",
        category: "Layers",
        description: "Show predefined popups from a web map.",
        instructions: "Click on the features to prompt a popup that displays information about the feature.",
        tags: new[] { "feature", "feature layer", "popup", "toolkit", "web map" })]
    public partial class ShowPopup
    {
        public ShowPopup()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Load the map.
            MyMapView.Map = new Map(new Uri("https://arcgisruntime.maps.arcgis.com/home/item.html?id=fb788308ea2e4d8682b9c05ef641f273"));
        }

        private async void MapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Get the feature layer from the map.
                FeatureLayer incidentLayer = MyMapView.Map.OperationalLayers.First() as FeatureLayer;

                // Identify the tapped on feature.
                IdentifyLayerResult result = await MyMapView.IdentifyLayerAsync(incidentLayer, e.Position, 12, true);

                if (result?.Popups?.FirstOrDefault() is Popup popup)
                {
                    // Remove the instructions label.
                    InstructionsLabel.Visibility = System.Windows.Visibility.Hidden;

                    // Create a new popup manager for the popup.
                    MyPopupViewer.PopupManager = new PopupManager(popup);

                    QueryParameters queryParams = new QueryParameters
                    {
                        // Set the geometry to selection envelope for selection by geometry.
                        Geometry = new Envelope((MapPoint)popup.GeoElement.Geometry, 6, 6)
                    };

                    // Select the features based on query parameters defined above.
                    await incidentLayer.SelectFeaturesAsync(queryParams, SelectionMode.New);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }
    }
}