// Copyright 2016 Esri.
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
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.ListGeodatabaseVersions
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "List geodatabase versions",
        "Geoprocessing",
        "This sample calls a custom GeoprocessingTask to get a list of available versions for an enterprise geodatabase. The task returns a table of geodatabase version information, which is displayed in the app as a list.",
        "")]
    public partial class ListGeodatabaseVersions : ContentPage
    {

        // Url to used geoprocessing service
        private const string ListVersionsUrl =
            "https://sampleserver6.arcgisonline.com/arcgis/rest/services/GDBVersions/GPServer/ListVersions";

        public ListGeodatabaseVersions()
        {
            InitializeComponent ();

            Title = "List Geodatabase Versions";

            // Create the UI, setup the control references and execute initialization
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
                theTextBox.Text = myStringBuilder.ToString();
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
                    await DisplayAlert("Geoprocessing error", "Executing geoprocessing failed. " + listVersionsJob.Error.Message, "OK");
                }
                else
                {
                    await DisplayAlert("Sample error", "An error occurred. " + ex.ToString(), "OK");
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
                MyActivityIndicator.IsVisible = true;
                MyActivityIndicator.IsRunning = true;
            }
            else
            {
                // Remove the busy activity indication
                MyActivityIndicator.IsRunning = false;
                MyActivityIndicator.IsVisible = false;

            }
        }
    }
}
