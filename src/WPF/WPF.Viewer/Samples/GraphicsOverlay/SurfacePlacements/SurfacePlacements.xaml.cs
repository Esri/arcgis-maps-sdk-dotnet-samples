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
using System.Drawing;

namespace ArcGIS.WPF.Samples.SurfacePlacements
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Surface placement",
        category: "GraphicsOverlay",
        description: "Position graphics relative to a surface using different surface placement modes.",
        instructions: "The application loads a scene showing four points that use individual surface placement modes (Absolute, Relative, Relative to Scene, and either Draped Billboarded or Draped Flat). Use the toggle to change the draped mode and the slider to dynamically adjust the Z value of the graphics. Explore the scene by zooming in/out and by panning around to observe the effects of the surface placement rules.",
        tags: new[] { "3D", "absolute", "altitude", "draped", "elevation", "floating", "relative", "scenes", "sea level", "surface placement" })]
    public partial class SurfacePlacements
    {
        private GraphicsOverlay _drapedBillboardedOverlay;
        private GraphicsOverlay _drapedFlatOverlay;

        public SurfacePlacements()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Scene.
            Scene myScene = new Scene
            {
                // Set the Scene's basemap property.
                Basemap = new Basemap(BasemapStyle.ArcGISImageryStandard)
            };

            // Create a camera with coordinates showing layer data.
            Camera camera = new Camera(48.389124348393182, -4.4595173327138591, 140, 322, 74, 0);

            // Assign the Scene to the SceneView.
            MySceneView.Scene = myScene;

            // Create ElevationSource from elevation data Uri.
            ArcGISTiledElevationSource elevationSource = new ArcGISTiledElevationSource(
                new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));

            // Create scene layer from the Brest, France scene server.
            var sceneLayer = new ArcGISSceneLayer(new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Brest/SceneServer"));
            MySceneView.Scene.OperationalLayers.Add(sceneLayer);

            // Add elevationSource to BaseSurface's ElevationSources.
            MySceneView.Scene.BaseSurface.ElevationSources.Add(elevationSource);

            // Set view point of scene view using camera.
            MySceneView.SetViewpointCameraAsync(camera);

            // Create overlays with elevation modes.
            _drapedBillboardedOverlay = new GraphicsOverlay();
            _drapedBillboardedOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.DrapedBillboarded;
            MySceneView.GraphicsOverlays.Add(_drapedBillboardedOverlay);

            _drapedFlatOverlay = new GraphicsOverlay();
            _drapedFlatOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.DrapedFlat;

            GraphicsOverlay relativeToSceneOverlay = new GraphicsOverlay();
            relativeToSceneOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.RelativeToScene;
            MySceneView.GraphicsOverlays.Add(relativeToSceneOverlay);

            GraphicsOverlay relativeToSurfaceOverlay = new GraphicsOverlay();
            relativeToSurfaceOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            MySceneView.GraphicsOverlays.Add(relativeToSurfaceOverlay);

            GraphicsOverlay absoluteOverlay = new GraphicsOverlay();
            absoluteOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            MySceneView.GraphicsOverlays.Add(absoluteOverlay);

            // Create point for graphic location.
            MapPoint sceneRelatedPoint = new MapPoint(-4.4610562, 48.3902727, 70, camera.Location.SpatialReference);
            MapPoint surfaceRelatedPoint = new MapPoint(-4.4609257, 48.3903965, 70, camera.Location.SpatialReference);

            // Create a red triangle symbol.
            var triangleSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, Color.FromArgb(255, 255, 0, 0), 10);

            // Create a text symbol for each elevation mode.
            TextSymbol drapedBillboardedText = new TextSymbol("DRAPED BILLBOARDED", Color.FromArgb(255, 0, 0, 255), 10,
                Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center,
                Esri.ArcGISRuntime.Symbology.VerticalAlignment.Middle);
            drapedBillboardedText.OffsetY += 20;

            TextSymbol drapedFlatText = new TextSymbol("DRAPED FLAT", Color.FromArgb(255, 0, 0, 255), 10,
                Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center,
                Esri.ArcGISRuntime.Symbology.VerticalAlignment.Middle);
            drapedFlatText.OffsetY += 20;

            TextSymbol relativeToSurfaceText = new TextSymbol("RELATIVE TO SURFACE", Color.FromArgb(255, 0, 0, 255), 10,
                Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center,
                Esri.ArcGISRuntime.Symbology.VerticalAlignment.Middle);
            relativeToSurfaceText.OffsetY += 20;

            TextSymbol relativeToSceneText = new TextSymbol("RELATIVE TO SCENE", Color.FromArgb(255, 0, 0, 255), 10,
                Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center,
                Esri.ArcGISRuntime.Symbology.VerticalAlignment.Middle);
            relativeToSceneText.OffsetY -= 20;

            TextSymbol absoluteText = new TextSymbol("ABSOLUTE", Color.FromArgb(255, 0, 0, 255), 10,
                Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center,
                Esri.ArcGISRuntime.Symbology.VerticalAlignment.Middle);
            absoluteText.OffsetY += 20;

            // Add the point graphic and text graphic to the corresponding graphics overlay.
            _drapedBillboardedOverlay.Graphics.Add(new Graphic(surfaceRelatedPoint, triangleSymbol));
            _drapedBillboardedOverlay.Graphics.Add(new Graphic(surfaceRelatedPoint, drapedBillboardedText));

            _drapedFlatOverlay.Graphics.Add(new Graphic(surfaceRelatedPoint, triangleSymbol));
            _drapedFlatOverlay.Graphics.Add(new Graphic(surfaceRelatedPoint, drapedFlatText));

            relativeToSceneOverlay.Graphics.Add(new Graphic(sceneRelatedPoint, triangleSymbol));
            relativeToSceneOverlay.Graphics.Add(new Graphic(sceneRelatedPoint, relativeToSceneText));

            relativeToSurfaceOverlay.Graphics.Add(new Graphic(surfaceRelatedPoint, triangleSymbol));
            relativeToSurfaceOverlay.Graphics.Add(new Graphic(surfaceRelatedPoint, relativeToSurfaceText));

            absoluteOverlay.Graphics.Add(new Graphic(surfaceRelatedPoint, triangleSymbol));
            absoluteOverlay.Graphics.Add(new Graphic(surfaceRelatedPoint, absoluteText));

            BillboardButton.Checked += BillboardedClick;
            FlatButton.Checked += FlatClick;
        }

        private void BillboardedClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MySceneView.GraphicsOverlays.Remove(_drapedFlatOverlay);
            MySceneView.GraphicsOverlays.Add(_drapedBillboardedOverlay);
        }

        private void FlatClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MySceneView.GraphicsOverlays.Remove(_drapedBillboardedOverlay);
            MySceneView.GraphicsOverlays.Add(_drapedFlatOverlay);
        }

        private void ZValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            foreach (GraphicsOverlay overlay in MySceneView.GraphicsOverlays)
            {
                foreach (Graphic graphic in overlay.Graphics)
                {
                    graphic.Geometry = GeometryEngine.SetZ(graphic.Geometry, e.NewValue);
                }
            }
        }
    }
}