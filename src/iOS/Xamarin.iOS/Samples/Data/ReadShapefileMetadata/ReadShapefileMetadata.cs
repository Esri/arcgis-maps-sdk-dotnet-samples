// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using ArcGISRuntime.Samples.Managers;
using CoreGraphics;
using Esri.ArcGISRuntime.UI;
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
    public class ReadShapefileMetadata : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private UIButton _showMetadataButton;

        // Store the shapefile metadata.
        private ShapefileInfo _shapefileMetadata;


        public ReadShapefileMetadata()
        {
            Title = "Read shapefile metadata";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat controlHeight = 30;
                nfloat margin = 5;
                nfloat toolbarHeight = controlHeight + 2 * margin;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _toolbar.Frame = new CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);
                _showMetadataButton.Frame = new CGRect(margin, _toolbar.Frame.Top + margin, View.Bounds.Width - 2 * margin, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            catch (NullReferenceException)
            {
            }
        }

        private async void Initialize()
        {
            // Create a new map to display in the map view with a streets basemap.
            Map streetMap = new Map(Basemap.CreateStreetsVector());

            // Get the path to the downloaded shapefile.
            string filepath = DataManager.GetDataFolder("d98b3e5293834c5f852f13c569930caa", "TrailBikeNetwork.shp");

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

        private void CreateLayout()
        {
            // Add a button at the bottom to show metadata dialog.
            _showMetadataButton = new UIButton();

            // Create button to show metadata.
            _showMetadataButton.SetTitle("Metadata", UIControlState.Normal);
            _showMetadataButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _showMetadataButton.TouchUpInside += OnMetadataButtonTouch;

            // Add MapView to the page.
            View.AddSubviews(_myMapView, _toolbar, _showMetadataButton);
        }

        private void OnMetadataButtonTouch(object sender, EventArgs e)
        {
            // Create a dialog to show metadata values that covers the entire view.
            CGRect ovBounds = new CGRect(0, 60, View.Bounds.Width, View.Bounds.Height);
            ShapefileMetadataDialog metadataDialog = new ShapefileMetadataDialog(ovBounds, 0.05f, UIColor.White, _shapefileMetadata);

            // Add the dialog (will show on top of the map view).
            View.Add(metadataDialog);

            // Action to decrease the view transparency (will be 100% opaque).
            Action MakeOpaqueAction = () => metadataDialog.Alpha = 1.0f;

            // Animate opacity to 100% in one second.
            UIView.Animate(1.00, MakeOpaqueAction, null);
        }
    }

    // View to display shapefile metadata info.
    public class ShapefileMetadataDialog : UIView
    {
        // ImageView to display the shapefile thumbnail.
        private readonly UIImageView _shapefileThumbnailImage;

        public ShapefileMetadataDialog(CGRect frame, nfloat opacity, UIColor color, ShapefileInfo metadata) : base(frame)
        {
            // Create a semi-transparent overlay with the specified background color.
            BackgroundColor = color;
            Alpha = opacity;

            // Variables for space between controls and for control width (height will vary).
            nfloat rowSpace = 5;
            nfloat controlWidth = Frame.Width - 20;

            // Find the center x and y of the view.
            nfloat centerX = Frame.Width / 2;

            // Find the start x and y for the control layout.
            nfloat controlX = centerX - controlWidth / 2;
            nfloat controlY = 20;

            // Label for credits metadata.
            UILabel creditsLabel = new UILabel(new CGRect(controlX, controlY, controlWidth, 20))
            {
                Text = metadata.Credits,
                TextColor = UIColor.Black
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 20 + rowSpace;

            // Label for the summary metadata.
            UILabel summaryLabel = new UILabel(new CGRect(controlX, controlY, controlWidth, 120))
            {
                LineBreakMode = UILineBreakMode.WordWrap,
                Lines = 0,
                Text = metadata.Summary
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 120 + rowSpace;

            // ImageView for metadata thumbnail.
            _shapefileThumbnailImage = new UIImageView(new CGRect(centerX - 80, controlY, 160, 160));
            LoadThumbnail(metadata);

            // Adjust the Y position for the next control.
            controlY = controlY + 160 + rowSpace;

            // Metadata tags.
            UILabel tagsLabel = new UILabel(new CGRect(controlX, controlY, controlWidth, 100))
            {
                LineBreakMode = UILineBreakMode.WordWrap,
                Lines = 0,
                Text = string.Join(",", metadata.Tags)
            };

            // Adjust the Y position for the next control.
            controlY = controlY + 100 + rowSpace;

            // Button to hide the dialog.
            UIButton hideButton = new UIButton(new CGRect(controlX, controlY, controlWidth, 20));
            hideButton.SetTitle("OK", UIControlState.Normal);
            hideButton.SetTitleColor(TintColor, UIControlState.Normal);
            hideButton.TouchUpInside += (s, e) => { Hide(); };

            // Add the controls.
            AddSubviews(creditsLabel, summaryLabel, _shapefileThumbnailImage, tagsLabel, hideButton);
        }

        private async void LoadThumbnail(ShapefileInfo metadata)
        {
            // Show the image in the UI.
            _shapefileThumbnailImage.Image = await metadata.Thumbnail.ToImageSourceAsync();
        }

        // Animate increasing transparency to completely hide the view, then remove it.
        private void Hide()
        {
            // Action to make the view transparent.
            Action MakeTransparentAction = () => Alpha = 0;

            // Action to remove the view.
            Action removeViewAction = RemoveFromSuperview;

            // Time to complete the animation (seconds).
            double secondsToComplete = 0.75;

            // Animate transparency to zero, then remove the view.
            Animate(secondsToComplete, MakeTransparentAction, removeViewAction);
        }
    }
}