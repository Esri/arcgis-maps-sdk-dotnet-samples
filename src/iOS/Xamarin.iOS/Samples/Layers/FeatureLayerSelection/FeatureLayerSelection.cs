// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Foundation;
using System;
using System.Drawing;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.FeatureLayerSelection
{
    [Register("FeatureLayerSelection")]
    public class FeatureLayerSelection : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView;

        // Create and hold reference to the feature layer
        private FeatureLayer _featureLayer;

        public FeatureLayerSelection()
        {
            Title = "Feature layer Selection";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
           
            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            var myMap = new Map(Basemap.CreateTopographic());

            // Create envelope to be used as a target extent for map's initial viewpoint
            Envelope myEnvelope = new Envelope(
                -1131596.019761, 3893114.069099, 3926705.982140, 7977912.461790, 
                SpatialReferences.WebMercator);

            // Set the initial viewpoint for map
            myMap.InitialViewpoint = new Viewpoint(myEnvelope);

            // Provide used Map to the MapView
            _myMapView.Map = myMap;

            // Create Uri for the feature service
            Uri featureServiceUri = new Uri(
                "http://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0");

            // Initialize feature table using a url to feature server url
            var featureTable = new ServiceFeatureTable(featureServiceUri);

            // Initialize a new feature layer based on the feature table
            _featureLayer = new FeatureLayer(featureTable);

            // Set the selection color for feature layer
            _featureLayer.SelectionColor = Color.Cyan;

            // Set the selection width
            _featureLayer.SelectionWidth = 3;

            // Make sure that used feature layer is loaded before we hook into the tapped event
            // This prevents us trying to do selection on the layer that isn't initialized
            await _featureLayer.LoadAsync();

            // Check for the load status. If the layer is loaded then add it to map
            if (_featureLayer.LoadStatus == LoadStatus.Loaded)
            {
                // Add the feature layer to the map
                myMap.OperationalLayers.Add(_featureLayer);

                // Add tap event handler for mapview
                _myMapView.GeoViewTapped += OnMapViewTapped;
            }
        }

        private async void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Define the selection tolerance
            double tolerance = 5;

            // Convert the tolerance to map units
            double mapTolerance = tolerance * _myMapView.UnitsPerPixel;

            // Define the envelope around the tap location for selecting features
            var selectionEnvelope = new Envelope(e.Location.X - mapTolerance, e.Location.Y - mapTolerance, e.Location.X + mapTolerance, 
                e.Location.Y + mapTolerance, _myMapView.Map.SpatialReference);

            // Define the query parameters for selecting features
            var queryParams = new QueryParameters();
            
            // Set the geometry to selection envelope for selection by geometry
            queryParams.Geometry = selectionEnvelope;

            // Select the features based on query parameters defined above
            await _featureLayer.SelectFeaturesAsync(queryParams, SelectionMode.New);
        }

        private void CreateLayout()
        {
            // Setup the visual frame for the MapView
            _myMapView = new MapView()
            {
                Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height)
            };

            // Add MapView to the page
            View.AddSubviews(_myMapView);
        }
    }
}