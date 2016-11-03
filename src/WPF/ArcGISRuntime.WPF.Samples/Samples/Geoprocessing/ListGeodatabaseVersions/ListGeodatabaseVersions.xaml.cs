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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.ListGeodatabaseVersions
{
    public partial class ListGeodatabaseVersions
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
            SetBusy(true);

            // Get versions from a geodatabase
            var versionsFeatureSet = await GetGeodatabaseVersionsAsync();
            if (versionsFeatureSet != null)
            {
                // Find a root node from the versions
                var rootFeatures = versionsFeatureSet.Where(x => x.Attributes["parentversionname"] == null).ToList();

                var versionsTreeSource = new List<TreeItem>();

                foreach (var rootItem in rootFeatures)
                {
                    var rootVersion = new TreeItem();
                    rootVersion.Name = rootItem.Attributes["name"].ToString();
                    if (rootItem.Attributes["created"] != null)
                    {
                        rootVersion.CreateAt = (DateTimeOffset)rootItem.Attributes["created"];
                    }

                    foreach (var version in versionsFeatureSet)
                    {
                        if (version.Attributes["parentversionname"] == null) continue;

                        // If version belongs to the root add to the root version as a child
                        if (version.Attributes["parentversionname"].ToString() == rootVersion.Name)
                        {
                            var childVersion = new TreeItem();
                            childVersion.Name = version.Attributes["name"].ToString();
                            childVersion.Parent = version.Attributes["parentversionname"].ToString();
                            if (version.Attributes["created"] != null)
                            {
                                childVersion.CreateAt = (DateTimeOffset)version.Attributes["created"];
                            }
                            rootVersion.Items.Add(childVersion);
                        }
                    }

                    versionsTreeSource.Add(rootVersion);
                }

                versionsTree.ItemsSource = versionsTreeSource;
            }

            SetBusy(false);
        }

        private async Task<IFeatureSet> GetGeodatabaseVersionsAsync()
        {
            // Results will be returned as a feature set
            IFeatureSet results = null; 

            // Create new geoprocessing task 
            var listVersionsTask = new GeoprocessingTask(new Uri(ListVersionsUrl));

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
                if (listVersionsJob.Status == JobStatus.Failed && listVersionsJob.Error != null)
                    MessageBox.Show("Executing geoprocessing failed. " + listVersionsJob.Error.Message, "Geoprocessing error");
                else
                    MessageBox.Show("An error occurred. " + ex.ToString(), "Sample error");
            }
            finally
            {
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

        internal class TreeItem
        {

            public TreeItem()
            {
                Items = new List<TreeItem>();
            }

            /// <summary>
            /// Gets or sets the name of the version.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the time when the version was created.
            /// </summary>
            public DateTimeOffset CreateAt { get; set; }


            /// <summary>
            /// Gets or sets the name of the parent version.
            /// </summary>
            public string Parent { get; set; }

            /// <summary>
            /// Gets or sets the sub items of the node.
            /// </summary>
            public List<TreeItem> Items { get; set; }
        }
    }
}
