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
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Drawing;

namespace ArcGISRuntime.Samples.SurfacePlacements
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Surface placement",
        "GraphicsOverlay",
        "Position graphics relative to a surface using different surface placement modes.",
        "The application loads a scene showing three points that use the individual surface placement rules (Absolute, Relative, and either Draped Billboarded or Draped Flat). Use the control to toggle the draped mode, then explore the scene by zooming in/out and by panning around to observe the effects of the surface placement rules.",
        "3D", "absolute", "altitude", "draped", "elevation", "floating", "relative", "scenes", "sea level", "surface placement", "Featured")]
    [Shared.Attributes.AndroidLayout("FindFeaturesUtilityNetwork.axml")]
    public class SurfacePlacements : Activity
    {
        // Hold references to UI elements.
        private SceneView _mySceneView;
        private RadioButton _billboardedButton;
        private RadioButton _flatButton;

        // Draped overlays.
        private GraphicsOverlay _drapedBillboardedOverlay;
        private GraphicsOverlay _drapedFlatOverlay;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Surface placement";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Scene
            Scene myScene = new Scene
            {

                // Set Scene's base map property
                Basemap = Basemap.CreateImagery()
            };

            // Create a camera with coordinates showing layer data
            Camera camera = new Camera(53.05, -4.01, 1115, 299, 88, 0);

            // Assign the Scene to the SceneView
            _mySceneView.Scene = myScene;

            // Create ElevationSource from elevation data Uri
            ArcGISTiledElevationSource elevationSource = new ArcGISTiledElevationSource(
                new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));

            // Add elevationSource to BaseSurface's ElevationSources
            _mySceneView.Scene.BaseSurface.ElevationSources.Add(elevationSource);

            // Set view point of scene view using camera
            _mySceneView.SetViewpointCameraAsync(camera);

            // Create overlays with elevation modes
            _drapedBillboardedOverlay = new GraphicsOverlay();
            _drapedBillboardedOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.DrapedBillboarded;
            _mySceneView.GraphicsOverlays.Add(_drapedBillboardedOverlay);

            _drapedFlatOverlay = new GraphicsOverlay();
            _drapedFlatOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.DrapedFlat;

            GraphicsOverlay relativeOverlay = new GraphicsOverlay();
            relativeOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            _mySceneView.GraphicsOverlays.Add(relativeOverlay);

            GraphicsOverlay absoluteOverlay = new GraphicsOverlay();
            absoluteOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            _mySceneView.GraphicsOverlays.Add(absoluteOverlay);

            // Create point for graphic location
            MapPoint point = new MapPoint(-4.04, 53.06, 1000, camera.Location.SpatialReference);

            // Create a red triangle symbol
            SimpleMarkerSymbol triangleSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, Color.FromArgb(255, 255, 0, 0), 10);

            // Create a text symbol for each elevation mode
            TextSymbol drapedBillboardedText = new TextSymbol("DRAPED BILLBOARDED", Color.FromArgb(255, 255, 255, 255), 10,
                HorizontalAlignment.Center,
                VerticalAlignment.Middle);
            drapedBillboardedText.OffsetY += 20;

            TextSymbol drapedFlatText = new TextSymbol("DRAPED FLAT", Color.FromArgb(255, 255, 255, 255), 10,
                HorizontalAlignment.Center,
                VerticalAlignment.Middle);
            drapedFlatText.OffsetY += 20;

            TextSymbol relativeText = new TextSymbol("RELATIVE", Color.FromArgb(255, 255, 255, 255), 10,
                HorizontalAlignment.Center,
                VerticalAlignment.Middle);
            relativeText.OffsetY += 20;

            TextSymbol absoluteText = new TextSymbol("ABSOLUTE", Color.FromArgb(255, 255, 255, 255), 10,
                HorizontalAlignment.Center,
                VerticalAlignment.Middle);
            absoluteText.OffsetY += 20;

            // Add the point graphic and text graphic to the corresponding graphics overlay
            _drapedBillboardedOverlay.Graphics.Add(new Graphic(point, triangleSymbol));
            _drapedBillboardedOverlay.Graphics.Add(new Graphic(point, drapedBillboardedText));

            _drapedFlatOverlay.Graphics.Add(new Graphic(point, triangleSymbol));
            _drapedFlatOverlay.Graphics.Add(new Graphic(point, drapedFlatText));

            relativeOverlay.Graphics.Add(new Graphic(point, triangleSymbol));
            relativeOverlay.Graphics.Add(new Graphic(point, relativeText));

            absoluteOverlay.Graphics.Add(new Graphic(point, triangleSymbol));
            absoluteOverlay.Graphics.Add(new Graphic(point, absoluteText));
        }

        private void SetBillboarded(object sender, EventArgs e)
        {
            _mySceneView.GraphicsOverlays.Remove(_drapedFlatOverlay);
            _mySceneView.GraphicsOverlays.Add(_drapedBillboardedOverlay);
        }

        private void SetFlat(object sender, EventArgs e)
        {
            _mySceneView.GraphicsOverlays.Remove(_drapedBillboardedOverlay);
            _mySceneView.GraphicsOverlays.Add(_drapedFlatOverlay);
        }

        private void CreateLayout()
        {
            // Create a new layout for the app.
            SetContentView(Resource.Layout.SurfacePlacements);

            _mySceneView = FindViewById<SceneView>(Resource.Id.SceneView);

            _billboardedButton = FindViewById<RadioButton>(Resource.Id.billboardedButton);
            _flatButton = FindViewById<RadioButton>(Resource.Id.flatButton);

            _billboardedButton.Click += SetBillboarded;
            _flatButton.Click += SetFlat;
        }
    }
}