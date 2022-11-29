// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;

namespace ArcGIS.WPF.Samples.GetElevationAtPoint
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Get elevation at a point",
        category: "Scene",
        description: "Get the elevation for a given point on a surface in a scene.",
        instructions: "Tap anywhere on the surface to get the elevation at that point. Elevation is reported in meters since the scene view is in WGS84.",
        tags: new[] { "elevation", "point", "surface" })]
    public partial class GetElevationAtPoint
    {
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

        public GetElevationAtPoint()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization.
            Initialize();

            // Handle taps on the scene view for getting elevation.
            MySceneView.GeoViewTapped += SceneViewTapped;
        }

        private void Initialize()
        {
            // Create the camera for the scene.
            Camera camera = new Camera(_observerPoint, 20000.0, 10.0, 70.0, 0.0);

            // Create a scene.
            Scene myScene = new Scene(BasemapStyle.ArcGISImagery)
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
            MySceneView.GraphicsOverlays.Add(_overlay);

            // Add the scene to the view.
            MySceneView.Scene = myScene;
        }

        private async void SceneViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            try
            {
                // Remove this method from the event handler to prevent concurrent calls.
                MySceneView.GeoViewTapped -= SceneViewTapped;

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
                Debug.WriteLine(ex.Message);
                MessageBox.Show(ex.Message, "Sample error");
            }
            finally
            {
                // Re-add to the event handler.
                MySceneView.GeoViewTapped += SceneViewTapped;
            }
        }
    }
}