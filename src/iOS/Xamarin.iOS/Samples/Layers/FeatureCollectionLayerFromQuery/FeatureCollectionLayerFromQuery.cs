// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureCollectionLayerFromQuery
{
    [Register("FeatureCollectionLayerFromQuery")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature collection layer (query)",
        "Layers",
        "Create a feature collection layer to show a query result from a service feature table.",
        "When launched, this sample displays a map with point features as a feature collection layer. Pan and zoom to explore the map.",
        "layer", "query", "search", "table")]
    public class FeatureCollectionLayerFromQuery : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        // URL for a feature service layer to query.
        private const string FeatureLayerUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Wildfire/FeatureServer/0";

        public FeatureCollectionLayerFromQuery()
        {
            Title = "Add a query result as a feature collection layer";
        }

        private void Initialize()
        {
            // Create a new map with the oceans basemap and add it to the map view
            _myMapView.Map = new Map(Basemap.CreateOceans());

            // Call a function that will create a new feature collection layer from a service query
            GetFeaturesFromQuery();
        }

        private async void GetFeaturesFromQuery()
        {
            try
            {
                // Create a service feature table to get features from.
                ServiceFeatureTable featTable = new ServiceFeatureTable(new Uri(FeatureLayerUrl));

                // Create a query to get all features in the table.
                QueryParameters queryParams = new QueryParameters
                {
                    WhereClause = "1=1"
                };

                // Query the table to get all features.
                FeatureQueryResult featureResult = await featTable.QueryFeaturesAsync(queryParams);

                // Create a new feature collection table from the result features.
                FeatureCollectionTable collectTable = new FeatureCollectionTable(featureResult);

                // Create a feature collection and add the table.
                FeatureCollection featCollection = new FeatureCollection();
                featCollection.Tables.Add(collectTable);

                // Create a layer to display the feature collection, add it to the map's operational layers.
                FeatureCollectionLayer featureCollectionLayer = new FeatureCollectionLayer(featCollection);
                _myMapView.Map.OperationalLayers.Add(featureCollectionLayer);

                // Zoom to the extent of the feature collection layer.
                await featureCollectionLayer.LoadAsync();
                await _myMapView.SetViewpointGeometryAsync(featureCollectionLayer.FullExtent, 50);
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }
    }
}