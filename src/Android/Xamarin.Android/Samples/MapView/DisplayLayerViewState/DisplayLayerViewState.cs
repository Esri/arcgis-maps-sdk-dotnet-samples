// Copyright 2016 Esri.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using Android.App;
using Android.OS;
using Android.Widget;

namespace ArcGISRuntime.Samples.DisplayLayerViewState
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display layer view state",
        "MapView",
        "This sample demonstrates how to get view status for layers in a map.",
        "")]
    public class DisplayLayerViewState : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Controls to show status of each layers' loading
        private TextView _TextViewTiledLayer;
        private TextView _TextViewImageLayer;
        private TextView _TextViewFeatureLayer;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Display Layer View State";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map
            Map myMap = new Map();

            // Create the uri for the tiled layer
            var tiledLayerUri = new Uri(
                "http://sampleserver6.arcgisonline.com/arcgis/rest/services/WorldTimeZones/MapServer");

            // Create a tiled layer using url
            ArcGISTiledLayer tiledLayer = new ArcGISTiledLayer(tiledLayerUri);
            tiledLayer.Name = "Tiled Layer";

            // Add the tiled layer to map
            myMap.OperationalLayers.Add(tiledLayer);

            // Create the uri for the ArcGISMapImage layer
            var imageLayerUri = new Uri(
                "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer");

            // Create ArcGISMapImage layer using a url
            ArcGISMapImageLayer imageLayer = new ArcGISMapImageLayer(imageLayerUri);
            imageLayer.Name = "Image Layer";

            // Set the visible scale range for the image layer
            imageLayer.MinScale = 40000000;
            imageLayer.MaxScale = 2000000;

            // Add the image layer to map
            myMap.OperationalLayers.Add(imageLayer);

            // Create Uri for feature layer
            var featureLayerUri = new Uri(
                "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Recreation/FeatureServer/0");

            // Create a feature layer using url
            FeatureLayer myFeatureLayer = new FeatureLayer(featureLayerUri);
            myFeatureLayer.Name = "Feature Layer";

            // Add the feature layer to map
            myMap.OperationalLayers.Add(myFeatureLayer);

            // Create a map point the map should zoom to
            MapPoint mapPoint = new MapPoint(-11000000, 4500000, SpatialReferences.WebMercator);

            // Set the initial viewpoint for map
            myMap.InitialViewpoint = new Viewpoint(mapPoint, 50000000);

            // Event for layer view state changed
            _myMapView.LayerViewStateChanged += OnLayerViewStateChanged;

            // Provide used Map to the MapView
            _myMapView.Map = myMap;
        }

        private void OnLayerViewStateChanged(object sender, LayerViewStateChangedEventArgs e)
        {
            // For each execution of the MapView.LayerViewStateChanged Event, get the name of
            // the layer and its LayerViewState.Status
            string lName = e.Layer.Name;
            string lViewStatus = e.LayerViewState.Status.ToString();

            // Display the layer name and view status in the appropriate TextView control
            switch (lName)
            {
                case "Tiled Layer":
                    _TextViewTiledLayer.Text = lName + " - " + lViewStatus;
                    break;
                case "Image Layer":
                    _TextViewImageLayer.Text = lName + " - " + lViewStatus;
                    break;
                case "Feature Layer":
                    _TextViewFeatureLayer.Text = lName + " - " + lViewStatus;
                    break;
                default:
                    break;
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the controls to show the various layers' loading status
            _TextViewTiledLayer = new TextView(this);
            layout.AddView(_TextViewTiledLayer);
            _TextViewImageLayer = new TextView(this);
            layout.AddView(_TextViewImageLayer);
            _TextViewFeatureLayer = new TextView(this);
            layout.AddView(_TextViewFeatureLayer);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}