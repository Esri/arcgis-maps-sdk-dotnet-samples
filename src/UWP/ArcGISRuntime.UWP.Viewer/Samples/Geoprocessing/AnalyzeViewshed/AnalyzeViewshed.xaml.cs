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
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.UWP.Samples.AnalyzeViewshed
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Viewshed (Geoprocessing)",
        "Geoprocessing",
        "This sample demonstrates how to use GeoprocessingTask to calculate a viewshed using a geoprocessing service. Click any point on the map to see all areas that are visible within a 1 kilometer radius. It may take a few seconds for the model to run and send back the results.",
        "")]
    public partial class AnalyzeViewshed
    {
        // Url for the geoprocessing service
        private const string _viewshedUrl =
            "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Elevation/ESRI_Elevation_World/GPServer/Viewshed";

        // Used to store state of the geoprocessing task
        private bool _isExecutingGeoprocessing;

        // The graphics overlay to show where the user clicked in the map
        private GraphicsOverlay _inputOverlay;

        // The graphics overlay to display the result of the viewshed analysis
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

            // Hook into the tapped event
            MyMapView.GeoViewTapped += OnMapViewTapped;

            // Create empty overlays for the user clicked location and the results of the viewshed analysis
            CreateOverlays();

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }

        private async void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // The geoprocessing task is still executing, don't do anything else (i.e. respond to
            // more user taps) until the processing is complete.
            if (_isExecutingGeoprocessing)
            {
                return;
            }

            // Indicate that the geoprocessing is running
            SetBusy();

            // Clear previous user click location and the viewshed geoprocessing task results
            _inputOverlay.Graphics.Clear();
            _resultOverlay.Graphics.Clear();

            // Get the tapped point
            MapPoint geometry = e.Location;

            // Create a marker graphic where the user clicked on the map and add it to the existing graphics overlay
            Graphic myInputGraphic = new Graphic(geometry);
            _inputOverlay.Graphics.Add(myInputGraphic);

            // Normalize the geometry if wrap-around is enabled
            //    This is necessary because of how wrapped-around map coordinates are handled by Runtime
            //    Without this step, the task may fail because wrapped-around coordinates are out of bounds.
            if (MyMapView.IsWrapAroundEnabled) { geometry = GeometryEngine.NormalizeCentralMeridian(geometry) as MapPoint; }

            // Execute the geoprocessing task using the user click location
            await CalculateViewshed(geometry);
        }

        private async Task CalculateViewshed(MapPoint location)
        {
            // This function will define a new geoprocessing task that performs a custom viewshed analysis based upon a
            // user click on the map and then display the results back as a polygon fill graphics overlay. If there
            // is a problem with the execution of the geoprocessing task an error message will be displayed

            // Create new geoprocessing task using the url defined in the member variables section
            var myViewshedTask = await GeoprocessingTask.CreateAsync(new Uri(_viewshedUrl));

            // Create a new feature collection table based upon point geometries using the current map view spatial reference
            var myInputFeatures = new FeatureCollectionTable(new List<Field>(), GeometryType.Point, MyMapView.SpatialReference);

            // Create a new feature from the feature collection table. It will not have a coordinate location (x,y) yet
            Feature myInputFeature = myInputFeatures.CreateFeature();

            // Assign a physical location to the new point feature based upon where the user clicked in the map view
            myInputFeature.Geometry = location;

            // Add the new feature with (x,y) location to the feature collection table
            await myInputFeatures.AddFeatureAsync(myInputFeature);

            // Create the parameters that are passed to the used geoprocessing task
            GeoprocessingParameters myViewshedParameters =
                new GeoprocessingParameters(GeoprocessingExecutionType.SynchronousExecute);

            // Request the output features to use the same SpatialReference as the map view
            myViewshedParameters.OutputSpatialReference = MyMapView.SpatialReference;

            // Add an input location to the geoprocessing parameters
            myViewshedParameters.Inputs.Add("Input_Observation_Point", new GeoprocessingFeatures(myInputFeatures));

            // Create the job that handles the communication between the application and the geoprocessing task
            var myViewshedJob = myViewshedTask.CreateJob(myViewshedParameters);

            try
            {
                // Execute analysis and wait for the results
                GeoprocessingResult myAnalysisResult = await myViewshedJob.GetResultAsync();

                // Get the results from the outputs
                GeoprocessingFeatures myViewshedResultFeatures = myAnalysisResult.Outputs["Viewshed_Result"] as GeoprocessingFeatures;

                // Add all the results as a graphics to the map
                IFeatureSet myViewshedAreas = myViewshedResultFeatures.Features;
                foreach (var myFeature in myViewshedAreas)
                {
                    _resultOverlay.Graphics.Add(new Graphic(myFeature.Geometry));
                }
            }
            catch (Exception ex)
            {
                // Display an error message if there is a problem
                if (myViewshedJob.Status == JobStatus.Failed && myViewshedJob.Error != null)
                {
                    var message = new MessageDialog("Executing geoprocessing failed. " + myViewshedJob.Error.Message, "Geoprocessing error");
                    await message.ShowAsync();
                }
                else
                {
                    var message = new MessageDialog("An error occurred. " + ex, "Sample error");
                    await message.ShowAsync();
                }
            }
            finally
            {
                // Indicate that the geoprocessing is not running
                SetBusy(false);
            }
        }

        private void CreateOverlays()
        {
            // This function will create the overlays that show the user clicked location and the results of the
            // viewshed analysis. Note: the overlays will not be populated with any graphics at this point

            // Create renderer for input graphic. Set the size and color properties for the simple renderer
            SimpleRenderer myInputRenderer = new SimpleRenderer()
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
                Renderer = myInputRenderer
            };

            // Create fill renderer for output of the viewshed analysis. Set the color property of the simple renderer
            SimpleRenderer myResultRenderer = new SimpleRenderer()
            {
                Symbol = new SimpleFillSymbol()
                {
                    Color = Color.FromArgb(100, 226, 119, 40)
                }
            };

            // Create overlay to where viewshed analysis graphic is shown
            _resultOverlay = new GraphicsOverlay()
            {
                Renderer = myResultRenderer
            };

            // Add the created overlays to the MapView
            MyMapView.GraphicsOverlays.Add(_inputOverlay);
            MyMapView.GraphicsOverlays.Add(_resultOverlay);
        }

        private void SetBusy(bool isBusy = true)
        {
            // This function toggles the visibility of the 'busyOverlay' Grid control defined in xaml,
            // sets the 'progress' control feedback status and updates the _isExecutingGeoprocessing
            // boolean to denote if the viewshed analysis is executing as a result of the user click
            // on the map

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