// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureCollectionLayerFromPortal
{
    [Register("FeatureCollectionLayerFromPortal")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature collection layer from portal item",
        "Layers",
        "This sample demonstrates opening a feature collection saved as a portal item.",
        "")]
    public class FeatureCollectionLayerFromPortal : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private UITextField _collectionItemIdTextBox;
        private UIButton _addFeaturesButton;

        // Default portal item Id to load features from.
        private const string FeatureCollectionItemId = "32798dfad17942858d5eef82ee802f0b";

        public FeatureCollectionLayerFromPortal()
        {
            Title = "Create a feature collection layer from a portal item";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the layout.
            CreateLayout();

            // Initialize the app.
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topStart = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat controlHeight = 30;
                nfloat controlWidth = View.Bounds.Width - 2 * margin;
                nfloat toolbarHeight = 2 * controlHeight + 3 * margin;

                // Setup the visual frames for the views.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topStart + toolbarHeight, 0, 0, 0);
                _toolbar.Frame = new CGRect(0, topStart, View.Bounds.Width, toolbarHeight);
                _collectionItemIdTextBox.Frame = new CGRect(margin, topStart + margin, controlWidth, controlHeight);
                _addFeaturesButton.Frame = new CGRect(margin, topStart + 2 * margin + controlHeight, controlWidth, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private void Initialize()
        {
            // Add a default value for the portal item Id.
            _collectionItemIdTextBox.Text = FeatureCollectionItemId;

            // Create a new map with the oceans basemap and add it to the map view
            _myMapView.Map = new Map(Basemap.CreateOceans());
        }

        private async void OpenFeaturesFromArcGISOnline(string itemId)
        {
            try
            {
                // Open a portal item containing a feature collection.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();
                PortalItem collectionItem = await PortalItem.CreateAsync(portal, itemId);

                // Verify that the item is a feature collection.
                if (collectionItem.Type == PortalItemType.FeatureCollection)
                {
                    // Create a new FeatureCollection from the item.
                    FeatureCollection featureCollection = new FeatureCollection(collectionItem);

                    // Create a layer to display the collection and add it to the map as an operational layer.
                    FeatureCollectionLayer featureCollectionLayer = new FeatureCollectionLayer(featureCollection)
                    {
                        Name = collectionItem.Title
                    };

                    _myMapView.Map.OperationalLayers.Add(featureCollectionLayer);
                }
                else
                {
                    var alert = new UIAlertView("Feature Collection", "Portal item with ID '" + itemId + "' is not a feature collection.", (IUIAlertViewDelegate) null, "OK");
                    alert.Show();
                }
            }
            catch (Exception ex)
            {
                var alert = new UIAlertView("Error", "Unable to open item with ID '" + itemId + "': " + ex.Message, (IUIAlertViewDelegate) null, "OK");
                alert.Show();
            }
        }

        private void OpenPortalFeatureCollection_Click(object sender, EventArgs e)
        {
            // Get the portal item Id from the user.
            string collectionItemId = _collectionItemIdTextBox.Text.Trim();

            // Make sure an Id was entered.
            if (string.IsNullOrEmpty(collectionItemId))
            {
                var alert = new UIAlertView("Feature Collection ID", "Please enter a portal item ID", (IUIAlertViewDelegate) null, "OK");
                alert.Show();
                return;
            }

            // Call a function to add the feature collection from the specified portal item.
            OpenFeaturesFromArcGISOnline(collectionItemId);
        }

        private void CreateLayout()
        {
            // Create a text input for the portal item Id.
            _collectionItemIdTextBox = new UITextField
            {
                BackgroundColor = UIColor.FromWhiteAlpha(1, .8f),
                BorderStyle = UITextBorderStyle.RoundedRect
            };

            // Allow pressing 'return' to dismiss the keyboard.
            _collectionItemIdTextBox.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            // Create a button for adding features from a portal item.
            _addFeaturesButton = new UIButton(UIButtonType.Custom);
            _addFeaturesButton.SetTitle("Add from portal item", UIControlState.Normal);
            _addFeaturesButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Assign a click handler to the button.
            _addFeaturesButton.TouchUpInside += OpenPortalFeatureCollection_Click;

            // Add the MapView, UITextField, and UIButton to the page.
            View.AddSubviews(_myMapView, _toolbar, _collectionItemIdTextBox, _addFeaturesButton);
        }
    }
}