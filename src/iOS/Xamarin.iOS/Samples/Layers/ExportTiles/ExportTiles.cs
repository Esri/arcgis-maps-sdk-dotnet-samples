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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using UIKit;

namespace ArcGISRuntime.Samples.ExportTiles
{
    [Register("ExportTiles")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Export tiles",
        "Layers",
        "This sample demonstrates how to export tiles from a map server.",
        "1. Pan and zoom until the area you want tiles for is within the red box.\n2. Click 'Export Tiles'.\n3. Pan and zoom to see the area covered by the downloaded tiles in the preview box.")]
    public class ExportTiles : UIViewController
    {
        // Reference to the MapView used in the sample
        private MapView _myMapView;

        // Reference to the preview MapView
        private MapView _myPreviewMapView;

        // Reference to the progress bar
        private UIActivityIndicatorView _myProgressBar;

        // Reference to the 'export' button
        private UIButton _myExportButton;

        // URL to the service tiles will be exported from
        private Uri _serviceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer");

        // Path to exported tiles on disk
        private string _tilePath;

        // Flag to indicate if an exported cache is being previewed
        private bool _previewOpen = false;

        public ExportTiles()
        {
            Title = "Export tiles";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the layout
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Hold a margin value
            int margin = 30;

            // Set up the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Set up the visual frame for the preview MapView
            _myPreviewMapView.Frame = new CoreGraphics.CGRect(margin, margin * 2, View.Bounds.Width - 2 * margin, View.Bounds.Height - 4 * margin);

            // Set up the visual frame for the progress bar
            _myProgressBar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 40, View.Bounds.Width, 20);

            // Set up the visual frame for the button
            _myExportButton.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 20, View.Bounds.Width, 20);

            base.ViewDidLayoutSubviews();
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
            _myMapView.Map = myMap;

            // Create a new symbol for the extent graphic
            //     This is the red box that visualizes the extent for which tiles will be exported
            SimpleLineSymbol myExtentSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 2);

            // Create graphics overlay for the extent graphic and apply a renderer
            GraphicsOverlay extentOverlay = new GraphicsOverlay();
            extentOverlay.Renderer = new SimpleRenderer(myExtentSymbol);

            // Add graphics overlay to the map view
            _myMapView.GraphicsOverlays.Add(extentOverlay);

            // Subscribe to changes in the mapview's viewpoint so the preview box can be kept in position
            _myMapView.ViewpointChanged += MyMapView_ViewpointChanged;

            // Update the graphic - needed in case the user decides not to interact before pressing the button
            UpdateMapExtentGraphic();

            // Enable the export button now that sample is ready
            _myExportButton.Enabled = true;
        }

        private void CreateLayout()
        {
            // Create the first mapview
            _myMapView = new MapView();

            // Create the preview mapview
            _myPreviewMapView = new MapView()
            {
                Hidden = true // hide it by default
            };

            // Set a border on the preview window
            _myPreviewMapView.Layer.BorderColor = new CoreGraphics.CGColor(.8f, .2f, .6f);
            _myPreviewMapView.Layer.BorderWidth = 2.0f;

            // Create the progress bar
            _myProgressBar = new UIActivityIndicatorView();

            // Set the progress bar to hide when not animating
            _myProgressBar.HidesWhenStopped = true;

            // Create the export button - disabled until sample is ready
            _myExportButton = new UIButton() { Enabled = false };
            _myExportButton.SetTitle("Export", UIControlState.Normal);

            // Set background color on the button and progressbar
            _myExportButton.BackgroundColor = UIColor.LightGray;
            _myProgressBar.BackgroundColor = UIColor.LightGray;

            // Get notified of button taps
            _myExportButton.TouchUpInside += MyExportButton_Click;

            // Add the views
            View.AddSubviews(_myMapView, _myProgressBar, _myExportButton, _myPreviewMapView);
        }

        private void MyMapView_ViewpointChanged(object sender, EventArgs e)
        {
            UpdateMapExtentGraphic();
        }

        /// <summary>
        /// Function used to keep the overlaid preview area marker in position.
        /// This is called by MyMapView_ViewpointChanged every time the user pans/zooms
        ///     and updates the red box graphic to outline 70% of the current view
        /// </summary>
        private void UpdateMapExtentGraphic()
        {
            // Return if mapview is null
            if (_myMapView == null) { return; }

            // Get the new viewpoint
            Viewpoint myViewPoint = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

            // Return if viewpoint is null
            if (myViewPoint == null) { return; }

            // Get the updated extent for the new viewpoint
            Envelope extent = myViewPoint.TargetGeometry as Envelope;

            // Return if extent is null
            if (extent == null) { return; }

            // Create an envelope that is a bit smaller than the extent
            EnvelopeBuilder envelopeBldr = new EnvelopeBuilder(extent);
            envelopeBldr.Expand(0.70);

            // Get the (only) graphics overlay in the map view
            GraphicsOverlay extentOverlay = _myMapView.GraphicsOverlays.FirstOrDefault();

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
            // Return a path
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

            // Update tile cache path
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
            GraphicsOverlay extentOverlay = _myMapView.GraphicsOverlays.First();

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
        private void Job_JobChanged(object sender, EventArgs e)
        {
            // Get reference to the job
            ExportTileCacheJob job = sender as ExportTileCacheJob;

            // Update the view if the job is complete
            if (job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Succeeded)
            {
                // Dispatcher is necessary due to the threading implementation;
                //     this method is called from a thread other than the UI thread
                InvokeOnMainThread(() =>
                {
                    // Show the exported tiles on the preview map
                    UpdatePreviewMap();

                    // Show the preview window
                    _myPreviewMapView.Hidden = false;

                    // Hide the progress bar
                    _myProgressBar.StopAnimating();

                    // Change the export button text
                    _myExportButton.SetTitle("Close Preview", UIControlState.Normal);

                    // Re-enable the button
                    _myExportButton.Enabled = true;

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
                InvokeOnMainThread(() =>
                {
                    // Hide the progress bar
                    _myProgressBar.StopAnimating();

                    // Change the export button text
                    _myExportButton.SetTitle("Export Tiles", UIControlState.Normal);

                    // Re-enable the export button
                    _myExportButton.Enabled = true;

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
            // Load the saved tile cache
            TileCache cache = new TileCache(_tilePath);

            // Load the cache
            await cache.LoadAsync();

            // Create a tile layer with the cache
            ArcGISTiledLayer myLayer = new ArcGISTiledLayer(cache);

            // Create a new map with the layer as basemap
            Map previewMap = new Map(new Basemap(myLayer));

            // Apply the map to the preview mapview
            _myPreviewMapView.Map = previewMap;
        }

        /// <summary>
        /// Begins the export process
        /// </summary>
        private void MyExportButton_Click(object sender, EventArgs e)
        {
            // If preview isn't open, start an export
            if (!_previewOpen)
            {
                // Disable the export button
                _myExportButton.Enabled = false;

                // Show the progress bar
                _myProgressBar.StartAnimating();

                // Hide the preview window if not already hidden
                _myPreviewMapView.Hidden = true;

                // Start the export
                StartExport();
            }
            else // Otherwise, close the preview
            {
                // Hide the preview
                _myPreviewMapView.Hidden = true;

                // Change the button text
                _myExportButton.SetTitle("Export Tiles", UIControlState.Normal);

                // Clear the preview open flag
                _previewOpen = false;
            }
        }

        /// <summary>
        /// Abstraction of platform-specific functionality to maximize re-use of common code
        /// </summary>
        private void ShowStatusMessage(string message)
        {
            // Display the message to the user
            UIAlertView alertView = new UIAlertView("alert", message, null, "OK", null);
            alertView.Show();
        }
    }
}