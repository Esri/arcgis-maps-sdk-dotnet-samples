// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Diagnostics;
using System.Drawing;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.GetElevationAtPoint
{
    [Register("GetElevationAtPoint")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Get elevation at a point",
        category: "Scene",
        description: "Get the elevation for a given point on a surface in a scene.",
        instructions: "Tap anywhere on the surface to get the elevation at that point. Elevation is reported in meters since the scene view is in WGS84.",
        tags: new[] { "elevation", "point", "surface" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class GetElevationAtPoint : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;

        // URL of the elevation service - provides elevation component of the scene.
        private readonly Uri _elevationUri = new Uri("http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

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
            Title = "Get elevation at a point";
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
                Debug.WriteLine(ex.Message);
                CreateErrorDialog(ex.Message);
            }
            finally
            {
                // Re-add to the event handler.
                _mySceneView.GeoViewTapped += SceneViewTapped;
            }
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            UILabel helpLabel = new UILabel
            {
                Text = "Tap to find the elevation for a point.",
                BackgroundColor = UIColor.FromWhiteAlpha(0f, .6f),
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_mySceneView, helpLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]{
                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                helpLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                helpLabel.HeightAnchor.ConstraintEqualTo(40)
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Handle taps on the scene view for getting elevation.
            _mySceneView.GeoViewTapped += SceneViewTapped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from event.
            _mySceneView.GeoViewTapped -= SceneViewTapped;
        }

        private void CreateErrorDialog(string message)
        {
            // Create Alert.
            UIAlertController okAlertController = UIAlertController.Create("Error", message, UIAlertControllerStyle.Alert);

            // Add Action.
            okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            // Present Alert.
            PresentViewController(okAlertController, true, null);
        }
    }
}