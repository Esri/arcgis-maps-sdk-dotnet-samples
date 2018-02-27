// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntime.Samples.FeatureCollectionLayerFromPortal
{
    [Activity(Label = "FeatureCollectionLayerFromPortal")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Create a feature collection layer from a portal item",
        "Layers",
        "This sample demonstrates opening a feature collection saved as a portal item.",
        "")]
    public class FeatureCollectionLayerFromPortal : Activity
    {
        // Store the map view displayed in the app
        private MapView _myMapView = new MapView();

        // Store a text box control with a portal item Id
        private EditText _portalItemIdEditText;

        // Default portal item Id to load features from
        private const string FeatureCollectionItemId = "5ffe7733754f44a9af12a489250fe12b";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Create a feature collection layer from a portal item";

            // Create the UI
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        private void Initialize()
        {
            // Add a default value for the portal item Id
            _portalItemIdEditText.Text = FeatureCollectionItemId;

            // Create a new map with the oceans basemap and add it to the map view
            var map = new Map(Basemap.CreateOceans());
            _myMapView.Map = map;
        }

        private async void OpenFeaturesFromArcGISOnline(string itemId)
        {
            try
            {
                // Open a portal item containing a feature collection
                var portal = await ArcGISPortal.CreateAsync();
                var collectionItem = await PortalItem.CreateAsync(portal, itemId);

                // Verify that the item is a feature collection
                if (collectionItem.Type == PortalItemType.FeatureCollection)
                {
                    // Create a new FeatureCollection from the item
                    var featureCollection = new FeatureCollection(collectionItem);

                    // Create a layer to display the collection and add it to the map as an operational layer
                    var featureCollectionLayer = new FeatureCollectionLayer(featureCollection);
                    featureCollectionLayer.Name = collectionItem.Title;

                    _myMapView.Map.OperationalLayers.Add(featureCollectionLayer);
                }
                else
                {
                    var alertBuilder = new AlertDialog.Builder(this);
                    alertBuilder.SetTitle("Feature Collection");
                    alertBuilder.SetMessage("Portal item with ID '" + itemId + "' is not a feature collection.");
                    alertBuilder.Show();
                }
            }
            catch (Exception ex)
            {
                var alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Error");
                alertBuilder.SetMessage("Unable to open item with ID '" + itemId + "': " + ex.Message);
                alertBuilder.Show();
            }
        }

        private void OpenPortalFeatureCollectionClick(object sender, EventArgs e)
        {
            // Get the portal item Id from the user
            var collectionItemId = _portalItemIdEditText.Text.Trim();

            // Make sure an Id was entered
            if (string.IsNullOrEmpty(collectionItemId))
            {
                var alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Feature Collection ID");
                alertBuilder.SetMessage("Please enter a portal item ID");
                alertBuilder.Show();
                return;
            }

            // Call a function to add the feature collection from the specified portal item
            OpenFeaturesFromArcGISOnline(collectionItemId);
        }

        private void CreateLayout()
        {
            // Create a new layout
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a button to load features from a portal item, add it to the layout
            var loadFeaturesButton = new Button(this) { Text = "Load features" };
            loadFeaturesButton.Click += OpenPortalFeatureCollectionClick;
            layout.AddView(loadFeaturesButton);

            // Create an edit text for the user to enter a portal item Id
            _portalItemIdEditText = new EditText(this);
            _portalItemIdEditText.Hint = "Portal Item Id";
            layout.AddView(_portalItemIdEditText);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}