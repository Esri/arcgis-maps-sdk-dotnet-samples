// Copyright 2023 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Offline;

namespace ArcGIS.Samples.AddVectorTiledLayerFromCustomStyle
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Add vector tiled layer from custom style",
        category: "Layers",
        description: "Load ArcGIS vector tiled layers using custom styles.",
        instructions: "Pan and zoom to explore the vector tile basemap.",
        tags: new[] { "tiles", "vector", "vector basemap", "vector tiled layer", "vector tiles" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("f4b742a57af344988b02227e2824ca5f")]
    public partial class AddVectorTiledLayerFromCustomStyle
    {
        // ArcGIS Online portal item strings.
        private readonly string[] _onlineItemIDs =
        {
            "1349bfa0ed08485d8a92c442a3850b06",
            "bd8ac41667014d98b933e97713ba8377",
            "02f85ec376084c508b9c8e5a311724fa",
            "1bf0cc4a4380468fbbff107e100f65a5",
        };

        private readonly string[] _offlineItemIDs =
        {
            "e01262ef2a4f4d91897d9bbd3a9b1075",
            "ce8a34e5d4ca4fa193a097511daa8855"
        };

        // Items to fill the ActionDisplaySheet for switching custom styles.
        private readonly string[] styles = { "Default", "Style 1", "Style 2", "Style 3", "Offline custom style - Light", "Offline custom style - Dark" };

        // Path to Dodge City vector tile package.
        private readonly string _localVectorPackagePath = DataManager.GetDataFolder("f4b742a57af344988b02227e2824ca5f", "dodge_city.vtpk");

        private List<PortalItem> _vectorTiledLayers = new List<PortalItem>();

        private readonly Viewpoint _defaultViewpoint = new Viewpoint(10, 5.5, 1e8);
        private readonly Viewpoint _dodgeCityViewpoint = new Viewpoint(37.76528, -100.01766, 4e4);

        private ItemResourceCache _lightStyleResourceCache;
        private ItemResourceCache _darkStyleResourceCache;

        public AddVectorTiledLayerFromCustomStyle()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Load the default portal.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();

                // Store a list of all portal items.
                foreach (string item in _onlineItemIDs)
                {
                    PortalItem portalItem = await PortalItem.CreateAsync(portal, item);
                    _vectorTiledLayers.Add(portalItem);
                }
                foreach (string item in _offlineItemIDs)
                {
                    PortalItem portalItem = await PortalItem.CreateAsync(portal, item);
                    _vectorTiledLayers.Add(portalItem);
                }

                // Create a map using defaults.
                MyMapView.Map = new Map(new Basemap(new ArcGISVectorTiledLayer(_vectorTiledLayers[0]))) { InitialViewpoint = _defaultViewpoint };

                // By default, the UI label will not reflect the default style.
                ChosenStyle.Text = "Current style: Default";

                // Export offline custom styles.
                _lightStyleResourceCache = await ExportStyle(_vectorTiledLayers[4]);
                _darkStyleResourceCache = await ExportStyle(_vectorTiledLayers[5]);
            }
            catch (Exception ex)
            {
                // Report exceptions.
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            _ = StyleChooser();
        }

        private async Task StyleChooser()
        {
            try
            {
                // Get the chosen style, determine that style's index, and update UI label.
                string style = await Application.Current.MainPage.DisplayActionSheet("Select:", "Cancel", null, styles);
                // - Don't attempt to change the style if the user cancels or misclicks.
                if (style is null || style.Equals("Cancel")) { return; }
                int styleIndex = Array.IndexOf(styles, style);
                ChosenStyle.Text = $"Current style: {style}";

                // Check if the user selected an online or offline custom style.
                // Create a new basemap with the appropriate style.
                if (_onlineItemIDs.Contains(_vectorTiledLayers[styleIndex].ItemId))
                {
                    MyMapView.Map.Basemap = new Basemap(new ArcGISVectorTiledLayer(_vectorTiledLayers[styleIndex]));
                    await MyMapView.SetViewpointAsync(_defaultViewpoint);
                }
                else
                {
                    // Determine which cache to use based on if the style selected is light (index 4) or dark.
                    ItemResourceCache cache = styleIndex == 4 ? _lightStyleResourceCache : _darkStyleResourceCache;
                    MyMapView.Map.Basemap = new Basemap(new ArcGISVectorTiledLayer(new VectorTileCache(_localVectorPackagePath), cache));
                    await MyMapView.SetViewpointAsync(_dodgeCityViewpoint);
                }
            }
            catch (Exception ex)
            {
                // Report exceptions.
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task<ItemResourceCache> ExportStyle(PortalItem vectorTiledLayer)
        {
            try
            {
                // Create the task.
                ExportVectorTilesTask exportTask = await ExportVectorTilesTask.CreateAsync(vectorTiledLayer.Url);

                // Get the item resource path for the basemap styling.
                string itemResourcePath = Path.Combine(Path.GetTempPath(), vectorTiledLayer.ItemId + "_styleItemResources");

                // If cache has been created previously, return.
                if (Directory.Exists(itemResourcePath)) { return new ItemResourceCache(itemResourcePath); }

                // Create the export job and start it.
                ExportVectorTilesJob job = exportTask.ExportStyleResourceCache(itemResourcePath);
                job.Start();

                // Wait for the job to complete.
                ExportVectorTilesResult vectorTilesResult = await job.GetResultAsync();

                return vectorTilesResult.ItemResourceCache;
            }
            catch (Exception ex)
            {
                // Report exceptions.
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                return null;
            }
        }
    }
}