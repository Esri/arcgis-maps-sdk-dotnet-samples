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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;

namespace ArcGISRuntime.WinUI.Samples.ExportTiles
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Export tiles",
        category: "Layers",
        description: "Download tiles to a local tile cache file stored on the device.",
        instructions: "Pan and zoom into the desired area, making sure the area is within the red boundary. Click the 'Export tiles' button to start the process. On successful completion you will see a preview of the downloaded tile package.",
        tags: new[] { "cache", "download", "export", "local", "offline", "package", "tiles" })]
    public partial class ExportTiles
    {
        // URL to the service that tiles will be exported from.
        private Uri _serviceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer");

        // Path to the tile package on disk.
        private string _tilePath;

        public ExportTiles()
        {
            InitializeComponent();

            // Call a function to set up the map.
            Initialize();
        }

        private async void Initialize()
        {
            // Create the tile layer.
            try
            {
                ArcGISTiledLayer myLayer = new ArcGISTiledLayer(_serviceUri);

                // Load the layer.
                await myLayer.LoadAsync();

                // Create the basemap with the layer.
                Map myMap = new Map(new Basemap(myLayer))
                {

                    // Set the min and max scale - export task fails if the scale is too big or small.
                    MaxScale = 5000000,
                    MinScale = 10000000
                };

                // Assign the map to the mapview.
                MyMapView.Map = myMap;

                // Create a new symbol for the extent graphic.
                //     This is the red box that visualizes the extent for which tiles will be exported.
                SimpleLineSymbol myExtentSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 2);

                // Create a graphics overlay for the extent graphic and apply a renderer.
                GraphicsOverlay extentOverlay = new GraphicsOverlay
                {
                    Renderer = new SimpleRenderer(myExtentSymbol)
                };

                // Add the graphics overlay to the map view.
                MyMapView.GraphicsOverlays.Add(extentOverlay);

                // Subscribe to changes in the mapview's viewpoint so the preview box can be kept in position.
                MyMapView.ViewpointChanged += MyMapView_ViewpointChanged;

                // Update the extent graphic so that it is valid before user interaction.
                UpdateMapExtentGraphic();

                // Enable the export tile button once sample is ready.
                MyExportButton.IsEnabled = true;

                // Set viewpoint of the map.
                MyMapView.SetViewpoint(new Viewpoint(-4.853791, 140.983598, myMap.MinScale));
            }
            catch (Exception ex)
            {
                ShowStatusMessage(ex.ToString());
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

            // Return if the extent overlay is null.
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
            // Get the parameters for the job.
            ExportTileCacheParameters parameters = GetExportParameters();

            // Create the task.
            ExportTileCacheTask exportTask = await ExportTileCacheTask.CreateAsync(_serviceUri);

            // Get the tile cache path.
            _tilePath = $"{Path.GetTempFileName()}.tpk";

            // Create the export job.
            ExportTileCacheJob job = exportTask.ExportTileCache(parameters, _tilePath);

            // Start the export job.
            job.Start();

            // Get the result.
            TileCache cache = await job.GetResultAsync();

            // Do the rest of the work.
            await HandleExportComplete(job, cache);
        }

        private async Task HandleExportComplete(ExportTileCacheJob job, TileCache cache)
        {
            // Update the view if the job is complete.
            if (job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Succeeded)
            {
                // Show the exported tiles on the preview map.
                await UpdatePreviewMap(cache);

                // Show the preview window.
                MyPreviewMapView.Visibility = Visibility.Visible;

                // Show the 'close preview' button.
                MyClosePreviewButton.Visibility = Visibility.Visible;

                // Hide the 'export tiles' button.
                MyExportButton.Visibility = Visibility.Collapsed;

                // Hide the progress bar.
                MyProgressBar.Visibility = Visibility.Collapsed;

                // Enable the 'export tiles' button.
                MyExportButton.IsEnabled = true;
            }
            else if (job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Failed)
            {
                // Notify the user.
                ShowStatusMessage("Job failed");

                // Hide the progress bar.
                MyProgressBar.Visibility = Visibility.Collapsed;
            }
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

        private async Task UpdatePreviewMap(TileCache cache)
        {
            // Load the cache.
            await cache.LoadAsync();

            // Create a tile layer with the cache.
            ArcGISTiledLayer myLayer = new ArcGISTiledLayer(cache);

            // Apply the map to the preview mapview.
            MyPreviewMapView.Map = new Map(new Basemap(myLayer));
        }

        private async void MyExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MyExportButton.IsEnabled = false;

                // Show the progress bar.
                MyProgressBar.Visibility = Visibility.Visible;

                // Hide the preview window if not already hidden.
                MyPreviewMapView.Visibility = Visibility.Collapsed;

                // Hide the 'close preview' button if not already hidden.
                MyClosePreviewButton.Visibility = Visibility.Collapsed;

                // Show the 'export tiles' button.
                MyExportButton.Visibility = Visibility.Visible;

                // Start the export.
                await StartExport();
            }
            catch (Exception ex)
            {
                ShowStatusMessage(ex.ToString());
            }
        }

        private async void ShowStatusMessage(string message)
        {
            // Display the message to the user.
            await new MessageDialog(message).ShowAsync();
        }

        private void ClosePreview_Click(object sender, RoutedEventArgs e)
        {
            // Hide the preview map.
            MyPreviewMapView.Visibility = Visibility.Collapsed;

            // Hide the 'close preview' button.
            MyClosePreviewButton.Visibility = Visibility.Collapsed;

            // Show the 'export tiles' button.
            MyExportButton.Visibility = Visibility.Visible;
        }
    }
}