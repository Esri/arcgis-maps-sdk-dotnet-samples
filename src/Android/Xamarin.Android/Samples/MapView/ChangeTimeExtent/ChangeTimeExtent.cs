// Copyright 2018 Esri.
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
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntime.Samples.ChangeTimeExtent
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change time extent",
        "MapView",
        "This sample demonstrates how to filter data in layers by applying a time extent to a MapView.",
        "Switch between the available options and observe how the data is filtered.")]
    public class ChangeTimeExtent : Activity
    {
        // Hold two map service URIs, one for use with an ArcGISMapImageLayer, the other for use with a FeatureLayer.
        private readonly Uri _mapServerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer");
        private readonly Uri _featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Earthquakes_Since1970/MapServer/0");

        // Create and hold reference to the MapView.
        private readonly MapView _myMapView = new MapView();

        // Create and hold references to the interval buttons.
        private Button _twoThousandButton;
        private Button _twoThousandFiveButton;

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
            // Create new Map with basemap and initial location.
            Map map = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView.
            _myMapView.Map = map;

            // Load the layers from the corresponding URIs.
            ArcGISMapImageLayer imageryLayer = new ArcGISMapImageLayer(_mapServerUri);
            FeatureLayer pointLayer = new FeatureLayer(_featureLayerUri);

            // Add the layers to the map.
            _myMapView.Map.OperationalLayers.Add(imageryLayer);
            _myMapView.Map.OperationalLayers.Add(pointLayer);
        }

        private void TwoThousandButtonClick(object sender, EventArgs e)
        {
            // Hard-coded start value: January 1st, 2000.
            DateTime start = new DateTime(2000, 1, 1);

            // Hard-coded end value: December 31st, 2000.
            DateTime end = new DateTime(2000, 12, 31);

            // Set the time extent on the map with the hard-coded values.
            _myMapView.TimeExtent = new TimeExtent(start, end);
        }

        private void TwoThousandFiveButton_Click(object sender, EventArgs e)
        {
            // Hard-coded start value: January 1st, 2005.
            DateTime start = new DateTime(2005, 1, 1);

            // Hard-coded end value: December 31st, 2005.
            DateTime end = new DateTime(2005, 12, 31);

            // Set the time extent on the map with the hard-coded values.
            _myMapView.TimeExtent = new TimeExtent(start, end);
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create and add a help tip.
            TextView helpLabel = new TextView(this)
            {
                Text = "Tap a year to filter the data."
            };
            layout.AddView(helpLabel);

            // Create the interval buttons.
            _twoThousandButton = new Button(this) { Text = "2000" };
            _twoThousandFiveButton = new Button(this) { Text = "2005" };

            // Subscribe to interval button clicks.
            _twoThousandButton.Click += TwoThousandButtonClick;
            _twoThousandFiveButton.Click += TwoThousandFiveButton_Click;

            // Add buttons to the layout.
            layout.AddView(_twoThousandButton);
            layout.AddView(_twoThousandFiveButton);

            // Add the map view to the layout.
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}