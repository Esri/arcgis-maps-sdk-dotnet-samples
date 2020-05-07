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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Drawing;
using UIKit;

namespace ArcGISRuntime.Samples.SurfacePlacements
{
    [Register("SurfacePlacements")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Surface placement",
        "GraphicsOverlay",
        "Position graphics relative to a surface using different surface placement modes.",
        "The application loads a scene showing three points that use the individual surface placement rules (Absolute, Relative, and either Draped Billboarded or Draped Flat). Use the control to toggle the draped mode, then explore the scene by zooming in/out and by panning around to observe the effects of the surface placement rules.",
        "3D", "absolute", "altitude", "draped", "elevation", "floating", "relative", "scenes", "sea level", "surface placement", "Featured")]
    public class SurfacePlacements : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;
        private UISegmentedControl _picker;

        // Draped overlays.
        private GraphicsOverlay _drapedBillboardedOverlay;
        private GraphicsOverlay _drapedFlatOverlay;

        public SurfacePlacements()
        {
            Title = "Surface placement";
        }

        private void Initialize()
        {
            // Create new scene with imagery basemap.
            Scene scene = new Scene(Basemap.CreateImagery());

            // Create a camera with coordinates showing layer data.
            Camera camera = new Camera(53.05, -4.01, 1115, 299, 88, 0);

            // Assign the Scene to the SceneView.
            _mySceneView.Scene = scene;

            // Create ElevationSource from elevation data Uri.
            ArcGISTiledElevationSource elevationSource = new ArcGISTiledElevationSource(
                new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));

            // Add elevationSource to BaseSurface's ElevationSources.
            _mySceneView.Scene.BaseSurface.ElevationSources.Add(elevationSource);

            // Set view point of scene view using camera.
            _mySceneView.SetViewpointCameraAsync(camera);

            // Create overlays with elevation modes.
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

            // Create point for graphic location.
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

        private void ChangeDraped(object sender, EventArgs e)
        {
            // Remove the current overlay.
            _mySceneView.GraphicsOverlays.Remove(_drapedFlatOverlay);
            _mySceneView.GraphicsOverlays.Remove(_drapedBillboardedOverlay);

            // Add the selected overlay.
            if (_picker.SelectedSegment == 0)
            {
                _mySceneView.GraphicsOverlays.Add(_drapedBillboardedOverlay);
            }
            else
            {
                _mySceneView.GraphicsOverlays.Add(_drapedFlatOverlay);
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
            View = new UIView() { BackgroundColor = UIColor.White };

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            _picker = new UISegmentedControl("Draped Billboarded", "Draped Flat");
            _picker.SelectedSegment = 0;
            _picker.TranslatesAutoresizingMaskIntoConstraints = false;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem(){ CustomView = _picker},
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
            };

            // Add the views.
            View.AddSubviews(_mySceneView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                toolbar.TopAnchor.ConstraintEqualTo(_mySceneView.BottomAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _picker.ValueChanged += ChangeDraped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _picker.ValueChanged += ChangeDraped;
        }
    }
}