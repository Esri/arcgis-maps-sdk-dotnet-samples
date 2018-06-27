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
using CoreGraphics;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.AnalyzeHotspots
{
    [Register("AnalyzeHotspots")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Analyze hotspots",
        "Geoprocessing",
        "This sample demonstrates how to execute the GeoprocessingTask asynchronously to calculate a hotspot analysis based on the frequency of 911 calls. It calculates the frequency of these calls within a given study area during a specified constrained time period set between 1/1/1998 and 5/31/1998.",
        "To run the hotspot analysis, select a data range and click on the 'Run analysis' button. Note the larger the date range, the longer it may take for the task to run and send back the results.")]
    public class AnalyzeHotspots : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private UILabel _startDateLabel;
        private UITextField _startDateTextField;
        private UILabel _endDateLabel;
        private UITextField _endDateTextField;
        private UIButton _runAnalysisButton;
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
                nfloat margin = 5;
                nfloat controlHeight = 30;
                nfloat columnSplit = 100;
                nfloat topStart = topMargin;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _toolbar.Frame = new CGRect(0, topStart, View.Bounds.Width, controlHeight * 3 + margin * 4);
                _startDateLabel.Frame = new CGRect(margin, topStart + margin, columnSplit - 2 * margin, controlHeight);
                _startDateTextField.Frame = new CGRect(columnSplit + margin, topStart + margin, View.Bounds.Width - columnSplit - 2 * margin, controlHeight);
                _endDateLabel.Frame = new CGRect(margin, topStart + controlHeight + 2 * margin, columnSplit - 2 * margin, controlHeight);
                _endDateTextField.Frame = new CGRect(columnSplit + margin, topStart + controlHeight + 2 * margin, View.Bounds.Width - columnSplit - 2 * margin, controlHeight);
                _runAnalysisButton.Frame = new CGRect(margin, topStart + 2 * controlHeight + 3 * margin, View.Bounds.Width - 2 * margin, controlHeight);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin + _toolbar.Frame.Height, 0, 0, 0);
                _progressBar.Frame = new CGRect(0, topMargin, View.Bounds.Width, View.Bounds.Height - topMargin);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private async void Initialize()
        {
            // Create and show a map with a topographic basemap.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

            // Create a new geoprocessing task.
            _hotspotTask = await GeoprocessingTask.CreateAsync(new Uri(HotspotUrl));
        }

        private async void OnRunAnalysisClicked(object sender, EventArgs e)
        {
            // Clear any existing results.
            _myMapView.Map.OperationalLayers.Clear();

            // Show the animating progress bar .
            _progressBar.StartAnimating();

            // Get the 'from' and 'to' dates from the date pickers for the geoprocessing analysis.
            DateTime fromDate;
            DateTime toDate;
            try
            {
                fromDate = Convert.ToDateTime(_startDateTextField.Text);
                toDate = Convert.ToDateTime(_endDateTextField.Text);
            }
            catch (Exception)
            {
                // Handle badly formatted dates.
                UIAlertController alert = UIAlertController.Create("Invalid date", "Please enter a valid date", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);

                // Stop the progress bar from animating (which also hides it as well).
                _progressBar.StopAnimating();

                return;
            }

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

        private void CreateLayout()
        {
            // Create label for the start date.
            _startDateLabel = new UILabel
            {
                Text = "Start date:",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Right
            };

            // Create text field for the initial start date "1/1/98" for the analysis.
            _startDateTextField = new UITextField
            {
                Text = "1/01/98",
                AdjustsFontSizeToFitWidth = true,
                BackgroundColor = UIColor.FromWhiteAlpha(1, .8f),
                BorderStyle = UITextBorderStyle.RoundedRect
            };

            // Allow pressing 'return' to dismiss the keyboard.
            _startDateTextField.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            // Create label for the end date.
            _endDateLabel = new UILabel
            {
                Text = "End date:",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Right
            };

            // Create text field for the initial end date "1/31/98" for the analysis.
            _endDateTextField = new UITextField
            {
                Text = "1/31/98",
                AdjustsFontSizeToFitWidth = true,
                BackgroundColor = UIColor.FromWhiteAlpha(1, .8f),
                BorderStyle = UITextBorderStyle.RoundedRect
            };

            // Allow pressing 'return' to dismiss the keyboard.
            _endDateTextField.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            // Create button to invoke the geoprocessing request.
            _runAnalysisButton = new UIButton();
            _runAnalysisButton.SetTitle("Run analysis", UIControlState.Normal);
            _runAnalysisButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Hook to touch event to do geoprocessing request.
            _runAnalysisButton.TouchUpInside += OnRunAnalysisClicked;

            // Hide the activity indicator (progress bar) when stopped.
            _progressBar = new UIActivityIndicatorView
            {
                BackgroundColor = UIColor.FromWhiteAlpha(0, .5f),
                HidesWhenStopped = true
            };

            // Add all of the UI controls to the page.
            View.AddSubviews(_myMapView, _toolbar, _startDateLabel, _startDateTextField, _endDateLabel, _endDateTextField, _runAnalysisButton, _progressBar);
        }
    }
}