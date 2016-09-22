// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;

namespace ArcGISRuntimeXamarin.Samples.DisplayDrawStatus
{
    [Activity]
    public class DisplayDrawStatus : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Create Control to show the drawing status
        ProgressDialog _activityIndicator;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Display draw status";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Hook up the DrawStatusChanged event
            _myMapView.DrawStatusChanged += OnDrawStatusChanged;

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
            _myMapView.Map = myMap;               
        }

        private void OnDrawStatusChanged(object sender, DrawStatus e)
        {
            // Make sure that the UI changes are done in the UI thread
            RunOnUiThread(() =>
            {
                // Show the activity indicator if the map is drawing
                if (e == DrawStatus.InProgress)
                {
                    _activityIndicator.SetMessage("Drawing is in progress");
                    _activityIndicator.Show();
                }
                else
                    _activityIndicator.Hide();
            });
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
            
            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Create an activity indicator
            _activityIndicator = new ProgressDialog(this);

            // Show the layout in the app
            SetContentView(layout);
        }     
    }
}