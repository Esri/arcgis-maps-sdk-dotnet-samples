// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

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
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("9d2987a825c646468b3ce7512fb76e2d")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Select ENC features",
        "Hydrography",
        "This sample demonstrates how to select an ENC feature.",
        "This sample automatically downloads ENC data from ArcGIS Online before displaying the map.")]
    public class SelectEncFeatures : UIViewController
    {
        // Create and hold a reference to the MapView.
        private MapView _myMapView;

        public SelectEncFeatures()
        {
            Title = "Select ENC features";
        }

        private async void Initialize()
        {
            // Initialize the map with an oceans basemap.
            _myMapView.Map = new Map(Basemap.CreateOceans());

            // Get the path to the ENC Exchange Set.
            string encPath = DataManager.GetDataFolder("9d2987a825c646468b3ce7512fb76e2d", "ExchangeSetwithoutUpdates", "ENC_ROOT", "CATALOG.031");

            // Create the Exchange Set.
            // Note: this constructor takes an array of paths because so that update sets can be loaded alongside base data.
            EncExchangeSet myEncExchangeSet = new EncExchangeSet(encPath);

            // Wait for the exchange set to load.
            await myEncExchangeSet.LoadAsync();

            // Store a list of data set extent's - will be used to zoom the mapview to the full extent of the Exchange Set.
            List<Envelope> dataSetExtents = new List<Envelope>();

            // Add each data set as a layer.
            foreach (EncDataset myEncDataset in myEncExchangeSet.Datasets)
            {
                // Create the cell and layer.
                EncLayer myEncLayer = new EncLayer(new EncCell(myEncDataset));

                // Add the layer to the map.
                _myMapView.Map.OperationalLayers.Add(myEncLayer);

                // Wait for the layer to load.
                await myEncLayer.LoadAsync();

                // Add the extent to the list of extents.
                dataSetExtents.Add(myEncLayer.FullExtent);
            }

            // Use the geometry engine to compute the full extent of the ENC Exchange Set.
            Envelope fullExtent = GeometryEngine.CombineExtents(dataSetExtents);

            // Set the viewpoint
            _myMapView.SetViewpoint(new Viewpoint(fullExtent));

            // Subscribe to tap events (in order to use them to identify and select features).
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        public override void LoadView()
        {
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            View = new UIView();
            View.AddSubviews(_myMapView);

            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        public override void ViewDidLoad()
        {
            Initialize();

            base.ViewDidLoad();
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
            EncLayer containingLayer = (EncLayer) firstResult.LayerContent;

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