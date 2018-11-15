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
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.AnalyzeHotspots
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Analyze hotspots",
        "Geoprocessing",
        "This sample demonstrates how to execute the GeoprocessingTask asynchronously to calculate a hotspot analysis based on the frequency of 911 calls. It calculates the frequency of these calls within a given study area during a specified constrained time period set between 1/1/1998 and 5/31/1998.",
        "To run the hotspot analysis, select a data range and click on the 'Run analysis' button. Note the larger the date range, the longer it may take for the task to run and send back the results.")]
    public partial class AnalyzeHotspots : ContentPage
    {

        // Url for the geoprocessing service
        private const string _hotspotUrl =
            "https://sampleserver6.arcgisonline.com/arcgis/rest/services/911CallsHotspot/GPServer/911%20Calls%20Hotspot";

        // The geoprocessing task for hot spot analysis 
        private GeoprocessingTask _hotspotTask;

        // The job that handles the communication between the application and the geoprocessing task
        private GeoprocessingJob _hotspotJob;

        public AnalyzeHotspots()
        {
            InitializeComponent();

            Title = "Analyze Hotspots";

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private async void Initialize()
        {
            // Create a map with a topographic basemap
            MyMapView.Map = new Map(Basemap.CreateTopographic());

            try
            {
                // Create a new geoprocessing task
                _hotspotTask = await GeoprocessingTask.CreateAsync(new Uri(_hotspotUrl));
            }
            catch (Exception e)
            {
                await ((Page)Parent).DisplayAlert("Error", e.ToString(), "OK");
            }
        }

        private async void OnRunAnalysisClicked(object sender, EventArgs e)
        {
            // Clear any existing results
            MyMapView.Map.OperationalLayers.Clear();

            // Show busy activity indication
            MyActivityIndicator.IsVisible = true;
            MyActivityIndicator.IsRunning = true;

            // The end date must be at least one day after the start date
            if (EndDate.Date <= StartDate.Date.AddDays(1))
            {
                // Show error message
                await ((Page)Parent).DisplayAlert("Invalid date range", "Please select valid time range. There has to be at least one day in between To and From dates.", "OK");

                // Remove the busy activity indication
                MyActivityIndicator.IsRunning = false;
                MyActivityIndicator.IsVisible = false;
                return;
            }

            // Create the parameters that are passed to the used geoprocessing task
            GeoprocessingParameters myHotspotParameters = new GeoprocessingParameters(GeoprocessingExecutionType.AsynchronousSubmit);

            // Construct the date query
            string myQueryString = $"(\"DATE\" > date '{StartDate.Date:yyyy-MM-dd} 00:00:00' AND \"DATE\" < date '{EndDate.Date:yyyy-MM-dd} 00:00:00')";

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
                    await ((Page)Parent).DisplayAlert("Geoprocessing error", "Executing geoprocessing failed. " + _hotspotJob.Error.Message, "OK");
                }
                else
                {
                    await ((Page)Parent).DisplayAlert("Sample error", "An error occurred. " + ex.ToString(), "OK");
                }
            }
            finally
            {
                // Remove the busy activity indication
                MyActivityIndicator.IsRunning = false;
                MyActivityIndicator.IsVisible = false;
            }
        }
    }
}
