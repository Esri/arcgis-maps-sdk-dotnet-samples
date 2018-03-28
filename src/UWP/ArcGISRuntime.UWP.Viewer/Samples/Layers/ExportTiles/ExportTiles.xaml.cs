// Copyright 2017 Esri.
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
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.UWP.Samples.ExportTiles
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Export tiles",
        "Layers",
        "This sample demonstrates how to export tiles from a map server.",
        "1. Pan and zoom until the area you want tiles for is within the red box.\n2. Click 'Export Tiles'.\n3. Pan and zoom to see the area covered by the downloaded tiles in the preview box.")]
    public partial class ExportTiles
    {
        // URL to the service tiles will be exported from
        private Uri _serviceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer");

        // Path to the tile package on disk
        private string _tilePath;

        public ExportTiles()
        {
            InitializeComponent();

            // Call a function to set up the map
            Initialize();
        }

        private async void Initialize()
        {
            // Create the tile layer
            ArcGISTiledLayer myLayer = new ArcGISTiledLayer(_serviceUri);

            // Load the layer
            await myLayer.LoadAsync();

            // Create the basemap with the layer
            Map myMap = new Map(new Basemap(myLayer));

            // Set the min and max scale - export task fails if the scale is too big or small
            myMap.MaxScale = 5000000;
            myMap.MinScale = 10000000;

            // Assign the map to the mapview
            MyMapView.Map = myMap;

            // Create a new symbol for the extent graphic
            //     This is the red box that visualizes the extent for which tiles will be exported
            SimpleLineSymbol myExtentSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Red, 2);

            // Create graphics overlay for the extent graphic and apply a renderer
            GraphicsOverlay extentOverlay = new GraphicsOverlay();
            extentOverlay.Renderer = new SimpleRenderer(myExtentSymbol);

            // Add graphics overlay to the map view
            MyMapView.GraphicsOverlays.Add(extentOverlay);

            // Subscribe to changes in the mapview's viewpoint so the preview box can be kept in position
            MyMapView.ViewpointChanged += MyMapView_ViewpointChanged;

            // Update the extent graphic so that it is valid before user interaction
            UpdateMapExtentGraphic();

            // Enable the export tile button once sample is ready
            MyExportButton.IsEnabled = true;
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
            // Return if mapview is null
            if (MyMapView == null) { return; }

            // Get the new viewpoint
            Viewpoint myViewPoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

            // Return if viewpoint is null
            if (myViewPoint == null) { return; }

            // Get the updated extent for the new viewpoint
            Envelope extent = myViewPoint.TargetGeometry as Envelope;

            // Return if extent is null
            if (extent == null) { return; }

            // Create an envelope that is a bit smaller than the extent
            EnvelopeBuilder envelopeBldr = new EnvelopeBuilder(extent);
            envelopeBldr.Expand(0.80);

            // Get the (only) graphics overlay in the map view
            GraphicsOverlay extentOverlay = MyMapView.GraphicsOverlays.FirstOrDefault();

            // Return if the extent overlay is null
            if (extentOverlay == null) { return; }

            // Get the extent graphic
            Graphic extentGraphic = extentOverlay.Graphics.FirstOrDefault();

            // Create the extent graphic and add it to the overlay if it doesn't exist
            if (extentGraphic == null)
            {
                extentGraphic = new Graphic(envelopeBldr.ToGeometry());
                extentOverlay.Graphics.Add(extentGraphic);
            }
            else
            {
                // Otherwise, simply update the graphic's geometry
                extentGraphic.Geometry = envelopeBldr.ToGeometry();
            }
        }

        private string GetTilePath()
        {
            // Return the platform-specific path for storing the tile cache
            return $"{Path.GetTempFileName()}.tpk";
        }

        /// <summary>
        /// Starts the export job and registers callbacks to be notified of changes to job status
        /// </summary>
        private async void StartExport()
        {
            // Get the parameters for the job
            ExportTileCacheParameters parameters = GetExportParameters();

            // Create the task
            ExportTileCacheTask exportTask = await ExportTileCacheTask.CreateAsync(_serviceUri);

            // Get the tile cache path
            _tilePath = GetTilePath();

            // Create the export job
            ExportTileCacheJob job = exportTask.ExportTileCache(parameters, _tilePath);

            // Subscribe to notifications for status updates
            job.JobChanged += Job_JobChanged;

            // Start the export job
            job.Start();
        }

        /// <summary>
        /// Constructs and returns parameters for the Export Tile Cache Job
        /// </summary>
        /// <returns></returns>
        private ExportTileCacheParameters GetExportParameters()
        {
            // Create a new parameters instance
            ExportTileCacheParameters parameters = new ExportTileCacheParameters();

            // Get the (only) graphics overlay in the map view
            GraphicsOverlay extentOverlay = MyMapView.GraphicsOverlays.First();

            // Get the area selection graphic's extent
            Graphic extentGraphic = extentOverlay.Graphics.First();

            // Set the area for the export
            parameters.AreaOfInterest = extentGraphic.Geometry;

            // Get the highest possible export quality
            parameters.CompressionQuality = 100;

            // Add level IDs 1-9
            //     Note: Failing to add at least one Level ID will result in job failure
            for (int x = 1; x < 10; x++)
            {
                parameters.LevelIds.Add(x);
            }

            // Return the parameters
            return parameters;
        }

        /// <summary>
        /// Called by the ExportTileCacheJob on any status changes
        /// </summary>
        private async void Job_JobChanged(object sender, EventArgs e)
        {
            // Get reference to the job
            ExportTileCacheJob job = sender as ExportTileCacheJob;

            // Update the view if the job is complete
            if (job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Succeeded)
            {
                // Dispatcher is necessary due to the threading implementation;
                //     this method is called from a thread other than the UI thread
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // Show the exported tiles on the preview map
                    UpdatePreviewMap();

                    // Show the preview window
                    MyPreviewMapView.Visibility = Visibility.Visible;

                    // Show the 'close preview' button
                    MyClosePreviewButton.Visibility = Visibility.Visible;

                    // Hide the 'export tiles' button
                    MyExportButton.Visibility = Visibility.Collapsed;

                    // Hide the progress bar
                    MyProgressBar.Visibility = Visibility.Collapsed;

                    // Enable the 'export tiles' button
                    MyExportButton.IsEnabled = true;
                });
            }
            else if (job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Failed)
            {
                // Notify the user
                ShowStatusMessage("Job failed");

                // Dispatcher is necessary due to the threading implementation;
                //     this method is called from a thread other than the UI thread
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // Hide the progress bar
                    MyProgressBar.Visibility = Visibility.Collapsed;
                });
            }
        }

        /// <summary>
        /// Loads the tile cache from disk and displays it in the preview map
        /// </summary>
        private async void UpdatePreviewMap()
        {
            // Load the saved tile cache
            TileCache cache = new TileCache(_tilePath);

            // Load the cache
            await cache.LoadAsync();

            // Create a tile layer with the cache
            ArcGISTiledLayer myLayer = new ArcGISTiledLayer(cache);

            // Create a new map with the layer as basemap
            Map previewMap = new Map(new Basemap(myLayer));

            // Apply the map to the preview mapview
            MyPreviewMapView.Map = previewMap;
        }

        /// <summary>
        /// Begins the export process
        /// </summary>
        private void MyExportButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable the export button
            MyExportButton.IsEnabled = false;

            // Show the progress bar
            MyProgressBar.Visibility = Visibility.Visible;

            // Hide the preview window if not already hidden
            MyPreviewMapView.Visibility = Visibility.Collapsed;

            // Hide the 'close preview' button if not already hidden
            MyClosePreviewButton.Visibility = Visibility.Collapsed;

            // Show the 'export tiles' button
            MyExportButton.Visibility = Visibility.Visible;

            // Start the export
            StartExport();
        }

        /// <summary>
        /// Abstraction of platform-specific functionality to maximize re-use of common code
        /// </summary>
        private async void ShowStatusMessage(string message)
        {
            // Display the message to the user
            MessageDialog dialog = new MessageDialog(message);
            await dialog.ShowAsync();
        }

        private void ClosePreview_Click(object sender, RoutedEventArgs e)
        {
            // Hide the preview map
            MyPreviewMapView.Visibility = Visibility.Collapsed;

            // Hide the 'close preview' button
            MyClosePreviewButton.Visibility = Visibility.Collapsed;

            // Show the 'export tiles' button
            MyExportButton.Visibility = Visibility.Visible;
        }
    }
}