// Copyright 2017 Esri.
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
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System.Linq;
using UIKit;

namespace ArcGISRuntime.Samples.RasterLayerGeoPackage
{
    [Register("RasterLayerGeoPackage")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("68ec42517cdd439e81b036210483e8e7")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Raster layer (GeoPackage)",
        category: "Data",
        description: "Display a raster contained in a GeoPackage.",
        instructions: "When the sample starts, a raster will be loaded from a GeoPackage and displayed in the map view.",
        tags: new[] { "OGC", "container", "data", "image", "import", "layer", "package", "raster", "visualization" })]
    public class RasterLayerGeoPackage : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        public RasterLayerGeoPackage()
        {
            Title = "Raster layer (GeoPackage)";
        }

        private async void Initialize()
        {
            // Create a new map.
            _myMapView.Map = new Map(BasemapStyle.ArcGISLightGray);

            // Get the GeoPackage path.
            string geoPackagePath = DataManager.GetDataFolder("68ec42517cdd439e81b036210483e8e7", "AuroraCO.gpkg");

            try
            {
                // Open the GeoPackage.
                GeoPackage geoPackage = await GeoPackage.OpenAsync(geoPackagePath);

                // Read the raster images and get the first one.
                Raster gpkgRaster = geoPackage.GeoPackageRasters.FirstOrDefault();

                // Make sure an image was found in the package.
                if (gpkgRaster == null)
                {
                    return;
                }

                // Create a layer to show the raster.
                RasterLayer newLayer = new RasterLayer(gpkgRaster);
                await newLayer.LoadAsync();

                // Set the viewpoint.
                await _myMapView.SetViewpointAsync(new Viewpoint(newLayer.FullExtent));

                // Add the image as a raster layer to the map (with default symbology).
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