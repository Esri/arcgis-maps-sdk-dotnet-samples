// Copyright 2023 Esri.
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

namespace ArcGIS.Samples.ExportVectorTiles
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Export vector tiles",
        category: "Layers",
        description: "Export tiles from an online vector tile service.",
        instructions: "When the vector tiled layer loads, zoom in to the extent you want to export. The red box shows the extent that will be exported. Tap the \"Export vector tiles\" button to start exporting the vector tiles. An error will show if the extent is larger than the maximum limit allowed. When finished, a new map view will show the exported result.",
        tags: new[] { "cache", "download", "offline", "vector" })]
    public partial class ExportVectorTiles : ContentPage
    {
        // Hold references to the variables used in the event handlers.
        private Graphic _downloadArea;
        private ArcGISVectorTiledLayer _vectorTiledLayer;

        // Flag to indicate whether an exported tile cache is being previewed.
        private bool _previewOpen = false;

        // Reference to the original basemap.
        private Map _basemap;

        // Reference to the original viewpoint (when previewing).
        private Viewpoint _originalView;

        // Holder for the Graphics Overlay (so that it can be hidden and re-added for preview/non-preview state).
        private GraphicsOverlay _overlay;

        // Hold a reference to the export job for use in event handler.
        private ExportVectorTilesJob _job;

        public ExportVectorTiles()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create the basemap.
            _basemap = new Map(BasemapStyle.ArcGISStreetsNight);

            // Assign the basemap to the map view.
            MyMapView.Map = _basemap;

            // Load the map to ensure that the tiles are available when the export task is called.
            await MyMapView.Map.LoadAsync();

            // Set the initial viewpoint.
            MyMapView.SetViewpoint(new Viewpoint(34.049, -117.181, 1e4));

            // Create a graphic to show a red outline square around the tiles to be downloaded.
            GraphicsOverlay graphicsOverlay = new GraphicsOverlay();
            _downloadArea = new Graphic()
            {
                Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 2)
            };
            graphicsOverlay.Graphics.Add(_downloadArea);
            MyMapView.GraphicsOverlays.Add(graphicsOverlay);

            // If the map has loaded check if the basemap layer is a vector tiled layer.
            // If the basemap layer is a vector tiled layer enable the export button.
            if (MyMapView.Map.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                if (MyMapView.Map.Basemap.BaseLayers[0] is ArcGISVectorTiledLayer vectorTiledLayer)
                {
                    _vectorTiledLayer = vectorTiledLayer;
                }

                MyExportPreviewButton.IsEnabled = true;
            }
        }

        #region Export Vector Tiles

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

            // Update the UI.
            MyProgressBar.Progress = 0.0;
            MyProgressBar.IsVisible = true;
            MyProgressBarLabel.IsVisible = true;
            MyCancelJobButton.IsVisible = true;
            MyExportPreviewButton.IsVisible = false;

            // Add an event handler to update the progress bar as the task progresses.
            _job.ProgressChanged += Job_ProgressChanged;

            // Add an event handler to hide the cancel job button if the job completes, fails or is cancelled.
            _job.StatusChanged += Job_StatusChanged;

            // Start the export job.
            _job.Start();

            // Wait for the job to complete.
            ExportVectorTilesResult vectorTilesResult = await _job.GetResultAsync();

            // Update the preview map and UI components.
            await HandleExportCompleted(vectorTilesResult);
        }

        private async Task HandleExportCompleted(ExportVectorTilesResult vectorTilesResult)
        {
            // Update the view if the job is complete.
            if (_job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Succeeded)
            {
                // Show the exported tiles on the preview map.
                await UpdatePreviewMap(vectorTilesResult);

                // Update the UI.
                MyExportPreviewButton.Text = "Return to online map";
                MyExportPreviewButton.IsVisible = true;
                MyExportPreviewButton.IsEnabled = true;

                // Set the preview open flag.
                _previewOpen = true;

                // Store the overlay for later.
                _overlay = MyMapView.GraphicsOverlays.FirstOrDefault();

                // Then hide it.
                MyMapView.GraphicsOverlays.Clear();
            }
            else if (_job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Failed)
            {
                // Notify the user.
                await Application.Current.MainPage.DisplayAlert("Error", "Job failed", "OK");

                // Update the UI.
                MyExportPreviewButton.Text = "Export Tiles";
                MyExportPreviewButton.IsVisible = true;
                MyExportPreviewButton.IsEnabled = true;

                // Set the preview open flag.
                _previewOpen = false;
            }

            // Remove the event handlers.
            _job.ProgressChanged -= Job_ProgressChanged;
            _job.StatusChanged -= Job_StatusChanged;
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
            MyMapView.Map = new Map(new Basemap(myLayer));
        }

        private async Task ExportPreviewButtonTask()
        {
            // If preview isn't open, start an export.
            try
            {
                if (!_previewOpen)
                {
                    // Disable the export button.
                    MyExportPreviewButton.IsEnabled = false;

                    // Save the map viewpoint.
                    _originalView = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

                    // Start the export.
                    await StartExport();
                }
                else // Otherwise, close the preview.
                {
                    // Update the UI.
                    MyExportPreviewButton.Text = "Export Tiles";
                    MyProgressBar.IsVisible = false;
                    MyProgressBarLabel.IsVisible = false;

                    // Clear the preview open flag.
                    _previewOpen = false;

                    // Re-apply the original map.
                    MyMapView.Map = _basemap;

                    // Re-apply the original viewpoint.
                    MyMapView.SetViewpoint(_originalView);

                    // Re-show the overlay.
                    MyMapView.GraphicsOverlays.Add(_overlay);

                    // Update the graphic.
                    UpdateMapExtentGraphic();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        #endregion Update Preview Map/Extent Graphic

        #region EventHandlers

        private void MyMapView_ViewpointChanged(object sender, EventArgs e)
        {
            UpdateMapExtentGraphic();
        }

        private void MyExportPreviewButton_Clicked(object sender, EventArgs e)
        {
            _ = ExportPreviewButtonTask();
        }

        private void MyCancelJobButton_Clicked(object sender, EventArgs e)
        {
            if (_job != null)
            {
                _ = _job.CancelAsync();

                // Remove the event handlers.
                _job.ProgressChanged -= Job_ProgressChanged;
                _job.StatusChanged -= Job_StatusChanged;

                // Update the UI.
                MyCancelJobButton.IsVisible = false;
                MyProgressBar.IsVisible = false;
                MyProgressBarLabel.IsVisible = false;
                MyExportPreviewButton.IsVisible = true;
                MyExportPreviewButton.IsEnabled = true;
            }
        }

        private void Job_ProgressChanged(object sender, EventArgs e)
        {
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                // Update the progress bar value.
                MyProgressBar.Progress = _job.Progress / 100.0;
                MyProgressBarLabel.Text = _job.Progress == 100 ? "Done" : $"{_job.Progress}%";
            });
        }

        private void Job_StatusChanged(object sender, Esri.ArcGISRuntime.Tasks.JobStatus e)
        {
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                MyCancelJobButton.IsVisible = (_job.Status != Esri.ArcGISRuntime.Tasks.JobStatus.Failed &&
                                               _job.Status != Esri.ArcGISRuntime.Tasks.JobStatus.Succeeded &&
                                               _job.Status != Esri.ArcGISRuntime.Tasks.JobStatus.Canceling);
            });
        }

        #endregion EventHandlers
    }
}