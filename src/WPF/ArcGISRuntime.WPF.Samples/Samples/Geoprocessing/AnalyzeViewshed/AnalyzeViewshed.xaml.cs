// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ArcGISRuntime.WPF.Samples.AnalyzeViewshed
{
    public partial class AnalyzeViewshed
    {
        // Url to used geoprocessing service
        private const string ViewshedUrl =
            "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Elevation/ESRI_Elevation_World/GPServer/Viewshed";

        // Used to store state of the geoprocessing task
        private bool _isExecutingGeoprocessing;

        // Used to show input and output
        private GraphicsOverlay _inputOverlay;
        private GraphicsOverlay _resultOverlay;

        public AnalyzeViewshed()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with topographic basemap and an initial location
            Map myMap = new Map(BasemapType.Topographic, 45.3790902612337, 6.84905317262762, 13);

            // Hook into tapped event
            MyMapView.GeoViewTapped += OnMapViewTapped;

            // Create overlay that shows clicked location
            CreateOverlays();

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }

        private async void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            if (_isExecutingGeoprocessing)
                return;

            SetBusy();

            // Clear previous location and results
            _inputOverlay.Graphics.Clear();
            _resultOverlay.Graphics.Clear();

            // Add marker to indicate analysis location
            Graphic inputGraphic = new Graphic(e.Location);
            _inputOverlay.Graphics.Add(inputGraphic);

            // Execute geoprocessing task
            await CalculateViewshed(e.Location);
        }

        private async Task CalculateViewshed(MapPoint location)
        {
            // Create new geoprocessing task 
            var viewshedTask = new GeoprocessingTask(new Uri(ViewshedUrl));

            // Create input collection that contains the requested location
            var inputFeatures = new FeatureCollectionTable(new List<Field>(), GeometryType.Point, MyMapView.SpatialReference);
            Feature inputFeature = inputFeatures.CreateFeature();
            inputFeature.Geometry = location;
            await inputFeatures.AddFeatureAsync(inputFeature);

            // Create parameters that are passed to the used geoprocessing task
            GeoprocessingParameters viewshedParameters =
                 new GeoprocessingParameters(GeoprocessingExecutionType.SynchronousExecute);

            // Request output features in same SpatialReference as view.
            // viewshedParameters.ProcessSpatialReference = inputFeatures.SpatialReference; 
            viewshedParameters.OutputSpatialReference = MyMapView.SpatialReference;

            // Add input location to geoprocessing parameters
            viewshedParameters.Inputs.Add("Input_Observation_Point", new GeoprocessingFeatures(inputFeatures));

            // Create job that handles the communication between the application and the geoprocessing task
            var viewshedJob = viewshedTask.CreateJob(viewshedParameters);
            try
            {
                // Execute analysis and wait for the results
                GeoprocessingResult analysisResult = await viewshedJob.GetResultAsync();

                // Get results from the outputs
                GeoprocessingFeatures viewshedResultFeatures = analysisResult.Outputs["Viewshed_Result"] as GeoprocessingFeatures;

                // Add all the results as a graphics to the map
                IFeatureSet viewshedAreas = viewshedResultFeatures.Features;
                foreach (var feature in viewshedAreas)
                {
                    _resultOverlay.Graphics.Add(new Graphic(feature.Geometry));
                }
            }
            catch (Exception ex)
            {
                if (viewshedJob.Status == JobStatus.Failed && viewshedJob.Error != null)
                    MessageBox.Show("Executing geoprocessing failed. " + viewshedJob.Error.Message, "Geoprocessing error");
                else
                    MessageBox.Show("An error occurred. " + ex.ToString(), "Sample error");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void CreateOverlays()
        {
            // Create renderer for input graphic
            SimpleRenderer inputRenderer = new SimpleRenderer()
            {
                Symbol = new SimpleMarkerSymbol()
                {
                    Size = 15,
                    Color = Colors.Red
                }
            };

            // Create overlay to where input graphic is shown
            _inputOverlay = new GraphicsOverlay()
            {
                Renderer = inputRenderer
            };

            // Create renderer for input graphic
            SimpleRenderer resultRenderer = new SimpleRenderer()
            {
                Symbol = new SimpleFillSymbol()
                {
                    Color = Color.FromArgb(100, 226, 119, 40)
                }
            };

            // Create overlay to where input graphic is shown
            _resultOverlay = new GraphicsOverlay()
            {
                Renderer = resultRenderer
            };

            // Add created overlays to the MapView
            MyMapView.GraphicsOverlays.Add(_inputOverlay);
            MyMapView.GraphicsOverlays.Add(_resultOverlay);
        }

        private void SetBusy(bool isBusy = true)
        {
            if (isBusy)
            {
                // Change UI to indicate that the geoprocessing is running
                _isExecutingGeoprocessing = true;
                busyOverlay.Visibility = Visibility.Visible;
                progress.IsIndeterminate = true;
            }
            else
            {
                // Change UI to indicate that the geoprocessing is not running
                _isExecutingGeoprocessing = false;
                busyOverlay.Visibility = Visibility.Collapsed;
                progress.IsIndeterminate = false;
            }
        }
    }
}
