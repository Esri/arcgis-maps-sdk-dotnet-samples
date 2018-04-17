// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.IO;
using System.Linq;
using System.Drawing;
using ArcGISRuntime.Samples.Managers;
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
        // URI for a feature service that supports geodatabase generation
        private Uri _featureServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/WildfireSync/FeatureServer");

        // Path to the geodatabase file on disk
        private string _gdbPath;

        // Task to be used for generating the geodatabase
        private GeodatabaseSyncTask _gdbSyncTask;

        // Job used to generate the geodatabase
        private GenerateGeodatabaseJob _generateGdbJob;

        // MapView control
        private MapView myMapView = new MapView();

        // Progress Bar
        private UIProgressView myProgressBar = new UIProgressView();

        // Generate button
        private UIButton myGenerateButton = new UIButton() { Enabled = false };

        public GenerateGeodatabase()
        {
            Title = "Generate geodatabase";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            // Place the MapView
            myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Place the Button
            myGenerateButton.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 40, View.Bounds.Width, 30);

            // Place the progress bar
            myProgressBar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 10, View.Bounds.Width, 10);
        }

        private void CreateLayout()
        {
            // Place the MapView
            myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Place the Button
            myGenerateButton.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 40, View.Bounds.Width, 30);
            myGenerateButton.SetTitle("Generate", UIControlState.Normal);
            myGenerateButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            myGenerateButton.BackgroundColor = UIColor.LightTextColor;
            myGenerateButton.TouchUpInside += GenerateButton_Clicked;

            // Place the progress bar
            myProgressBar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 10, View.Bounds.Width, 10);

            // Add the views
            View.AddSubviews(myMapView, myProgressBar, myGenerateButton);
        }

        private async void Initialize()
        {
            // Create a tile cache and load it with the SanFrancisco streets tpk
            TileCache _tileCache = new TileCache(GetTpkPath());

            // Create the corresponding layer based on the tile cache
            ArcGISTiledLayer _tileLayer = new ArcGISTiledLayer(_tileCache);

            // Create the basemap based on the tile cache
            Basemap _sfBasemap = new Basemap(_tileLayer);

            // Create the map with the tile-based basemap
            Map myMap = new Map(_sfBasemap);

            // Assign the map to the MapView
            myMapView.Map = myMap;

            // Create a new symbol for the extent graphic
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 2);

            // Create graphics overlay for the extent graphic and apply a renderer
            GraphicsOverlay extentOverlay = new GraphicsOverlay();
            extentOverlay.Renderer = new SimpleRenderer(lineSymbol);

            // Add graphics overlay to the map view
            myMapView.GraphicsOverlays.Add(extentOverlay);

            // Set up an event handler for when the viewpoint (extent) changes
            myMapView.ViewpointChanged += MapViewExtentChanged;

            // Create a task for generating a geodatabase (GeodatabaseSyncTask)
            _gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(_featureServiceUri);

            // Add all graphics from the service to the map
            foreach (var layer in _gdbSyncTask.ServiceInfo.LayerInfos)
            {
                // Create the ServiceFeatureTable for this particular layer
                ServiceFeatureTable onlineTable = new ServiceFeatureTable(new Uri(_featureServiceUri + "/" + layer.Id));

                // Wait for the table to load
                await onlineTable.LoadAsync();

                // Add the layer to the map's operational layers if load succeeds
                if (onlineTable.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
                {
                    myMap.OperationalLayers.Add(new FeatureLayer(onlineTable));
                }
            }

            // Update the graphic - needed in case the user decides not to interact before pressing the button
            UpdateMapExtent();

            // Enable the generate button now that the sample is ready
            myGenerateButton.Enabled = true;
        }

        private void UpdateMapExtent()
        {
            // Return if mapview is null
            if (myMapView == null) { return; }

            // Get the new viewpoint
            Viewpoint myViewPoint = myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

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
            var extentOverlay = myMapView.GraphicsOverlays.FirstOrDefault();

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

        private async void StartGeodatabaseGeneration()
        {
            // Update geodatabase path
            _gdbPath = GetGdbPath();

            // Create a task for generating a geodatabase (GeodatabaseSyncTask)
            _gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(_featureServiceUri);

            // Get the current extent of the red preview box
            Envelope extent = myMapView.GraphicsOverlays.First().Graphics.First().Geometry as Envelope;

            // Get the default parameters for the generate geodatabase task
            GenerateGeodatabaseParameters generateParams = await _gdbSyncTask.CreateDefaultGenerateGeodatabaseParametersAsync(extent);

            // Create a generate geodatabase job
            _generateGdbJob = _gdbSyncTask.GenerateGeodatabase(generateParams, _gdbPath);

            // Handle the job changed event
            _generateGdbJob.JobChanged += GenerateGdbJobChanged;

            // Handle the progress changed event (to show progress bar)
            _generateGdbJob.ProgressChanged += ((sender, e) =>
            {
                UpdateProgressBar();
            });

            // Start the job
            _generateGdbJob.Start();
        }

        private async void HandleGenerationStatusChange(GenerateGeodatabaseJob job)
        {
            JobStatus status = job.Status;

            // If the job completed successfully, add the geodatabase data to the map
            if (status == JobStatus.Succeeded)
            {
                // Clear out the existing layers
                myMapView.Map.OperationalLayers.Clear();

                // Get the new geodatabase
                Geodatabase resultGdb = await job.GetResultAsync();

                // Loop through all feature tables in the geodatabase and add a new layer to the map
                foreach (GeodatabaseFeatureTable table in resultGdb.GeodatabaseFeatureTables)
                {
                    // Create a new feature layer for the table
                    FeatureLayer _layer = new FeatureLayer(table);

                    // Add the new layer to the map
                    myMapView.Map.OperationalLayers.Add(_layer);
                }
                // Best practice is to unregister the geodatabase
                await _gdbSyncTask.UnregisterGeodatabaseAsync(resultGdb);

                // Tell the user that the geodatabase was unregistered
                ShowStatusMessage("Since no edits will be made, the local geodatabase has been unregistered per best practice.");

                // Re-enable the generate button
                myGenerateButton.Enabled = true;
            }

            // See if the job failed
            if (status == JobStatus.Failed)
            {
                // Create a message to show the user
                string message = "Generate geodatabase job failed";

                // Show an error message (if there is one)
                if (job.Error != null)
                {
                    message += ": " + job.Error.Message;
                }
                else
                {
                    // If no error, show messages from the job
                    var m = from msg in job.Messages select msg.Message;
                    message += ": " + string.Join<string>("\n", m);
                }

                ShowStatusMessage(message);

                // Re-enable the generate button
                myGenerateButton.Enabled = true;
            }
        }

        // Get the path to the tile package used for the basemap
        private string GetTpkPath()
        {
            return DataManager.GetDataFolder("3f1bbf0ec70b409a975f5c91f363fe7d", "SanFrancisco.tpk");
        }

        private string GetGdbPath()
        {
            // Return a path
            return $"{Path.GetTempFileName()}.geodatabase";
        }

        private void ShowStatusMessage(string message)
        {
            // Display the message to the user
            UIAlertView alertView = new UIAlertView("alert", message, null, "OK", null);
            alertView.Show();
        }

        // Handler for the generate button clicked event
        private void GenerateButton_Clicked(object sender, EventArgs e)
        {
            // Disable the generate button
            myGenerateButton.Enabled = false;

            // Call the cross-platform geodatabase generation method
            StartGeodatabaseGeneration();
        }

        // Handler for the MapView Extent Changed event
        private void MapViewExtentChanged(object sender, EventArgs e)
        {
            // Call the cross-platform map extent update method
            UpdateMapExtent();
        }

        // Handler for the job changed event
        private void GenerateGdbJobChanged(object sender, EventArgs e)
        {
            // Get the job object; will be passed to HandleGenerationStatusChange
            GenerateGeodatabaseJob job = sender as GenerateGeodatabaseJob;

            // Due to the nature of the threading implementation,
            //     the dispatcher needs to be used to interact with the UI
            InvokeOnMainThread(() =>
            {
                // Do the remainder of the job status changed work
                HandleGenerationStatusChange(job);
            });
        }

        private void UpdateProgressBar()
        {
            // Due to the nature of the threading implementation,
            //     the dispatcher needs to be used to interact with the UI
            InvokeOnMainThread(() =>
            {
                // Update the progress bar value
                myProgressBar.Progress = (float)(_generateGdbJob.Progress / 100.0);
            });
        }
    }
}