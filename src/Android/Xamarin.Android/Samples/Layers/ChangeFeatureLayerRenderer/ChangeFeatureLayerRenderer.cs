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
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Drawing;

namespace ArcGISRuntime.Samples.ChangeFeatureLayerRenderer
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Change feature layer renderer",
        category: "Layers",
        description: "Change the appearance of a feature layer with a renderer.",
        instructions: "Use the buttons to change the renderer on the feature layer. The original renderer displays orange circles, the diameters of which are proportional to carbon storage of each tree. When the blue renderer in this sample is applied, it displays the location of the trees simply as blue points.",
        tags: new[] { "feature layer", "renderer", "visualization" })]
    public class ChangeFeatureLayerRenderer : Activity
    {
        // Hold a reference to the map view
        private MapView _myMapView;

        // Create and hold reference to the feature layer
        private FeatureLayer _featureLayer;

        private Button _overrideButton;

        private Button _resetButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Change feature layer renderer";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create and set initial map area
            Envelope initialLocation = new Envelope(
                -1.30758164047166E7, 4014771.46954516, -1.30730056797177E7, 4016869.78617381,
                SpatialReferences.WebMercator);

            // Set the initial viewpoint for map
            myMap.InitialViewpoint = new Viewpoint(initialLocation);

            // Provide used Map to the MapView
            _myMapView.Map = myMap;

            // Create uri to the used feature service
            Uri serviceUri = new Uri(
               "https://sampleserver6.arcgisonline.com/arcgis/rest/services/PoolPermits/FeatureServer/0");

            // Initialize feature table using a url to feature server url
            ServiceFeatureTable featureTable = new ServiceFeatureTable(serviceUri);

            // Initialize a new feature layer based on the feature table
            _featureLayer = new FeatureLayer(featureTable);
            myMap.OperationalLayers.Add(_featureLayer);
        }

        private void OnOverrideButtonClicked(object sender, EventArgs e)
        {
            // Create a symbol to be used in the renderer
            SimpleLineSymbol symbol = new SimpleLineSymbol()
            {
                Color = Color.Blue,
                Width = 2,
                Style = SimpleLineSymbolStyle.Solid
            };

            // Create a new renderer using the symbol just created
            SimpleRenderer renderer = new SimpleRenderer(symbol);

            // Assign the new renderer to the feature layer
            _featureLayer.Renderer = renderer;

            // Enable the reset button.
            _overrideButton.Enabled = false;
            _resetButton.Enabled = true;
        }

        private void OnResetButtonClicked(object sender, EventArgs e)
        {
            // Reset the renderer to default
            _featureLayer.ResetRenderer();

            // Re-enable the override button.
            _overrideButton.Enabled = true;
            _resetButton.Enabled = false;
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a button to apply new renderer
            _overrideButton = new Button(this)
            {
                Text = "Override"
            };
            _overrideButton.Click += OnOverrideButtonClicked;

            // Create a button to reset the renderer
            _resetButton = new Button(this)
            {
                Text = "Reset",
                Enabled = false
            };
            _resetButton.Click += OnResetButtonClicked;

            // Add Override Button to the layout
            layout.AddView(_overrideButton);

            // Add Reset Button to the layout
            layout.AddView(_resetButton);

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}