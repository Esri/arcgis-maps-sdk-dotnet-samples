// Copyright 2018 Esri.
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
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.AnalyzeHotspots
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Analyze hotspots",
        "Geoprocessing",
        "Use a geoprocessing service and a set of features to identify statistically significant hot spots and cold spots.",
        "Select a date range (between 1998-01-01 and 1998-05-31) from the dialog and tap on Analyze. The results will be shown on the map upon successful completion of the `GeoprocessingJob`.",
        "Geoprocessing", "GeoprocessingJob", "GeoprocessingParameters", "GeoprocessingResult")]
    public class AnalyzeHotspots : Activity
    {
        // Hold a reference to the map view.
        private MapView _myMapView;

        // Button to define the start date for the date range.
        private Button _startDateButton;

        // Button to define the end date for the date range.
        private Button _endDateButton;

        // Alert dialog to show when the geoprocessing task is working.
        private AlertDialog _alert;

        // Url for the geoprocessing service.
        private const string _hotspotUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/911CallsHotspot/GPServer/911%20Calls%20Hotspot";

        // The geoprocessing task for hot spot analysis.
        private GeoprocessingTask _hotspotTask;

        // The job that handles the communication between the application and the geoprocessing task.
        private GeoprocessingJob _hotspotJob;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Analyze Hotspots";

            // Create the UI, setup the control references and execute initialization .
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Create a map with a topographic basemap.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

            try
            {
                // Create a new geoprocessing task.
                _hotspotTask = await GeoprocessingTask.CreateAsync(new Uri(_hotspotUrl));

                // Zoom into Portland, Oregon.
                await _myMapView.SetViewpointCenterAsync(new MapPoint(-122.66, 45.52, SpatialReferences.Wgs84), 1000000);
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }

        private async void OnRunAnalysisClicked(object sender, EventArgs e)
        {
            // Get the 'from' and 'to' dates from the date edit text's for the geoprocessing analysis.
            DateTime myFromDate;
            DateTime myToDate;

            try
            {
                myFromDate = Convert.ToDateTime(_startDateButton.Text);
                myToDate = Convert.ToDateTime(_endDateButton.Text);
            }
            catch (Exception exception)
            {
                // Show error message and quit.
                new AlertDialog.Builder(this).SetMessage(exception.Message).Show();
                return;
            }

            // Clear any existing results.
            _myMapView.Map.OperationalLayers.Clear();

            // Show busy activity indication.
            _alert.Show();

            // The end date must be at least one day after the start date.
            if (myToDate <= myFromDate.AddDays(1))
            {
                _alert.Cancel();

                // Show error message.
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Invalid date range");
                alertBuilder.SetMessage("Please select valid time range.There has to be at least one day in between To and From dates.");
                alertBuilder.Show();
                return;
            }

            // Create the parameters that are passed to the used geoprocessing task.
            GeoprocessingParameters myHotspotParameters = new GeoprocessingParameters(GeoprocessingExecutionType.AsynchronousSubmit);

            // Construct the date query.
            string myQueryString = $"(\"DATE\" > date '{myFromDate:yyyy-MM-dd} 00:00:00' AND \"DATE\" < date '{myToDate:yyyy-MM-dd} 00:00:00')";

            // Add the query that contains the date range used in the analysis.
            myHotspotParameters.Inputs.Add("Query", new GeoprocessingString(myQueryString));

            // Create job that handles the communication between the application and the geoprocessing task.
            _hotspotJob = _hotspotTask.CreateJob(myHotspotParameters);
            try
            {
                // Execute the geoprocessing analysis and wait for the results.
                GeoprocessingResult myAnalysisResult = await _hotspotJob.GetResultAsync();

                // Add results to a map using map server from a geoprocessing task.
                // Load to get access to full extent.
                await myAnalysisResult.MapImageLayer.LoadAsync();

                // Add the analysis layer to the map view.
                _myMapView.Map.OperationalLayers.Add(myAnalysisResult.MapImageLayer);

                // Zoom to the results.
                await _myMapView.SetViewpointAsync(new Viewpoint(myAnalysisResult.MapImageLayer.FullExtent));

                // Remove the loading alert dialog.
                _alert.Cancel();
            }
            catch (TaskCanceledException)
            {
                // This is thrown if the task is canceled. Ignore.
            }
            catch (Exception ex)
            {
                // Remove the loading alert dialog.
                _alert.Cancel();

                // Display error messages if the geoprocessing task fails.
                if (_hotspotJob.Status == JobStatus.Failed && _hotspotJob.Error != null)
                {
                    AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this);
                    alertBuilder.SetTitle("Geoprocessing error");
                    alertBuilder.SetMessage("Executing geoprocessing failed. " + _hotspotJob.Error.Message);
                    alertBuilder.Show();
                }
                else
                {
                    AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this);
                    alertBuilder.SetTitle("Sample error");
                    alertBuilder.SetMessage("An error occurred. " + ex.ToString());
                    alertBuilder.Show();
                }
            }
        }

        private void OnDateClicked(object sender, EventArgs a)
        {
            // Get the date from the button text.
            DateTime buttonDate = Convert.ToDateTime(((Button)sender).Text);

            // Create a new DatePickerDialog using the date from the button.
            DatePickerDialog dialog = new DatePickerDialog(this, (EventHandler<DatePickerDialog.DateSetEventArgs>)null, buttonDate.Year, buttonDate.Month - 1, buttonDate.Day);

            // Add an event handler that changes the button text when a date is picked.
            dialog.DateSet += (s, e) => { ((Button)sender).Text = e.Date.ToShortDateString(); };

            // Display the dialog to the user.
            dialog.Show();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            LinearLayout.LayoutParams buttonParam = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent,
                1.0f
            );
            LinearLayout.LayoutParams labelParam = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent,
                3.0f
            );

            // Create a horizontal sub layout for the start date.
            LinearLayout startDateSubLayout = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Label for the start date.
            TextView startDateLabel = new TextView(this)
            {
                Text = "Start Date:",
                LayoutParameters = labelParam,
                Gravity = GravityFlags.Center
            };
            startDateSubLayout.AddView(startDateLabel);

            // Button for the start date.
            _startDateButton = new Button(this)
            {
                Text = "1/01/1998",
                LayoutParameters = buttonParam
            };
            _startDateButton.Click += OnDateClicked;
            startDateSubLayout.AddView(_startDateButton);

            // Add the start date information to the general layout.
            layout.AddView(startDateSubLayout);

            // Create a horizontal sub layout for the end date.
            LinearLayout endDateSubLayout = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Label for the end date.
            TextView endDateLabel = new TextView(this)
            {
                Text = "End Date:",
                LayoutParameters = labelParam,
                Gravity = GravityFlags.Center
            };

            endDateSubLayout.AddView(endDateLabel);

            // Button for the end date.
            _endDateButton = new Button(this)
            {
                Text = "1/31/1998",
                LayoutParameters = buttonParam
            };
            _endDateButton.Click += OnDateClicked;
            endDateSubLayout.AddView(_endDateButton);

            // Add the start date information to the general layout.
            layout.AddView(endDateSubLayout);

            // Add a button to the run the hot spot analysis; wire up the click event as well
            Button mapsButton = new Button(this)
            {
                Text = "Run Analysis"
            };
            mapsButton.Click += OnRunAnalysisClicked;
            layout.AddView(mapsButton);

            // Create a layout to be used to alert the user when processing is happening.
            LinearLayout alertLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };

            // Create paramaters for the items in the alert layout.
            LinearLayout.LayoutParams alertParam = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent,
                1.0f
            );
            alertParam.SetMargins(0, 10, 0, 10);

            // Text for the processing alert.
            TextView processingText = new TextView(this)
            {
                Text = "Processing...",
                LayoutParameters = alertParam,
                Gravity = GravityFlags.Center,
            };

            // Add the progress bar to indicate the geoprocessing task is running.
            ProgressBar progressBar = new ProgressBar(this)
            {
                Indeterminate = true,
                LayoutParameters = alertParam,
                TextAlignment = TextAlignment.Center
            };

            // Add the text and progress bar to the Linear Layout.
            alertLayout.AddView(processingText);
            alertLayout.AddView(progressBar);

            // Create the alert dialog.
            _alert = new AlertDialog.Builder(this).Create();
            _alert.SetCanceledOnTouchOutside(false);
            _alert.Show();
            _alert.Cancel();

            // Add the layout to the alert.
            _alert.AddContentView(alertLayout, buttonParam);

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}