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
using CoreGraphics;
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
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("3f1bbf0ec70b409a975f5c91f363fe7d")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Generate geodatabase",
        "Data",
        "This sample demonstrates how to take a feature service offline by generating a geodatabase.",
        "1. Pan and zoom to the area you would like to download features for, ensuring that all features are within the rectangle.\n2. Tap on the button. This will start the process of generating the offline geodatabase.\n3. Observe that the sample unregisters the geodatabase. This is best practice when changes won't be edited and synced back to the service.\n\nNote that the basemap will be automatically downloaded from an ArcGIS Online portal.")]
    public class GenerateGeodatabase : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIProgressView _progressBar = new UIProgressView();
        private readonly UIToolbar _toolbar = new UIToolbar();

        private readonly UIButton _generateButton = new UIButton
        {
            Enabled = false
        };

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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat controlHeight = 30;
                nfloat margin = 5;
                nfloat toolbarHeight = controlHeight + 2 * margin;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _toolbar.Frame = new CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);
                _generateButton.Frame = new CGRect(margin, _toolbar.Frame.Top + margin, View.Bounds.Width - 2 * margin, controlHeight);
                _progressBar.Frame = new CGRect(0, View.Bounds.Height - 42, View.Bounds.Width, 2);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private void CreateLayout()
        {
            _generateButton.SetTitle("Generate", UIControlState.Normal);
            _generateButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _generateButton.SetTitleColor(UIColor.Gray, UIControlState.Disabled);
            _generateButton.TouchUpInside += GenerateButton_Clicked;

            // Add the views.
            View.AddSubviews(_myMapView, _toolbar, _generateButton);
        }

        private async void Initialize()
        {
            try
            {
                // Create a tile cache and load it with the SanFrancisco streets tpk.
                TileCache tileCache = new TileCache(DataManager.GetDataFolder("3f1bbf0ec70b409a975f5c91f363fe7d", "SanFrancisco.tpk"));

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

                // Set up an event handler for when the viewpoint (extent) changes.
                _myMapView.ViewpointChanged += MapViewExtentChanged;

                // Create a task for generating a geodatabase (GeodatabaseSyncTask).
                _gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(_featureServiceUri);

                // Add all graphics from the service to the map.
                foreach (var layer in _gdbSyncTask.ServiceInfo.LayerInfos)
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
            var extentOverlay = _myMapView.GraphicsOverlays.FirstOrDefault();

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
            _generateGdbJob.ProgressChanged += (sender, e) => { UpdateProgressBar(); };

            // Show the progress bar.
            View.AddSubview(_progressBar);

            // Start the job.
            _generateGdbJob.Start();

            // Get the result.
            Geodatabase resultGdb = await _generateGdbJob.GetResultAsync();

            // Do the rest of the work.
            await HandleGenerationStatusChange(_generateGdbJob, resultGdb);
        }

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

            // Hide the progress bar.
            _progressBar.RemoveFromSuperview();
        }

        private void ShowStatusMessage(string message)
        {
            // Display the message to the user.
            UIAlertView alertView = new UIAlertView("alert", message, (IUIAlertViewDelegate) null, "OK", null);
            alertView.Show();
        }

        private async void GenerateButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Disable the generate button.
                _generateButton.Enabled = false;

                // Call the geodatabase generation method.
                await StartGeodatabaseGeneration();
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
    }
}