// Copyright 2018 Esri.
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
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using System;
using System.Timers;
using Android.Icu.Util;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime;
using Debug = System.Diagnostics.Debug;

namespace ArcGISRuntime.Samples.DistanceMeasurement
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Distance measurement analysis",
        "Analysis",
        "This sample demonstrates measuring 3D distances between two points in a scene. The distance measurement analysis allows you to add the same measuring experience found in ArcGIS Pro, City Engine, and the ArcGIS API for JavaScript to your app. You can set the unit system of measurement (metric or imperial) and have the units automatically switch to one appropriate for the current scale. The rendering is handled internally so they do not interfere with other analyses like viewsheds.",
        "Tap to set a new end point for the measurement.",
        "Featured")]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("DistanceMeasurement.axml")]
    public class DistanceMeasurement : Activity
    {
        // Reference to UI controls.
        private TextView _directLabel;
        private TextView _verticalLabel;
        private TextView _horizontalLabel;
        private SceneView _mySceneView;
        private Spinner _unitSpinner;

        // URLs to various services used to provide an interesting scene for the sample.
        private readonly Uri _buildingService =
            new Uri(
                "http://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Brest/SceneServer/layers/0");

        private readonly Uri _localElevationService =
            new Uri("https://tiles.arcgis.com/tiles/d3voDfTFbHOCRwVR/arcgis/rest/services/MNT_IDF/ImageServer");

        private readonly Uri _worldElevationService =
            new Uri("http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

        // Reference to the measurement used.
        private LocationDistanceMeasurement _distanceMeasurement;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Distance measurement analysis";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a scene with elevation.
            var sceneSurface = new Surface();
            sceneSurface.ElevationSources.Add(new ArcGISTiledElevationSource(_worldElevationService));
            sceneSurface.ElevationSources.Add(new ArcGISTiledElevationSource(_localElevationService));
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
                RunOnUiThread(() =>
                {
                    // Update the label with new values in the format {value} {unit system}.
                    _directLabel.Text = $"{_distanceMeasurement.DirectDistance.Value:F} {_distanceMeasurement.DirectDistance.Unit.Abbreviation}";
                    _verticalLabel.Text = $"{_distanceMeasurement.VerticalDistance.Value:F} {_distanceMeasurement.VerticalDistance.Unit.Abbreviation}";
                    _horizontalLabel.Text = $"{_distanceMeasurement.HorizontalDistance.Value:F} {_distanceMeasurement.HorizontalDistance.Unit.Abbreviation}";
                });
            };

            // Update the unit spinner with the options.
            _unitSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, Enum.GetNames(typeof(UnitSystem)));
            _unitSpinner.ItemSelected += (sender, args) =>
            {
                UnitSystem[] values = (UnitSystem[])Enum.GetValues(typeof(UnitSystem));
                _distanceMeasurement.UnitSystem = values[args.Position];
            };

            // Show the scene in the view.
            _mySceneView.Scene = myScene;

            // Subscribe to tap events to enable updating the measurement.
            _mySceneView.GeoViewTapped += _mySceneView_GeoViewTapped;
        }

        private async void _mySceneView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
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
            // Load the layout for the sample from the .axml file.
            SetContentView(Resource.Layout.DistanceMeasurement);

            // Update control references to point to the controls defined in the layout
            _mySceneView = FindViewById<SceneView>(Resource.Id.distanceMeasurement_sceneView);
            _directLabel = FindViewById<TextView>(Resource.Id.distanceMeasurement_directLabel);
            _verticalLabel = FindViewById<TextView>(Resource.Id.distanceMeasurement_verticalLabel);
            _horizontalLabel = FindViewById<TextView>(Resource.Id.distanceMeasurement_horizontalLabel);
            _unitSpinner = FindViewById<Spinner>(Resource.Id.distanceMeasurement_unitSpinner);

            // Subscribe to the tap event.
            _mySceneView.GeoViewTapped += _mySceneView_GeoViewTapped;
        }
    }
}