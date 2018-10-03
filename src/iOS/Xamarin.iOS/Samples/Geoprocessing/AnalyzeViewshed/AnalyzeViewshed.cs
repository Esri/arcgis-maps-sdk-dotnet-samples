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
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.AnalyzeViewshed
{
    [Register("AnalyzeViewshed")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Viewshed (Geoprocessing)",
        "Geoprocessing",
        "This sample demonstrates how to use GeoprocessingTask to calculate a viewshed using a geoprocessing service. Click any point on the map to see all areas that are visible within a 1 kilometer radius. It may take a few seconds for the model to run and send back the results.",
        "")]
    public class AnalyzeViewshed : UIViewController
    {
        // Create and hold a reference to the MapView.
        private MapView _myMapView;

        // URL for the geoprocessing service.
        private const string ViewshedServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Elevation/ESRI_Elevation_World/GPServer/Viewshed";

        // The graphics overlay to show where the user clicked in the map.
        private GraphicsOverlay _inputOverlay;

        // The graphics overlay to display the result of the viewshed analysis.
        private GraphicsOverlay _resultOverlay;

        public AnalyzeViewshed()
        {
            Title = "Viewshed (Geoprocessing)";
        }

        public override void LoadView()
        {
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            View = new UIView();
            View.AddSubviews(_myMapView);

            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Initialize();
        }

        private void Initialize()
        {
            // Create and show a map with topographic basemap and an initial location.
            _myMapView.Map = new Map(BasemapType.Topographic, 45.3790902612337, 6.84905317262762, 13);

            // Hook into the MapView tapped event.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            // Create empty overlays for the user clicked location and the results of the viewshed analysis.
            CreateOverlays();
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Clear previous user click location and the viewshed geoprocessing task results.
            _inputOverlay.Graphics.Clear();
            _resultOverlay.Graphics.Clear();

            // Get the tapped point.
            MapPoint geometry = e.Location;

            // Create a marker graphic where the user clicked on the map and add it to the existing graphics overlay.
            Graphic inputGraphic = new Graphic(geometry);
            _inputOverlay.Graphics.Add(inputGraphic);

            // Normalize the geometry if wrap-around is enabled.
            //    This is necessary because of how wrapped-around map coordinates are handled by Runtime.
            //    Without this step, the task may fail because wrapped-around coordinates are out of bounds.
            if (_myMapView.IsWrapAroundEnabled)
            {
                geometry = (MapPoint) GeometryEngine.NormalizeCentralMeridian(geometry);
            }

            // Execute the geoprocessing task using the user click location.
            await CalculateViewshed(geometry);
        }

        private async Task CalculateViewshed(MapPoint location)
        {
            // This function will define a new geoprocessing task that performs a custom viewshed analysis based upon a
            // user click on the map and then display the results back as a polygon fill graphics overlay. If there
            // is a problem with the execution of the geoprocessing task an error message will be displayed.

            // Create new geoprocessing task using the URL defined in the member variables section.
            GeoprocessingTask viewshedTask = await GeoprocessingTask.CreateAsync(new Uri(ViewshedServiceUrl));

            // Create a new feature collection table based upon point geometries using the current map view spatial reference.
            FeatureCollectionTable inputFeatures = new FeatureCollectionTable(new List<Field>(), GeometryType.Point, _myMapView.SpatialReference);

            // Create a new feature from the feature collection table. It will not have a coordinate location (x,y) yet.
            Feature inputFeature = inputFeatures.CreateFeature();

            // Assign a physical location to the new point feature based upon where the user clicked in the map view.
            inputFeature.Geometry = location;

            // Add the new feature with (x,y) location to the feature collection table.
            await inputFeatures.AddFeatureAsync(inputFeature);

            // Create the parameters that are passed to the used geoprocessing task.
            GeoprocessingParameters viewshedParameters = new GeoprocessingParameters(GeoprocessingExecutionType.SynchronousExecute)
            {
                OutputSpatialReference = _myMapView.SpatialReference
            };

            // Add an input location to the geoprocessing parameters.
            viewshedParameters.Inputs.Add("Input_Observation_Point", new GeoprocessingFeatures(inputFeatures));

            // Create the job that handles the communication between the application and the geoprocessing task.
            GeoprocessingJob viewshedJob = viewshedTask.CreateJob(viewshedParameters);

            try
            {
                // Execute analysis and wait for the results.
                GeoprocessingResult analysisResult = await viewshedJob.GetResultAsync();

                // Get the results from the outputs.
                GeoprocessingFeatures viewshedResultFeatures = (GeoprocessingFeatures) analysisResult.Outputs["Viewshed_Result"];

                // Add all the results as a graphics to the map.
                foreach (Feature feature in viewshedResultFeatures.Features)
                {
                    _resultOverlay.Graphics.Add(new Graphic(feature.Geometry));
                }
            }
            catch (Exception ex)
            {
                // Display an error message if there is a problem.
                if (viewshedJob.Status == JobStatus.Failed && viewshedJob.Error != null)
                {
                    // Report error
                    UIAlertController alert = UIAlertController.Create("Geoprocessing Error", viewshedJob.Error.Message, UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else
                {
                    // Report error
                    UIAlertController alert = UIAlertController.Create("Sample Error", ex.ToString(), UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
            }
        }

        private void CreateOverlays()
        {
            // This function will create the overlays that show the user clicked location and the results of the
            // viewshed analysis. Note: the overlays will not be populated with any graphics at this point

            // Create renderer for input graphic. Set the size and color properties for the simple renderer.
            SimpleRenderer inputRenderer = new SimpleRenderer
            {
                Symbol = new SimpleMarkerSymbol
                {
                    Size = 15,
                    Color = System.Drawing.Color.Red
                }
            };

            // Create overlay to show input graphic.
            _inputOverlay = new GraphicsOverlay
            {
                Renderer = inputRenderer
            };

            // Create fill renderer for output of the viewshed analysis. Set the color property of the simple renderer.
            SimpleRenderer resultRenderer = new SimpleRenderer
            {
                Symbol = new SimpleFillSymbol
                {
                    Color = System.Drawing.Color.FromArgb(100, 226, 119, 40)
                }
            };

            // Create overlay to show the result.
            _resultOverlay = new GraphicsOverlay
            {
                Renderer = resultRenderer
            };

            // Add the created overlays to the MapView.
            _myMapView.GraphicsOverlays.Add(_inputOverlay);
            _myMapView.GraphicsOverlays.Add(_resultOverlay);
        }
    }
}