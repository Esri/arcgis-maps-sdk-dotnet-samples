// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
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
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.ExportTiles
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Export tiles",
        category: "Layers",
        description: "Download tiles to a local tile cache file stored on the device.",
        instructions: "Pan and zoom into the desired area, making sure the area is within the red boundary. Tap the 'Export tiles' button to start the process. On successful completion you will see a preview of the downloaded tile package.",
        tags: new[] { "cache", "download", "export", "local", "offline", "package", "tiles" })]
    public class ExportTiles : Activity
    {
        // Reference to the MapView used in the sample.
        private MapView _myMapView;

        // Reference to the base tiled map.
        private Map _basemap;

        // Reference to the viewpoint (for previewing).
        private Viewpoint _originalViewpoint;

        // Reference to the progress bar.
        private ProgressBar _myProgressBar;

        // Reference to the 'export' button.
        private Button _myExportButton;

        // URL to the service tiles will be exported from.
        private Uri _serviceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer");

        // Path to the tile cache.
        private string _tilePath;

        // Flag to indicate if an exported cache is being previewed.
        private bool _previewOpen = false;

        // Layout container for setting a margin on the mapview.
        private RelativeLayout _layoutContainer;

        // Hold the graphics overlay so that overlay graphic can be added and removed.
        private GraphicsOverlay _overlay;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Export tiles";

            // Create the layout.
            CreateLayout();

            // Initialize the app.
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a stack layout.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            layout.LayoutParameters = new ViewGroup.MarginLayoutParams(ViewGroup.MarginLayoutParams.MatchParent, ViewGroup.MarginLayoutParams.MatchParent);

            // Create the mapview.
            _myMapView = new MapView(this);

            // Create the mapview's container.
            _layoutContainer = new RelativeLayout(this);
            _layoutContainer.AddView(_myMapView);

            // Create the progress bar.
            _myProgressBar = new ProgressBar(this)
            {
                Indeterminate = true, // Set the progress bar to be indeterminate (no value).
                Visibility = ViewStates.Gone
            };

            // Create the export button (disabled until sample is ready).
            _myExportButton = new Button(this)
            {
                Text = "Export Tiles",
                Enabled = false
            };

            // Get notified of button taps.
            _myExportButton.Click += MyExportButton_Click;

            // Add views to the layout.
            layout.AddView(_myProgressBar);
            layout.AddView(_myExportButton);
            layout.AddView(_layoutContainer);

            // Set the layout background color.
            layout.SetBackgroundColor(Android.Graphics.Color.MediumVioletRed);

            // Set the layout as the sample view.
            SetContentView(layout);
        }

        private async void Initialize()
        {
            try
            {
                // Create the tile layer.
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
                _myMapView.Map = _basemap;

                // Create a new symbol for the extent graphic.
                //     This is the red box that visualizes the extent for which tiles will be exported.
                SimpleLineSymbol myExtentSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 2);

                // Create a graphics overlay for the extent graphic and apply a renderer.
                GraphicsOverlay extentOverlay = new GraphicsOverlay
                {
                    Renderer = new SimpleRenderer(myExtentSymbol)
                };

                // Add the graphics overlay to the map view.
                _myMapView.GraphicsOverlays.Add(extentOverlay);

                // Subscribe to changes in the mapview's viewpoint so the preview box can be kept in position.
                _myMapView.ViewpointChanged += MyMapView_ViewpointChanged;

                // Update the graphic - in case user doesn't interact with the map.
                UpdateMapExtentGraphic();

                // Enable export button now that sample is ready.
                _myExportButton.Enabled = true;

                // Set viewpoint of the map.
                _myMapView.SetViewpoint(new Viewpoint(-4.853791, 140.983598, _basemap.MinScale));
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
        /// Function used to keep the overlaid preview area marker in position.
        /// This is called by MyMapView_ViewpointChanged every time the user pans/zooms
        ///     and updates the red box graphic to outline 80% of the current view.
        /// </summary>
        private void UpdateMapExtentGraphic()
        {
            // Return if mapview is null.
            if (_myMapView == null) { return; }

            // Get the new viewpoint.
            Viewpoint myViewPoint = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

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
            GraphicsOverlay extentOverlay = _myMapView.GraphicsOverlays.FirstOrDefault();

            // Return if the extent overlay is null.
            if (extentOverlay == null) { return; }

            // Get the extent graphic.
            Graphic extentGraphic = extentOverlay.Graphics.FirstOrDefault();

            // Create the extent graphic and add it to the overlay if it doesn't exist and the sample is not in preview.
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
            // Update the path to the tile cache.
            _tilePath = $"{System.IO.Path.GetTempFileName()}.tpk";

            // Get the parameters for the job.
            ExportTileCacheParameters parameters = GetExportParameters();

            // Create the task.
            ExportTileCacheTask exportTask = await ExportTileCacheTask.CreateAsync(_serviceUri);

            // Create the export job.
            ExportTileCacheJob job = exportTask.ExportTileCache(parameters, _tilePath);

            // Start the export job.
            job.Start();

            // Wait for the tile cache.
            TileCache resultTileCache = await job.GetResultAsync();

            // Do the rest of the work.
            await HandleExportCompleted(job, resultTileCache);
        }

        private ExportTileCacheParameters GetExportParameters()
        {
            // Create a new parameters instance.
            ExportTileCacheParameters parameters = new ExportTileCacheParameters();

            // Get the (only) graphics overlay in the map view.
            GraphicsOverlay extentOverlay = _myMapView.GraphicsOverlays.First();

            // Get the area selection graphic's extent.
            Graphic extentGraphic = extentOverlay.Graphics.First();

            // Set the area for the export.
            parameters.AreaOfInterest = extentGraphic.Geometry;

            // Get the highest possible export quality.
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

        private async Task HandleExportCompleted(ExportTileCacheJob job, TileCache cache)
        {
            // Update the view if the job is complete.
            if (job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Succeeded)
            {
                // Store the original map viewpoint.
                _originalViewpoint = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

                // Show the exported tiles on the preview map.
                await UpdatePreviewMap(cache);

                // Hide the progress bar.
                _myProgressBar.Visibility = ViewStates.Gone;

                // Change the export button text.
                _myExportButton.Text = "Close Preview";

                // Re-enable the button.
                _myExportButton.Enabled = true;

                // Set the preview open flag.
                _previewOpen = true;

                // Store the graphics overlay and then hide it.
                _overlay = _myMapView.GraphicsOverlays.FirstOrDefault();
                _myMapView.GraphicsOverlays.Clear();
            }
            else if (job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Failed)
            {
                // Notify the user.
                ShowStatusMessage("Job failed");

                // Hide the progress bar.
                _myProgressBar.Visibility = ViewStates.Gone;

                // Change the export button text.
                _myExportButton.Text = "Export tiles";

                // Re-enable the export button.
                _myExportButton.Enabled = true;

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

            // Show the tiles in a new map.
            _myMapView.Map = new Map(new Basemap(myLayer));

            // Set the margin.
            SetMapviewMargin(40);
        }

        private async void MyExportButton_Click(object sender, EventArgs e)
        {
            try
            {
                // If preview isn't open, start an export.
                if (!_previewOpen)
                {
                    // Disable the export button.
                    _myExportButton.Enabled = false;

                    // Show the progress bar.
                    _myProgressBar.Visibility = ViewStates.Visible;

                    // Start the export.
                    await StartExport();
                }
                else // Otherwise, close the preview.
                {
                    // Apply the old basemap.
                    _myMapView.Map = _basemap;

                    // Apply the old viewpoint.
                    _myMapView.SetViewpoint(_originalViewpoint);

                    // Reset the margin.
                    SetMapviewMargin(0);

                    // Change the button text.
                    _myExportButton.Text = "Export Tiles";

                    // Clear the preview open flag.
                    _previewOpen = false;

                    // Re-show the graphics overlay.
                    _myMapView.GraphicsOverlays.Add(_overlay);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage(ex.ToString());
            }
        }

        private void ShowStatusMessage(string message)
        {
            // Display the message to the user.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetMessage(message).SetTitle("Alert").Show();
        }

        private void SetMapviewMargin(int margin)
        {
            // Get the layout parameters for the container.
            ViewGroup.MarginLayoutParams parameters = (ViewGroup.MarginLayoutParams)_layoutContainer.LayoutParameters;

            // Set the margins.
            parameters.SetMargins(margin, margin, margin, margin);

            // Apply the layout.
            _layoutContainer.LayoutParameters = parameters;
        }
    }
}