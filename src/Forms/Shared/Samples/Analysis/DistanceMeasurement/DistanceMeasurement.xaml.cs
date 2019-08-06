// Copyright 2018 Esri.
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
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using Esri.ArcGISRuntime.Xamarin.Forms;
using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.DistanceMeasurement
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Distance measurement analysis",
        "Analysis",
        "This sample demonstrates measuring 3D distances between two points in a scene. The distance measurement analysis allows you to add the same measuring experience found in ArcGIS Pro, City Engine, and the ArcGIS API for JavaScript to your app. You can set the unit system of measurement (metric or imperial) and have the units automatically switch to one appropriate for the current scale. The rendering is handled internally so they do not interfere with other analyses like viewsheds.",
        "Tap to set a new end point for the measurement.",
        "Featured")]
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
            Scene myScene = new Scene(Basemap.CreateImagery())
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
                Device.BeginInvokeOnMainThread(() =>
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
                _distanceMeasurement.UnitSystem = (UnitSystem) UnitSystemCombo.SelectedItem;
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