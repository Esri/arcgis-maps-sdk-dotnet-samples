// Copyright 2017 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Threading.Tasks;
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
        // Constant holding offset where the MapView control should start
        private const int _yPageOffset = 60;

        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Create a label to display "Start Date:"
        private UILabel _startDateLabel;

        // Create a text field to display an initial start date "1/1/98" for the analysis  
        private UITextField _startDateTextField;

        // Create a label to display "End Date:"
        private UILabel _endDateLabel;

        // Create a text field to display an initial end date "1/31/98" for the analysis
        private UITextField _endDateTextField;

        // Create button to execute the geoprocessing analyze hot spots function
        private UIButton _runAnalysisButton;

        // Create the progress indicator
        private UIActivityIndicatorView _myProgressBar;

        // Url for the geoprocessing service
        private const string _hotspotUrl =
            "https://sampleserver6.arcgisonline.com/arcgis/rest/services/911CallsHotspot/GPServer/911%20Calls%20Hotspot";

        // The geoprocessing task for hot spots analysis 
        private GeoprocessingTask _hotspotTask;

        // The job that handles the communication between the application and the geoprocessing task
        private GeoprocessingJob _hotspotJob;

        public AnalyzeHotspots()
        {
            Title = "Analyze Hotspots";
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
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 100, View.Bounds.Width, View.Bounds.Height);

            // Setup the visual frame for the start date UILabel
            _startDateLabel.Frame = new CoreGraphics.CGRect(0, _yPageOffset, View.Bounds.Width, 40);

            // Setup the visual frame for the initial start date text ("1/1/98") for the analysis in a UITextField
            _startDateTextField.Frame = new CoreGraphics.CGRect(100, _yPageOffset, View.Bounds.Width, 40);

            // Setup the visual frame for the end date UILabel
            _endDateLabel.Frame = new CoreGraphics.CGRect(0, _yPageOffset + 40, View.Bounds.Width, 40);

            // Setup the visual frame for the initial end date text ("1/31/98") for the analysis in a UITextField
            _endDateTextField.Frame = new CoreGraphics.CGRect(100, _yPageOffset + 40, View.Bounds.Width, 40);

            // Setup the visual frame for the button to execute the geoprocessing analyze hot spots function
            _runAnalysisButton.Frame = new CoreGraphics.CGRect(0, _yPageOffset + 80, View.Bounds.Width, 40);

            // The progress bar is will appear overlaid in the middle of the map view
            _myProgressBar.Frame = new CoreGraphics.CGRect(0, 300, View.Bounds.Width, 40);
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

            // Show the animating progress bar 
            _myProgressBar.StartAnimating();

            // Get the 'from' and 'to' dates from the date pickers for the geoprocessing analysis
            DateTime myFromDate;
            DateTime myToDate;
            try
            {
                myFromDate = Convert.ToDateTime(_startDateTextField.Text);
                myToDate = Convert.ToDateTime(_endDateTextField.Text);
            }
            catch (Exception)
            {
                // Handle badly formatted dates
                UIAlertController alert = UIAlertController.Create("Invalid date", "Please enter a valid date", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);

                // Stop the progress bar from animating (which also hides it as well)
                _myProgressBar.StopAnimating();

                return;
            }

            // The end date must be at least one day after the start date
            if (myToDate <= myFromDate.AddDays(1))
            {
                // Show error message
                UIAlertController alert = UIAlertController.Create("Invalid date range", 
                    "Please enter a valid date range. There has to be at least one day in between To and From dates.", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);

                // Stop the progress bar from animating (which also hides it as well)
                _myProgressBar.StopAnimating();

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
                    // Report error
                    UIAlertController alert = UIAlertController.Create("Geoprocessing Error", _hotspotJob.Error.Message, UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else
                {
                    // Report error
                    UIAlertController alert = UIAlertController.Create("Sample error", ex.ToString(), UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
            }
            finally
            {
                // Stop the progress bar from animating (which also hides it as well)
                _myProgressBar.StopAnimating();
            }
        }

        private void CreateLayout()
        {
            // Create label for the start date
            _startDateLabel = new UILabel();
            _startDateLabel.Text = "Start Date:";
            _startDateLabel.AdjustsFontSizeToFitWidth = true;
            _startDateLabel.BackgroundColor = UIColor.White;

            // Create text field for the initial start date "1/1/98" for the analysis
            _startDateTextField = new UITextField();
            _startDateTextField.Text = "1/01/98";
            _startDateTextField.AdjustsFontSizeToFitWidth = true;
            _startDateTextField.BackgroundColor = UIColor.White;
            // Allow pressing 'return' to dismiss the keyboard
            _startDateTextField.ShouldReturn += (textField) => { textField.ResignFirstResponder(); return true; };

            // Create label for the end date
            _endDateLabel = new UILabel();
            _endDateLabel.Text = "End Date:";
            _endDateLabel.AdjustsFontSizeToFitWidth = true;
            _endDateLabel.BackgroundColor = UIColor.White;

            // Create text field for the initial end date "1/31/98" for the analysis
            _endDateTextField = new UITextField();
            _endDateTextField.Text = "1/31/98";
            _endDateTextField.AdjustsFontSizeToFitWidth = true;
            _endDateTextField.BackgroundColor = UIColor.White;
            // Allow pressing 'return' to dismiss the keyboard
            _endDateTextField.ShouldReturn += (textField) => { textField.ResignFirstResponder(); return true; };

            // Create button to invoke the geoprocessing request
            _runAnalysisButton = new UIButton();
            _runAnalysisButton.SetTitle("Run Analysis", UIControlState.Normal);
            _runAnalysisButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _runAnalysisButton.BackgroundColor = UIColor.White;

            // Hook to touch event to do geoprocessing request
            _runAnalysisButton.TouchUpInside += OnRunAnalysisClicked;

            // Hide the activity indicator (progress bar) when stopped
            _myProgressBar = new UIActivityIndicatorView();
            _myProgressBar.BackgroundColor = UIColor.Black;
            _myProgressBar.HidesWhenStopped = true;

            // Add all of the UI controls to the page
            View.AddSubviews(_myMapView, _startDateLabel, _startDateTextField, _endDateLabel, _endDateTextField, _runAnalysisButton, _myProgressBar);
        }
    }
}