// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Diagnostics;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.DistanceMeasurement
{
    [Register("DistanceMeasurement")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Distance measurement analysis",
        "Analysis",
        "This sample demonstrates measuring 3D distances between two points in a scene. The distance measurement analysis allows you to add the same measuring experience found in ArcGIS Pro, City Engine, and the ArcGIS API for JavaScript to your app. You can set the unit system of measurement (metric or imperial) and have the units automatically switch to one appropriate for the current scale. The rendering is handled internally so they do not interfere with other analyses like viewsheds.",
        "Tap to set a new end point for the measurement.",
        "Featured")]
    public class DistanceMeasurement : UIViewController
    {
        // Hold references to the UI controls.
        private SceneView _mySceneView;
        private UILabel _resultLabel;

        // URLs to various services used to provide an interesting scene for the sample.
        private readonly Uri _buildingService = new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Brest/SceneServer/layers/0");
        private readonly Uri _worldElevationService = new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

        // Reference to the measurement.
        private LocationDistanceMeasurement _distanceMeasurement;

        public DistanceMeasurement()
        {
            Title = "Distance measurement analysis";
        }

        private void Initialize()
        {
            // Create a scene with elevation.
            Surface sceneSurface = new Surface();
            sceneSurface.ElevationSources.Add(new ArcGISTiledElevationSource(_worldElevationService));
            Scene myScene = new Scene(Basemap.CreateImagery())
            {
                BaseSurface = sceneSurface
            };

            // Create and add a building layer.
            ArcGISSceneLayer buildingsLayer = new ArcGISSceneLayer(_buildingService);
            myScene.OperationalLayers.Add(buildingsLayer);

            // Create and add an analysis overlay.
            AnalysisOverlay measureAnalysisOverlay = new AnalysisOverlay();
            _mySceneView.AnalysisOverlays.Add(measureAnalysisOverlay);

            // Create an initial distance measurement and show it.
            MapPoint start = new MapPoint(-4.494677, 48.384472, 24.772694, SpatialReferences.Wgs84);
            MapPoint end = new MapPoint(-4.495646, 48.384377, 58.501115, SpatialReferences.Wgs84);
            _distanceMeasurement = new LocationDistanceMeasurement(start, end);
            measureAnalysisOverlay.Analyses.Add(_distanceMeasurement);
            _mySceneView.SetViewpointCamera(new Camera(start, 200, 45, 45, 0));

            // Keep the UI updated.
            _distanceMeasurement.MeasurementChanged += (o, e) =>
            {
                // This is needed because measurement change events occur on a non-UI thread and this code accesses UI object.
                BeginInvokeOnMainThread(() =>
                {
                    // Update the labels with new values in the format {value} {unit system}.
                    string direct =
                        $"{_distanceMeasurement.DirectDistance.Value:F} {_distanceMeasurement.DirectDistance.Unit.Abbreviation}";
                    string vertical =
                        $"{_distanceMeasurement.VerticalDistance.Value:F} {_distanceMeasurement.VerticalDistance.Unit.Abbreviation}";
                    string horizontal =
                        $"{_distanceMeasurement.HorizontalDistance.Value:F} {_distanceMeasurement.HorizontalDistance.Unit.Abbreviation}";
                    _resultLabel.Text = $"Direct: {direct}, V: {vertical}, H: {horizontal}";
                });
            };

            // Show the scene in the view.
            _mySceneView.Scene = myScene;

            // Subscribe to tap events to enable updating the measurement.
            _mySceneView.GeoViewTapped += MySceneView_GeoViewTapped;
        }

        private async void MySceneView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Get the geographic location for the current mouse position.
                MapPoint geoPoint = await _mySceneView.ScreenToLocationAsync(e.Position);

                if (geoPoint == null) return;

                // Update the location distance measurement.
                _distanceMeasurement.EndLocation = geoPoint;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void UnitChangeButton_TouchUpInside(object sender, EventArgs e)
        {
            // Create the view controller that will present the list of unit systems.
            UIAlertController unitSystemSelectionAlert =
                UIAlertController.Create("Change unit system", "", UIAlertControllerStyle.ActionSheet);

            // Needed to prevent a crash on iPad.
            UIPopoverPresentationController
                presentationPopover = unitSystemSelectionAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Show an option for each unit system.
            foreach (UnitSystem system in Enum.GetValues(typeof(UnitSystem)))
            {
                // Upon selecting a unit system, update the distance measure.
                unitSystemSelectionAlert.AddAction(UIAlertAction.Create(system.ToString(), UIAlertActionStyle.Default,
                    action => _distanceMeasurement.UnitSystem = system));
            }

            // Show the alert.
            PresentViewController(unitSystemSelectionAlert, true, null);
        }

        private void ShowHelp_Click(object sender, EventArgs e)
        {
            // Prompt for the type of convex hull to create.
            UIAlertController unionAlert = UIAlertController.Create("Tap to update", "Tap in the scene to set a new end point for the distance measurement.", UIAlertControllerStyle.Alert);
            unionAlert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));

            // Show the alert.
            PresentViewController(unionAlert, true, null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create and configure the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            _resultLabel = new UILabel
            {
                Text = "Tap to measure distance.",
                BackgroundColor = UIColor.FromWhiteAlpha(0f, .6f),
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            toolbar.Items = new[]
            {
                new UIBarButtonItem("Help", UIBarButtonItemStyle.Plain, ShowHelp_Click),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem("Change units", UIBarButtonItemStyle.Plain, UnitChangeButton_TouchUpInside)
            };

            // Add the views.
            View.AddSubviews(_mySceneView, toolbar, _resultLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new []
            {
                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                _resultLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _resultLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _resultLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _resultLabel.HeightAnchor.ConstraintEqualTo(40)
            });
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe to tap events. The view will never be disposed otherwise.
            _mySceneView.GeoViewTapped -= MySceneView_GeoViewTapped;
        }
    }
}