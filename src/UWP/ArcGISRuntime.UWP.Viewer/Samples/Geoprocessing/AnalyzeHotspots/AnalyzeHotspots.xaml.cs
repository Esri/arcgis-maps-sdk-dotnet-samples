// Copyright 2016 Esri.
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
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Analyze hotspots",
        "Geoprocessing",
        "This sample demonstrates how to execute the GeoprocessingTask asynchronously to calculate a hotspot analysis based on the frequency of 911 calls. It calculates the frequency of these calls within a given study area during a specified constrained time period set between 1/1/1998 and 5/31/1998.",
        "To run the hotspot analysis, select a data range and click on the 'Run analysis' button. Note the larger the date range, the longer it may take for the task to run and send back the results.")]
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
            InitializeComponent();

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
            DateTimeOffset myFromDate = new DateTimeOffset(new DateTime(1998, 1, 1));
            FromDate.Date = myFromDate;

            // Set the initial end date for the DatePicker defined in xaml
            DateTimeOffset myToDate = new DateTimeOffset(new DateTime(1998, 1, 31));
            ToDate.Date = myToDate;
        }

        private void OnCancelTaskClicked(object sender, RoutedEventArgs e)
        {
            // Cancel current geoprocessing job
            if (_hotspotJob.Status == JobStatus.Started)
                _hotspotJob.Cancel();

            // Hide the BusyOverlay indication
            ShowBusyOverlay(false);
        }

        private async void OnAnalyzeHotspotsClicked(object sender, RoutedEventArgs e)
        {
            // Clear any existing results
            MyMapView.Map.OperationalLayers.Clear();

            // Show the BusyOverlay indication
            ShowBusyOverlay();

            // Get the 'from' and 'to' dates from the date pickers for the geoprocessing analysis
            DateTimeOffset myFromDate = FromDate.Date;
            DateTimeOffset myToDate = ToDate.Date;

            // The end date must be at least one day after the start date
            if (myToDate <= myFromDate.AddDays(1))
            {
                // Show error message
                MessageDialog message = new MessageDialog("Please select valid time range. There has to be at least one day in between To and From dates.", 
                    "Invalid date range");
                await message.ShowAsync();

                // Remove the BusyOverlay
                ShowBusyOverlay(false);
                return;
            }

            // Create the parameters that are passed to the used geoprocessing task
            GeoprocessingParameters myHotspotParameters = new GeoprocessingParameters(GeoprocessingExecutionType.AsynchronousSubmit);

            // Construct the date query
            string myQueryString = $"(\"DATE\" > date '{myFromDate:yyyy-MM-dd} 00:00:00' AND \"DATE\" < date '{myToDate:yyyy-MM-dd} 00:00:00')";

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
                    MessageDialog message = new MessageDialog("Executing geoprocessing failed. " + _hotspotJob.Error.Message, "Geoprocessing error");
                    await message.ShowAsync();
                }
                else
                {
                    MessageDialog message = new MessageDialog("An error occurred. " + ex.ToString(), "Sample error");
                    await message.ShowAsync();
                }
            }
            finally
            {
                // Remove the BusyOverlay
                ShowBusyOverlay(false);
            }
        }

        private void ShowBusyOverlay(bool visibility = true)
        {
            // Function to toggle the visibility of interaction with the GUI for the user to 
            // specify dates for the hot spot analysis. When the analysis is running, the GUI
            // for changing the dates is 'grayed-out' and the progress bar with a cancel 
            // button (aka. BusyOverlay object) becomes active.

            if (visibility)
            {
                // The geoprocessing task is processing. The busyOverly is present.
                BusyOverlay.Visibility = Visibility.Visible;
                Progress.IsIndeterminate = true;
            }
            else
            {
                // The user can interact with the date pickers. The BusyOverlay is invisible.
                BusyOverlay.Visibility = Visibility.Collapsed;
                Progress.IsIndeterminate = false;
            }
        }
    }
}
