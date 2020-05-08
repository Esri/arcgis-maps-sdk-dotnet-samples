// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.LineOfSightLocation
{
    [Register("LineOfSightLocation")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Line of sight (location)",
        category: "Analysis",
        description: "Perform a line of sight analysis between two points in real time.",
        instructions: "Tap to place the starting point for the line. Tap again to place the end point.",
        tags: new[] { "3D", "line of sight", "visibility", "visibility analysis" })]
    public class LineOfSightLocation : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;

        // URL for an image service to use as an elevation source.
        private const string ElevationSourceUrl = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        // Location line of sight analysis.
        private LocationLineOfSight _lineOfSightAnalysis;

        // Observer location for line of sight.
        private MapPoint _observerLocation;

        // Target location for line of sight.
        private MapPoint _targetLocation;

        // Offset (meters) to use for the observer/target height (z-value for the points).
        private const double ZOffset = 2.0;

        public LineOfSightLocation()
        {
            Title = "Line of sight (location)";
        }

        private void Initialize()
        {
            // Create a new Scene with an imagery basemap.
            Scene scene = new Scene(Basemap.CreateImagery());

            // Create an elevation source for the Scene.
            ArcGISTiledElevationSource elevationSrc = new ArcGISTiledElevationSource(new Uri(ElevationSourceUrl));
            scene.BaseSurface.ElevationSources.Add(elevationSrc);

            // Add the Scene to the SceneView.
            _mySceneView.Scene = scene;

            // Set the viewpoint with a new camera.
            Camera newCamera = new Camera(new MapPoint(-121.7, 45.4, SpatialReferences.Wgs84), 10000, 0, 45, 0);
            _mySceneView.SetViewpointCameraAsync(newCamera);

            // Create a new line of sight analysis with arbitrary points (observer and target will be defined by the user).
            _lineOfSightAnalysis = new LocationLineOfSight(new MapPoint(0.0, 0.0, SpatialReferences.Wgs84), new MapPoint(0.0, 0.0, SpatialReferences.Wgs84));

            // Set the visible and obstructed colors (default would be green/red).
            // These are static properties that apply to all line of sight analyses in the scene view.
            LineOfSight.VisibleColor = System.Drawing.Color.Cyan;
            LineOfSight.ObstructedColor = System.Drawing.Color.Magenta;

            // Create an analysis overlay to contain the analysis and add it to the scene view.
            AnalysisOverlay lineOfSightOverlay = new AnalysisOverlay();
            lineOfSightOverlay.Analyses.Add(_lineOfSightAnalysis);
            _mySceneView.AnalysisOverlays.Add(lineOfSightOverlay);
        }

        private void SceneViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Ignore if tapped out of bounds (e.g. the sky).
            if (e.Location == null)
            {
                return;
            }

            // When the view is tapped, define the observer or target location with the tap point as appropriate.
            if (_observerLocation == null)
            {
                // Define the observer location (plus an offset for observer height) and set the target to the same point.
                _observerLocation = new MapPoint(e.Location.X, e.Location.Y, e.Location.Z + ZOffset);
                _lineOfSightAnalysis.ObserverLocation = _observerLocation;
                _lineOfSightAnalysis.TargetLocation = _observerLocation;

                // Clear the target location (if any) so the next click will define the target.
                _targetLocation = null;
            }
            else if (_targetLocation == null)
            {
                // Define the target.
                _targetLocation = new MapPoint(e.Location.X, e.Location.Y, e.Location.Z + ZOffset);
                _lineOfSightAnalysis.TargetLocation = _targetLocation;

                // Clear the observer location so it can be defined again.
                _observerLocation = null;
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            View = new UIView {BackgroundColor = UIColor.White};

            // Add the views.
            View.AddSubviews(_mySceneView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _mySceneView.GeoViewTapped += SceneViewTapped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _mySceneView.GeoViewTapped -= SceneViewTapped;
        }
    }
}