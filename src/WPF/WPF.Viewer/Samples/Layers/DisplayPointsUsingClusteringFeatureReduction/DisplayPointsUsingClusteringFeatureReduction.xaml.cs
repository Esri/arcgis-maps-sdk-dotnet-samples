// Copyright 2023 Esri.
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
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Reduction;

namespace ArcGIS.WPF.Samples.DisplayPointsUsingClusteringFeatureReduction
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        "Display points using clustering feature reduction",
        "Layers",
        "Display a web map with a point feature layer that has feature reduction enabled to aggregate points into clusters.",
        "")]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class DisplayPointsUsingClusteringFeatureReduction
    {
        FeatureLayer _layer;
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
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            MyMapView.DismissCallout();

            // Identify the tapped observation.
            IdentifyLayerResult results = await MyMapView.IdentifyLayerAsync(_layer, e.Position, 3, false);
            if (results.GeoElements.Count == 0) return;

            var geoelement = results.GeoElements.FirstOrDefault();
            if (geoelement.GetType() == typeof(AggregateGeoElement))
            {
                
            }
            else
            {
                // ArcGISFeature
            }
        }

        // Enable feature reduction if the checkbox has been checked, disable otherwise.
        private void CheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            _layer.FeatureReduction.IsEnabled = (bool)(sender as CheckBox).IsChecked;
        }
    }
}
