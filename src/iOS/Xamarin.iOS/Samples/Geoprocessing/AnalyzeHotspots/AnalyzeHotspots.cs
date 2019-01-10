// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.AnalyzeHotspots
{
    [Register("AnalyzeHotspots")]
    [ArcGISRuntime.Samples.Shared.Attributes.ClassFile("DateSelectionViewController")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Analyze hotspots",
        "Geoprocessing",
        "This sample demonstrates how to execute the GeoprocessingTask asynchronously to calculate a hotspot analysis based on the frequency of 911 calls. It calculates the frequency of these calls within a given study area during a specified constrained time period set between 1/1/1998 and 5/31/1998.",
        "To run the hotspot analysis, select a data range and click on the 'Run analysis' button. Note the larger the date range, the longer it may take for the task to run and send back the results.")]
    public class AnalyzeHotspots : UIViewController
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _configureButton;
        private UIBarButtonItem _startButton;
        private DateSelectionViewController _selectionView;
        private UIActivityIndicatorView _progressBar;

        // URL for the geoprocessing service.
        private const string HotspotUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/911CallsHotspot/GPServer/911%20Calls%20Hotspot";

        // The geoprocessing task for hot spots analysis.
        private GeoprocessingTask _hotspotTask;

        // The job that handles the communication between the application and the geoprocessing task.
        private GeoprocessingJob _hotspotJob;

        public AnalyzeHotspots()
        {
            Title = "Analyze hotspots";
        }

        private async void Initialize()
        {
            // Create and show a map with a topographic basemap.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

            try
            {
                // Create a new geoprocessing task.
                _hotspotTask = await GeoprocessingTask.CreateAsync(new Uri(HotspotUrl));
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private async void OnRunAnalysisClicked(object sender, EventArgs e)
        {
            // Clear any existing results.
            _myMapView.Map.OperationalLayers.Clear();

            // Show the animating progress bar .
            _progressBar.StartAnimating();

            // Get the 'from' and 'to' dates from the date pickers for the geoprocessing analysis.
            DateTime fromDate = (DateTime) _selectionView.StartPicker.Date;
            DateTime toDate = (DateTime) _selectionView.EndPicker.Date;

            // The end date must be at least one day after the start date.
            if (toDate <= fromDate.AddDays(1))
            {
                // Show error message.
                UIAlertController alert = UIAlertController.Create("Invalid date range",
                    "Please enter a valid date range. There has to be at least one day in between To and From dates.", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);

                // Stop the progress bar from animating (which also hides it as well).
                _progressBar.StopAnimating();

                return;
            }

            // Create the parameters that are passed to the used geoprocessing task.
            GeoprocessingParameters hotspotParameters = new GeoprocessingParameters(GeoprocessingExecutionType.AsynchronousSubmit);

            // Construct the date query.
            string myQueryString = $"(\"DATE\" > date '{fromDate:yyyy-MM-dd 00:00:00}' AND \"DATE\" < date '{toDate:yyy-MM-dd 00:00:00}')";

            // Add the query that contains the date range used in the analysis.
            hotspotParameters.Inputs.Add("Query", new GeoprocessingString(myQueryString));

            // Create job that handles the communication between the application and the geoprocessing task.
            _hotspotJob = _hotspotTask.CreateJob(hotspotParameters);
            try
            {
                // Execute the geoprocessing analysis and wait for the results.
                GeoprocessingResult analysisResult = await _hotspotJob.GetResultAsync();

                // Add results to a map using map server from a geoprocessing task.
                // Load to get access to full extent.
                await analysisResult.MapImageLayer.LoadAsync();

                // Add the analysis layer to the map view.
                _myMapView.Map.OperationalLayers.Add(analysisResult.MapImageLayer);

                // Zoom to the results.
                await _myMapView.SetViewpointAsync(new Viewpoint(analysisResult.MapImageLayer.FullExtent));
            }
            catch (TaskCanceledException)
            {
                // This is thrown if the task is canceled. Ignore.
            }
            catch (Exception ex)
            {
                // Display error messages if the geoprocessing task fails.
                if (_hotspotJob.Status == JobStatus.Failed && _hotspotJob.Error != null)
                {
                    // Report error.
                    UIAlertController alert = UIAlertController.Create("Geoprocessing Error", _hotspotJob.Error.Message, UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else
                {
                    // Report error.
                    UIAlertController alert = UIAlertController.Create("Sample error", ex.ToString(), UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
            }
            finally
            {
                // Stop the progress bar from animating (which also hides it).
                _progressBar.StopAnimating();
            }
        }

        private void ShowConfiguration(object sender, EventArgs e)
        {
            NavigationController.PushViewController(_selectionView, true);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _selectionView = new DateSelectionViewController();

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _configureButton = new UIBarButtonItem("Configure", UIBarButtonItemStyle.Plain, ShowConfiguration);
            _startButton = new UIBarButtonItem("Run analysis", UIBarButtonItemStyle.Plain, OnRunAnalysisClicked);

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                _configureButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _startButton
            };

            // Hide the activity indicator (progress bar) when stopped.
            _progressBar = new UIActivityIndicatorView
            {
                BackgroundColor = UIColor.FromWhiteAlpha(0, .5f),
                HidesWhenStopped = true,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar, _progressBar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new []
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                _progressBar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _progressBar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _progressBar.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _progressBar.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            });
        }
    }
}