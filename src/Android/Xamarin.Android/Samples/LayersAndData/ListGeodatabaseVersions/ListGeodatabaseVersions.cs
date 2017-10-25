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
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using System;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.ListGeodatabaseVersions
{
    [Activity]
    public class ListGeodatabaseVersions : Activity
    {
        // Progress bar to show when the geoprocessing task is working
        ProgressBar _myProgressBar;

        // Edit text to display the list of geodatabases
        EditText _myEditText_ListGeodatabases;

        // Url to used geoprocessing service
        private const string ListVersionsUrl =
            "https://sampleserver6.arcgisonline.com/arcgis/rest/services/GDBVersions/GPServer/ListVersions";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "List Geodatabase Versions";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
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
            var listVersionsTask = await GeoprocessingTask.CreateAsync(new Uri(ListVersionsUrl));

            // Create parameters that are passed to the used geoprocessing task
            GeoprocessingParameters listVersionsParameters =
                 new GeoprocessingParameters(GeoprocessingExecutionType.SynchronousExecute);

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
                    var alertBuilder = new AlertDialog.Builder(this);
                    alertBuilder.SetTitle("Geoprocessing error");
                    alertBuilder.SetMessage("Executing geoprocessing failed. " + listVersionsJob.Error.Message);
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
                _myProgressBar.Visibility = ViewStates.Visible;
            }
            else
            {
                // Remove the busy activity indication
                _myProgressBar.Visibility = ViewStates.Invisible;
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Label for the user instructions
            var textview_Label1 = new TextView(this);
            textview_Label1.Text = "Current versions:";
            layout.AddView(textview_Label1);

            // Add the progress bar to indicate the geoprocessing task is running; make invisible by default
            _myProgressBar = new ProgressBar(this);
            _myProgressBar.Indeterminate = true;
            _myProgressBar.Visibility = ViewStates.Invisible;
            layout.AddView(_myProgressBar);

            // Edit text for the start date (user can change if desired)
            _myEditText_ListGeodatabases = new EditText(this);
            layout.AddView(_myEditText_ListGeodatabases);

            // Show the layout in the app
            SetContentView(layout);
        }

    }
}