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
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.ChangeTimeExtent
{
    [Activity]
    public class ChangeTimeExtent : Activity
    {
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

        private void Initialize()
        {
            // Create new Map with basemap and initial location
            Map myMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView
            _myMapView.Map = myMap;
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

        private void _intervalButton1_Click(object sender, EventArgs e)
        {
        }

        private void _intervalButton2_Click(object sender, EventArgs e)
        {
        }
    }
}