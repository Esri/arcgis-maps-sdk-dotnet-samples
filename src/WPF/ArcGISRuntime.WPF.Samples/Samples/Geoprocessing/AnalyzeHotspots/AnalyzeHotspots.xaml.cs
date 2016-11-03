// Copyright 2016 Esri.
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
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.AnalyzeHotspots
{
    public partial class AnalyzeHotspots
    {
        // Url to used geoprocessing service
        private const string HotspotsUrl = 
            "http://sampleserver6.arcgisonline.com/arcgis/rest/services/911CallsHotspot/GPServer/911%20Calls%20Hotspot";

        private GeoprocessingTask _hotspotTask;
        private GeoprocessingJob _hotspotJob;

        // Format for the query that is used in the geoprocessing task.
        private string _queryFormat =
                "(\"DATE\" > date '{0} 00:00:00' AND \"DATE\" < date '{1} 00:00:00')";

        public AnalyzeHotspots()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with 'Imagery with Labels' basemap and an initial location
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create new geoprocessing task
            _hotspotTask = new GeoprocessingTask(new Uri(HotspotsUrl));

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }

        private async void OnAnalyzeHotspotsClicked(object sender, RoutedEventArgs e)
        {
            // Show busy indication
            ShowBusyOverlay();

            var fromDate = FromDate.SelectedDate.Value;
            var toDate = ToDate.SelectedDate.Value;

            if (toDate <= fromDate.AddDays(1))
            {
                // Show error message
                MessageBox.Show(
                    "Please select valid time range. There has to be at least one day in between To and From dates.", 
                    "Invalid date range");
                // Remove overlay
                ShowBusyOverlay(false);
                return;
            }

            // Create parameters that are passed to the used geoprocessing task
           GeoprocessingParameters hotspotParameters = 
                new GeoprocessingParameters(GeoprocessingExecutionType.AsynchronousSubmit);

            // Construct used query
            var queryString = string.Format("(\"DATE\" > date '{0} 00:00:00' AND \"DATE\" < date '{1} 00:00:00')",
                fromDate.ToString("yyyy-MM-dd"),
                toDate.ToString("yyyy-MM-dd"));

            // Add query that contains the date range and the days of the week that are used in analysis
            hotspotParameters.Inputs.Add("Query", new GeoprocessingString(queryString));

            // Create job that handles the communication between the application and the geoprocessing task
            _hotspotJob = _hotspotTask.CreateJob(hotspotParameters);
            try
            {
                // Execute analysis and wait for the results
                GeoprocessingResult analysisResult = await _hotspotJob.GetResultAsync();

                // Add results to a map using map server from a geoprocessing task
                // Load to get access to full extent
                await analysisResult.MapImageLayer.LoadAsync();
                // Add layers to the map view
                MyMapView.Map.OperationalLayers.Add(analysisResult.MapImageLayer);
                // Zoom to the results
                await MyMapView.SetViewpointAsync(
                    new Viewpoint(analysisResult.MapImageLayer.FullExtent));
            }
            catch (TaskCanceledException)
            {
                // This is thrown if the task is canceled. Ignore.
            }
            catch (Exception ex)
            {
                if (_hotspotJob.Status == JobStatus.Failed && _hotspotJob.Error != null)
                    MessageBox.Show("Executing geoprocessing failed. " + _hotspotJob.Error.Message, "Geoprocessing error");
                else
                    MessageBox.Show("An error occurred. " + ex.ToString(), "Sample error");
            }
            finally
            {
                // Remove overlay
                ShowBusyOverlay(false);
            }
        }

        private void OnCancelTaskClicked(object sender, RoutedEventArgs e)
        {
            // Cancel current geoprocessing job
            if (_hotspotJob.Status == JobStatus.Started)
                _hotspotJob.Cancel();

            // Hide busy indication
            ShowBusyOverlay(false);
        }

        private void ShowBusyOverlay(bool visibility = true)
        {
            if (visibility)
            {
                busyOverlay.Visibility = Visibility.Visible;
                progress.IsIndeterminate = true;
            }
            else
            {
                busyOverlay.Visibility = Visibility.Collapsed;
                progress.IsIndeterminate = false;
            }
        }
    }
}
