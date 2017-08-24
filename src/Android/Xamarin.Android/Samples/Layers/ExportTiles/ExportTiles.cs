// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Drawing;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.ExportTiles
{
    [Activity(Label = "ExportTiles")]
    public class ExportTiles : Activity
    {
        // Reference to the MapView used in the sample
        private MapView _myMapView;

        // Reference to the base tiled map
        Map _basemap;

        // Reference to the progress bar
        private ProgressBar _myProgressBar;

        // Reference to the 'export' button
        private Button _myExportButton;

        // URL to the service tiles will be exported from
        private Uri _serviceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer");

        // Flag to indicate if an exported cache is being previewed
        private bool _previewOpen = false;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			Title = "Export tiles";

			// Create the layout
			CreateLayout();

			// Initialize the app
			Initialize();
		}

        private void CreateLayout()
        {
            // Create a stack layout
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the mapview
            _myMapView = new MapView();

            // Create the progress bar
            _myProgressBar = new ProgressBar(this)
            {
                Indeterminate = true, // Set the progress bar to be indeterminate (no value)
                Visibility = Android.Views.ViewStates.Gone
            };

            // Create the export button
            _myExportButton = new Button(this)
            {
                Text = "Export Tiles"
            };

            // Get notified of button taps
            _myExportButton.Click += MyExportButton_Click;

            // Add views to the layout
            layout.AddView(_myProgressBar);
            layout.AddView(_myExportButton);
			layout.AddView(_myMapView);

            // Set the layout as the sample view
            SetContentView(layout);
        }

		private async void Initialize()
		{
			// Create the tile layer
			ArcGISTiledLayer layer = new ArcGISTiledLayer(_serviceUri);

			// Load the layer
			await layer.LoadAsync();

			// Create the basemap with the layer
			_basemap = new Map(new Basemap(layer));

			// Assign the map to the mapview
			_myMapView.Map = _basemap;

			// Create a new symbol for the extent graphic
			SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 2);

			// Create graphics overlay for the extent graphic and apply a renderer
			GraphicsOverlay extentOverlay = new GraphicsOverlay();
			extentOverlay.Renderer = new SimpleRenderer(lineSymbol);

			// Add graphics overlay to the map view
			_myMapView.GraphicsOverlays.Add(extentOverlay);

			// Subscribe to changes in the mapview's viewpoint so the preview box can be kept in position
			_myMapView.ViewpointChanged += MyMapView_ViewpointChanged;
		}

        private void MyMapView_ViewpointChanged(object sender, EventArgs e)
        {
            UpdateMapExtent();
        }

        /// <summary>
        /// Function used to keep the overlaid preview area marker in position
        /// </summary>
        private void UpdateMapExtent()
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
            envelopeBldr.Expand(0.80);

            // Get the (only) graphics overlay in the map view
            GraphicsOverlay extentOverlay = _myMapView.GraphicsOverlays.FirstOrDefault();

            // Return if the extent overlay is null
            if (extentOverlay == null) { return; }

            // Get the extent graphic
            Graphic extentGraphic = extentOverlay.Graphics.FirstOrDefault();

            // Create the extent graphic and add it to the overlay if it doesn't exist and we're not in preview
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
            return GetFileStreamPath("myTileCache.tpk").AbsolutePath;
        }

        /// <summary>
        /// Starts the export job and registers callbacks to keep aprised of job status
        /// </summary>
        private async void StartExport()
        {
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
            GraphicsOverlay extentOverlay = _myMapView.GraphicsOverlays.FirstOrDefault();

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
                RunOnUiThread(() =>
                {
                    // Show the exported tiles on the preview map
                    UpdatePreviewMap();

                    // Hide the progress bar
                    _myProgressBar.Visibility = Android.Views.ViewStates.Gone;

                    // Change the export button text
                    _myExportButton.Text = "Close Preview";

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
                RunOnUiThread(() =>
                {
                    // Hide the progress bar
                    _myProgressBar.Visibility = Android.Views.ViewStates.Gone;

                    // Change the export button text
                    _myExportButton.Text = "Export tiles";

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
            ArcGISTiledLayer layer = new ArcGISTiledLayer(cache);

            // Create a new map with the layer as basemap
            Map previewMap = new Map(new Basemap(layer));

            // Apply the map to the mapview
            _myMapView.Map = previewMap;
        }

        /// <summary>
        /// Begins the export process
        /// </summary>
        private void MyExportButton_Click(object sender, EventArgs e)
        {
            // If preview isn't open, start an export
            if (!_previewOpen)
            {
                // Show the progress bar
                _myProgressBar.Visibility = Android.Views.ViewStates.Visible;


                // Start the export
                StartExport();
            }
            else // Otherwise, close the preview
            {
                // Apply the old basemap
                _myMapView.Map = _basemap;

                // Change the button text
                _myExportButton.Text = "Export Tiles";

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
            var builder = new AlertDialog.Builder(this);
            builder.SetMessage(message).SetTitle("Alert").Show();
        }
    }
}