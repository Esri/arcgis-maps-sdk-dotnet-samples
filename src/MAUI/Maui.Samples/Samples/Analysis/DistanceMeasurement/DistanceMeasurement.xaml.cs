// Copyright 2023 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Maui;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using Microsoft.Maui.ApplicationModel;
using System.Diagnostics;

namespace ArcGIS.Samples.DistanceMeasurement
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Distance measurement analysis",
        category: "Analysis",
        description: "Measure distances between two points in 3D.",
        instructions: "Choose a unit system for the measurement. Tap any location in the scene to start measuring. Move the mouse to an end location, and tap to complete the measurement. Tap a new location to clear and start a new measurement.",
        tags: new[] { "3D", "analysis", "distance", "measure" })]
    public partial class DistanceMeasurement : ContentPage
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

            // Create the UI, setup the control references and execute initialization.
            Initialize();
        }

        private void Initialize()
        {
            // Create a scene with elevation.
            Surface sceneSurface = new Surface();
            sceneSurface.ElevationSources.Add(new ArcGISTiledElevationSource(_worldElevationService));
            Scene myScene = new Scene(BasemapStyle.ArcGISTopographic)
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
            _distanceMeasurement.MeasurementChanged += (o, e) =>
            {
                // This is needed because measurement change events occur on a non-UI thread and this code accesses UI object.
                Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Update the labels with new values in the format {value} {unit system}.
                    DirectMeasureLabel.Text =
                        $"{_distanceMeasurement.DirectDistance.Value:F} {_distanceMeasurement.DirectDistance.Unit.Abbreviation}";
                    VerticalMeasureLabel.Text =
                        $"{_distanceMeasurement.VerticalDistance.Value:F} {_distanceMeasurement.VerticalDistance.Unit.Abbreviation}";
                    HorizontalMeasureLabel.Text =
                        $"{_distanceMeasurement.HorizontalDistance.Value:F} {_distanceMeasurement.HorizontalDistance.Unit.Abbreviation}";
                });
            };

            // Configure the unit system selection box.
            UnitSystemCombo.ItemsSource = Enum.GetValues(typeof(UnitSystem));
            UnitSystemCombo.SelectedItem = _distanceMeasurement.UnitSystem;

            // Update the unit system selection.
            UnitSystemCombo.SelectedIndexChanged += (sender, args) =>
            {
                _distanceMeasurement.UnitSystem = (UnitSystem)UnitSystemCombo.SelectedItem;
            };

            // Show the scene in the view.
            MySceneView.Scene = myScene;
            MySceneView.SetViewpointCamera(new Camera(start, 200, 0, 45, 0));

            // Subscribe to tap events to enable updating the measurement.
            MySceneView.GeoViewTapped += MySceneView_GeoViewTapped;
        }

        private async void MySceneView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Get the geographic location for the current mouse position.
                MapPoint geoPoint = await MySceneView.ScreenToLocationAsync(e.Position);

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