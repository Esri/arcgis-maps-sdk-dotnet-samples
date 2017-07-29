// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Xamarin.Forms;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using Xamarin.Forms;

#if WINDOWS_UWP
using Colors = Windows.UI.Colors;
#else
using Colors = System.Drawing.Color;
#endif

namespace ArcGISRuntimeXamarin.Samples.GenerateGeodatabase
{
    public partial class GenerateGeodatabase : ContentPage
    {
        // URI for a feature service that supports geodatabase generation
        private Uri _featureServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/WildfireSync/FeatureServer");

        // Path to the geodatabase file on disk
        private string _gdbPath;

        // Task to be used for generating the geodatabase
        private GeodatabaseSyncTask _gdbSyncTask;

        // Job used to generate the geodatabase
        GenerateGeodatabaseJob _generateGdbJob;

        public GenerateGeodatabase()
        {
            InitializeComponent ();

            Title = "Generate geodatabase";

            // Create the UI, setup the control references and execute initialization 
            Initialize();
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
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Red, 2);

            // Create graphics overlay for the extent graphic and apply a renderer
            GraphicsOverlay extentOverlay = new GraphicsOverlay();
            extentOverlay.Renderer = new SimpleRenderer(lineSymbol);

            // Add graphics overlay to the map view
            myMapView.GraphicsOverlays.Add(extentOverlay);

            // Set up an event handler for when the viewpoint (extent) changes
            myMapView.ViewpointChanged += MapViewExtentChanged;

            // Update the local data path for the geodatabase file
            SetGdbPath();

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
        }

        private void UpdateMapExtent()
        {
            // Get the updated extent for the new viewpoint
            Envelope extent = myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;

            // Return if extent is null 
            if (extent == null) { return; }

            // Create an envelope that is a bit smaller than the extent
            EnvelopeBuilder envelopeBldr = new EnvelopeBuilder(extent);
            envelopeBldr.Expand(0.80);

            // Get the (only) graphics overlay in the map view (make sure it exists)
            var extentOverlay = myMapView.GraphicsOverlays.FirstOrDefault();
            if (extentOverlay == null)
            {
                return;
            }

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
            // Create a task for generating a geodatabase (GeodatabaseSyncTask)
            _gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(_featureServiceUri);

            // Get the current extent of the map view
            Envelope extent = myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;

            // Get the default parameters for the generate geodatabase task
            GenerateGeodatabaseParameters generateParams = await _gdbSyncTask.CreateDefaultGenerateGeodatabaseParametersAsync(extent);

            // Create a generate geodatabase job
            _generateGdbJob = _gdbSyncTask.GenerateGeodatabase(generateParams, _gdbPath);

            // Handle the job changed event
            _generateGdbJob.JobChanged += GenerateGdbJobChanged;

            // Handle the progress changed event (to show progress bar)
            _generateGdbJob.ProgressChanged += ((object sender, EventArgs e) =>
            {
                UpdateProgressBar();
            });

            // Start the job
            _generateGdbJob.Start();
        }

        private async void HandleGenerationStatusChange(GenerateGeodatabaseJob job, MapView mmv)
        {
            JobStatus status = job.Status;

            // If the job completed successfully, add the geodatabase data to the map
            if (status == JobStatus.Succeeded)
            {
                // Clear out the existing layers
                mmv.Map.OperationalLayers.Clear();

                // Get the new geodatabase
                Geodatabase resultGdb = await job.GetResultAsync();

                // Loop through all feature tables in the geodatabase and add a new layer to the map
                foreach (GeodatabaseFeatureTable table in resultGdb.GeodatabaseFeatureTables)
                {
                    // Create a new feature layer for the table
                    FeatureLayer _layer = new FeatureLayer(table);

                    // Add the new layer to the map
                    mmv.Map.OperationalLayers.Add(_layer);
                }
                // Best practice is to unregister the geodatabase
                await _gdbSyncTask.UnregisterGeodatabaseAsync(resultGdb);

                // Tell the user that the geodatabase was unregistered
                ShowStatusMessage("Geodatabase was unregistered per best practice");
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
            }
        }
        // Platform-specific implementations & handlers
        #region platform-specific

        // Get the path to the tile package used for the basemap
        private string GetTpkPath()
        {
            // Because each platform handles resource embedding differently,
            //     we take a three-part approach:
            //     1. Include the tile package as an 'embedded resource'
            //     2. Copy the embedded resource (opened with a stream) to the platform-specific home directory
            //     3. Get the platform-specific file path

            string tpkName = "SanFrancisco.tpk";
#if NETFX_CORE // UWP
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var folder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            var resourcePrefix = "ArcGISRuntimeXamarin.";
#elif __IOS__
            var resourcePrefix = "ArcGISRuntimeXamarin.";
            var assembly = this.GetType().Assembly;
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif __ANDROID__
            var resourcePrefix = "ArcGISRuntimeXamarin.";
            var assembly = this.GetType().Assembly;
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif

            // The path on disk for the file
            var path = Path.Combine(folder, tpkName);

            // Copy the file to disk if it isn't already there
            if (!File.Exists(path))
            {
                var resourceName = resourcePrefix + "Resources.TileCaches.SanFrancisco.tpk";
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    using (var mem = new MemoryStream())
                    {
                        stream.CopyTo(mem);
                        File.WriteAllBytes(path, mem.ToArray());
                    }
                }
            }

            // Return the final path
            return path;

        }

        private void SetGdbPath()
        {
            // Set the platform-specific path for storing the geodatabase
            String folder = "";

#if NETFX_CORE //UWP
            folder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#elif __IOS__
            folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif __ANDROID__
            folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
            // Set the final path
            _gdbPath = Path.Combine(folder + "wildfire.geodatabase");
        }

        private void ShowStatusMessage(string message)
        {
            // Display the message to the user
            DisplayAlert("Alert", message, "OK");
        }

        // Handler for the generate button clicked event
        private async void GenerateButton_Clicked(object sender, EventArgs e)
        {
            // Call the cross-platform geodatabase generation method
            StartGeodatabaseGeneration();
        }

        // Handler for the MapView Extent Changed event
        private async void MapViewExtentChanged(object sender, EventArgs e)
        {
            // Call the cross-platform map extent update method
            UpdateMapExtent();
        }

        // Handler for the job changed event
        private async void GenerateGdbJobChanged(object sender, EventArgs e)
        {
            // Get the job object; will be passed to HandleGenerationStatusChange
            GenerateGeodatabaseJob job = sender as GenerateGeodatabaseJob;

            // Due to the nature of the threading implementation,
            //     the dispatcher needs to be used to interact with the UI
            Device.BeginInvokeOnMainThread(() =>
            {
                // Hide the progress bar if the job is finished
                if (job.Status == JobStatus.Succeeded || job.Status == JobStatus.Failed)
                {
                    myProgressBar.IsVisible = false;
                }
                else // Show it otherwise
                {
                    myProgressBar.IsVisible = true;
                }

                // Do the remainder of the job status changed work
                HandleGenerationStatusChange(job, myMapView);
            });
        }

        private async void UpdateProgressBar()
        {
            // Due to the nature of the threading implementation,
            //     the dispatcher needs to be used to interact with the UI
            Device.BeginInvokeOnMainThread(() =>
            {
                // Update the progress bar value
                myProgressBar.Progress = _generateGdbJob.Progress / 100.0;
            });
        }
#endregion

    }
}
