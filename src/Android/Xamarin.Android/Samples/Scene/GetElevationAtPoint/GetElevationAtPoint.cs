// Copyright 2019 Esri.
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
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Drawing;
using Surface = Esri.ArcGISRuntime.Mapping.Surface;

namespace ArcGISRuntimeXamarin.Samples.GetElevationAtPoint
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Get elevation at a point",
        category: "Scene",
        description: "Get the elevation for a given point on a surface in a scene.",
        instructions: "Tap anywhere on the surface to get the elevation at that point. Elevation is reported in meters since the scene view is in WGS84.",
        tags: new[] { "elevation", "point", "surface" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class GetElevationAtPoint : Activity
    {
        // Hold references to the UI controls.
        private SceneView _mySceneView;

        // URL of the elevation service - provides elevation component of the scene.
        private readonly Uri _elevationUri = new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

        // Starting point of the observer.
        private readonly MapPoint _observerPoint = new MapPoint(83.9, 28.42, SpatialReferences.Wgs84);

        // Graphics overlay.
        private GraphicsOverlay _overlay;

        // Surface (for elevation).
        private Surface _baseSurface;

        // Create symbols for the text and marker.
        private SimpleMarkerSceneSymbol _elevationMarker;
        private TextSymbol _elevationTextSymbol;
        private readonly Graphic _elevationTextGraphic = new Graphic();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Get elevation at a point";

            CreateLayout();
            Initialize();

            _mySceneView.GeoViewTapped += SceneViewTapped;
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a TextView for instructions.
            TextView sampleInstructionsTextView = new TextView(this)
            {
                Text = "Tap to find the elevation for a point."
            };
            layout.AddView(sampleInstructionsTextView);

            // Add the map view to the layout.
            _mySceneView = new SceneView(this);
            layout.AddView(_mySceneView);

            // Show the layout in the app.
            SetContentView(layout);
        }

        private void Initialize()
        {
            // Create the camera for the scene.
            Camera camera = new Camera(_observerPoint, 20000.0, 10.0, 70.0, 0.0);

            // Create a scene.
            Scene myScene = new Scene(Basemap.CreateImageryWithLabels())
            {
                // Set the initial viewpoint.
                InitialViewpoint = new Viewpoint(_observerPoint, 1000000, camera)
            };

            // Create the marker for showing where the user taps.
            _elevationMarker = SimpleMarkerSceneSymbol.CreateCylinder(Color.Red, 10, 750);

            // Create the text for displaying the elevation value.
            _elevationTextSymbol = new TextSymbol("", Color.Red, 20, Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center, Esri.ArcGISRuntime.Symbology.VerticalAlignment.Middle);
            _elevationTextGraphic.Symbol = _elevationTextSymbol;

            // Create the base surface.
            _baseSurface = new Surface();
            _baseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(_elevationUri));

            // Add the base surface to the scene.
            myScene.BaseSurface = _baseSurface;

            // Graphics overlay for displaying points.
            _overlay = new GraphicsOverlay
            {
                SceneProperties = new LayerSceneProperties(SurfacePlacement.Absolute)
            };
            _mySceneView.GraphicsOverlays.Add(_overlay);

            // Add the scene to the view.
            _mySceneView.Scene = myScene;
        }

        private async void SceneViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            try
            {
                // Remove this method from the event handler to prevent concurrent calls.
                _mySceneView.GeoViewTapped -= SceneViewTapped;

                // Check that the point is on the surface.
                if (e.Location != null)
                {
                    // Clear any existing graphics from the graphics overlay.
                    _overlay.Graphics.Clear();

                    // Get the elevation value.
                    double elevation = await _baseSurface.GetElevationAsync(e.Location);

                    // Set the text displaying the elevation.
                    _elevationTextSymbol.Text = $"{Math.Round(elevation)} m";
                    _elevationTextGraphic.Geometry = new MapPoint(e.Location.X, e.Location.Y, e.Location.Z + 850);

                    // Add the text to the graphics overlay.
                    _overlay.Graphics.Add(_elevationTextGraphic);

                    // Add the marker indicating where the user tapped.
                    _overlay.Graphics.Add(new Graphic(e.Location, _elevationMarker));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                CreateErrorDialog(ex.Message);
            }
            finally
            {
                // Re-add to the event handler.
                _mySceneView.GeoViewTapped += SceneViewTapped;
            }
        }

        private void CreateErrorDialog(string message)
        {
            // Create a dialog to show message to user.
            AlertDialog alert = new AlertDialog.Builder(this).Create();
            alert.SetMessage(message);
            alert.Show();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Remove the sceneview
            (_mySceneView.Parent as ViewGroup).RemoveView(_mySceneView);
            _mySceneView.Dispose();
            _mySceneView = null;
        }
    }
}