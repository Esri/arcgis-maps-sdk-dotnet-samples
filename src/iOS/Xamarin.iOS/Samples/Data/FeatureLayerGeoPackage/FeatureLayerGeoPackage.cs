// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Linq;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureLayerGeoPackage
{
    [Register("FeatureLayerGeoPackage")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("68ec42517cdd439e81b036210483e8e7")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Feature layer (GeoPackage)",
        category: "Data",
        description: "Display features from a local GeoPackage.",
        instructions: "Pan and zoom around the map. View the data loaded from the geopackage.",
        tags: new[] { "OGC", "feature table", "geopackage", "gpkg", "package", "standards" })]
    public class FeatureLayerGeoPackage : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        public FeatureLayerGeoPackage()
        {
            Title = "Feature layer (GeoPackage)";
        }

        private async void Initialize()
        {
            // Create a new map centered on Aurora Colorado.
            _myMapView.Map = new Map(BasemapStyle.ArcGISLightGray);
            _myMapView.Map.InitialViewpoint = new Viewpoint(39.7294, -104.8319, 9);

            // Get the full path.
            string geoPackagePath = DataManager.GetDataFolder("68ec42517cdd439e81b036210483e8e7", "AuroraCO.gpkg");

            try
            {
                // Open the GeoPackage.
                GeoPackage geoPackage = await GeoPackage.OpenAsync(geoPackagePath);

                // Read the feature tables and get the first one.
                FeatureTable geoPackageTable = geoPackage.GeoPackageFeatureTables.FirstOrDefault();

                // Make sure a feature table was found in the package.
                if (geoPackageTable == null)
                {
                    return;
                }

                // Create a layer to show the feature table.
                FeatureLayer newLayer = new FeatureLayer(geoPackageTable);
                await newLayer.LoadAsync();

                // Add the feature table as a layer to the map (with default symbology).
                _myMapView.Map.OperationalLayers.Add(newLayer);
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