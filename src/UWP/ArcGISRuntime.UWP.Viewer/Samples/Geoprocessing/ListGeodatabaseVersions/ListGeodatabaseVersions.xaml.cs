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
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.UWP.Samples.ListGeodatabaseVersions
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "List geodatabase versions",
        category: "Geoprocessing",
        description: "Connect to a service and list versions of the geodatabase.",
        instructions: "When the sample loads, a list of geodatabase versions and their properties will be displayed.",
        tags: new[] { "conflict resolution", "data management", "database", "multi-user", "sync", "version" })]
    public sealed partial class ListGeodatabaseVersions
    {
        // Url to used geoprocessing service
        private const string ListVersionsUrl =
            "https://sampleserver6.arcgisonline.com/arcgis/rest/services/GDBVersions/GPServer/ListVersions";

        public ListGeodatabaseVersions()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private async void Initialize()
        {
            // Set the UI to indicate that the geoprocessing is running
            SetBusy(true);

            try
            {
                // Get versions from a geodatabase
                IFeatureSet versionsFeatureSet = await GetGeodatabaseVersionsAsync();

                // Continue if we got a valid geoprocessing result
                if (versionsFeatureSet != null)
                {
                    // Create a string builder to hold all of the information from the geoprocessing 
                    // task to display in the UI 
                    StringBuilder myStringBuilder = new StringBuilder();

                    // Loop through each Feature in the FeatureSet 
                    foreach (Feature version in versionsFeatureSet)
                    {
                        // Get the attributes (a dictionary of <key,value> pairs) from the Feature
                        IDictionary<string,object> myDictionary = version.Attributes;

                        // Loop through each attribute (a <key,value> pair)
                        foreach (KeyValuePair<string,object> attribute in myDictionary)
                        {
                            // Add the key and value strings to the string builder 
                            myStringBuilder.AppendLine(attribute.Key + ": " + attribute.Value);
                        }

                        // Add a blank line after each Feature (the listing of geodatabase versions)
                        myStringBuilder.AppendLine();
                    }

                    // Display the result in the textbox
                    theTextBox.Text = myStringBuilder.ToString();
                }
            }
            catch (Exception e)
            {
                await new MessageDialog(e.ToString(), "Error").ShowAsync();
            }

            // Set the UI to indicate that the geoprocessing is not running
            SetBusy(false);
        }

        private async Task<IFeatureSet> GetGeodatabaseVersionsAsync()
        {
            // Results will be returned as a feature set
            IFeatureSet results = null;

            // Create new geoprocessing task 
            GeoprocessingTask listVersionsTask = await GeoprocessingTask.CreateAsync(new Uri(ListVersionsUrl));

            // Create default parameters that are passed to the geoprocessing task
            GeoprocessingParameters listVersionsParameters = await listVersionsTask.CreateDefaultParametersAsync();

            // Create job that handles the communication between the application and the geoprocessing task
            GeoprocessingJob listVersionsJob = listVersionsTask.CreateJob(listVersionsParameters);
            try
            {
                // Execute analysis and wait for the results
                GeoprocessingResult analysisResult = await listVersionsJob.GetResultAsync();

                // Get results from the outputs
                GeoprocessingFeatures listVersionsResults = (GeoprocessingFeatures)analysisResult.Outputs["Versions"];

                // Set results
                results = listVersionsResults.Features;
            }
            catch (Exception ex)
            {
                // Error handling if something goes wrong
                if (listVersionsJob.Status == JobStatus.Failed && listVersionsJob.Error != null)
                {
                    MessageDialog message = new MessageDialog("Executing geoprocessing failed. " + listVersionsJob.Error.Message, "Geoprocessing error");
                    await message.ShowAsync();
                }

                else
                {
                    MessageDialog message = new MessageDialog("An error occurred. " + ex.ToString(), "Sample error");
                    await message.ShowAsync();
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
            if (isBusy)
            {
                // Change UI to indicate that the geoprocessing is running
                busyOverlay.Visibility = Visibility.Visible;
                progress.IsIndeterminate = true;
            }
            else
            {
                // Change UI to indicate that the geoprocessing is not running
                busyOverlay.Visibility = Visibility.Collapsed;
                progress.IsIndeterminate = false;
            }
        }
    }
}
