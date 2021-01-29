// Copyright 2018 Esri.
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
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntime.Samples.FeatureCollectionLayerFromPortal
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Create feature collection layer (Portal item)",
        category: "Layers",
        description: "Create a feature collection layer from a portal item.",
        instructions: "The feature collection is loaded from the Portal item when the sample starts.",
        tags: new[] { "collection", "feature collection", "feature collection layer", "id", "item", "map notes", "portal" })]
    public class FeatureCollectionLayerFromPortal : Activity
    {
        // Hold a reference to the map view.
        private MapView _myMapView;

        // Store a text box control with a portal item Id.
        private EditText _portalItemIdEditText;

        // Default portal item Id to load features from.
        private const string FeatureCollectionItemId = "32798dfad17942858d5eef82ee802f0b";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Create a feature collection layer from a portal item";

            // Create the UI.
            CreateLayout();

            // Initialize the app.
            Initialize();
        }

        private void Initialize()
        {
            // Add a default value for the portal item Id.
            _portalItemIdEditText.Text = FeatureCollectionItemId;

            // Create a new map with the oceans basemap and add it to the map view.
            _myMapView.Map = new Map(BasemapStyle.ArcGISOceans);
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
                    AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this);
                    alertBuilder.SetTitle("Feature Collection");
                    alertBuilder.SetMessage("Portal item with ID '" + itemId + "' is not a feature collection.");
                    alertBuilder.Show();
                }
            }
            catch (Exception ex)
            {
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Error");
                alertBuilder.SetMessage("Unable to open item with ID '" + itemId + "': " + ex.Message);
                alertBuilder.Show();
            }
        }

        private void OpenPortalFeatureCollectionClick(object sender, EventArgs e)
        {
            // Get the portal item Id from the user.
            string collectionItemId = _portalItemIdEditText.Text.Trim();

            // Make sure an Id was entered.
            if (String.IsNullOrEmpty(collectionItemId))
            {
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Feature Collection ID");
                alertBuilder.SetMessage("Please enter a portal item ID");
                alertBuilder.Show();
                return;
            }

            // Call a function to add the feature collection from the specified portal item.
            OpenFeaturesFromArcGISOnline(collectionItemId);
        }

        private void CreateLayout()
        {
            // Create a new layout.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a button to load features from a portal item, add it to the layout.
            Button loadFeaturesButton = new Button(this) { Text = "Load features" };
            loadFeaturesButton.Click += OpenPortalFeatureCollectionClick;
            layout.AddView(loadFeaturesButton);

            // Create an edit text for the user to enter a portal item Id.
            _portalItemIdEditText = new EditText(this)
            {
                Hint = "Portal Item Id"
            };
            layout.AddView(_portalItemIdEditText);

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}