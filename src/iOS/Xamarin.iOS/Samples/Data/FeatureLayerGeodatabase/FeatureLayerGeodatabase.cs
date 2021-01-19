// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureLayerGeodatabase
{
    [Register("FeatureLayerGeodatabase")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("2b0f9e17105847809dfeb04e3cad69e0")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Feature layer (geodatabase)",
        category: "Data",
        description: "Display features from a local geodatabase.",
        instructions: "",
        tags: new[] { "geodatabase", "mobile", "offline" })]
    public class FeatureLayerGeodatabase : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        public FeatureLayerGeodatabase()
        {
            Title = "Feature layer (geodatabase)";
        }

        private async void Initialize()
        {
            // Create a new map to display in the map view with a streets basemap.
            _myMapView.Map = new Map(BasemapStyle.ArcGISStreets);

            // Get the path to the downloaded mobile geodatabase (.geodatabase file).
            string mobileGeodatabaseFilePath = DataManager.GetDataFolder("2b0f9e17105847809dfeb04e3cad69e0", "LA_Trails.geodatabase");

            try
            {
                // Open the mobile geodatabase.
                Geodatabase mobileGeodatabase = await Geodatabase.OpenAsync(mobileGeodatabaseFilePath);

                // Get the 'Trailheads' geodatabase feature table from the mobile geodatabase.
                GeodatabaseFeatureTable trailheadsGeodatabaseFeatureTable = mobileGeodatabase.GeodatabaseFeatureTable("Trailheads");

                // Asynchronously load the 'Trailheads' geodatabase feature table.
                await trailheadsGeodatabaseFeatureTable.LoadAsync();

                // Create a feature layer based on the geodatabase feature table.
                FeatureLayer trailheadsFeatureLayer = new FeatureLayer(trailheadsGeodatabaseFeatureTable);

                // Add the feature layer to the operations layers collection of the map.
                _myMapView.Map.OperationalLayers.Add(trailheadsFeatureLayer);

                // Zoom the map to the extent of the feature layer.
                await _myMapView.SetViewpointGeometryAsync(trailheadsFeatureLayer.FullExtent, 50);
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
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

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