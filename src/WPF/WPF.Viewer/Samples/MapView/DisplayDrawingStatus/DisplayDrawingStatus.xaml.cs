// Copyright 2016 Esri.
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
using Esri.ArcGISRuntime.UI;
using System;
using System.Windows.Threading;

namespace ArcGIS.WPF.Samples.DisplayDrawingStatus
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display draw status",
        category: "MapView",
        description: "Get the draw status of your map view or scene view to know when all layers in the map or scene have finished drawing.",
        instructions: "Pan and zoom around the map. Observe how the status changes from a loading animation to solid, indicating that drawing has completed.",
        tags: new[] { "draw", "loading", "map", "render" })]
    public partial class DisplayDrawingStatus
    {
        public DisplayDrawingStatus()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization.
            Initialize();
        }

        private void Initialize()
        {
            // Hook up the DrawStatusChanged event.
            MyMapView.DrawStatusChanged += OnDrawStatusChanged;

            // Create new Map with basemap.
            Map myMap = new Map(BasemapStyle.ArcGISTopographic);

            // Create uri to the used feature service.
            Uri serviceUri = new Uri(
                "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0");

            // Initialize a new feature layer.
            ServiceFeatureTable myFeatureTable = new ServiceFeatureTable(serviceUri);
            FeatureLayer myFeatureLayer = new FeatureLayer(myFeatureTable);

            // Add the feature layer to the Map.
            myMap.OperationalLayers.Add(myFeatureLayer);

            // Provide used Map to the MapView.
            MyMapView.Map = myMap;

            // Zoom to the United States.
            MyMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);
        }

        private void OnDrawStatusChanged(object sender, DrawStatusChangedEventArgs e)
        {
            // Update the load status information.
            Dispatcher.Invoke(delegate ()
            {
                // Show the activity indicator if the map is drawing.
                if (e.Status == DrawStatus.InProgress)
                {
                    ActivityIndicator.IsEnabled = true;
                    ActivityIndicator.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    ActivityIndicator.IsEnabled = false;
                    ActivityIndicator.Visibility = System.Windows.Visibility.Collapsed;
                }
            });
        }
    }
}