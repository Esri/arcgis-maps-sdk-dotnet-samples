// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
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
        name: "Create feature collection layer (Portal item)",
        category: "Layers",
        description: "Create a feature collection layer from a portal item.",
        instructions: "The feature collection is loaded from the Portal item when the sample starts.",
        tags: new[] { "collection", "feature collection", "feature collection layer", "id", "item", "map notes", "portal" })]
    public class FeatureCollectionLayerFromPortal : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        // Portal item Id to load features from.
        private const string FeatureCollectionItemId = "32798dfad17942858d5eef82ee802f0b";

        public FeatureCollectionLayerFromPortal()
        {
            Title = "Create a feature collection layer from a portal item";
        }

        private async void Initialize()
        {
            // Create a new map with the oceans basemap and add it to the map view
            _myMapView.Map = new Map(BasemapStyle.ArcGISOceans);

            try
            {
                // Open a portal item containing a feature collection.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();
                PortalItem collectionItem = await PortalItem.CreateAsync(portal, FeatureCollectionItemId);

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
                    UIAlertView alert = new UIAlertView("Feature Collection", "Portal item with ID '" + FeatureCollectionItemId + "' is not a feature collection.", (IUIAlertViewDelegate) null, "OK");
                    alert.Show();
                }
            }
            catch (Exception ex)
            {
                UIAlertView alert = new UIAlertView("Error", "Unable to open item with ID '" + FeatureCollectionItemId + "': " + ex.Message, (IUIAlertViewDelegate) null, "OK");
                alert.Show();
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