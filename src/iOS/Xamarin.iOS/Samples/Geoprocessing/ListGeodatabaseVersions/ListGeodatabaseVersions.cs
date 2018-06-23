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
using System.Collections.Generic;
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
        // Create and hold references to the UI controls.
        private readonly UIActivityIndicatorView _progressBar = new UIActivityIndicatorView();
        private readonly UITextView _geodatabaseListField = new UITextView();

        // URL to used geoprocessing service
        private readonly Uri _listVersionsUrl = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/GDBVersions/GPServer/ListVersions");

        public ListGeodatabaseVersions()
        {
            Title = "List geodatabase versions";
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

                // Reposition controls.
                _geodatabaseListField.Frame = new CoreGraphics.CGRect(0, topMargin, View.Bounds.Width, View.Bounds.Height - topMargin);
                _progressBar.Frame = new CoreGraphics.CGRect(0, topMargin, View.Bounds.Width, View.Bounds.Height - topMargin);
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private async void Initialize()
        {
            // Set the UI to indicate that the geoprocessing is running.
            SetBusy(true);

            // Get versions from a geodatabase.
            IFeatureSet versionsFeatureSet = await GetGeodatabaseVersionsAsync();

            // Continue if there is a valid geoprocessing result.
            if (versionsFeatureSet != null)
            {
                // Create a string builder to hold all of the information from the geoprocessing
                // task to display in the UI.
                var stringBuilder = new System.Text.StringBuilder();

                // Loop through each Feature in the FeatureSet.
                foreach (var version in versionsFeatureSet)
                {
                    // Loop through each attribute (a <key,value> pair).
                    foreach (KeyValuePair<string, object> oneAttribute in version.Attributes)
                    {
                        // Get the key.
                        string key = oneAttribute.Key;

                        // Get the value.
                        var value = oneAttribute.Value;

                        // Add the key and value strings to the string builder.
                        stringBuilder.AppendLine(key + ": " + value);
                    }

                    // Add a blank line after each Feature (the listing of geodatabase versions).
                    stringBuilder.AppendLine();
                }

                // Display the results to the user.
                _geodatabaseListField.Text = stringBuilder.ToString();
            }

            // Set the UI to indicate that the geoprocessing is not running.
            SetBusy(false);
        }

        private async Task<IFeatureSet> GetGeodatabaseVersionsAsync()
        {
            // Results will be returned as a feature set.
            IFeatureSet results = null;

            // Create new geoprocessing task.
            var listVersionsTask = await GeoprocessingTask.CreateAsync(_listVersionsUrl);

            // Create default parameters that are passed to the geoprocessing task.
            GeoprocessingParameters listVersionsParameters = await listVersionsTask.CreateDefaultParametersAsync();

            // Create job that handles the communication between the application and the geoprocessing task.
            var listVersionsJob = listVersionsTask.CreateJob(listVersionsParameters);
            try
            {
                // Execute analysis and wait for the results.
                GeoprocessingResult analysisResult = await listVersionsJob.GetResultAsync();

                // Get results from the outputs.
                GeoprocessingFeatures listVersionsResults = analysisResult.Outputs["Versions"] as GeoprocessingFeatures;

                // Set results.
                results = listVersionsResults.Features;
            }
            catch (Exception ex)
            {
                // Error handling if something goes wrong.
                if (listVersionsJob.Status == JobStatus.Failed && listVersionsJob.Error != null)
                {
                    UIAlertController alert = new UIAlertController
                    {
                        Message = "Executing geoprocessing failed. " + listVersionsJob.Error.Message
                    };
                    alert.ShowViewController(this, this);
                }
                else
                {
                    UIAlertController alert = new UIAlertController
                    {
                        Message = "An error occurred. " + ex
                    };
                    alert.ShowViewController(this, this);
                }
            }
            finally
            {
                // Set the UI to indicate that the geoprocessing is not running.
                SetBusy(false);
            }

            return results;
        }

        private void SetBusy(bool isBusy = true)
        {
            // This function toggles running of the 'progress' control feedback status to denote if 
            // the viewshed analysis is executing as a result of the user click on the map.
            _progressBar.Hidden = !isBusy;
        }

        private void CreateLayout()
        {
            // Hide the progress bar.
            _progressBar.Hidden = true;

            // Enable animation.
            _progressBar.StartAnimating();

            // Set the color.
            _progressBar.Color = View.TintColor;
            _progressBar.BackgroundColor = UIColor.FromWhiteAlpha(.5f, .8f);

            // Make the view background color white (so the navigation bar looks good).
            View.BackgroundColor = UIColor.White;

            // Add views to the layout.
            View.AddSubviews(_geodatabaseListField, _progressBar);
        }
    }
}