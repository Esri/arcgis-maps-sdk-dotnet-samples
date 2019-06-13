// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Drawing;
using System.Windows;
using Point = System.Windows.Point;

namespace ArcGISRuntime.WPF.Samples.GetElevationAtAPoint
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Get elevation at a point",
        "Map",
        "",
        "")]
    public partial class GetElevationAtAPoint
    {
        // URL of the elevation service - provides elevation component of the scene
        private readonly Uri _elevationUri = new Uri("http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

        // Starting point of the observation point
        private readonly MapPoint _observerPoint = new MapPoint(83.9, 28.42, SpatialReferences.Wgs84);

        // Create the graphics overlay
        private readonly GraphicsOverlay _overlay = new GraphicsOverlay();

        public GetElevationAtAPoint()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization.
            Initialize();

            // Handle taps on the scene view to define the observer or target point for the line of sight
            MySceneView.GeoViewTapped += SceneViewTapped;
        }

        private async void Initialize()
        {
            try
            {
                Camera camera = new Camera(_observerPoint, 30000.0, 10.0, 80.0, 0.0);

                // Create scene
                Scene myScene = new Scene(Basemap.CreateImageryWithLabels())
                {
                    // Set initial viewpoint
                    InitialViewpoint = new Viewpoint(_observerPoint, 1000000, camera)
                };

                // add base surface for elevation data
                myScene.BaseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(_elevationUri));

                MySceneView.GraphicsOverlays.Add(_overlay);

                // Add the scene to the view
                MySceneView.Scene = myScene;

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        private void SceneViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            // get the clicked screenPoint
            Point screenPoint = new Point(e.Position.X, e.Position.Y);
            // convert the screen point to a point on the surface
            MapPoint relativeSurfacePoint = MySceneView.ScreenToBaseSurface(screenPoint);

            // check that the point is on the surface
            if (relativeSurfacePoint != null)
            { 
                // clear any existing graphics from the graphics overlay
                _overlay.Graphics.Clear();

                SimpleMarkerSceneSymbol _viewpointSymbol = SimpleMarkerSceneSymbol.CreateCylinder(Color.Blue, 10, 750);

                _overlay.Graphics.Add(new Graphic(relativeSurfacePoint, _viewpointSymbol));

            }
        }

    }
}