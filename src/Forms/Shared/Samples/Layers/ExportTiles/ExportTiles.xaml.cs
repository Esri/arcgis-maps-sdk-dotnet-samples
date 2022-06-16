// Copyright 2018 Esri.
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
using Xamarin.Forms;
using Colors = System.Drawing.Color;

namespace ArcGISRuntime.Samples.ExportTiles
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Export tiles",
        category: "Layers",
        description: "Download tiles to a local tile cache file stored on the device.",
        instructions: "Pan and zoom into the desired area, making sure the area is within the red boundary. Tap the 'Export tiles' button to start the process. On successful completion you will see a preview of the downloaded tile package.",
        tags: new[] { "cache", "download", "export", "local", "offline", "package", "tiles" })]
    public partial class ExportTiles : ContentPage
    {
        // URL to the service tiles will be exported from.
        private Uri _serviceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer");

        // Path to exported tile cache.
        private string _tilePath;

        // Flag to indicate whether an exported tile cache is being previewed.
        private bool _previewOpen = false;

        // Reference to the original basemap.
        private Map _basemap;

        // Reference to the original viewpoint (when previewing).
        private Viewpoint _originalView;

        // Holder for the Graphics Overlay (so that it can be hidden and re-added for preview/non-preview state).
        private GraphicsOverlay _overlay;

        // Hold a reference to the export job for use in event handler.
        private ExportTileCacheJob _job;

        public ExportTiles()
        {
            InitializeComponent();

            // Call a function to set up the sample.
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create the tile layer.
            try
            {
                ArcGISTiledLayer myLayer = new ArcGISTiledLayer(_serviceUri);

                // Load the layer.
                await myLayer.LoadAsync();

                // Create the basemap with the layer.
                _basemap = new Map(new Basemap(myLayer))
                {
                    // Set the min and max scale - export task fails if the scale is too big or small.
                    MaxScale = 5000000,
                    MinScale = 10000000
                };

                // Assign the map to the mapview.
                MyMapView.Map = _basemap;

                // Create a new symbol for the extent graphic.
                //     This is the red box that visualizes the extent for which tiles will be exported.
                SimpleLineSymbol myExtentSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Red, 2);

                // Create graphics overlay for the extent graphic and apply a renderer.
                GraphicsOverlay extentOverlay = new GraphicsOverlay
                {
                    Renderer = new SimpleRenderer(myExtentSymbol)
                };

                // Add graphics overlay to the map view.
                MyMapView.GraphicsOverlays.Add(extentOverlay);

                // Subscribe to changes in the mapview's viewpoint so the preview box can be kept in position.
                MyMapView.ViewpointChanged += MyMapView_ViewpointChanged;

                // Update the graphic - needed in case the user decides not to interact before pressing the button.
                UpdateMapExtentGraphic();

                // Enable the export button once the sample is ready.
                MyExportPreviewButton.IsEnabled = true;

                // Set viewpoint of the map.
                MyMapView.SetViewpoint(new Viewpoint(-4.853791, 140.983598, _basemap.MinScale));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void MyMapView_ViewpointChanged(object sender, EventArgs e)
        {
            UpdateMapExtentGraphic();
        }

        /// <summary>
        /// Function used to keep the overlaid preview area marker in position
        /// This is called by MyMapView_ViewpointChanged every time the user pans/zooms
        ///     and updates the red box graphic to outline 80% of the current view
        /// </summary>
        private void UpdateMapExtentGraphic()
        {
            // Return if mapview is null.
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
            EnvelopeBuilder envelopeBldr = new EnvelopeBuilder(extent);
            envelopeBldr.Expand(0.80);

            // Get the (only) graphics overlay in the map view.
            GraphicsOverlay extentOverlay = MyMapView.GraphicsOverlays.FirstOrDefault();

            // Return if there is none (in preview, the overlay shouldn't be there).
            if (extentOverlay == null) { return; }

            // Get the extent graphic.
            Graphic extentGraphic = extentOverlay.Graphics.FirstOrDefault();

            // Create the extent graphic and add it to the overlay if it doesn't exist.
            if (extentGraphic == null)
            {
                extentGraphic = new Graphic(envelopeBldr.ToGeometry());
                extentOverlay.Graphics.Add(extentGraphic);
            }
            else
            {
                // Otherwise, update the graphic's geometry.
                extentGraphic.Geometry = envelopeBldr.ToGeometry();
            }
        }

        private async Task StartExport()
        {
            try
            {
                // Update the tile cache path.
                _tilePath = $"{Path.GetTempFileName()}.tpk";

                // Get the parameters for the job.
                ExportTileCacheParameters parameters = GetExportParameters();

                // Create the task.
                ExportTileCacheTask exportTask = await ExportTileCacheTask.CreateAsync(_serviceUri);

                // Create the export job.
                _job = exportTask.ExportTileCache(parameters, _tilePath);

                // Set the value of the progress bar to 0, this clears any previous progress on the bar.
                MyProgressBar.Progress = 0.0;

                // Show the progress bar and label.
                MyProgressBar.IsVisible = true;
                MyProgressBarLabel.IsVisible = true;

                // Show the cancel job button.
                MyCancelJobButton.IsVisible = true;

                // Hide the export/close preview button.
                MyExportPreviewButton.IsVisible = false;

                // Add an event handler to update the progress bar as the task progresses.
                _job.ProgressChanged += (o, e) =>
                {
                    UpdateProgressBar(_job.Progress);
                };

                // Add an event handler to hide the cancel job button if the job completes, fails or is cancelled.
                _job.StatusChanged += (s, e) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (_job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Failed
                        || _job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Succeeded
                        || _job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Canceling)
                        {
                            MyCancelJobButton.IsVisible = false;
                        }
                    });
                };

                // Start the export job.
                _job.Start();

                // Get the tile cache result.
                TileCache resultTileCache = await _job.GetResultAsync();

                // Do the rest of the work.
                await HandleJobCompletion(resultTileCache);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
        private void UpdateProgressBar(int progress)
        {
            // Due to the nature of the threading implementation,
            //     the dispatcher needs to be used to interact with the UI.
            // The dispatcher takes an Action, provided here as a lambda function.
            Device.BeginInvokeOnMainThread(() =>
            {
                // Update the progress bar value.
                MyProgressBar.Progress = progress / 100.0;
                MyProgressBarLabel.Text = progress == 100 ? "Done" : $"{progress}%";
            });
        }

        private ExportTileCacheParameters GetExportParameters()
        {
            // Create a new parameters instance.
            ExportTileCacheParameters parameters = new ExportTileCacheParameters();

            // Get the (only) graphics overlay in the map view.
            GraphicsOverlay extentOverlay = MyMapView.GraphicsOverlays.First();

            // Get the area selection graphic's extent.
            Graphic extentGraphic = extentOverlay.Graphics.First();

            // Set the area for the export.
            parameters.AreaOfInterest = extentGraphic.Geometry;

            // Set the highest possible export quality.
            parameters.CompressionQuality = 100;

            // Add level IDs 1-9.
            //     Note: Failing to add at least one Level ID will result in job failure.
            for (int x = 1; x < 10; x++)
            {
                parameters.LevelIds.Add(x);
            }

            // Return the parameters.
            return parameters;
        }

        private async Task HandleJobCompletion(TileCache cache)
        {
            // Update the view if the job is complete.
            if (_job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Succeeded)
            {
                // Show the exported tiles on the preview map.
                await UpdatePreviewMap(cache);

                // Change the export button text.
                MyExportPreviewButton.Text = "Close Preview";

                // Show the export/close preview button.
                MyExportPreviewButton.IsVisible = true;

                // Re-enable the button.
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
                await Application.Current.MainPage.DisplayAlert("Error", "Job Failed", "OK");

                // Change the export button text.
                MyExportPreviewButton.Text = "Export Tiles";

                // Show the export/close preview button.
                MyExportPreviewButton.IsVisible = true;

                // Re-enable the export button.
                MyExportPreviewButton.IsEnabled = true;

                // Set the preview open flag.
                _previewOpen = false;
            }
        }

        private async Task UpdatePreviewMap(TileCache cache)
        {
            // Load the cache.
            await cache.LoadAsync();

            // Create a tile layer with the cache.
            ArcGISTiledLayer myLayer = new ArcGISTiledLayer(cache);

            // Show the layer in a new map.
            MyMapView.Map = new Map(new Basemap(myLayer));

            // Re-size the mapview.
            MyMapView.Margin = new Thickness(40);
        }

        private async void MyExportPreviewButton_Clicked(object sender, EventArgs e)
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
                    // Change the button text.
                    MyExportPreviewButton.Text = "Export Tiles";

                    // Hide the progress bar and label.
                    MyProgressBar.IsVisible = false;
                    MyProgressBarLabel.IsVisible = false;

                    // Clear the preview open flag.
                    _previewOpen = false;

                    // Re-size the mapview.
                    MyMapView.Margin = new Thickness(0);

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

        private void MyCancelJobButton_Clicked(object sender, EventArgs e)
        {
            if (_job != null)
            {
                _ = _job.CancelAsync();

                // Hide the cancel job button.
                MyCancelJobButton.IsVisible = false;

                // Hide the progress bar and label.
                MyProgressBar.IsVisible = false;
                MyProgressBarLabel.IsVisible = false;

                // Show the export/close preview button.
                MyExportPreviewButton.IsVisible = true;

                // Re-enable the export button.
                MyExportPreviewButton.IsEnabled = true;
            }
        }
    }
}