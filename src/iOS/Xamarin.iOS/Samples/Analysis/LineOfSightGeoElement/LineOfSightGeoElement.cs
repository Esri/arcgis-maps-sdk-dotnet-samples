// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Timers;
using ArcGISRuntime.Samples.Managers;
using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.LineOfSightGeoElement
{
    [Register("LineOfSightGeoElement")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("3af5cfec0fd24dac8d88aea679027cb9")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Line of Sight (GeoElement)",
        "Analysis",
        "This sample demonstrates how to perform a dynamic line of sight analysis between two moving GeoElements.",
        "Use the slider to adjust the height of the observer.",
        "Featured")]
    public class LineOfSightGeoElement : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly SceneView _mySceneView = new SceneView();
        private readonly UIToolbar _labelToolbar = new UIToolbar();
        private readonly UIToolbar _sliderToolbar = new UIToolbar();
        private readonly UISlider _mySlider = new UISlider();

        private readonly UILabel _myStatusLabel = new UILabel
        {
            Text = "Status: ",
            TextAlignment = UITextAlignment.Center,
            AdjustsFontSizeToFitWidth = true
        };

        // URL of the elevation service - provides elevation component of the scene.
        private readonly Uri _elevationUri = new Uri("http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

        // URL of the building service - provides building models.
        private readonly Uri _buildingsUri = new Uri("https://tiles.arcgis.com/tiles/z2tnIkrLQ2BRzr6P/arcgis/rest/services/New_York_LoD2_3D_Buildings/SceneServer/layers/0");

        // Starting point of the observation point.
        private readonly MapPoint _observerPoint = new MapPoint(-73.984988, 40.748131, 20, SpatialReferences.Wgs84);

        // Graphic to represent the observation point.
        private Graphic _observerGraphic;

        // Graphic to represent the observed target.
        private Graphic _taxiGraphic;

        // Line of Sight Analysis.
        private GeoElementLineOfSight _geoLine;

        // For taxi animation - four points in a loop.
        private readonly MapPoint[] _points =
        {
            new MapPoint(-73.984513, 40.748469, SpatialReferences.Wgs84),
            new MapPoint(-73.985068, 40.747786, SpatialReferences.Wgs84),
            new MapPoint(-73.983452, 40.747091, SpatialReferences.Wgs84),
            new MapPoint(-73.982961, 40.747762, SpatialReferences.Wgs84)
        };

        // For taxi animation - tracks animation state.
        private int _pointIndex = 0;
        private int _frameIndex = 0;
        private const int FrameMax = 150;

        public LineOfSightGeoElement()
        {
            Title = "Line of Sight (GeoElement)";
        }

        private async void Initialize()
        {
            // Create scene.
            Scene myScene = new Scene(Basemap.CreateImageryWithLabels())
            {
                // Set initial viewpoint.
                InitialViewpoint = new Viewpoint(_observerPoint, 1000000)
            };

            // Create the elevation source.
            ElevationSource myElevationSource = new ArcGISTiledElevationSource(_elevationUri);
            // Add the elevation source to the scene.
            myScene.BaseSurface.ElevationSources.Add(myElevationSource);
            // Create the building scene layer.
            ArcGISSceneLayer mySceneLayer = new ArcGISSceneLayer(_buildingsUri);
            // Add the building layer to the scene.
            myScene.OperationalLayers.Add(mySceneLayer);

            // Add the observer to the scene.
            // Create a graphics overlay with relative surface placement; relative surface placement allows the Z position of the observation point to be adjusted.
            GraphicsOverlay overlay = new GraphicsOverlay {SceneProperties = new LayerSceneProperties(SurfacePlacement.Relative)};
            // Create the symbol that will symbolize the observation point.
            SimpleMarkerSceneSymbol symbol = new SimpleMarkerSceneSymbol(SimpleMarkerSceneSymbolStyle.Sphere, System.Drawing.Color.Red, 10, 10, 10, SceneSymbolAnchorPosition.Bottom);
            // Create the observation point graphic from the point and symbol.
            _observerGraphic = new Graphic(_observerPoint, symbol);
            // Add the observer to the overlay.
            overlay.Graphics.Add(_observerGraphic);
            // Add the overlay to the scene.
            _mySceneView.GraphicsOverlays.Add(overlay);

            // Add the taxi to the scene.
            // Get the path to the model.
            string taxiModelPath = DataManager.GetDataFolder("3af5cfec0fd24dac8d88aea679027cb9", "dolmus.3ds");
            // Create the model symbol for the taxi.
            ModelSceneSymbol taxiSymbol = await ModelSceneSymbol.CreateAsync(new Uri(taxiModelPath));
            // Set the anchor position for the mode; ensures that the model appears above the ground.
            taxiSymbol.AnchorPosition = SceneSymbolAnchorPosition.Bottom;
            // Create the graphic from the taxi starting point and the symbol.
            _taxiGraphic = new Graphic(_points[0], taxiSymbol);
            // Add the taxi graphic to the overlay.
            overlay.Graphics.Add(_taxiGraphic);

            // Create GeoElement Line of sight analysis (taxi to building).
            // Create the analysis.
            _geoLine = new GeoElementLineOfSight(_observerGraphic, _taxiGraphic)
            {
                // Apply an offset to the target. This helps avoid some false negatives.
                TargetOffsetZ = 2
            };
            // Create the analysis overlay.
            AnalysisOverlay myAnalysisOverlay = new AnalysisOverlay();
            // Add the analysis to the overlay.
            myAnalysisOverlay.Analyses.Add(_geoLine);
            // Add the analysis overlay to the scene.
            _mySceneView.AnalysisOverlays.Add(myAnalysisOverlay);

            // Create a timer; this will enable animating the taxi.
            var timer = new Timer(120);
            // Move the taxi every time the timer expires.
            timer.Elapsed += AnimationTimer_Elapsed;
            // Keep the timer running continuously.
            timer.AutoReset = true;
            // Start the timer.
            timer.Start();

            // Subscribe to TargetVisible events; allows for updating the UI and selecting the taxi when it is visible.
            _geoLine.TargetVisibilityChanged += Geoline_TargetVisibilityChanged;

            // Add the scene to the view.
            _mySceneView.Scene = myScene;
        }

        private void AnimationTimer_Elapsed(object sender, EventArgs e)
        {
            // Note: the contents of this function are solely related to animating the taxi.

            // Increment the frame counter.
            _frameIndex++;

            // Reset the frame counter once one segment of the path has been traveled.
            if (_frameIndex == FrameMax)
            {
                _frameIndex = 0;

                // Start navigating toward the next point.
                _pointIndex++;

                // Restart if finished circuit.
                if (_pointIndex == _points.Length)
                {
                    _pointIndex = 0;
                }
            }

            // Get the point the taxi is traveling from.
            MapPoint starting = _points[_pointIndex];
            // Get the point the taxi is traveling to.
            MapPoint ending = _points[(_pointIndex + 1) % _points.Length];
            // Calculate the progress based on the current frame.
            double progress = _frameIndex / (double) FrameMax;
            // Calculate the position of the taxi when it is {progress}% of the way through.
            _taxiGraphic.Geometry = InterpolatedPoint(starting, ending, progress);
        }

        private MapPoint InterpolatedPoint(MapPoint firstPoint, MapPoint secondPoint, double progress)
        {
            // This function returns a MapPoint that is the result of traveling {progress}% of the way from {firstPoint} to {secondPoint}.

            // Get the difference between the two points.
            MapPoint difference = new MapPoint(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y, secondPoint.Z - firstPoint.Z, SpatialReferences.Wgs84);
            // Scale the difference by the progress towards the destination.
            MapPoint scaled = new MapPoint(difference.X * progress, difference.Y * progress, difference.Z * progress);
            // Add the scaled progress to the starting point.
            return new MapPoint(firstPoint.X + scaled.X, firstPoint.Y + scaled.Y, firstPoint.Z + scaled.Z);
        }

        private void Geoline_TargetVisibilityChanged(object sender, EventArgs e)
        {
            // This is needed because Runtime delivers notifications from a different thread that doesn't have access to UI controls.
            BeginInvokeOnMainThread(UpdateUiAndSelection);
        }

        private void UpdateUiAndSelection()
        {
            switch (_geoLine.TargetVisibility)
            {
                case LineOfSightTargetVisibility.Obstructed:
                    _myStatusLabel.Text = "Status: Obstructed";
                    _taxiGraphic.IsSelected = false;
                    break;

                case LineOfSightTargetVisibility.Visible:
                    _myStatusLabel.Text = "Status: Visible";
                    _taxiGraphic.IsSelected = true;
                    break;

                default:
                case LineOfSightTargetVisibility.Unknown:
                    _myStatusLabel.Text = "Status: Unknown";
                    _taxiGraphic.IsSelected = false;
                    break;
            }
        }

        private void MyHeightSlider_ValueChanged(object sender, EventArgs e)
        {
            // Update the height of the observer based on the slider value.

            // Constrain the min and max to 20 and 150 units.
            double minHeight = 20;
            double maxHeight = 150;

            // Scale the slider value; its default range is 0-10.
            double value = _mySlider.Value;

            // Get the current point.
            MapPoint oldPoint = (MapPoint) _observerGraphic.Geometry;

            // Update geometry with a new point with the same (x,y) but updated z.
            _observerGraphic.Geometry = new MapPoint(oldPoint.X, oldPoint.Y, (maxHeight - minHeight) * value + minHeight);
        }

        private void CreateLayout()
        {
            // Add views to the page
            View.AddSubviews(_mySceneView, _labelToolbar, _sliderToolbar, _mySlider, _myStatusLabel);

            // Subscribe to slider events
            _mySlider.ValueChanged += MyHeightSlider_ValueChanged;
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat controlHeight = 30;
                nfloat toolbarHeight = 40;
                nfloat controlWidth = View.Bounds.Width - 2 * margin;
                nfloat sliderMargin = 50;

                // Reposition the controls.
                _mySceneView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _labelToolbar.Frame = new CGRect(0, topMargin, View.Bounds.Width, toolbarHeight);
                _sliderToolbar.Frame = new CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);
                _myStatusLabel.Frame = new CGRect(margin, topMargin + margin, controlWidth, controlHeight);
                _mySlider.Frame = new CGRect(sliderMargin, _sliderToolbar.Frame.Top + margin, View.Bounds.Width - 2 * sliderMargin, controlHeight);
                _mySceneView.ViewInsets = new UIEdgeInsets(_labelToolbar.Frame.Bottom, 0, _sliderToolbar.Frame.Height, 0);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }
    }
}