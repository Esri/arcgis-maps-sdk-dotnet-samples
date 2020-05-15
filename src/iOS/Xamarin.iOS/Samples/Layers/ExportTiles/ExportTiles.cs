// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ExportTiles
{
    [Register("ExportTiles")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Export tiles",
        category: "Layers",
        description: "Download tiles to a local tile cache file stored on the device.",
        instructions: "Pan and zoom into the desired area, making sure the area is within the red boundary. Tap the 'Export tiles' button to start the process. On successful completion you will see a preview of the downloaded tile package.",
        tags: new[] { "cache", "download", "export", "local", "offline", "package", "tiles" })]
    public class ExportTiles : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _exportTilesButton;
        private UIActivityIndicatorView _statusIndicator;

        // URL to the service tiles will be exported from.
        private readonly Uri _serviceUri =
            new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer");

        // Path to exported tiles on disk.
        private string _tilePath;

        public ExportTiles()
        {
            Title = "Export tiles";
        }

        private async void Initialize()
        {
            try
            {
                // Create the tile layer.
                ArcGISTiledLayer myLayer = new ArcGISTiledLayer(_serviceUri);

                // Load the layer.
                await myLayer.LoadAsync();

                // Create and show the basemap with the layer.
                _myMapView.Map = new Map(new Basemap(myLayer))
                {
                    // Set the min and max scale - export task fails if the scale is too big or small.
                    MaxScale = 5000000,
                    MinScale = 10000000
                };

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

                // Update the graphic - needed in case the user decides not to interact before pressing the button.
                UpdateMapExtentGraphic();

                // Enable the export button now that sample is ready.
                _exportTilesButton.Enabled = true;

                // Set viewpoint of the map.
                _myMapView.SetViewpoint(new Viewpoint(-4.853791, 140.983598, _myMapView.Map.MinScale));
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
        ///     and updates the red box graphic to outline 70% of the current view.
        /// </summary>
        private void UpdateMapExtentGraphic()
        {
            // Get the new viewpoint.
            Viewpoint myViewPoint = _myMapView?.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

            // Get the updated extent for the new viewpoint.
            Envelope extent = myViewPoint?.TargetGeometry as Envelope;

            // Return if extent is null.
            if (extent == null)
            {
                return;
            }

            // Create an envelope that is a bit smaller than the extent.
            EnvelopeBuilder envelopeBldr = new EnvelopeBuilder(extent);
            envelopeBldr.Expand(0.70);

            // Get the (only) graphics overlay in the map view.
            GraphicsOverlay extentOverlay = _myMapView.GraphicsOverlays.FirstOrDefault();

            // Return if the extent overlay is null.
            if (extentOverlay == null)
            {
                return;
            }

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

            // Update tile cache path.
            _tilePath = $"{Path.GetTempFileName()}.tpk";

            // Create the export job.
            ExportTileCacheJob job = exportTask.ExportTileCache(parameters, _tilePath);

            // Start the export job.
            job.Start();

            // Get the result.
            TileCache resultTileCache = await job.GetResultAsync();

            // Do the rest of the work.
            HandleExportCompletion(job, resultTileCache);
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

        private void HandleExportCompletion(ExportTileCacheJob job, TileCache cache)
        {
            // Hide the progress bar.
            _statusIndicator.StopAnimating();
            switch (job.Status)
            {
                // Update the view if the job is complete.
                case Esri.ArcGISRuntime.Tasks.JobStatus.Succeeded:
                    // Dispatcher is necessary due to the threading implementation;
                    //     this method is called from a thread other than the UI thread.
                    InvokeOnMainThread(async () =>
                    {
                        // Show the exported tiles on the preview map.
                        try
                        {
                            await UpdatePreviewMap(cache);
                        }
                        catch (Exception ex)
                        {
                            ShowStatusMessage(ex.ToString());
                        }
                    });
                    break;
                case Esri.ArcGISRuntime.Tasks.JobStatus.Failed:
                    // Notify the user.
                    ShowStatusMessage("Job failed");

                    // Dispatcher is necessary due to the threading implementation;
                    //     this method is called from a thread other than the UI thread.
                    InvokeOnMainThread(() =>
                    {
                        // Re-enable the export button.
                        _exportTilesButton.Enabled = true;
                    });
                    break;
            }
        }

        private async Task UpdatePreviewMap(TileCache cache)
        {
            // Load the cache.
            await cache.LoadAsync();

            // Create a tile layer with the cache.
            ArcGISTiledLayer myLayer = new ArcGISTiledLayer(cache);

            // Show the exported tiles in new map.
            var vc = new UIViewController();
            Map previewMap = new Map();
            previewMap.OperationalLayers.Add(myLayer);
            vc.View = new MapView {Map = previewMap};
            vc.Title = "Exported tiles";
            NavigationController.PushViewController(vc, true);
        }

        private async void MyExportButton_Click(object sender, EventArgs e)
        {
            // If preview isn't open, start an export.
            try
            {
                // Disable the export button.
                _exportTilesButton.Enabled = false;

                // Show the progress bar.
                _statusIndicator.StartAnimating();

                // Start the export.
                await StartExport();
            }
            catch (Exception ex)
            {
                _statusIndicator.StopAnimating();
                ShowStatusMessage(ex.ToString());
            }
        }

        private void ShowStatusMessage(string message)
        {
            // Display the message to the user.
            UIAlertView alertView = new UIAlertView("alert", message, (IUIAlertViewDelegate) null, "OK", null);
            alertView.Show();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _exportTilesButton = new UIBarButtonItem();
            _exportTilesButton.Title = "Export tiles";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _exportTilesButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            _statusIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            _statusIndicator.TranslatesAutoresizingMaskIntoConstraints = false;
            _statusIndicator.HidesWhenStopped = true;
            _statusIndicator.BackgroundColor = UIColor.FromWhiteAlpha(0f, .8f);

            // Add the views.
            View.AddSubviews(_myMapView, toolbar, _statusIndicator);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                _statusIndicator.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                _statusIndicator.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _statusIndicator.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _statusIndicator.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.ViewpointChanged += MyMapView_ViewpointChanged;
            _exportTilesButton.Clicked += MyExportButton_Click;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.ViewpointChanged -= MyMapView_ViewpointChanged;
            _exportTilesButton.Clicked -= MyExportButton_Click;
        }
    }
}