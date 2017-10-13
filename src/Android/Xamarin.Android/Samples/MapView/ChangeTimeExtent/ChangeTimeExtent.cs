// Copyright 2017 Esri.
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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime;
using System;

namespace ArcGISRuntimeXamarin.Samples.ChangeTimeExtent
{
    [Activity]
    public class ChangeTimeExtent : Activity
    {
        // Hold two map service URIs, one for use with a ArcGISMapImageLayer, the other for use with a FeatureLayer
        private Uri _mapServerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer");

        private Uri _featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer/1");

        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Create and hold references to the interval buttons
        private Button _intervalButton1;

        private Button _intervalButton2;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Change time extent";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the interval buttons
            _intervalButton1 = new Button(this);
            _intervalButton1.Text = "Interval 1";
            _intervalButton2 = new Button(this);
            _intervalButton2.Text = "Interval 2";

            // Subscribe to interval button clicks
            _intervalButton1.Click += _intervalButton1_Click;
            _intervalButton2.Click += _intervalButton2_Click;

            // Add buttons to the layout
            layout.AddView(_intervalButton1);
            layout.AddView(_intervalButton2);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

        private void Initialize()
        {
            // Create new Map with basemap and initial location
            Map myMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView
            _myMapView.Map = myMap;

            // Load the layers from the corresponding URIs
            ArcGISMapImageLayer myImageryLayer = new ArcGISMapImageLayer(_mapServerUri);
            FeatureLayer myFeatureLayer = new FeatureLayer(_featureLayerUri);

            // Add the layers to the map
            _myMapView.Map.OperationalLayers.Add(myImageryLayer);
            _myMapView.Map.OperationalLayers.Add(myFeatureLayer);
        }

        private void _intervalButton1_Click(object sender, EventArgs e)
        {
            // Hard-coded start value: August 4th, 2000
            DateTime start = new DateTime(2000, 8, 4);

            // Hard-coded end value: September 4th, 2000
            DateTime end = new DateTime(2000, 9, 4);

            // Set the time extent on the map with the hard-coded values
            _myMapView.TimeExtent = new TimeExtent(new DateTimeOffset(start), new DateTimeOffset(end));
        }

        private void _intervalButton2_Click(object sender, EventArgs e)
        {
            // Hard-coded start value: September 22nd, 2000
            DateTime start = new DateTime(2000, 9, 22);

            // Hard-coded end value: October 22nd, 2000
            DateTime end = new DateTime(2000, 10, 22);

            // Set the time extent on the map with the hard-coded values
            _myMapView.TimeExtent = new TimeExtent(start, end);
        }
    }
}