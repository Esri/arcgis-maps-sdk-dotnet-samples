// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Analysis;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Editing;
using Esri.ArcGISRuntime.Maui;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGIS.Samples.GetFieldFunctionValueFromAPoint
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Get field function value from a point",
        category: "Analysis",
        description: "Sample field function values at a given position from a field analysis.",
        instructions: "The sample opens displaying both an elevation field (purple-gray tones) and a boolean field that shows where an aircraft flying with an altitude of 2000m above sea level would not have ground clearance (red). The ground clearance field is determined by comparing the elevation field with an altitude value. A callout in the centre of the data displays the values of each field, which have been sampled from the callout's position. Tap the map, or drag the sampling point, to sample the values of each field across the data extent. Use the control panel to adjust the altitude value and see the ground clearance's spatial extent update across the map. The tapped position's sampled value of the ground clearance boolean field will update automatically.",
        tags: new[] { "analysis overlay", "elevation", "field analysis", "interactive", "map algebra", "raster", "sample", "spatial analysis", "terrain" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("6bb0ad0e3bd5410fa52191184578f0b2")]
    [ArcGIS.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\helicopter.png")]
    public partial class GetFieldFunctionValueFromAPoint : ContentPage
    {
        private const double AltitudeStep = (2595.0 - 1500.0) / 100;

        private readonly FloatSupplier _altitudeSupplier = new FloatSupplier(2000);
        private readonly Graphic _sampleGraphic = new Graphic();
        private readonly GeometryEditor _pointEditor = new GeometryEditor();
        private readonly HashSet<FieldAnalysis> _updatingAnalyses = new HashSet<FieldAnalysis>();
        private ContinuousFieldSampleFunction _elevationSampleFunction;
        private BooleanFieldSampleFunction _clearanceSampleFunction;
        private MapPoint _samplePoint;

        private bool _isReady;
        private bool _isSampling;
        private bool _isActive = true;

        public GetFieldFunctionValueFromAPoint()
        {
            InitializeComponent();
            Unloaded += OnSampleUnloaded;
            _ = Initialize();
        }

        private async Task Initialize()
        {
            MyMapView.Map = new Map(BasemapStyle.ArcGISHillshadeDark);
            MyMapView.InteractionOptions = new MapViewInteractionOptions { IsPanEnabled = false };

            var graphicsOverlay = new GraphicsOverlay();
            graphicsOverlay.Graphics.Add(_sampleGraphic);
            MyMapView.GraphicsOverlays.Add(graphicsOverlay);

            var analysisOverlay = new AnalysisOverlay();
            MyMapView.AnalysisOverlays.Add(analysisOverlay);

            try
            {
                string resourceName = GetType().Assembly.GetManifestResourceNames()
                    .Single(name => name.EndsWith(".helicopter.png"));
                using (Stream image = GetType().Assembly.GetManifestResourceStream(resourceName))
                {
                    var symbol = await PictureMarkerSymbol.CreateAsync(image);
                    symbol.Width = 28;
                    symbol.Height = 28;
                    _sampleGraphic.Symbol = symbol;
                }

                // Load the elevation field and map before displaying analyses in the map's spatial reference.
                string rasterPath = DataManager.GetDataFolder("6bb0ad0e3bd5410fa52191184578f0b2", "Meribel.tif");
                var elevationField = await ContinuousField.CreateAsync(new[] { rasterPath }, 0);
                await MyMapView.Map.LoadAsync();
                if (!_isActive) return;

                // Sampling functions share the same fields that the analysis overlay displays.
                var elevationFunction = new ContinuousFieldSupplier(elevationField);
                _elevationSampleFunction = new ContinuousFieldSampleFunction(elevationFunction, null);

                // True means terrain is below the aircraft. Changing the supplier updates this expression in place.
                var clearanceFunction = elevationFunction.IsLessThan(_altitudeSupplier);
                _clearanceSampleFunction = new BooleanFieldSampleFunction(clearanceFunction, null);

                var elevationRenderer = new StretchRenderer(
                    new MinMaxStretchParameters(new[] { 1199.0 }, new[] { 2740.0 }),
                    new[] { 1.0 }, false,
                    ColorRamp.Create(new[] { System.Drawing.Color.FromArgb(96, 98, 148), System.Drawing.Color.White }, 256));

                // Boolean fields use index 0 for false (no clearance) and 1 for true (clear).
                var clearanceRenderer = new ColormapRenderer(Colormap.Create(new[]
                {
                    System.Drawing.Color.FromArgb(255, 210, 60, 60),
                    System.Drawing.Color.FromArgb(0, 50, 170, 90)
                }));

                MyMapView.AnalysisViewStateChanged += OnAnalysisViewStateChanged;
                analysisOverlay.Analyses.Add(new FieldAnalysis(elevationFunction, elevationRenderer) { Opacity = 0.5f });
                analysisOverlay.Analyses.Add(new FieldAnalysis(clearanceFunction, clearanceRenderer) { Opacity = 0.5f });

                MyMapView.SetViewpoint(new Viewpoint(elevationField.Extent.GetCenter(), 50000));

                // Use the geometry editor for native point dragging; the graphics overlay supplies the visible marker.
                var vertexTool = new VertexTool();
                vertexTool.Style.Opacity = 0;
                _pointEditor.Tool = vertexTool;
                MyMapView.GeometryEditor = _pointEditor;
                _pointEditor.PropertyChanged += OnPointGeometryChanged;
                _pointEditor.InteractionPreviewChanged += OnPointPreviewChanged;
                _pointEditor.Start(elevationField.Extent.GetCenter());
                _pointEditor.SelectGeometry();

                _isReady = true;
                AltitudeSlider.IsEnabled = true;
                await SampleAtAsync(elevationField.Extent.GetCenter());
            }
            catch (Exception ex)
            {
                if (_isActive) ErrorMessage.Text = $"Unable to initialize field sampling: {ex.Message}";
            }
        }

        private async Task SampleAtAsync(MapPoint location)
        {
            if (!_isReady || location == null) return;

            _samplePoint = new MapPoint(location.X, location.Y, _altitudeSupplier.Value.Value, location.SpatialReference);
            _sampleGraphic.Geometry = _samplePoint;
            if (_isSampling) return;
            _isSampling = true;

            try
            {
                // Coalesce rapid drags and slider changes, always displaying the most recently requested sample.
                while (_isReady)
                {
                    MapPoint point = _samplePoint;
                    float? altitude = _altitudeSupplier.Value;
                    _elevationSampleFunction.Position = point;
                    _clearanceSampleFunction.Position = point;

                    var elevationSample = await _elevationSampleFunction.EvaluateAsync();
                    var clearanceSample = await _clearanceSampleFunction.EvaluateAsync();
                    if (!_isActive || !_isReady) return;
                    if (!ReferenceEquals(point, _samplePoint)) continue;

                    // A null sample is NoData, including positions outside the raster extent.
                    string elevation = elevationSample.Value?.ToString("F2") ?? "null";
                    string clearance = (altitude - elevationSample.Value)?.ToString("F2") ?? "null";
                    string status = clearanceSample.Value.HasValue
                        ? (clearanceSample.Value.Value ? "Clear" : "Not Clear") : "null";
                    // Native mobile callout details are single-line; use wrapping content for all three readings.
                    var callout = new Label
                    {
                        Text = $"Elevation (m): {elevation}\nClearance above the ground (m): {clearance}\nGround clearance status: {status}",
                        LineBreakMode = LineBreakMode.WordWrap,
                        MaximumWidthRequest = 320
                    };
                    callout.SetAppThemeColor(Label.TextColorProperty, Colors.Black, Colors.White);
                    MyMapView.ShowCalloutAt(point, callout, default);
                    ErrorMessage.Text = string.Empty;
                    break;
                }
            }
            catch (Exception ex)
            {
                if (_isActive)
                {
                    MyMapView.DismissCallout();
                    ErrorMessage.Text = $"Field sampling failed: {ex.Message}";
                }
            }
            finally
            {
                _isSampling = false;
            }
        }

        private void OnAltitudeChanged(object sender, ValueChangedEventArgs e)
        {
            if (!_isReady) return;

            double altitude = 1500 + Math.Round((e.NewValue - 1500) / AltitudeStep) * AltitudeStep;
            _altitudeSupplier.Value = (float)altitude;
            AltitudeLabel.Text = $"Aircraft altitude (MSL): {altitude:F0} m";

            // The overlay redraws automatically; reevaluate the sample functions to refresh the callout.
            _ = SampleAtAsync(_samplePoint);
        }

        private void OnAnalysisViewStateChanged(object sender, AnalysisViewStateChangedEventArgs e)
        {
            Dispatcher.Dispatch(() =>
            {
                if (!_isActive || !(e.Analysis is FieldAnalysis analysis)) return;

                if (e.AnalysisViewState.Status == AnalysisViewStatus.Updating)
                    _updatingAnalyses.Add(analysis);
                else
                    _updatingAnalyses.Remove(analysis);

                LoadingIndicator.IsVisible = _updatingAnalyses.Count > 0;
                AnalysisStatus.Text = _updatingAnalyses.Count > 0 ? "Analysis updating..." : "Analysis idle";

                if (e.AnalysisViewState.Status == AnalysisViewStatus.Error)
                {
                    _isReady = false;
                    AltitudeSlider.IsEnabled = false;
                    MyMapView.DismissCallout();
                    ErrorMessage.Text = $"Field analysis failed: {e.AnalysisViewState.Error?.Message ?? "Unknown analysis error."}";
                }
            });
        }

        private void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            if (!_isReady || e.Location == null) return;
            _pointEditor.ReplaceGeometry(e.Location);
            _pointEditor.SelectGeometry();
            e.Handled = true;
        }

        private void OnPointGeometryChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GeometryEditor.Geometry) && _pointEditor.Geometry is MapPoint point)
                Dispatcher.Dispatch(() => _ = SampleAtAsync(point));
        }

        private void OnPointPreviewChanged(object sender, GeometryEditorInteractionPreviewEventArgs e)
        {
            // Preview geometry changes continuously during a drag, before the edit is committed.
            if ((e.InteractionPreview?.PreviewGeometry ?? _pointEditor.Geometry) is MapPoint point)
                Dispatcher.Dispatch(() => _ = SampleAtAsync(point));
        }

        private void OnSampleUnloaded(object sender, EventArgs e)
        {
            _isActive = false;
            _isReady = false;
            MyMapView.AnalysisViewStateChanged -= OnAnalysisViewStateChanged;
            _pointEditor.PropertyChanged -= OnPointGeometryChanged;
            _pointEditor.InteractionPreviewChanged -= OnPointPreviewChanged;
            if (_pointEditor.IsStarted) _pointEditor.Stop();
        }
    }
}
