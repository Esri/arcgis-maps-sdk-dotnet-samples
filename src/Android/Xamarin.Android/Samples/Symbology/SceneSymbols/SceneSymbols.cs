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

namespace ArcGISRuntimeXamarin.Samples.SceneSymbols
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Scene symbols",
        category: "Symbology",
        description: "Show various kinds of 3D symbols in a scene.",
        instructions: "When the scene loads, note the different types of 3D symbols that you can create.",
        tags: new[] { "3D", "cone", "cube", "cylinder", "diamond", "geometry", "graphic", "graphics overlay", "pyramid", "scene", "shape", "sphere", "symbol", "tetrahedron", "tube", "visualization" })]
    public class SceneSymbols : Activity
    {
        // Hold a reference to the scene view.
        private SceneView _mySceneView;
        
        private readonly string _elevationServiceUrl = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Scene symbols";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Configure the scene with an imagery basemap.
            _mySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Add a surface to the scene for elevation.
            ArcGISTiledElevationSource elevationSource = new ArcGISTiledElevationSource(new Uri(_elevationServiceUrl));
            Surface elevationSurface = new Surface();
            elevationSurface.ElevationSources.Add(elevationSource);
            _mySceneView.Scene.BaseSurface = elevationSurface;

            // Create the graphics overlay.
            GraphicsOverlay overlay = new GraphicsOverlay();

            // Set the surface placement mode for the overlay.
            overlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;

            // Create a graphic for each symbol type and add it to the scene.
            int index = 0;
            Color[] colors = {Color.Red, Color.Green, Color.Blue, Color.Purple, Color.Turquoise, Color.White};
            Array symbolStyles = Enum.GetValues(typeof(SimpleMarkerSceneSymbolStyle));
            foreach (SimpleMarkerSceneSymbolStyle symbolStyle in symbolStyles)
            {
                // Create the symbol.
                SimpleMarkerSceneSymbol symbol = new SimpleMarkerSceneSymbol(symbolStyle, colors[index], 200, 200, 200, SceneSymbolAnchorPosition.Center);

                // Offset each symbol so that they aren't in the same spot.
                double positionOffset = 0.01 * index;
                MapPoint point = new MapPoint(44.975 + positionOffset, 29, 500, SpatialReferences.Wgs84);

                // Create the graphic from the geometry and the symbol.
                Graphic item = new Graphic(point, symbol);

                // Add the graphic to the overlay.
                overlay.Graphics.Add(item);

                // Increment the index.
                index++;
            }

            // Show the graphics overlay in the scene.
            _mySceneView.GraphicsOverlays.Add(overlay);

            // Set the initial viewpoint.
            Camera initalViewpoint = new Camera(28.9672, 44.9858, 2495, 12, 53, 0);
            _mySceneView.SetViewpointCamera(initalViewpoint);
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout.
            _mySceneView = new SceneView(this);
            layout.AddView(_mySceneView);

            // Show the layout in the app.
            SetContentView(layout);
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
