// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Windows.Threading;

namespace ArcGISRuntime.WPF.Samples.DisplayDrawingStatus
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display drawing status",
        "MapView",
        "This sample demonstrates how to use the DrawStatus value of the MapView to notify user that the MapView is drawing.",
        "")]
    public partial class DisplayDrawingStatus
    {
        public DisplayDrawingStatus()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Hook up the DrawStatusChanged event
            MyMapView.DrawStatusChanged += OnDrawStatusChanged;

            // Create new Map with basemap
            Map myMap = new Map(BasemapType.Topographic, 34.056, -117.196, 4);

            // Create uri to the used feature service
            var serviceUri = new Uri(
                "http://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0");

            // Initialize a new feature layer
            ServiceFeatureTable myFeatureTable = new ServiceFeatureTable(serviceUri);
            FeatureLayer myFeatureLayer = new FeatureLayer(myFeatureTable);

            // Add the feature layer to the Map
            myMap.OperationalLayers.Add(myFeatureLayer);

            // Provide used Map to the MapView
            MyMapView.Map = myMap;
        }

        private void OnDrawStatusChanged(object sender, DrawStatusChangedEventArgs e)
        {
            // Update the load status information
            Dispatcher.Invoke(delegate ()
            {
                // Show the activity indicator if the map is drawing
                if (e.Status == DrawStatus.InProgress)
                {
                    activityIndicator.IsEnabled = true;
                    activityIndicator.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    activityIndicator.IsEnabled = false;
                    activityIndicator.Visibility = System.Windows.Visibility.Collapsed;
                }
            });
        }
    }
}
