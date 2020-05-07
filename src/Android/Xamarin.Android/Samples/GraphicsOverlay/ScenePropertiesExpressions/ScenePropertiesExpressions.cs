// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System.Drawing;
using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;

namespace ArcGISRuntimeXamarin.Samples.ScenePropertiesExpressions
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Scene properties expressions",
        "GraphicsOverlay",
        "Update the orientation of a graphic using expressions based on its attributes.",
        "Adjust the heading and pitch sliders to rotate the cone.",
        "3D", "expression", "graphics", "heading", "pitch", "rotation", "scene", "symbology")]
    public class ScenePropertiesExpressions : Activity
    {
        // Hold reference to the used MapView.
        private SceneView _mySceneView;
        private SeekBar _headingSlider;
        private SeekBar _pitchSlider;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Scene properties expressions";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Set up the scene with an imagery basemap.
            _mySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Set the initial viewpoint for the scene.
            MapPoint point = new MapPoint(83.9, 28.4, 1000, SpatialReferences.Wgs84);
            Camera initialCamera = new Camera(point, 1000, 0, 50, 0);
            _mySceneView.SetViewpointCamera(initialCamera);

            // Create a graphics overlay.
            GraphicsOverlay overlay = new GraphicsOverlay();
            overlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            _mySceneView.GraphicsOverlays.Add(overlay);

            // Add a renderer using rotation expressions.
            SimpleRenderer renderer = new SimpleRenderer();
            renderer.SceneProperties.HeadingExpression = "[HEADING]";
            renderer.SceneProperties.PitchExpression = "[PITCH]";

            // Apply the renderer to the graphics overlay.
            overlay.Renderer = renderer;

            // Create a red cone graphic.
            SimpleMarkerSceneSymbol coneSymbol = SimpleMarkerSceneSymbol.CreateCone(Color.Red, 100, 100);
            coneSymbol.Pitch = -90;
            MapPoint conePoint = new MapPoint(83.9, 28.41, 200, SpatialReferences.Wgs84);
            Graphic cone = new Graphic(conePoint, coneSymbol);

            // Add the cone graphic to the overlay.
            overlay.Graphics.Add(cone);

            // Listen for changes in slider values and update graphic properties.
            _headingSlider.ProgressChanged += (sender, e) => { cone.Attributes["HEADING"] = _headingSlider.Progress; };
            _pitchSlider.ProgressChanged += (sender, e) => { cone.Attributes["PITCH"] = _pitchSlider.Progress - 90; };
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) {Orientation = Orientation.Vertical};

            _mySceneView = new SceneView(this);
            TextView headingLabel = new TextView(this);
            headingLabel.Text = "Heading:";
            _headingSlider = new SeekBar(this);
            _headingSlider.Min = 0;
            _headingSlider.Max = 360;
            TextView pitchLabel = new TextView(this);
            pitchLabel.Text = "Pitch:";
            _pitchSlider = new SeekBar(this);
            _pitchSlider.Min = 0;
            _pitchSlider.Max = 180;

            // Add the map view to the layout.
            layout.AddView(headingLabel);
            layout.AddView(_headingSlider);
            layout.AddView(pitchLabel);
            layout.AddView(_pitchSlider);
            layout.AddView(_mySceneView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}