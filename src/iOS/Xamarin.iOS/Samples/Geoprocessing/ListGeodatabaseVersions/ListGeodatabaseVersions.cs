// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ListGeodatabaseVersions
{
    [Register("ListGeodatabaseVersions")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "List geodatabase versions",
        "Geoprocessing",
        "Connect to a service and list versions of the geodatabase.",
        "When the sample loads, a list of geodatabase versions and their properties will be displayed.",
        "conflict resolution", "data management", "database", "multi-user", "sync", "version")]
    public class ListGeodatabaseVersions : UIViewController
    {
        // Hold references to UI controls.
        private UIActivityIndicatorView _progressBar;
        private UITextView _geodatabaseListField;

        // URL pointing to the service.
        private readonly Uri _listVersionsUrl =
            new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/GDBVersions/GPServer/ListVersions");

        public ListGeodatabaseVersions()
        {
            Title = "List geodatabase versions";
        }

        private async void Initialize()
        {
            try
            {
                // Get versions from a geodatabase.
                IFeatureSet versionsFeatureSet = await GetGeodatabaseVersionsAsync();

                // Continue if there is a valid geoprocessing result.
                if (versionsFeatureSet != null)
                {
                    // Create a string builder to hold all of the information from the geoprocessing
                    // task to display in the UI.
                    StringBuilder stringBuilder = new StringBuilder();

                    // Loop through each Feature in the FeatureSet.
                    foreach (Feature version in versionsFeatureSet)
                    {
                        // Loop through each attribute (a <key,value> pair).
                        foreach (KeyValuePair<string, object> attribute in version.Attributes)
                        {
                            // Add the key and value strings to the string builder.
                            stringBuilder.AppendLine(attribute.Key + ": " + attribute.Value);
                        }

                        // Add a blank line after each Feature (the listing of geodatabase versions).
                        stringBuilder.AppendLine();
                    }

                    // Display the results to the user.
                    _geodatabaseListField.Text = stringBuilder.ToString();
                }
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private async Task<IFeatureSet> GetGeodatabaseVersionsAsync()
        {
            // Start animating the activity indicator.
            _progressBar.StartAnimating();

            // Results will be returned as a feature set.
            IFeatureSet results = null;

            // Create new geoprocessing task.
            GeoprocessingTask listVersionsTask = await GeoprocessingTask.CreateAsync(_listVersionsUrl);

            // Create default parameters that are passed to the geoprocessing task.
            GeoprocessingParameters listVersionsParameters = await listVersionsTask.CreateDefaultParametersAsync();

            // Create job that handles the communication between the application and the geoprocessing task.
            GeoprocessingJob listVersionsJob = listVersionsTask.CreateJob(listVersionsParameters);
            try
            {
                // Execute analysis and wait for the results.
                GeoprocessingResult analysisResult = await listVersionsJob.GetResultAsync();

                // Get results from the outputs.
                GeoprocessingFeatures listVersionsResults = (GeoprocessingFeatures) analysisResult.Outputs["Versions"];

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
                // Stop the activity animation.
                _progressBar.StopAnimating();
            }

            return results;
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

            _progressBar = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            _progressBar.TranslatesAutoresizingMaskIntoConstraints = false;
            _progressBar.BackgroundColor = UIColor.FromWhiteAlpha(.3f, .8f);
            _progressBar.HidesWhenStopped = true;

            _geodatabaseListField = new UITextView();
            _geodatabaseListField.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_geodatabaseListField, _progressBar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _geodatabaseListField.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _geodatabaseListField.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                _geodatabaseListField.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
                _geodatabaseListField.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                _progressBar.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _progressBar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _progressBar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _progressBar.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            });
        }
    }
}