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
using System;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.FeatureCollectionLayerFromPortal
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Create a feature collection layer from a portal item",
        "Layers",
        "This sample demonstrates opening a feature collection saved as a portal item.",
        "")]
    public partial class FeatureCollectionLayerFromPortal : ContentPage
    { 
        // Default portal item Id to load features from
        private const string FeatureCollectionItemId = "5ffe7733754f44a9af12a489250fe12b";

        public FeatureCollectionLayerFromPortal()
        {
            InitializeComponent();
            
            Title = "Feature collection layer from portal";

            // call a function to initialize a map to display in the MyMapView control
            Initialize();
        }

        private void Initialize()
        {
            // Add a default value for the portal item Id
            CollectionItemIdTextBox.Text = FeatureCollectionItemId;

            // Create a new map with the oceans basemap and add it to the map view
            Map myMap = new Map(Basemap.CreateOceans());
            MyMapView.Map = myMap;
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

                    MyMapView.Map.OperationalLayers.Add(featureCollectionLayer);
                }
                else
                {
                    await DisplayAlert("Feature Collection", "Portal item with ID '" + itemId + "' is not a feature collection.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Unable to open item with ID '" + itemId + "': " + ex.Message, "OK");
            }
        }

        private async void OpenPortalFeatureCollectionClick(object sender, EventArgs e)
        {
            // Get the portal item Id from the user
            var collectionItemId = CollectionItemIdTextBox.Text.Trim();

            // Make sure an Id was entered
            if (string.IsNullOrEmpty(collectionItemId))
            {
                await DisplayAlert("Feature Collection ID", "Please enter a portal item ID", "OK");
                return;
            }

            // Call a function to add the feature collection from the specified portal item
            OpenFeaturesFromArcGISOnline(collectionItemId);
        }
    }
}
