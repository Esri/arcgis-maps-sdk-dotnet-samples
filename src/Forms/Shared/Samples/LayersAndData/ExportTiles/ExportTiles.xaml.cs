﻿// Copyright 2017 Esri.
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
using System.Linq;
using Xamarin.Forms;
using System.IO;

#if WINDOWS_UWP
using Colors = Windows.UI.Colors;
#else
using Colors = System.Drawing.Color;
#endif

namespace ArcGISRuntimeXamarin.Samples.ExportTiles
{
    public partial class ExportTiles : ContentPage
    {
        // URL to the service tiles will be exported from
        private Uri _serviceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer");

        // Flag to indicate whether an exported tile cache is being previewed
        private bool _previewOpen = false;

        // Keep count of how many times the sample has been run to avoid overwriting cache files
        private int _runCount = 0;

        // Reference to the original basemap
        private Map _basemap;

        // Reference to the original viewpoint (when previewing)
        private Viewpoint _originalView;

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
            _basemap = new Map(new Basemap(myLayer));

            // Assign the map to the mapview
            MyMapView.Map = _basemap;

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
            String folder = "";

#if NETFX_CORE //UWP
            folder = Windows.Storage.ApplicationData.Current.LocalFolder.Path.ToString();
#elif __IOS__
            folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif __ANDROID__
            folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
            // Set the final path
            return Path.Combine(folder, String.Format("myTileCache{0}.tpk", _runCount));
        }

        /// <summary>
        /// Starts the export job and registers callbacks to be notified of changes to job status
        /// </summary>
        private async void StartExport()
        {
            // Increment the run count
            _runCount++;

            // Get the parameters for the job
            ExportTileCacheParameters parameters = GetExportParameters();

            // Create the task
            ExportTileCacheTask exportTask = await ExportTileCacheTask.CreateAsync(_serviceUri);

            // Get the tile cache path
            String filePath = GetTilePath();

            // Create the export job
            ExportTileCacheJob job = exportTask.ExportTileCache(parameters, filePath);

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
            GraphicsOverlay extentOverlay = MyMapView.GraphicsOverlays.FirstOrDefault();

            // Get the area selection graphic's extent
            Graphic extentGraphic = extentOverlay.Graphics.FirstOrDefault();

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
        private void Job_JobChanged(object sender, EventArgs e)
        {
            // Get reference to the job
            ExportTileCacheJob job = sender as ExportTileCacheJob;

            // Update the view if the job is complete
            if (job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Succeeded)
            {
                // Dispatcher is necessary due to the threading implementation;
                //     this method is called from a thread other than the UI thread
                Device.BeginInvokeOnMainThread(() =>
                {
                    // Show the exported tiles on the preview map
                    UpdatePreviewMap();

                    // Hide the progress bar
                    MyProgressBar.IsVisible = false;

                    // Change the export button text
                    MyExportPreviewButton.Text = "Close Preview";

                    // Set the preview open flag
                    _previewOpen = true;
                });
            }
            else if (job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Failed)
            {
                // Notify the user
                ShowStatusMessage("Job failed");

                // Dispatcher is necessary due to the threading implementation;
                //     this method is called from a thread other than the UI thread
                Device.BeginInvokeOnMainThread(() =>
                {
                    // Hide the progress bar
                    MyProgressBar.IsVisible = false;

                    // Change the export button text
                    MyExportPreviewButton.Text = "Export Tiles";

                    // Set the preview open flag
                    _previewOpen = false;
                });
            }
        }

        /// <summary>
        /// Loads the tile cache from disk and displays it in the preview map
        /// </summary>
        private async void UpdatePreviewMap()
        {
            // Get the path to the cache
            String filePath = GetTilePath();

            // Load the saved tile cache
            TileCache cache = new TileCache(filePath);

            // Load the cache
            await cache.LoadAsync();

            // Create a tile layer with the cache
            ArcGISTiledLayer myLayer = new ArcGISTiledLayer(cache);

            // Create a new map with the layer as basemap
            Map previewMap = new Map(new Basemap(myLayer));

            // Apply the map to the preview mapview
            MyMapView.Map = previewMap;

            // Re-size the mapview
            MyMapView.Margin = new Thickness(40);
        }

        /// <summary>
        /// Begins the export process
        /// </summary>
        private void MyExportPreviewButton_Clicked(object sender, EventArgs e)
        {
            // If preview isn't open, start an export
            if (!_previewOpen)
            {
                // Show the progress bar
                MyProgressBar.IsVisible = true;

                // Save the map viewpoint
                _originalView = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

                // Start the export
                StartExport();
            }
            else // Otherwise, close the preview
            {
                // Change the button text
                MyExportPreviewButton.Text = "Export Tiles";

                // Clear the preview open flag
                _previewOpen = false;

                // Re-size the mapview
                MyMapView.Margin = new Thickness(0);

                // Re-apply the original map
                MyMapView.Map = _basemap;

                // Re-apply the original viewpoint
                MyMapView.SetViewpoint(_originalView);
            }
        }

        /// <summary>
        /// Abstraction of platform-specific functionality to maximize re-use of common code
        /// </summary>
        private async void ShowStatusMessage(string message)
        {
            // Display the message to the user
            await DisplayAlert("Alert", message, "OK");
        }
    }
}