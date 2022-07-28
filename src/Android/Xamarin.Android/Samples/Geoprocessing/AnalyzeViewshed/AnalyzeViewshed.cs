// Copyright 2018 Esri.
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

namespace ArcGISRuntime.Samples.AnalyzeViewshed
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Analyze viewshed (geoprocessing)",
        category: "Geoprocessing",
        description: "Calculate a viewshed using a geoprocessing service, in this case showing what parts of a landscape are visible from points on mountainous terrain.",
        instructions: "Tap the map to see all areas visible from that point within a 15km radius. Clicking on an elevated area will highlight a larger part of the surrounding landscape. It may take a few seconds for the task to run and send back the results.",
        tags: new[] { "geoprocessing", "heat map", "heatmap", "viewshed" })]
    public class AnalyzeViewshed : Activity
    {
        // Hold a reference to the map view.
        private MapView _myMapView;

        // Url for the geoprocessing service
        private const string _viewshedUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Elevation/ESRI_Elevation_World/GPServer/Viewshed";

        // The graphics overlay to show where the user clicked in the map.
        private GraphicsOverlay _inputOverlay;

        // The graphics overlay to display the result of the viewshed analysis.
        private GraphicsOverlay _resultOverlay;

        // Alert dialog to show when the geoprocessing task is working.
        private AlertDialog _alert;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Viewshed (Geoprocessing)";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with topographic basemap and an initial location
            Map myMap = new Map(BasemapStyle.ArcGISTopographic);
            myMap.InitialViewpoint = new Viewpoint(45.3790902612337, 6.84905317262762, 70000);

            // Hook into the MapView tapped event
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            // Create empty overlays for the user clicked location and the results of the viewshed analysis
            CreateOverlays();

            // Assign the map to the MapView
            _myMapView.Map = myMap;
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
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
            if (_myMapView.IsWrapAroundEnabled) { geometry = (MapPoint)GeometryEngine.NormalizeCentralMeridian(geometry); }

            try
            {
                // Execute the geoprocessing task using the user click location
                await CalculateViewshed(geometry);
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private async Task CalculateViewshed(MapPoint location)
        {
            // This function will define a new geoprocessing task that performs a custom viewshed analysis based upon a
            // user click on the map and then display the results back as a polygon fill graphics overlay. If there
            // is a problem with the execution of the geoprocessing task an error message will be displayed

            // Create new geoprocessing task using the url defined in the member variables section
            GeoprocessingTask myViewshedTask = await GeoprocessingTask.CreateAsync(new Uri(_viewshedUrl));

            // Create a new feature collection table based upon point geometries using the current map view spatial reference
            FeatureCollectionTable myInputFeatures = new FeatureCollectionTable(new List<Field>(), GeometryType.Point, _myMapView.SpatialReference);

            // Create a new feature from the feature collection table. It will not have a coordinate location (x,y) yet
            Feature myInputFeature = myInputFeatures.CreateFeature();

            // Assign a physical location to the new point feature based upon where the user clicked in the map view
            myInputFeature.Geometry = location;

            // Add the new feature with (x,y) location to the feature collection table
            await myInputFeatures.AddFeatureAsync(myInputFeature);

            // Create the parameters that are passed to the used geoprocessing task
            GeoprocessingParameters myViewshedParameters =
                new GeoprocessingParameters(GeoprocessingExecutionType.SynchronousExecute)
                {
                    // Request the output features to use the same SpatialReference as the map view
                    OutputSpatialReference = _myMapView.SpatialReference
                };

            // Add an input location to the geoprocessing parameters
            myViewshedParameters.Inputs.Add("Input_Observation_Point", new GeoprocessingFeatures(myInputFeatures));

            // Create the job that handles the communication between the application and the geoprocessing task
            GeoprocessingJob myViewshedJob = myViewshedTask.CreateJob(myViewshedParameters);

            try
            {
                // Execute analysis and wait for the results
                GeoprocessingResult myAnalysisResult = await myViewshedJob.GetResultAsync();

                // Get the results from the outputs
                GeoprocessingFeatures myViewshedResultFeatures = (GeoprocessingFeatures)myAnalysisResult.Outputs["Viewshed_Result"];

                // Add all the results as a graphics to the map
                IFeatureSet myViewshedAreas = myViewshedResultFeatures.Features;
                foreach (Feature myFeature in myViewshedAreas)
                {
                    _resultOverlay.Graphics.Add(new Graphic(myFeature.Geometry));
                }
            }
            catch (Exception ex)
            {
                // Display an error message if there is a problem
                if (myViewshedJob.Status == JobStatus.Failed && myViewshedJob.Error != null)
                {
                    AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this);
                    alertBuilder.SetTitle("Geoprocessing error");
                    alertBuilder.SetMessage("Executing geoprocessing failed. " + myViewshedJob.Error.Message);
                    alertBuilder.Show();
                }
                else
                {
                    AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this);
                    alertBuilder.SetTitle("Sample error");
                    alertBuilder.SetMessage("An error occurred. " + ex);
                    alertBuilder.Show();
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

                    // Account for the Color differences supported by the various platforms
                    Color = System.Drawing.Color.Red
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
                    Color = System.Drawing.Color.FromArgb(100, 226, 119, 40)
                }
            };

            // Create overlay to where viewshed analysis graphic is shown
            _resultOverlay = new GraphicsOverlay()
            {
                Renderer = myResultRenderer
            };

            // Add the created overlays to the MapView
            _myMapView.GraphicsOverlays.Add(_inputOverlay);
            _myMapView.GraphicsOverlays.Add(_resultOverlay);
        }

        private void SetBusy(bool isBusy = true)
        {
            // This function toggles running of the 'progress' control feedback status to denote if
            // the viewshed analysis is executing as a result of the user click on the map.
            if (isBusy)
            {
                // Show the busy alert dialog.
                _alert.Show();
            }
            else
            {
                // Remove the busy alert dialog.
                _alert.Hide();
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Label for the user instructions
            TextView textview_Label1 = new TextView(this)
            {
                Text = "Click a location on the map to perform the viewshed analysis."
            };
            layout.AddView(textview_Label1);

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);

            // Create a layout to be used to alert the user when processing is happening.
            LinearLayout alertLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };

            // Create paramaters for the items in the alert layout.
            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent,
                1.0f
            );
            param.SetMargins(0, 10, 0, 10);

            // Text for the alert.
            TextView processingText = new TextView(this)
            {
                Text = "Processing...",
                LayoutParameters = param,
                Gravity = GravityFlags.Center,
            };

            // Add the progress bar to indicate the geoprocessing task is running; make invisible by default
            ProgressBar progressBar = new ProgressBar(this)
            {
                Indeterminate = true,
                //Visibility = ViewStates.Invisible,
                LayoutParameters = param,
                TextAlignment = TextAlignment.Center
            };

            // Add the text and progress bar to the Linear Layout.
            alertLayout.AddView(processingText);
            alertLayout.AddView(progressBar);

            // Create the alert dialog.
            _alert = new AlertDialog.Builder(this).Create();
            _alert.SetCanceledOnTouchOutside(false);
            _alert.Show();
            _alert.Cancel();

            // Add the layout to the alert
            _alert.AddContentView(alertLayout, param);
        }
    }
}