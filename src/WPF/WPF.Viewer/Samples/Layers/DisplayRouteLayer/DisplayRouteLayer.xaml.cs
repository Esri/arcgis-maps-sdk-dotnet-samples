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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime;
using System.Windows.Controls;

namespace ArcGIS.WPF.Samples.DisplayRouteLayer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        "Display route layer",
        "Layers",
        "Display a route layer and its directions using feature collection.",
        "")]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class DisplayRouteLayer
    {
        private readonly string _itemId = "0e3c8e86b4544274b45ecb61c9f41336";

        public DisplayRouteLayer()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {

            // Create a new map with the topographic basemap style.
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

            try
            {
                // Set the portal.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();

                // Get the portal item containing route data.
                PortalItem item = await PortalItem.CreateAsync(portal, _itemId);

                // Create a collection of feature layers, then load.
                var featureCollection = new FeatureCollection(item);
                await featureCollection.LoadAsync();

                if (featureCollection.LoadStatus == LoadStatus.Loaded)
                {
                    // Select all tables.
                    IList<FeatureCollectionTable> tables = featureCollection.Tables;
                    
                    // List the turn by turn directions.
                    var directionPoints = tables.Where(t => t.TableName == "DirectionPoints").FirstOrDefault();
                    DirectionsList.ItemsSource = directionPoints;

                    // Add the feature collection layers to the map.
                    MyMapView.Map.OperationalLayers.Add(new FeatureCollectionLayer(featureCollection));

                    // Set the viewpoint to Oregon, US.
                    await MyMapView.SetViewpointAsync(new Viewpoint(45.2281, -122.8309, 57e4));

                    // Make controls relating to UI visible.
                    Border.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                // Report exceptions.
                MessageBox.Show("Error: " + ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
