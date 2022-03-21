// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using CoreGraphics;
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
        name: "Surface placement",
        category: "GraphicsOverlay",
        description: "Position graphics relative to a surface using different surface placement modes.",
        instructions: "The application loads a scene showing four points that use individual surface placement modes (Absolute, Relative, Relative to Scene, and either Draped Billboarded or Draped Flat). Use the toggle to change the draped mode and the slider to dynamically adjust the Z value of the graphics. Explore the scene by zooming in/out and by panning around to observe the effects of the surface placement rules.",
        tags: new[] { "3D", "absolute", "altitude", "draped", "elevation", "floating", "relative", "scenes", "sea level", "surface placement" })]
    public class SurfacePlacements : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;
        private UISegmentedControl _picker;
        private UISlider _zSlider;
        private UILabel _valueLabel;

        // Draped overlays.
        private GraphicsOverlay _drapedBillboardedOverlay;
        private GraphicsOverlay _drapedFlatOverlay;

        public SurfacePlacements()
        {
            Title = "Surface placement";
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
            _mySceneView.Scene = myScene;

            // Create ElevationSource from elevation data Uri.
            ArcGISTiledElevationSource elevationSource = new ArcGISTiledElevationSource(
                new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));

            // Create scene layer from the Brest, France scene server.
            var sceneLayer = new ArcGISSceneLayer(new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Brest/SceneServer"));
            _mySceneView.Scene.OperationalLayers.Add(sceneLayer);

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

            GraphicsOverlay relativeToSceneOverlay = new GraphicsOverlay();
            relativeToSceneOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.RelativeToScene;
            _mySceneView.GraphicsOverlays.Add(relativeToSceneOverlay);

            GraphicsOverlay relativeToSurfaceOverlay = new GraphicsOverlay();
            relativeToSurfaceOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            _mySceneView.GraphicsOverlays.Add(relativeToSurfaceOverlay);

            GraphicsOverlay absoluteOverlay = new GraphicsOverlay();
            absoluteOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            _mySceneView.GraphicsOverlays.Add(absoluteOverlay);

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

        private void ZValueChanged(object sender, EventArgs e)
        {
            foreach (GraphicsOverlay overlay in _mySceneView.GraphicsOverlays)
            {
                foreach (Graphic graphic in overlay.Graphics)
                {
                    graphic.Geometry = GeometryEngine.SetZ(graphic.Geometry, _zSlider.Value) ;
                }
            }

            _valueLabel.Text = $"{_zSlider.Value} meters";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            _picker = new UISegmentedControl("Draped Billboarded", "Draped Flat");
            _picker.SelectedSegment = 0;
            _picker.TranslatesAutoresizingMaskIntoConstraints = false;

            _zSlider = new UISlider() { TranslatesAutoresizingMaskIntoConstraints = false };
            _zSlider.MinValue = 0;
            _zSlider.MaxValue = 140;
            _zSlider.SetValue(70, false);

            _valueLabel = new UILabel { Text = "70 meters", TranslatesAutoresizingMaskIntoConstraints = false };

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem(){ CustomView = _picker},
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
            };

            UIToolbar sliderToolbar = new UIToolbar();
            sliderToolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            sliderToolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem {CustomView = _zSlider, Width = 200},
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem {CustomView = _valueLabel, Width = 150 },
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
            };

            // Add the views.
            View.AddSubviews(_mySceneView, toolbar, sliderToolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(sliderToolbar.TopAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                sliderToolbar.TopAnchor.ConstraintEqualTo(_mySceneView.BottomAnchor),
                sliderToolbar.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),
                sliderToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                sliderToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                toolbar.TopAnchor.ConstraintEqualTo(sliderToolbar.BottomAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _picker.ValueChanged += ChangeDraped;
            _zSlider.ValueChanged += ZValueChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _picker.ValueChanged -= ChangeDraped;
            _zSlider.ValueChanged -= ZValueChanged;
        }
    }
}