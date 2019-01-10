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

namespace ArcGISRuntime.Samples.ReadShapefileMetadata
{
    [Register("ReadShapefileMetadata")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("d98b3e5293834c5f852f13c569930caa")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Read shapefile metadata",
        "Data",
        "This sample demonstrates how to open a shapefile stored on the device, read metadata that describes the dataset, and display it as a feature layer with default symbology.",
        "The shapefile will be downloaded from an ArcGIS Online portal automatically.",
        "Featured")]
    [ArcGISRuntime.Samples.Shared.Attributes.ClassFile("MetadataDisplayViewController.cs")]
    public class ReadShapefileMetadata : UIViewController
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _showMetadataButton;

        // Store the shapefile metadata.
        private ShapefileInfo _shapefileMetadata;

        public ReadShapefileMetadata()
        {
            Title = "Read shapefile metadata";
        }

        private async void Initialize()
        {
            // Create a new map to display in the map view with a streets basemap.
            Map streetMap = new Map(Basemap.CreateStreetsVector());

            // Get the path to the downloaded shapefile.
            string filepath = DataManager.GetDataFolder("d98b3e5293834c5f852f13c569930caa", "TrailBikeNetwork.shp");

            try
            {
                // Open the shapefile.
                ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(filepath);

                // Read metadata about the shapefile and display it in the UI.
                _shapefileMetadata = myShapefile.Info;

                // Create a feature layer to display the shapefile.
                FeatureLayer newFeatureLayer = new FeatureLayer(myShapefile);

                // Zoom the map to the extent of the shapefile.
                _myMapView.SpatialReferenceChanged += async (s, e) => { await _myMapView.SetViewpointGeometryAsync(newFeatureLayer.FullExtent); };

                // Add the feature layer to the map.
                streetMap.OperationalLayers.Add(newFeatureLayer);

                // Show the map in the MapView.
                _myMapView.Map = streetMap;
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private void OnMetadataButtonTouch(object sender, EventArgs e)
        {
            NavigationController.PushViewController(
                new MetadataDisplayViewController(_shapefileMetadata), true);
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
            View.BackgroundColor = UIColor.White;

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _showMetadataButton = new UIBarButtonItem("See metadata", UIBarButtonItemStyle.Plain, OnMetadataButtonTouch);

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _showMetadataButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }
    }
}