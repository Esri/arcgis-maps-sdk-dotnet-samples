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
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.GeoAnalysis;

namespace ArcGISRuntime.WinUI.Samples.DistanceMeasurement
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Distance measurement analysis",
        category: "Analysis",
        description: "Measure distances between two points in 3D.",
        instructions: "Choose a unit system for the measurement. Click any location in the scene to start measuring. Move the mouse to an end location, and click to complete the measurement. Click a new location to clear and start a new measurement.",
        tags: new[] { "3D", "analysis", "distance", "measure" })]
    public partial class DistanceMeasurement
    {
        // URLs to various services used to provide an interesting scene for the sample.
        private readonly Uri _buildingService =
            new Uri(
                "https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Brest/SceneServer/layers/0");

        private readonly Uri _worldElevationService =
            new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

        // Reference to the measurement used.
        private LocationDistanceMeasurement _distanceMeasurement;

        public DistanceMeasurement()
        {
            InitializeComponent();

            // Setup the control references and execute initialization.
            Initialize();
        }

        private void Initialize()
        {
            // Create a scene with elevation.
            Surface sceneSurface = new Surface();
            sceneSurface.ElevationSources.Add(new ArcGISTiledElevationSource(_worldElevationService));
            Scene myScene = new Scene(Basemap.CreateTopographic())
            {
                BaseSurface = sceneSurface
            };

            // Create and add a building layer.
            ArcGISSceneLayer buildingsLayer = new ArcGISSceneLayer(_buildingService);
            myScene.OperationalLayers.Add(buildingsLayer);

            // Create and add an analysis overlay.
            AnalysisOverlay measureAnalysisOverlay = new AnalysisOverlay();
            MySceneView.AnalysisOverlays.Add(measureAnalysisOverlay);

            // Create an initial distance measurement and show it.
            MapPoint start = new MapPoint(-4.494677, 48.384472, 24.772694, SpatialReferences.Wgs84);
            MapPoint end = new MapPoint(-4.495646, 48.384377, 58.501115, SpatialReferences.Wgs84);
            _distanceMeasurement = new LocationDistanceMeasurement(start, end);
            measureAnalysisOverlay.Analyses.Add(_distanceMeasurement);
            
            // Keep the UI updated.
            _distanceMeasurement.MeasurementChanged += async (o, e) =>
            {
                // This is needed because measurement change events occur on a non-UI thread and this code accesses UI object.
                DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                {
                    // Update the labels with new values in the format {value} {unit system}.
                    DirectMeasureTextBlock.Text = $"{_distanceMeasurement.DirectDistance.Value:F} {_distanceMeasurement.DirectDistance.Unit.Abbreviation}";
                    VerticalMeasureTextBlock.Text = $"{_distanceMeasurement.VerticalDistance.Value:F} {_distanceMeasurement.VerticalDistance.Unit.Abbreviation}";
                    HorizontalMeasureTextBlock.Text = $"{_distanceMeasurement.HorizontalDistance.Value:F} {_distanceMeasurement.HorizontalDistance.Unit.Abbreviation}";
                });
            };
            
            // Configure the unit system selection box.
            UnitSystemCombo.ItemsSource = Enum.GetValues(typeof(UnitSystem)).OfType<UnitSystem>().Select(e => e.ToString());
            UnitSystemCombo.SelectedItem = _distanceMeasurement.UnitSystem;

            // Update the unit system selection.
            UnitSystemCombo.SelectionChanged += (sender, args) =>
            {
                _distanceMeasurement.UnitSystem = Enum.Parse<UnitSystem>(UnitSystemCombo.SelectedItem as string);
            };
            
            // Show the scene in the view.
            MySceneView.Scene = myScene;
            MySceneView.SetViewpointCamera(new Camera(start, 200, 0, 45, 0));
            
            // Enable the 'New measurement' button.
            NewMeasureButton.IsEnabled = true;
        }

        private void MySceneView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Unsubscribe from events to finish the measurement.
            MySceneView.PointerMoved -= MySceneView_PointerMoved;
            MySceneView.GeoViewTapped -= MySceneView_GeoViewTapped;

            // Re-enable the new measurement button.
            NewMeasureButton.IsEnabled = true;

            // Re-set the help label.
            HelpTextBlock.Text = "Tap 'New measurement' to start.";
        }

        private void NewMeasureButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Subscribe to mouse move to change the measurement.
            MySceneView.PointerMoved += MySceneView_PointerMoved;

            // Subscribe to the tap method to enable finishing a measurement.
            MySceneView.GeoViewTapped += MySceneView_GeoViewTapped;

            // Disable the button.
            NewMeasureButton.IsEnabled = false;

            // Update the help label.
            HelpTextBlock.Text = "Move the mouse to update the end point. Tap again to finish.";
        }

        private async void MySceneView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                // Get the geographic location for the current mouse position.
                MapPoint geoPoint = await MySceneView.ScreenToLocationAsync(e.GetCurrentPoint(MySceneView).Position);

                if (geoPoint == null) return;

                // Update the location distance measurement.
                _distanceMeasurement.EndLocation = geoPoint;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}