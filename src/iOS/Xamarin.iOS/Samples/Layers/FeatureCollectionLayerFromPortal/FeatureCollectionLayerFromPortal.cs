// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
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
        // Reference to the MapView used in the app
        private MapView _myMapView;

        // Default portal item Id to load features from
        private const string FeatureCollectionItemId = "5ffe7733754f44a9af12a489250fe12b";

        // Text field for specifying a portal item Id
        UITextField _collectionItemIdTextBox;

        UIButton _addFeaturesButton;

        public FeatureCollectionLayerFromPortal()
        {
            Title = "Create a feature collection layer from a portal item";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the layout
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height - 50);

            _collectionItemIdTextBox.Frame = new CoreGraphics.CGRect(10, View.Bounds.Height - 50, View.Bounds.Width - 10, 20);

            _addFeaturesButton.Frame =  new CoreGraphics.CGRect(0, View.Bounds.Height -30, View.Bounds.Width, 30);

            base.ViewDidLayoutSubviews();
        }

        private void Initialize()
        {
            // Add a default value for the portal item Id
            _collectionItemIdTextBox.Text = FeatureCollectionItemId;

            // Create a new map with the oceans basemap and add it to the map view
            Map myMap = new Map(Basemap.CreateOceans());
            _myMapView.Map = myMap;
        }

        private async void OpenFeaturesFromArcGISOnline(string itemId)
        {
            try
            {
                // Open a portal item containing a feature collection
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();
                PortalItem collectionItem = await PortalItem.CreateAsync(portal, itemId);

                // Verify that the item is a feature collection
                if (collectionItem.Type == PortalItemType.FeatureCollection)
                {
                    // Create a new FeatureCollection from the item
                    FeatureCollection featureCollection = new FeatureCollection(collectionItem);

                    // Create a layer to display the collection and add it to the map as an operational layer
                    FeatureCollectionLayer featureCollectionLayer = new FeatureCollectionLayer(featureCollection);
                    featureCollectionLayer.Name = collectionItem.Title;

                    _myMapView.Map.OperationalLayers.Add(featureCollectionLayer);
                }
                else
                {
                    var alert = new UIAlertView("Feature Collection", "Portal item with ID '" + itemId + "' is not a feature collection.", null, "OK");
                    alert.Show();
                }
            }
            catch (Exception ex)
            {
                var alert = new UIAlertView("Error", "Unable to open item with ID '" + itemId + "': " + ex.Message, null, "OK");
                alert.Show();
            }
        }

        private void OpenPortalFeatureCollectionClick(object sender, EventArgs e)
        {
            // Get the portal item Id from the user
            var collectionItemId = _collectionItemIdTextBox.Text.Trim();

            // Make sure an Id was entered
            if (string.IsNullOrEmpty(collectionItemId))
            {
                var alert = new UIAlertView("Feature Collection ID", "Please enter a portal item ID", null, "OK");
                alert.Show();
                return;
            }

            // Call a function to add the feature collection from the specified portal item
            OpenFeaturesFromArcGISOnline(collectionItemId);
        }

        private void CreateLayout()
        {
            // Create a new MapView
            _myMapView = new MapView();

            // Create a text input for the portal item Id
            _collectionItemIdTextBox = new UITextField();
            _collectionItemIdTextBox.BackgroundColor = UIColor.LightGray;
            // Allow pressing 'return' to dismiss the keyboard
            _collectionItemIdTextBox.ShouldReturn += (textField) => { textField.ResignFirstResponder(); return true; };

            // Create a button for adding features from a portal item
            _addFeaturesButton = new UIButton(UIButtonType.Custom);
            _addFeaturesButton.SetTitle("Add from portal item", UIControlState.Normal);
            _addFeaturesButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _addFeaturesButton.BackgroundColor = UIColor.White;

            // Assign a click handler to the UIButton
            _addFeaturesButton.TouchUpInside += OpenPortalFeatureCollectionClick;

            // Add the MapView, UITextField, and UIButton to the page
            View.AddSubviews(_myMapView, _collectionItemIdTextBox, _addFeaturesButton);
            View.BackgroundColor = UIColor.White;
        }
    }
}