// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Offline;

namespace ArcGIS.Samples.AddVectorTiledLayerFromCustomStyle
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Basic readme template",
        category: "Layers",
        description: "",
        instructions: "")]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class AddVectorTiledLayerFromCustomStyle
    {
        // List all portal item IDs of vector tiled layers.
        private List<string> _itemIDs;

        // These layers can be selected from the ComboBox.
        private List<ArcGISVectorTiledLayer> _vectorTiledLayers;

        private Viewpoint _defaultViewpoint = new Viewpoint(10, 5.5, 1e8);
        private Viewpoint _dodgeCityViewpoint = new Viewpoint(37.76528, -100.01766, 4e4);

        private string[] styles = { "Default", "Style 1", "Style 2", "Style 3", "Offline custom style: Light", "Offline custom style: Dark" };

        public AddVectorTiledLayerFromCustomStyle()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            _itemIDs = new List<string>
            {
                // Vector tiled layers from ArcGISOnline.
                "1349bfa0ed08485d8a92c442a3850b06",
                "bd8ac41667014d98b933e97713ba8377",
                "02f85ec376084c508b9c8e5a311724fa",
                "1bf0cc4a4380468fbbff107e100f65a5",

                // A vector tiled layer created by the local VTPK and light custom style.
                "e01262ef2a4f4d91897d9bbd3a9b1075",

                // A vector tiled layer created by the local VTPK and dark custom style.
                "ce8a34e5d4ca4fa193a097511daa8855"
            };

            // Load the default portal.
            ArcGISPortal portal = await ArcGISPortal.CreateAsync();

            // Create vector tiled layers for each portal item.
            _vectorTiledLayers = new List<ArcGISVectorTiledLayer>();

            // Store a list of tiled layers in the different styles.
            foreach (string item in _itemIDs)
            {
                PortalItem websceneItem = await PortalItem.CreateAsync(portal, item);
                ArcGISVectorTiledLayer vectorTiledLayer = new ArcGISVectorTiledLayer(websceneItem);
                _vectorTiledLayers.Add(vectorTiledLayer);
            }

            // Create a map with the default style.
            MyMapView.Map = new Map(SpatialReferences.WebMercator);
            MyMapView.Map.Basemap.BaseLayers.Add(_vectorTiledLayers[0]);

            await MyMapView.SetViewpointAsync(_defaultViewpoint);
        }

        private async void Export(ArcGISVectorTiledLayer vectorTiledLayer)
        {
            // Create the task.
            ExportVectorTilesTask exportTask = await ExportVectorTilesTask.CreateAsync(vectorTiledLayer.Source);

            ExportVectorTilesParameters parameters = await exportTask.CreateDefaultExportVectorTilesParametersAsync(MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry, MyMapView.MapScale * 0.1);

            // By using the UseReducedFontService download option the file download speed is reduced.
            // This limits vector tiled layers character sets and may not be suitable in every use case.
            parameters.EsriVectorTilesDownloadOption = EsriVectorTilesDownloadOption.UseReducedFontsService;

            // Get the tile cache path and item resource path for the base layer styling.
            string tilePath = Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), Path.GetTempFileName() + ".vtpk");
            string itemResourcePath = Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), Path.GetTempFileName() + "_styleItemResources");

            // Create the export job and start it.
            ExportVectorTilesJob _job = exportTask.ExportVectorTiles(parameters, tilePath, itemResourcePath);
            _job.Start();

            // Wait for the job to complete.
            ExportVectorTilesResult vectorTilesResult = await _job.GetResultAsync();

            // Load the vector tile cache.
            VectorTileCache vectorTileCache = vectorTilesResult.VectorTileCache;
            await vectorTileCache.LoadAsync();

            // Create a tile layer from the tile cache.
            ArcGISVectorTiledLayer myLayer = new ArcGISVectorTiledLayer(vectorTileCache, vectorTilesResult.ItemResourceCache);
        }

        private async void StyleChooserClick(object sender, EventArgs e)
        {
            try
            {
                string style = await Application.Current.MainPage.DisplayActionSheet("Select:", "Cancel", null, styles);
                int styleIndex = Array.IndexOf(styles, style);

                // Determine the style from the
                ArcGISVectorTiledLayer vectorTiledLayer = _vectorTiledLayers[styleIndex];

                // Clear the map of the currently loaded layer.
                MyMapView.Map.Basemap.BaseLayers.RemoveAt(0);

                if (styleIndex <= 3)
                {
                    // No need to export - change the basemap style.
                    await MyMapView.SetViewpointAsync(_defaultViewpoint);
                }
                else
                {
                    // Export a custom style.
                    await MyMapView.SetViewpointAsync(_dodgeCityViewpoint);
                    Export(vectorTiledLayer);
                }
                MyMapView.Map.Basemap.BaseLayers.Add(vectorTiledLayer);
            }
            catch (ArgumentOutOfRangeException)
            {
                // This exception will be thrown if the user cancels the DisplayActionSheet, so ignore.
            }
        }
    }
}