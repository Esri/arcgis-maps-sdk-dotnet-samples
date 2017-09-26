﻿// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.UWP.Samples.AnalyzeHotspots
{
    public partial class AnalyzeHotspots
    {
        // Url for the geoprocessing service
        private const string _hotspotUrl =
            "http://sampleserver6.arcgisonline.com/arcgis/rest/services/911CallsHotspot/GPServer/911%20Calls%20Hotspot";

        // The geoprocessing task for hot spot analysis 
        private GeoprocessingTask _hotspotTask;

        // The job that handles the communication between the application and the geoprocessing task
        private GeoprocessingJob _hotspotJob;

        public AnalyzeHotspots()
        {
            this.InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private async void Initialize()
        {
            // Create a map with a topographic basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create a new geoprocessing task
            _hotspotTask = await GeoprocessingTask.CreateAsync(new Uri(_hotspotUrl));

            // Assign the map to the MapView
            MyMapView.Map = myMap;

            // Set the initial start date for the DatePicker defined in xaml
            var myFromDate = new DateTimeOffset(new DateTime(1998, 1, 1));
            FromDate.Date = myFromDate;

            // Set the initial end date for the DatePicker defined in xaml
            var myToDate = new DateTimeOffset(new DateTime(1998, 1, 31));
            ToDate.Date = myToDate;
        }

        private void OnCancelTaskClicked(object sender, RoutedEventArgs e)
        {
            // Cancel current geoprocessing job
            if (_hotspotJob.Status == JobStatus.Started)
                _hotspotJob.Cancel();

            // Hide the busyOverlay indication
            ShowBusyOverlay(false);
        }

        private async void OnAnalyzeHotspotsClicked(object sender, RoutedEventArgs e)
        {
            // Show the busyOverlay indication
            ShowBusyOverlay();

            // Get the 'from' and 'to' dates from the date pickers for the geoprocessing analysis
            var myFromDate = FromDate.Date;
            var myToDate = ToDate.Date;

            // The end date must be at least one day after the start date
            if (myToDate <= myFromDate.AddDays(1))
            {
                // Show error message
                var message = new MessageDialog("Please select valid time range. There has to be at least one day in between To and From dates.", 
                    "Invalid date range");
                await message.ShowAsync();

                // Remove the busyOverlay
                ShowBusyOverlay(false);
                return;
            }

            // Create the parameters that are passed to the used geoprocessing task
            GeoprocessingParameters myHotspotParameters = new GeoprocessingParameters(GeoprocessingExecutionType.AsynchronousSubmit);

            // Construct the date query
            var myQueryString = string.Format("(\"DATE\" > date '{0} 00:00:00' AND \"DATE\" < date '{1} 00:00:00')",
                myFromDate.ToString("yyyy-MM-dd"),
                myToDate.ToString("yyyy-MM-dd"));

            // Add the query that contains the date range used in the analysis
            myHotspotParameters.Inputs.Add("Query", new GeoprocessingString(myQueryString));

            // Create job that handles the communication between the application and the geoprocessing task
            _hotspotJob = _hotspotTask.CreateJob(myHotspotParameters);
            try
            {
                // Execute the geoprocessing analysis and wait for the results
                GeoprocessingResult myAnalysisResult = await _hotspotJob.GetResultAsync();

                // Add results to a map using map server from a geoprocessing task
                // Load to get access to full extent
                await myAnalysisResult.MapImageLayer.LoadAsync();

                // Add the analysis layer to the map view
                MyMapView.Map.OperationalLayers.Add(myAnalysisResult.MapImageLayer);

                // Zoom to the results
                await MyMapView.SetViewpointAsync(new Viewpoint(myAnalysisResult.MapImageLayer.FullExtent));
            }
            catch (TaskCanceledException)
            {
                // This is thrown if the task is canceled. Ignore.
            }
            catch (Exception ex)
            {
                // Display error messages if the geoprocessing task fails
                if (_hotspotJob.Status == JobStatus.Failed && _hotspotJob.Error != null)
                {
                    var message = new MessageDialog("Executing geoprocessing failed. " + _hotspotJob.Error.Message, "Geoprocessing error");
                    await message.ShowAsync();
                }
                else
                {
                    var message = new MessageDialog("An error occurred. " + ex.ToString(), "Sample error");
                    await message.ShowAsync();
                }
            }
            finally
            {
                // Remove the busyOverlay
                ShowBusyOverlay(false);
            }
        }

        private void ShowBusyOverlay(bool visibility = true)
        {
            // Function to toggle the visibility of interaction with the GUI for the user to 
            // specify dates for the hot spot analysis. When the analysis is running, the GUI
            // for changing the dates is 'grayed-out' and the progress bar with a cancel 
            // button (aka. busyOverlay object) becomes active.

            if (visibility)
            {
                // The geoprocessing task is processing. The busyOverly is present.
                busyOverlay.Visibility = Visibility.Visible;
                progress.IsIndeterminate = true;
            }
            else
            {
                // The user can interact with the date pickers. The busyOverlay is invisible.
                busyOverlay.Visibility = Visibility.Collapsed;
                progress.IsIndeterminate = false;
            }
        }
    }
}
