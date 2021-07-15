// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.GenerateGeodatabase
{
    [Register("GenerateGeodatabase")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("e4a398afe9a945f3b0f4dca1e4faccb5")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Generate geodatabase",
        category: "Data",
        description: "Generate a local geodatabase from an online feature service.",
        instructions: "Zoom to any extent. Then tap the generate button to generate a geodatabase of features from a feature service filtered to the current extent. A red outline will show the extent used. The job's progress is shown while the geodatabase is generated.",
        tags: new[] { "disconnected", "local geodatabase", "offline", "sync" })]
    public class GenerateGeodatabase : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIProgressView _progressBar;
        private UIBarButtonItem _generateButton;

        // URL for a feature service that supports geodatabase generation.
        private readonly Uri _featureServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/WildfireSync/FeatureServer");

        // Path to the geodatabase file on disk.
        private string _gdbPath;

        // Task to be used for generating the geodatabase.
        private GeodatabaseSyncTask _gdbSyncTask;

        // Job used to generate the geodatabase.
        private GenerateGeodatabaseJob _generateGdbJob;

        public GenerateGeodatabase()
        {
            Title = "Generate geodatabase";
        }

        private async void Initialize()
        {
            try
            {
                // Create a tile cache and load it with the SanFrancisco streets tpk.
                TileCache tileCache = new TileCache(DataManager.GetDataFolder("e4a398afe9a945f3b0f4dca1e4faccb5", "SanFrancisco.tpkx"));

                // Create the corresponding layer based on the tile cache.
                ArcGISTiledLayer tileLayer = new ArcGISTiledLayer(tileCache);

                // Create the basemap based on the tile cache.
                Basemap sfBasemap = new Basemap(tileLayer);

                // Create the map with the tile-based basemap.
                Map myMap = new Map(sfBasemap);

                // Assign the map to the MapView.
                _myMapView.Map = myMap;

                // Create a new symbol for the extent graphic.
                SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 2);

                // Create graphics overlay for the extent graphic and apply a renderer.
                GraphicsOverlay extentOverlay = new GraphicsOverlay
                {
                    Renderer = new SimpleRenderer(lineSymbol)
                };

                // Add graphics overlay to the map view.
                _myMapView.GraphicsOverlays.Add(extentOverlay);

                // Create a task for generating a geodatabase (GeodatabaseSyncTask).
                _gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(_featureServiceUri);

                // Add all graphics from the service to the map.
                foreach (IdInfo layer in _gdbSyncTask.ServiceInfo.LayerInfos)
                {
                    // Create the ServiceFeatureTable for this particular layer.
                    ServiceFeatureTable onlineTable = new ServiceFeatureTable(new Uri(_featureServiceUri + "/" + layer.Id));

                    // Wait for the table to load.
                    await onlineTable.LoadAsync();

                    // Add the layer to the map's operational layers if load succeeds.
                    if (onlineTable.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
                    {
                        myMap.OperationalLayers.Add(new FeatureLayer(onlineTable));
                    }
                }

                // Update the graphic - needed in case the user decides not to interact before pressing the button.
                UpdateMapExtent();

                // Enable the generate button now that the sample is ready.
                _generateButton.Enabled = true;
            }
            catch (Exception ex)
            {
                ShowStatusMessage(ex.ToString());
            }
        }

        private void UpdateMapExtent()
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

        private async Task StartGeodatabaseGeneration()
        {
            // Update geodatabase path.
            _gdbPath = $"{Path.GetTempFileName()}.geodatabase";

            // Create a task for generating a geodatabase (GeodatabaseSyncTask).
            _gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(_featureServiceUri);

            // Get the current extent of the red preview box.
            Envelope extent = _myMapView.GraphicsOverlays.First().Graphics.First().Geometry as Envelope;

            // Get the default parameters for the generate geodatabase task.
            GenerateGeodatabaseParameters generateParams = await _gdbSyncTask.CreateDefaultGenerateGeodatabaseParametersAsync(extent);

            // Create a generate geodatabase job.
            _generateGdbJob = _gdbSyncTask.GenerateGeodatabase(generateParams, _gdbPath);

            // Handle the progress changed event (to show progress bar).
            _generateGdbJob.ProgressChanged += GenerateGdbJob_ProgressChanged;

            // Start the job.
            _generateGdbJob.Start();

            // Get the result.
            Geodatabase resultGdb = await _generateGdbJob.GetResultAsync();

            // Do the rest of the work.
            await HandleGenerationStatusChange(_generateGdbJob, resultGdb);
        }

        private void GenerateGdbJob_ProgressChanged(object sender, EventArgs e) => UpdateProgressBar();

        private async Task HandleGenerationStatusChange(GenerateGeodatabaseJob job, Geodatabase resultGdb)
        {
            switch (job.Status)
            {
                // If the job completed successfully, add the geodatabase data to the map.
                case JobStatus.Succeeded:
                    // Clear out the existing layers.
                    _myMapView.Map.OperationalLayers.Clear();

                    // Loop through all feature tables in the geodatabase and add a new layer to the map.
                    foreach (GeodatabaseFeatureTable table in resultGdb.GeodatabaseFeatureTables)
                    {
                        // Create a new feature layer for the table.
                        FeatureLayer layer = new FeatureLayer(table);

                        // Add the new layer to the map.
                        _myMapView.Map.OperationalLayers.Add(layer);
                    }

                    // Best practice is to unregister the geodatabase.
                    await _gdbSyncTask.UnregisterGeodatabaseAsync(resultGdb);

                    // Tell the user that the geodatabase was unregistered.
                    ShowStatusMessage("Since no edits will be made, the local geodatabase has been unregistered per best practice.");

                    // Re-enable the generate button.
                    _generateButton.Enabled = true;
                    break;
                // See if the job failed.
                case JobStatus.Failed:
                    // Create a message to show the user.
                    string message = "Generate geodatabase job failed";

                    // Show an error message (if there is one).
                    if (job.Error != null)
                    {
                        message += ": " + job.Error.Message;
                    }
                    else
                    {
                        // If no error, show messages from the job.
                        IEnumerable<string> m = from msg in job.Messages select msg.Message;
                        message += ": " + string.Join("\n", m);
                    }

                    ShowStatusMessage(message);

                    // Re-enable the generate button.
                    _generateButton.Enabled = true;
                    break;
            }
        }

        private void ShowStatusMessage(string message)
        {
            // Display the message to the user.
            UIAlertView alertView = new UIAlertView("Alert", message, (IUIAlertViewDelegate) null, "OK", null);
            alertView.Show();
        }

        private async void GenerateButton_Clicked(object sender, EventArgs e)
        {
            // Fix the extent of the graphic.
            _myMapView.ViewpointChanged -= MapViewExtentChanged;

            try
            {
                // Disable the generate button.
                _generateButton.Enabled = false;

                // Call the geodatabase generation method.
                await StartGeodatabaseGeneration();
            }
            catch (TaskCanceledException)
            {
                ShowStatusMessage("Geodatabase generation cancelled.");
            }
            catch (Exception ex)
            {
                ShowStatusMessage(ex.ToString());
            }
        }

        private void MapViewExtentChanged(object sender, EventArgs e)
        {
            // Call the map extent update method.
            UpdateMapExtent();
        }

        private void UpdateProgressBar()
        {
            // Needed because this could be called from a non-UI thread.
            InvokeOnMainThread(() =>
            {
                // Update the progress bar value.
                _progressBar.Progress = _generateGdbJob.Progress / 100.0f;
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = ApplicationTheme.BackgroundColor};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _generateButton = new UIBarButtonItem();
            _generateButton.Title = "Generate geodatabase";
            _generateButton.Enabled = false;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _generateButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            _progressBar = new UIProgressView();
            _progressBar.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView, toolbar, _progressBar);

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

                _progressBar.TopAnchor.ConstraintEqualTo(_myMapView.TopAnchor),
                _progressBar.HeightAnchor.ConstraintEqualTo(8),
                _progressBar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _progressBar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.ViewpointChanged += MapViewExtentChanged;
            _generateButton.Clicked += GenerateButton_Clicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            if (_generateGdbJob != null)
            {
                _generateGdbJob.Cancel();
                _generateGdbJob.ProgressChanged -= GenerateGdbJob_ProgressChanged;
            }

            _myMapView.ViewpointChanged -= MapViewExtentChanged;
            _generateButton.Clicked -= GenerateButton_Clicked;
        }
    }
}