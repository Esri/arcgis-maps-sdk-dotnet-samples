// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntime.Samples.ReadGeoPackage
{
    [Register("ReadGeoPackage")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Read a GeoPackage",
        "Data",
        "Add rasters and feature tables from GeoPackages to a map.",
        "When the sample loads, the feature tables and rasters from the GeoPackage will be shown on the map.")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("68ec42517cdd439e81b036210483e8e7")]
    public class ReadGeoPackage : UIViewController
    {
        // Hold references to the UI controls.
        private MapView _myMapView;

        public ReadGeoPackage()
        {
            Title = "Read a GeoPackage";
        }

        private async void Initialize()
        {
            // Create a new map centered on Aurora Colorado.
            _myMapView.Map = new Map(BasemapType.Streets, 39.7294, -104.70, 11);

            // Get the full path to the GeoPackage on the device.
            string geoPackagePath = DataManager.GetDataFolder("68ec42517cdd439e81b036210483e8e7", "AuroraCO.gpkg");

            try
            {
                // Open the GeoPackage.
                GeoPackage geoPackage = await GeoPackage.OpenAsync(geoPackagePath);

                // Loop through each GeoPackageRaster.
                foreach (GeoPackageRaster oneGeoPackageRaster in geoPackage.GeoPackageRasters)
                {
                    // Create a RasterLayer from the GeoPackageRaster.
                    RasterLayer rasterLayer = new RasterLayer(oneGeoPackageRaster)
                    {
                        // Set the opacity on the RasterLayer to partially visible.
                        Opacity = 0.55
                    };

                    // Add the layer to the map.
                    _myMapView.Map.OperationalLayers.Add(rasterLayer);
                }

                // Loop through each GeoPackageFeatureTable from the GeoPackage.
                foreach (GeoPackageFeatureTable oneGeoPackageFeatureTable in geoPackage.GeoPackageFeatureTables)
                {
                    // Create a FeatureLayer from the GeoPackageFeatureLayer.
                    FeatureLayer featureLayer = new FeatureLayer(oneGeoPackageFeatureTable);

                    // Add the layer to the map.
                    _myMapView.Map.OperationalLayers.Add(featureLayer);
                }
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
            View = new UIView();

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView);

            // Lay out the views
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            });
        }
    }
}