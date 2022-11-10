// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISMapsSDK.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;

namespace ArcGISMapsSDKMaui.Samples.SelectEncFeatures
{
    [ArcGISMapsSDK.Samples.Shared.Attributes.Sample(
        name: "Select ENC features",
        category: "Hydrography",
        description: "Select features in an ENC layer.",
        instructions: "Tap to select ENC features. Feature properties will be displayed in a callout.",
        tags: new[] { "IHO", "S-57", "S57", "chart", "hydrography", "identify", "maritime", "select", "selection" })]
    [ArcGISMapsSDK.Samples.Shared.Attributes.OfflineData("9d2987a825c646468b3ce7512fb76e2d")]
    public partial class SelectEncFeatures : ContentPage
    {
        public SelectEncFeatures()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Initialize the map with an oceans basemap
            MyMapView.Map = new Map(BasemapStyle.ArcGISOceans);

            // Get the path to the ENC Exchange Set
            string encPath = DataManager.GetDataFolder("9d2987a825c646468b3ce7512fb76e2d", "ExchangeSetwithoutUpdates", "ENC_ROOT", "CATALOG.031");

            // Create the Exchange Set
            // Note: this constructor takes an array of paths because so that update sets can be loaded alongside base data
            EncExchangeSet myEncExchangeSet = new EncExchangeSet(encPath);

            try
            {
                // Wait for the exchange set to load
                await myEncExchangeSet.LoadAsync();

                // Store a list of data set extent's - will be used to zoom the mapview to the full extent of the Exchange Set
                List<Envelope> dataSetExtents = new List<Envelope>();

                // Add each data set as a layer
                foreach (EncDataset myEncDataset in myEncExchangeSet.Datasets)
                {
                    // Create the cell and layer
                    EncLayer myEncLayer = new EncLayer(new EncCell(myEncDataset));

                    // Add the layer to the map
                    MyMapView.Map.OperationalLayers.Add(myEncLayer);

                    // Wait for the layer to load
                    await myEncLayer.LoadAsync();

                    // Add the extent to the list of extents
                    dataSetExtents.Add(myEncLayer.FullExtent);
                }

                // Use the geometry engine to compute the full extent of the ENC Exchange Set
                Envelope fullExtent = GeometryEngine.CombineExtents(dataSetExtents);

                // Set the viewpoint
                MyMapView.SetViewpoint(new Viewpoint(fullExtent));

                // Subscribe to tap events (in order to use them to identify and select features)
                MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            }
        }

        private void ClearAllSelections()
        {
            // For each layer in the operational layers that is an ENC layer
            foreach (EncLayer layer in MyMapView.Map.OperationalLayers.OfType<EncLayer>())
            {
                // Clear the layer's selection
                layer.ClearSelection();
            }

            // Clear the callout
            MyMapView.DismissCallout();
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            // First clear any existing selections
            ClearAllSelections();

            try
            {
                // Perform the identify operation.
                IReadOnlyList<IdentifyLayerResult> results = await MyMapView.IdentifyLayersAsync(e.Position, 10, false);

                // Return if there are no results.
                if (results.Count < 1) { return; }

                // Get the results that are from ENC layers.
                IEnumerable<IdentifyLayerResult> encResults = results.Where(result => result.LayerContent is EncLayer);

                // Get the first result with ENC features. (Depending on the data, there may be more than one IdentifyLayerResult that contains ENC features.)
                IdentifyLayerResult firstResult = encResults.First();

                // Get the layer associated with this set of results.
                EncLayer containingLayer = (EncLayer)firstResult.LayerContent;

                // Get the GeoElement identified in this layer.
                EncFeature encFeature = (EncFeature)firstResult.GeoElements.First();

                // Select the feature.
                containingLayer.SelectFeature(encFeature);

                // Create the callout definition.
                CalloutDefinition definition = new CalloutDefinition(encFeature.Acronym, encFeature.Description);

                // Show the callout.
                MyMapView.ShowCalloutAt(e.Location, definition);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private static string GetEncPath()
        {
            return DataManager.GetDataFolder("a490098c60f64d3bbac10ad131cc62c7", "GB5X01NW.000");
        }
    }
}