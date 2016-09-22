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
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Drawing;

namespace ArcGISRuntimeXamarin.Samples.ChangeFeatureLayerRenderer
{
    [Activity]
    public class ChangeFeatureLayerRenderer : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Create and hold reference to the feature layer
        private FeatureLayer _featureLayer;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Change feature layer renderer";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
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
            var serviceUri = new Uri(
               "http://sampleserver6.arcgisonline.com/arcgis/rest/services/PoolPermits/FeatureServer/0");

            // Initialize feature table using a url to feature server url
            ServiceFeatureTable featureTable = new ServiceFeatureTable(serviceUri);

            // Initialize a new feature layer based on the feature table
            _featureLayer = new FeatureLayer(featureTable);

            // Make sure that the feature layer gets loaded
            await _featureLayer.LoadAsync();

            // Check for the load status. If the layer is loaded then add it to map
            if (_featureLayer.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                // Add the feature layer to the map
                myMap.OperationalLayers.Add(_featureLayer);
            }
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
        }

        private void OnResetButtonClicked(object sender, EventArgs e)
        {
            // Reset the renderer to default
            _featureLayer.ResetRenderer();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a button to reset the renderer
            var resetButton = new Button(this);
            resetButton.Text = "Reset";
            resetButton.Click += OnResetButtonClicked;

            // Create a button to apply new renderer
            var overrideButton = new Button(this);
            overrideButton.Text = "Override";
            overrideButton.Click += OnOverrideButtonClicked;

            // Add Reset Button to the layout
            layout.AddView(resetButton);

            // Add Override Button to the layout
            layout.AddView(overrideButton);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}