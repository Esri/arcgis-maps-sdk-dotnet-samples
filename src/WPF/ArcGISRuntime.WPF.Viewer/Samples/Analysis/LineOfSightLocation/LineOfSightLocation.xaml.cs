// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using System;
using System.Drawing;

namespace ArcGISRuntime.WPF.Samples.LineOfSightLocation
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Line of sight from location",
        "Analysis",
        "This sample demonstrates a `LocationLineOfSight` analysis that shows segments that are visible or obstructed along a line drawn from observer to target.",
        "Click to define a location for the observer, then again to define the target. The result will show visible segments in cyan and obstructed ones in magenta.")]
    public partial class LineOfSightLocation
    {
        // URL for an image service to use as an elevation source
        private string _elevationSourceUrl = @"https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        // Location line of sight analysis
        private LocationLineOfSight _lineOfSightAnalysis;

        // Observer location for line of sight
        private MapPoint _observerLocation;

        // Target location for line of sight
        private MapPoint _targetLocation;

        // Offset (meters) to use for the observer/target height (z-value for the points)
        private double _zOffset = 2.0; 

        public LineOfSightLocation()
        {
            InitializeComponent();

            // Create the Scene, basemap, line of sight analysis, and analysis overlay
            Initialize();

            // Handle taps on the scene view to define the observer or target point for the line of sight
            MySceneView.GeoViewTapped += SceneViewTapped;
        }

        private void Initialize()
        {
            // Create a new Scene with an imagery basemap
            Scene myScene = new Scene(Basemap.CreateImagery());

            // Create an elevation source for the Scene
            ArcGISTiledElevationSource elevationSrc = new ArcGISTiledElevationSource(new Uri(_elevationSourceUrl));
            myScene.BaseSurface.ElevationSources.Add(elevationSrc);

            // Add the Scene to the SceneView
            MySceneView.Scene = myScene;
            
            // Set the viewpoint with a new camera
            Camera newCamera = new Camera(new MapPoint(-121.7, 45.4, SpatialReferences.Wgs84), 10000, 0, 45, 0);
            MySceneView.SetViewpointCameraAsync(newCamera);

            // Create a new line of sight analysis with arbitrary points (observer and target will be defined by the user)
            _lineOfSightAnalysis = new LocationLineOfSight(new MapPoint(0.0, 0.0, SpatialReferences.Wgs84), new MapPoint(0.0, 0.0, SpatialReferences.Wgs84));

            // Set the visible and obstructed colors (default would be green/red)
            // These are static properties that apply to all line of sight analyses in the scene view
            LineOfSight.VisibleColor = Color.Cyan;
            LineOfSight.ObstructedColor = Color.Magenta;
            
            // Create an analysis overlay to contain the analysis and add it to the scene view
            AnalysisOverlay lineOfSightOverlay = new AnalysisOverlay();
            lineOfSightOverlay.Analyses.Add(_lineOfSightAnalysis);
            MySceneView.AnalysisOverlays.Add(lineOfSightOverlay);
        }

        private void SceneViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            // Ignore if tapped out of bounds (e.g. the sky).
            if (e.Location == null)
            {
                return;
            }

            // When the view is tapped, define the observer or target location with the tap point as appropriate
            if (_observerLocation == null)
            {
                // Define the observer location (plus an offset for observer height) and set the target to the same point
                _observerLocation = new MapPoint(e.Location.X, e.Location.Y, e.Location.Z + _zOffset);
                _lineOfSightAnalysis.ObserverLocation = _observerLocation;
                _lineOfSightAnalysis.TargetLocation = _observerLocation;

                // Clear the target location (if any) so the next click will define the target
                _targetLocation = null;
            }
            else if (_targetLocation == null)
            {
                // Define the target
                _targetLocation = new MapPoint(e.Location.X, e.Location.Y, e.Location.Z + _zOffset);
                _lineOfSightAnalysis.TargetLocation = _targetLocation;

                // Clear the observer location so it can be defined again
                _observerLocation = null;
            }
        }
    }
}
