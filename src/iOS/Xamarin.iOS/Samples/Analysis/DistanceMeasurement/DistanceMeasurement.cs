// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using CoreGraphics;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using Foundation;
using System;
using System.Diagnostics;
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
        // UI controls.
        private SceneView _mySceneView;
        private UILabel _helpLabel;
        private UILabel _resultLabel;
        private UIToolbar _resultArea;
        private UIButton _unitChangeButton;

        // URLs to various services used to provide an interesting scene for the sample.
        private readonly Uri _buildingService =
            new Uri(
                "http://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Brest/SceneServer/layers/0");

        private readonly Uri _worldElevationService =
            new Uri("http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

        // Reference to the measurement used.
        private LocationDistanceMeasurement _distanceMeasurement;

        public DistanceMeasurement()
        {
            Title = "Distance measurement analysis";
        }

        private void Initialize()
        {
            // Create a scene with elevation.
            var sceneSurface = new Surface();
            sceneSurface.ElevationSources.Add(new ArcGISTiledElevationSource(_worldElevationService));
            var myScene = new Scene(Basemap.CreateImagery())
            {
                BaseSurface = sceneSurface
            };

            // Create and add a building layer.
            var buildingsLayer = new ArcGISSceneLayer(_buildingService);
            myScene.OperationalLayers.Add(buildingsLayer);

            // Create and add an analysis overlay.
            var measureAnalysisOverlay = new AnalysisOverlay();
            _mySceneView.AnalysisOverlays.Add(measureAnalysisOverlay);

            // Create an initial distance measurement and show it.
            var start = new MapPoint(-4.494677, 48.384472, 24.772694, SpatialReferences.Wgs84);
            var end = new MapPoint(-4.495646, 48.384377, 58.501115, SpatialReferences.Wgs84);
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

        private void CreateLayout()
        {
            // Create the scene view.
            _mySceneView = new SceneView();

            // Create the help label.
            _helpLabel = new UILabel
            {
                TextColor = UIColor.Red,
                Text = "Tap to update.",
                TextAlignment = UITextAlignment.Center
            };

            // Create the result label.
            _resultLabel = new UILabel
            {
                TextColor = View.TintColor,
                TextAlignment = UITextAlignment.Center
            };

            // Create the result toolbar.
            _resultArea = new UIToolbar();

            // Create the unit change button.
            _unitChangeButton = new UIButton();
            _unitChangeButton.SetTitle("Unit systems", UIControlState.Normal);
            _unitChangeButton.BackgroundColor = View.TintColor;
            _unitChangeButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            _unitChangeButton.Layer.CornerRadius = 10;
            _unitChangeButton.ClipsToBounds = true;

            // Handle the unit change button press.
            _unitChangeButton.TouchUpInside += UnitChangeButton_TouchUpInside;

            // Add views to the page.
            View.AddSubviews(_mySceneView, _resultArea, _helpLabel, _resultLabel, _unitChangeButton);
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

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            var topMargin = NavigationController.NavigationBar.Frame.Height +
                            UIApplication.SharedApplication.StatusBarFrame.Height + 10;
            nfloat toolbarHeight = 30;

            // Place the scene view and update the insets to avoid hiding view elements like the attribution bar.
            _mySceneView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _mySceneView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);

            // Place the help label.
            _helpLabel.Frame = new CGRect(0, topMargin + 10, View.Bounds.Width, toolbarHeight);

            // Place the result toolbar.
            _resultArea.Frame = new CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);

            // Place the result label.
            _resultLabel.Frame = new CGRect(10, View.Bounds.Height - toolbarHeight + 5, View.Bounds.Width - 20,
                toolbarHeight - 10);

            // Place the unit system change button.
            _unitChangeButton.Frame = new CGRect(View.Bounds.Width / 4, View.Bounds.Height - (3 * toolbarHeight),
                View.Bounds.Width / 2, toolbarHeight);

            base.ViewDidLayoutSubviews();
        }
    }
}