// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Foundation;
using System;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntime.Samples.ListGeodatabaseVersions
{
    [Register("ListGeodatabaseVersions")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "List geodatabase versions",
        "Geoprocessing",
        "This sample calls a custom GeoprocessingTask to get a list of available versions for an enterprise geodatabase. The task returns a table of geodatabase version information, which is displayed in the app as a list.",
        "")]
    public class ListGeodatabaseVersions : UIViewController
    {
        // Progress bar to show when the geoprocessing task is working
        UIActivityIndicatorView _myProgressBar = new UIActivityIndicatorView();

        // Text view to display the list of geodatabases
        UITextView _myEditText_ListGeodatabases = new UITextView();

        // Url to used geoprocessing service
        private Uri ListVersionsUrl = 
            new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/GDBVersions/GPServer/ListVersions");

        public ListGeodatabaseVersions()
        {
            Title = "List geodatabase versions";
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
            // Set the visual frame for the text view
            _myEditText_ListGeodatabases.Frame = new CoreGraphics.CGRect(10, 80, View.Bounds.Width - 20, View.Bounds.Height - 80);

            // Set the visual frame for the progress view
            _myProgressBar.Frame = new CoreGraphics.CGRect(View.Bounds.Width / 2 - 50, View.Bounds.Height / 2 - 50, 100, 100);
        }

        private async void Initialize()
        {
            // Set the UI to indicate that the geoprocessing is running
            SetBusy(true);

            // Get versions from a geodatabase
            IFeatureSet versionsFeatureSet = await GetGeodatabaseVersionsAsync();

            // Continue if we got a valid geoprocessing result
            if (versionsFeatureSet != null)
            {
                // Create a string builder to hold all of the information from the geoprocessing 
                // task to display in the UI 
                var myStringBuilder = new System.Text.StringBuilder();

                // Loop through each Feature in the FeatureSet 
                foreach (var version in versionsFeatureSet)
                {
                    // Get the attributes (a dictionary of <key,value> pairs) from the Feature
                    var myDictionary = version.Attributes;

                    // Loop through each attribute (a <key,value> pair)
                    foreach (var oneAttribute in myDictionary)
                    {
                        // Get the key
                        var myKey = oneAttribute.Key;

                        // Get the value
                        var myValue = oneAttribute.Value;

                        // Add the key and value strings to the string builder 
                        myStringBuilder.AppendLine(myKey + ": " + myValue);
                    }

                    // Add a blank line after each Feature (the listing of geodatabase versions)
                    myStringBuilder.AppendLine();
                }

                // Display the results to the user
                _myEditText_ListGeodatabases.Text = myStringBuilder.ToString();
            }

            // Set the UI to indicate that the geoprocessing is not running
            SetBusy(false);
        }

        private async Task<IFeatureSet> GetGeodatabaseVersionsAsync()
        {
            // Results will be returned as a feature set
            IFeatureSet results = null;

            // Create new geoprocessing task 
            var listVersionsTask = await GeoprocessingTask.CreateAsync(ListVersionsUrl);

            // Create default parameters that are passed to the geoprocessing task
            GeoprocessingParameters listVersionsParameters = await listVersionsTask.CreateDefaultParametersAsync();

            // Create job that handles the communication between the application and the geoprocessing task
            var listVersionsJob = listVersionsTask.CreateJob(listVersionsParameters);
            try
            {
                // Execute analysis and wait for the results
                GeoprocessingResult analysisResult = await listVersionsJob.GetResultAsync();

                // Get results from the outputs
                GeoprocessingFeatures listVersionsResults = analysisResult.Outputs["Versions"] as GeoprocessingFeatures;

                // Set results
                results = listVersionsResults.Features;
            }
            catch (Exception ex)
            {
                // Error handling if something goes wrong
                if (listVersionsJob.Status == JobStatus.Failed && listVersionsJob.Error != null)
                {
                    UIAlertController alert = new UIAlertController();
                    alert.Message = "Executing geoprocessing failed. " + listVersionsJob.Error.Message;
                    alert.ShowViewController(this, this);
                }
                else
                {
                    UIAlertController alert = new UIAlertController();
                    alert.Message = "An error occurred. " + ex.ToString();
                    alert.ShowViewController(this, this);
                }
            }
            finally
            {
                // Set the UI to indicate that the geoprocessing is not running
                SetBusy(false);
            }

            return results;
        }

        private void SetBusy(bool isBusy = true)
        {
            // This function toggles running of the 'progress' control feedback status to denote if 
            // the viewshed analysis is executing as a result of the user click on the map
            if (isBusy)
            {
                // Show busy activity indication
                _myProgressBar.Hidden = false;
            }
            else
            {
                // Remove the busy activity indication
                _myProgressBar.Hidden = true;
            }
        }

        private void CreateLayout()
        {
            // Hide the progress bar
            _myProgressBar.Hidden = true;

            // Enable animation
            _myProgressBar.StartAnimating();

            // Set the color
            _myProgressBar.Color = UIColor.Blue;

            // Add views to the layout
            View.AddSubviews(_myEditText_ListGeodatabases, _myProgressBar);
        }
    }
}