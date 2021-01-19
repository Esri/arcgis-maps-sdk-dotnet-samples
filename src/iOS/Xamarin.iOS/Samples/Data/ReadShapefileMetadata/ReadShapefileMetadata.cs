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
        name: "Read shapefile metadata",
        category: "Data",
        description: "Read a shapefile and display its metadata.",
        instructions: "The shapefile's metadata will be displayed when you open the sample.",
        tags: new[] { "credits", "description", "metadata", "package", "shape file", "shapefile", "summary", "symbology", "tags", "visualization" })]
    [ArcGISRuntime.Samples.Shared.Attributes.ClassFile("MetadataDisplayViewController.cs")]
    public class ReadShapefileMetadata : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _showMetadataButton;

        // Store the shapefile metadata.
        private ShapefileInfo _shapefileMetadata;

        // Hold a reference to the feature layer.
        private FeatureLayer _featureLayer;

        public ReadShapefileMetadata()
        {
            Title = "Read shapefile metadata";
        }

        private async void Initialize()
        {
            // Create a new map to display in the map view with a streets basemap.
            Map streetMap = new Map(BasemapStyle.ArcGISStreets);

            // Get the path to the downloaded shapefile.
            string filepath = DataManager.GetDataFolder("d98b3e5293834c5f852f13c569930caa", "TrailBikeNetwork.shp");

            try
            {
                // Open the shapefile.
                ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(filepath);

                // Read metadata about the shapefile and display it in the UI.
                _shapefileMetadata = myShapefile.Info;

                // Create a feature layer to display the shapefile.
                _featureLayer = new FeatureLayer(myShapefile);

                // Zoom the map to the extent of the shapefile.
                _myMapView.SpatialReferenceChanged += MapView_SpatialReferenceChanged;

                // Add the feature layer to the map.
                streetMap.OperationalLayers.Add(_featureLayer);

                // Show the map in the MapView.
                _myMapView.Map = streetMap;
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private async void MapView_SpatialReferenceChanged(object sender, EventArgs e)
        {
            // Unsubscribe from event.
            _myMapView.SpatialReferenceChanged -= MapView_SpatialReferenceChanged;

            // Set the viewpoint.
            await _myMapView.SetViewpointGeometryAsync(_featureLayer.FullExtent);
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
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _showMetadataButton = new UIBarButtonItem();
            _showMetadataButton.Title = "See metadata";

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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _showMetadataButton.Clicked += OnMetadataButtonTouch;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _showMetadataButton.Clicked -= OnMetadataButtonTouch;
        }
    }
}