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

namespace ArcGISRuntimeXamarin.Samples.AnalyzeHotspots
{
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

        private void Initialize()
        {
            // Create a map with a topographic basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create a new geoprocessing task
            _hotspotTask = new GeoprocessingTask(new Uri(_hotspotUrl));

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }

        private async void OnRunAnalysisClicked(object sender, EventArgs e)
        {
            // Show busy activity indication
            MyActivityInidicator.IsVisible = true;
            MyActivityInidicator.IsRunning = true;

            // Get the 'from' and 'to' dates from the date pickers for the geoprocessing analysis
            DateTime myFromDate;
            DateTime myToDate;
            try
            {
                myFromDate = Convert.ToDateTime(StartDate.Text);
                myToDate = Convert.ToDateTime(EndDate.Text);
            } catch (Exception)
            {
                // Handle badly formatted dates
                await DisplayAlert("Invalid date", "Please enter a valid date", "OK");

                // Remove the busy activity indication
                MyActivityInidicator.IsRunning = false;
                MyActivityInidicator.IsVisible = false;
                return;
            }

            // The end date must be at least one day after the start date
            if (myToDate <= myFromDate.AddDays(1))
            {
                // Show error message
                await DisplayAlert("Invalid date range", "Please select valid time range. There has to be at least one day in between To and From dates.", "OK");

                // Remove the busy activity indication
                MyActivityInidicator.IsRunning = false;
                MyActivityInidicator.IsVisible = false;
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
                    await DisplayAlert("Geoprocessing error", "Executing geoprocessing failed. " + _hotspotJob.Error.Message, "OK");
                }
                else
                {
                    await DisplayAlert("Sample error", "An error occurred. " + ex.ToString(), "OK");
                }
            }
            finally
            {
                // Remove the busy activity indication
                MyActivityInidicator.IsRunning = false;
                MyActivityInidicator.IsVisible = false;
            }
        }
    }
}
