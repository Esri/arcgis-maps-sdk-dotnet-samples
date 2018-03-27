// Copyright 2016 Esri.
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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.AnalyzeHotspots
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Analyze hotspots",
        "Geoprocessing",
        "This sample demonstrates how to execute the GeoprocessingTask asynchronously to calculate a hotspot analysis based on the frequency of 911 calls. It calculates the frequency of these calls within a given study area during a specified constrained time period set between 1/1/1998 and 5/31/1998.",
        "To run the hotspot analysis, select a data range and click on the 'Run analysis' button. Note the larger the date range, the longer it may take for the task to run and send back the results.")]
    public class AnalyzeHotspots : Activity
    {

        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Progress bar to show when the geoprocessing task is working
        ProgressBar _myProgressBar;

        // Edit text to define the start date for the date range
        EditText _myEditText_StartDate;

        // Edit text to define the end date for the date range
        EditText _myEditText_EndDate;

        // Url for the geoprocessing service
        private const string _hotspotUrl =
            "https://sampleserver6.arcgisonline.com/arcgis/rest/services/911CallsHotspot/GPServer/911%20Calls%20Hotspot";

        // The geoprocessing task for hot spot analysis 
        private GeoprocessingTask _hotspotTask;

        // The job that handles the communication between the application and the geoprocessing task
        private GeoprocessingJob _hotspotJob;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Analyze Hotspots";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Create a map with a topographic basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create a new geoprocessing task
            _hotspotTask = await GeoprocessingTask.CreateAsync(new Uri(_hotspotUrl));

            // Assign the map to the MapView
            _myMapView.Map = myMap;
        }

        private async void OnRunAnalysisClicked(object sender, EventArgs e)
        {
            // Clear any existing results
            _myMapView.Map.OperationalLayers.Clear();

            // Show busy activity indication
            _myProgressBar.Visibility = ViewStates.Visible;

            // Get the 'from' and 'to' dates from the date edit text's for the geoprocessing analysis
            DateTime myFromDate = Convert.ToDateTime(_myEditText_StartDate.Text);
            DateTime myToDate = Convert.ToDateTime(_myEditText_EndDate.Text);

            // The end date must be at least one day after the start date
            if (myToDate <= myFromDate.AddDays(1))
            {
                // Show error message
                var alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Invalid date range");
                alertBuilder.SetMessage("Please select valid time range.There has to be at least one day in between To and From dates.");
                alertBuilder.Show();

                // Remove the busy activity indication
                _myProgressBar.Visibility = ViewStates.Invisible;
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
                _myMapView.Map.OperationalLayers.Add(myAnalysisResult.MapImageLayer);

                // Zoom to the results
                await _myMapView.SetViewpointAsync(new Viewpoint(myAnalysisResult.MapImageLayer.FullExtent));
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
                    var alertBuilder = new AlertDialog.Builder(this);
                    alertBuilder.SetTitle("Geoprocessing error");
                    alertBuilder.SetMessage("Executing geoprocessing failed. " + _hotspotJob.Error.Message);
                    alertBuilder.Show();
                }
                else
                {
                    var alertBuilder = new AlertDialog.Builder(this);
                    alertBuilder.SetTitle("Sample error");
                    alertBuilder.SetMessage("An error occurred. " + ex.ToString());
                    alertBuilder.Show();
                }
            }
            finally
            {
                // Remove the busy activity indication
                _myProgressBar.Visibility = ViewStates.Invisible;
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a horizontal sub layout for the start date
            var subLayout1 = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Label for the start date
            var textview_Label1 = new TextView(this);
            textview_Label1.Text = "Start Date:";
            subLayout1.AddView(textview_Label1);

            // Edit text for the start date (user can change if desired)
            _myEditText_StartDate = new EditText(this);
            _myEditText_StartDate.Text = "1/01/98";
            subLayout1.AddView(_myEditText_StartDate);

            // Add the start date information to the general layout
            layout.AddView(subLayout1);

            // Create a horizontal sub layout for the end date
            var subLayout2 = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Label for the end date
            var textview_Label2 = new TextView(this);
            textview_Label2.Text = "End Date:";
            subLayout2.AddView(textview_Label2);

            // Edit text for the end date (user can change if desired)
            _myEditText_EndDate = new EditText(this);
            _myEditText_EndDate.Text = "1/31/98";
            subLayout2.AddView(_myEditText_EndDate);

            // Add the start date information to the general layout
            layout.AddView(subLayout2);

            // Add a button to the run the hot spot analysis; wire up the click event as well 
            var mapsButton = new Button(this);
            mapsButton.Text = "Run Analysis";
            mapsButton.Click += OnRunAnalysisClicked;
            layout.AddView(mapsButton);
           
            // Add the progress bar to indicate the geoprocessing task is running; make invisible by default
            _myProgressBar = new ProgressBar(this);
            _myProgressBar.Indeterminate = true;
            _myProgressBar.Visibility = ViewStates.Invisible;
            layout.AddView(_myProgressBar);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

    }
}