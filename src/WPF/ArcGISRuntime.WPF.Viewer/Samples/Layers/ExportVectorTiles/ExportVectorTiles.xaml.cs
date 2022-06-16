﻿// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.ExportVectorTiles
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Export vector tiles",
        category: "Layers",
        description: "Export tiles from an online vector tile service.",
        instructions: "When the vector tiled layer loads, zoom in to the extent you want to export. The red box shows the extent that will be exported. Tap the \"Export vector tiles\" button to start exporting the vector tiles. An error will show if the extent is larger than the maximum limit allowed. When finished, a new map view will show the exported result.",
        tags: new[] { "cache", "download", "offline", "vector" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class ExportVectorTiles
    {
        // Hold references to the variables used in the event handlers.
        private Graphic _downloadArea;
        private ArcGISVectorTiledLayer _vectorTiledLayer;
        private ExportVectorTilesJob _job;

        public ExportVectorTiles()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create the basemap.
            MyMapView.Map = new Map(BasemapStyle.ArcGISStreetsNight);

            // Load the map to ensure that the tiles are available when the export task is called.
            await MyMapView.Map.LoadAsync();

            // Set the initial viewpoint.
            MyMapView.SetViewpoint(new Viewpoint(34.049, -117.181, 1e4));

            // Create a graphics overlay for the extent graphic.
            GraphicsOverlay graphicsOverlay = new GraphicsOverlay();

            // Create a graphic to show a red outline square around the tiles to be downloaded.
            _downloadArea = new Graphic();
            graphicsOverlay.Graphics.Add(_downloadArea);

            // Create a symbol for the extent graphic.
            SimpleLineSymbol simpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 2);
            _downloadArea.Symbol = simpleLineSymbol;

            // Add the overlay to the map view.
            MyMapView.GraphicsOverlays.Add(graphicsOverlay);

            // If the map has loaded check if the basemap layer is a vector tiled layer.
            // If the basemap layer is a vector tiled layer enable the export button.
            if (MyMapView.Map.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                if (MyMapView.Map.Basemap.BaseLayers[0] is ArcGISVectorTiledLayer vectorTiledLayer)
                {
                    _vectorTiledLayer = vectorTiledLayer;
                }

                MyExportButton.IsEnabled = true;
            }
        }

        #region Export Vector Tiles

        private async Task StartExportTask()
        {
            await StartExport();
        }

        private async Task StartExport()
        {
            // Create the task.
            ExportVectorTilesTask exportTask = await ExportVectorTilesTask.CreateAsync(_vectorTiledLayer.Source);

            // Get the parameters for the job.
            // The max scale parameter is set to 10% of the map's scale to limit the
            // number of tiles exported to within the vector tiled layer's max tile export limit.
            ExportVectorTilesParameters parameters = await exportTask.CreateDefaultExportVectorTilesParametersAsync(_downloadArea.Geometry, MyMapView.MapScale * 0.1);

            // By using the UseReducedFontService download option the file download speed is reduced.
            // This limits vector tiled layers character sets and may not be suitable in every use case.
            parameters.EsriVectorTilesDownloadOption = EsriVectorTilesDownloadOption.UseReducedFontsService;

            // Get the tile cache path and item resource path for the base layer styling.
            string tilePath = Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), Path.GetTempFileName() + ".vtpk");
            string itemResourcePath = Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), Path.GetTempFileName() + "_styleItemResources");

            // Create the export job.
            _job = exportTask.ExportVectorTiles(parameters, tilePath, itemResourcePath);

            // Set the value of the progress bar to 0, this clears any previous progress on the bar.
            MyProgressBar.Value = 0;

            // Show the progress bar and label.
            MyProgressBar.Visibility = Visibility.Visible;
            MyProgressBarLabel.Visibility = Visibility.Visible;

            // Show the cancel job button.
            MyCancelJobButton.Visibility = Visibility.Visible;

            // Add an event handler to update the progress bar as the task progresses.
            _job.ProgressChanged += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    MyProgressBar.Value = _job.Progress;
                });
            };

            // Add an event handler to hide the cancel job button if the job completes, fails or is cancelled.
            _job.StatusChanged += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (_job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Failed
                    || _job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Succeeded
                    || _job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Canceling)
                    {
                        MyCancelJobButton.Visibility = Visibility.Collapsed;
                    }
                });
            };

            // Start the export job.
            _job.Start();

            // Wait for the job to complete.
            ExportVectorTilesResult vectorTilesResult = await _job.GetResultAsync();

            // Update the preview map and UI components.
            await HandleExportCompleted(_job, vectorTilesResult);
        }

        private async Task HandleExportCompleted(ExportVectorTilesJob job, ExportVectorTilesResult vectorTilesResult)
        {
            if (job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Succeeded)
            {
                // Show the exported tiles on the preview map.
                await UpdatePreviewMap(vectorTilesResult);

                // Show the preview window.
                MyPreviewMapView.Visibility = Visibility.Visible;

                // Show the 'close preview' button.
                MyClosePreviewButton.Visibility = Visibility.Visible;

                // Hide the 'export tiles' button.
                MyExportButton.Visibility = Visibility.Collapsed;

                // Hide the progress bar and label.
                MyProgressBar.Visibility = Visibility.Collapsed;
                MyProgressBarLabel.Visibility = Visibility.Collapsed;
            }
            else if (job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Failed)
            {
                // Notify the user.
                MessageBox.Show("Job failed");

                // Hide the progress bar.
                MyProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        #endregion Export Vector Tiles

        #region Update Preview Map/Extent Graphic

        private void UpdateMapExtentGraphic()
        {
            // Return if MyMapView is null.
            if (MyMapView == null) { return; }

            // Get the new viewpoint.
            Viewpoint myViewPoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

            // Return if viewpoint is null.
            if (myViewPoint == null) { return; }

            // Get the updated extent for the new viewpoint.
            Envelope extent = myViewPoint.TargetGeometry as Envelope;

            // Return if extent is null.
            if (extent == null) { return; }

            // Create an envelope that is a bit smaller than the extent.
            EnvelopeBuilder envelopeBuilder = new EnvelopeBuilder(extent);
            envelopeBuilder.Expand(0.80);

            // Get the (only) graphics overlay in the map view.
            GraphicsOverlay extentOverlay = MyMapView.GraphicsOverlays.FirstOrDefault();

            // Return if the extent overlay is null.
            if (extentOverlay == null) { return; }

            // Get the extent graphic.
            Graphic extentGraphic = extentOverlay.Graphics.FirstOrDefault();

            // Create the extent graphic and add it to the overlay if it doesn't exist.
            if (extentGraphic == null)
            {
                extentGraphic = new Graphic(envelopeBuilder.ToGeometry());
                extentOverlay.Graphics.Add(extentGraphic);
            }
            else
            {
                // Otherwise, simply update the graphic's geometry.
                extentGraphic.Geometry = envelopeBuilder.ToGeometry();
            }
        }

        private async Task UpdatePreviewMap(ExportVectorTilesResult vectorTilesResult)
        {
            // Load the vector tile cache.
            VectorTileCache vectorTileCache = vectorTilesResult.VectorTileCache;
            await vectorTileCache.LoadAsync();

            // Create a tile layer from the tile cache.
            ArcGISVectorTiledLayer myLayer = new ArcGISVectorTiledLayer(vectorTileCache, vectorTilesResult.ItemResourceCache);

            // Show the layer in a new map.
            MyPreviewMapView.Map = new Map(new Basemap(myLayer));
        }

        #endregion Update Preview Map/Extent Graphic

        #region EventHandlers

        private void MyMapView_ViewpointChanged(object sender, EventArgs e)
        {
            UpdateMapExtentGraphic();
        }

        private void MyExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Hide the preview window.
                MyPreviewMapView.Visibility = Visibility.Collapsed;

                // Hide the 'close preview' button.
                MyClosePreviewButton.Visibility = Visibility.Collapsed;

                // Show the 'export tiles' button.
                MyExportButton.Visibility = Visibility.Visible;

                // Disable the 'export tiles' button.
                MyExportButton.IsEnabled = false;

                // Start the export.
                _ = StartExportTask();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void MyClosePreviewButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide the preview map.
            MyPreviewMapView.Visibility = Visibility.Collapsed;

            // Hide the close preview button.
            MyClosePreviewButton.Visibility = Visibility.Collapsed;

            // Show the 'export tiles' button.
            MyExportButton.Visibility = Visibility.Visible;

            // Re-enable the export button.
            MyExportButton.IsEnabled = true;
        }

        private void MyCancelJobButton_Click(object sender, RoutedEventArgs e)
        {
            if (_job != null)
            {
                _ = _job.CancelAsync();

                // Hide the cancel job button.
                MyCancelJobButton.Visibility = Visibility.Collapsed;

                // Hide the progress bar and label.
                MyProgressBar.Visibility = Visibility.Collapsed;
                MyProgressBarLabel.Visibility = Visibility.Collapsed;

                // Re-enable the export button.
                MyExportButton.IsEnabled = true;
            }
        }

        #endregion EventHandlers
    }
}