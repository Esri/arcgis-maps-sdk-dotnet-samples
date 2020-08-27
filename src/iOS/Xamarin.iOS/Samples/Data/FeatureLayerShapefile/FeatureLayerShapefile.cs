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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureLayerShapefile
{
    [Register("FeatureLayerShapefile")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("d98b3e5293834c5f852f13c569930caa")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Feature layer (shapefile)",
        category: "Data",
        description: "Open a shapefile stored on the device and display it as a feature layer with default symbology.",
        instructions: "Pan and zoom around the map to observe the data from the shapefile.",
        tags: new[] { "layers", "shapefile", "shp", "vector" })]
    public class FeatureLayerShapefile : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        public FeatureLayerShapefile()
        {
            Title = "Feature layer (shapefile)";
        }

        private async void Initialize()
        {
            // Create a new map to display in the map view with a streets basemap.
            _myMapView.Map = new Map(Basemap.CreateStreetsVector());

            // Get the path to the downloaded shapefile.
            string filepath = DataManager.GetDataFolder("d98b3e5293834c5f852f13c569930caa", "Public_Art.shp");

            try
            {
                // Open the shapefile.
                ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(filepath);

                // Create a feature layer to display the shapefile.
                FeatureLayer newFeatureLayer = new FeatureLayer(myShapefile);

                // Add the feature layer to the map.
                _myMapView.Map.OperationalLayers.Add(newFeatureLayer);

                // Zoom the map to the extent of the shapefile.
                await _myMapView.SetViewpointGeometryAsync(newFeatureLayer.FullExtent, 50);
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
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

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