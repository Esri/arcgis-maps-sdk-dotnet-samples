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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using Android.Views;
using Esri.ArcGISRuntime.Geometry;

namespace ArcGISRuntime.Samples.StyleWmsLayer
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Style WMS layers",
        "Layers",
        "Change the style of a Web Map Service (WMS) layer.",
        "Once the layer loads, the toggle button will be enabled. Tap it to toggle between the first and second styles of the WMS layer.",
        "WMS", "imagery", "styles", "visualization")]
    public class StyleWmsLayer : Activity
    {
        // Hold the URL to the service, which has satellite imagery covering the state of Minnesota.
        private Uri _wmsUrl = new Uri("https://imageserver.gisdata.mn.gov/cgi-bin/mncomp?SERVICE=WMS&VERSION=1.3.0&REQUEST=GetCapabilities");

        // Hold a list of uniquely-identifying WMS layer names to display.
        private List<String> _wmsLayerNames = new List<string> { "mncomp" };

        // Hold a reference to the layer to enable re-styling.
        private WmsLayer _mnWmsLayer;

        // Hold references to the UI controls.
        private MapView _myMapView;
        private Button _firstStyleButton;
        private Button _secondStyleButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Style WMS layers";

            // Create the UI, setup the control references.
            CreateLayout();

            // Initialize the map.
            InitializeAsync();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the UI components.
            TextView helpLabel = new TextView(this)
            {
                Text = "Choose a style",
                TextAlignment = TextAlignment.Center
            };
            _firstStyleButton = new Button(this)
            {
                Text = "Default",
                Enabled = false
            };
            _secondStyleButton = new Button(this)
            {
                Text = "Contrast stretch",
                Enabled = false
            };

            // Subscribe to events.
            _firstStyleButton.Click += FirstStyleButton_Clicked;
            _secondStyleButton.Click += SecondStyleButton_Clicked;

            // Add the views to the layout.
            layout.AddView(helpLabel);
            layout.AddView(_firstStyleButton);
            layout.AddView(_secondStyleButton);
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }

        private async void InitializeAsync()
        {
            try
            {
                // Create a map with spatial reference appropriate for the service.
                Map myMap = new Map(SpatialReference.Create(26915)) {MinScale = 7000000.0};

                // Create a new WMS layer displaying the specified layers from the service.
                // The default styles are chosen by default.
                _mnWmsLayer = new WmsLayer(_wmsUrl, _wmsLayerNames);

                // Wait for the layer to load.
                await _mnWmsLayer.LoadAsync();

                // Center the map on the layer's contents.
                myMap.InitialViewpoint = new Viewpoint(_mnWmsLayer.FullExtent);

                // Add the layer to the map.
                myMap.OperationalLayers.Add(_mnWmsLayer);

                // Add the map to the view.
                _myMapView.Map = myMap;

                // Enable the buttons.
                _firstStyleButton.Enabled = true;
                _secondStyleButton.Enabled = true;
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private void FirstStyleButton_Clicked(object sender, EventArgs e)
        {
            // Get the available styles from the first sublayer.
            IReadOnlyList<string> styles = _mnWmsLayer.Sublayers[0].SublayerInfo.Styles;

            // Apply the first style to the first sublayer.
            _mnWmsLayer.Sublayers[0].CurrentStyle = styles[0];
        }

        private void SecondStyleButton_Clicked(object sender, EventArgs e)
        {
            // Get the available styles from the first sublayer.
            IReadOnlyList<string> styles = _mnWmsLayer.Sublayers[0].SublayerInfo.Styles;

            // Apply the second style to the first sublayer.
            _mnWmsLayer.Sublayers[0].CurrentStyle = styles[1];
        }
    }
}