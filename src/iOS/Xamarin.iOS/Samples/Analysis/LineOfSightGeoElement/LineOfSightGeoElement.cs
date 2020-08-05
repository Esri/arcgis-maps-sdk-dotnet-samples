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
        name: "Line of sight (geoelement)",
        category: "Analysis",
        description: "Show a line of sight between two moving objects.",
        instructions: "A line of sight will display between a point on the Empire State Building (observer) and a taxi (target).",
        tags: new[] { "3D", "line of sight", "visibility", "visibility analysis" })]
    public class LineOfSightGeoElement : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;
        private UILabel _statusLabel;
        private UISlider _heightSlider;

        // URL of the elevation service - provides elevation component of the scene.
        private readonly Uri _elevationUri = new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

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

        // Timer to run the taxi animation.
        private Timer _animationTimer;

        public LineOfSightGeoElement()
        {
            Title = "Line of sight (GeoElement)";
        }

        private async void Initialize()
        {
            // Create scene.
            Scene myScene = new Scene(Basemap.CreateImageryWithLabels())
            {
                // Set initial viewpoint.
                InitialViewpoint = new Viewpoint(_observerPoint, 1600)
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

            try
            {
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
                _animationTimer = new Timer(120);
                // Keep the timer running continuously.
                _animationTimer.AutoReset = true;

                // Add the scene to the view.
                _mySceneView.Scene = myScene;
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
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

            // Update the taxi rotation.
            GeodeticDistanceResult distance = GeometryEngine.DistanceGeodetic(starting, ending, LinearUnits.Meters, AngularUnits.Degrees, GeodeticCurveType.Geodesic);
            ((ModelSceneSymbol)_taxiGraphic.Symbol).Heading = distance.Azimuth1;
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
                    _statusLabel.Text = "Status: Obstructed";
                    _taxiGraphic.IsSelected = false;
                    break;

                case LineOfSightTargetVisibility.Visible:
                    _statusLabel.Text = "Status: Visible";
                    _taxiGraphic.IsSelected = true;
                    break;

                default:
                case LineOfSightTargetVisibility.Unknown:
                    _statusLabel.Text = "Status: Unknown";
                    _taxiGraphic.IsSelected = false;
                    break;
            }
        }

        private void MyHeightSlider_ValueChanged(object sender, EventArgs e)
        {
            // Constrain the min and max to 20 and 150 units.
            const double minHeight = 20;
            const double maxHeight = 150;

            // Scale the slider value; its default range is 0-10.
            double value = (sender as UISlider).Value;

            // Get the current point.
            MapPoint oldPoint = (MapPoint) _observerGraphic.Geometry;

            // Update geometry with a new point with the same (x,y) but updated z.
            _observerGraphic.Geometry = new MapPoint(oldPoint.X, oldPoint.Y, (maxHeight - minHeight) * value + minHeight);
        }

        public override void ViewDidLoad()
        {
            Initialize();

            base.ViewDidLoad();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };
            View.BackgroundColor = UIColor.White;

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            _heightSlider = new UISlider();
            _heightSlider.TranslatesAutoresizingMaskIntoConstraints = false;

            UIBarButtonItem sliderWrapper = new UIBarButtonItem(_heightSlider);
            sliderWrapper.Width = 300;

            UIToolbar sliderToolbar = new UIToolbar();
            sliderToolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            sliderToolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                sliderWrapper,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            _statusLabel = new UILabel
            {
                BackgroundColor = UIColor.FromWhiteAlpha(0f, .6f),
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_mySceneView, sliderToolbar, _statusLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                sliderToolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                sliderToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                sliderToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(sliderToolbar.TopAnchor),

                _statusLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _statusLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _statusLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _statusLabel.HeightAnchor.ConstraintEqualTo(40)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _heightSlider.ValueChanged += MyHeightSlider_ValueChanged;
            _geoLine.TargetVisibilityChanged += Geoline_TargetVisibilityChanged;
            _animationTimer.Elapsed += AnimationTimer_Elapsed;
            _animationTimer.Start();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _geoLine.TargetVisibilityChanged -= Geoline_TargetVisibilityChanged;
            _animationTimer.Stop();
            _animationTimer.Elapsed -= AnimationTimer_Elapsed;
            _heightSlider.ValueChanged -= MyHeightSlider_ValueChanged;
        }
    }
}