// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.SelectEncFeatures
{
    [Register("SelectEncFeatures")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("a490098c60f64d3bbac10ad131cc62c7")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Select ENC features",
        "Hydrography",
        "This sample demonstrates how to select an ENC feature.",
        "This sample automatically downloads ENC data from ArcGIS Online before displaying the map.")]
    public class SelectEncFeatures : UIViewController
    {
        // Create and hold a reference to the MapView.
        private readonly MapView _myMapView = new MapView();

        public SelectEncFeatures()
        {
            Title = "Select ENC features";
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        private async void Initialize()
        {
            // Initialize the map with an oceans basemap.
            _myMapView.Map = new Map(Basemap.CreateOceans());

            // Get the path to the ENC Exchange Set.
            string encPath = DataManager.GetDataFolder("a490098c60f64d3bbac10ad131cc62c7", "GB5X01NW.000");

            // Create the cell and layer.
            EncLayer encLayer = new EncLayer(new EncCell(encPath));

            // Add the layer to the map.
            _myMapView.Map.OperationalLayers.Add(encLayer);

            // Wait for the layer to load.
            await encLayer.LoadAsync();

            // Set the viewpoint.
            await _myMapView.SetViewpointAsync(new Viewpoint(encLayer.FullExtent));

            // Subscribe to tap events (in order to use them to identify and select features).
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private void CreateLayout()
        {
            // Add MapView to the page.
            View.AddSubviews(_myMapView);
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

                // Reposition controls.
                _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 0, 0);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private void ClearAllSelections()
        {
            // For each layer in the operational layers that is an ENC layer.
            foreach (EncLayer layer in _myMapView.Map.OperationalLayers.OfType<EncLayer>())
            {
                // Clear the layer's selection.
                layer.ClearSelection();
            }

            // Clear the callout.
            _myMapView.DismissCallout();
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // First clear any existing selections.
            ClearAllSelections();

            // Perform the identify operation.
            IReadOnlyList<IdentifyLayerResult> results = await _myMapView.IdentifyLayersAsync(e.Position, 5, false);

            // Return if there are no results.
            if (results.Count < 1)
            {
                return;
            }

            // Get the results that are from ENC layers.
            IEnumerable<IdentifyLayerResult> encResults = results.Where(result => result.LayerContent is EncLayer);

            // Get the ENC results that have features.
            IEnumerable<IdentifyLayerResult> encResultsWithFeatures = encResults.Where(result => result.GeoElements.Count > 0);

            // Get the first result with ENC features.
            IdentifyLayerResult firstResult = encResultsWithFeatures.First();

            // Get the layer associated with this set of results.
            EncLayer containingLayer = (EncLayer)firstResult.LayerContent;

            // Get the first identified ENC feature.
            EncFeature smallestFeature = (EncFeature) firstResult.GeoElements.OrderBy(f => GeometryEngine.Area(f.Geometry)).First();

            // Select the feature.
            containingLayer.SelectFeature(smallestFeature);

            // Create the callout definition.
            CalloutDefinition definition = new CalloutDefinition(smallestFeature.Acronym, smallestFeature.Description);

            // Show the callout.
            _myMapView.ShowCalloutAt(e.Location, definition);
        }
    }
}