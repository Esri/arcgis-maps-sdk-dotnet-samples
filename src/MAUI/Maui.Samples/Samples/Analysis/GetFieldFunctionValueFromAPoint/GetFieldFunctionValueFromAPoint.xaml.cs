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

        // Share a mutable aircraft altitude between the clearance analysis and its sampling function.
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
            // Create a map with a dark hillshade basemap to provide terrain context.
            MyMapView.Map = new Map(BasemapStyle.ArcGISHillshadeDark);

            // Disable panning so dragging can be used to move the sampling point.
            MyMapView.InteractionOptions = new MapViewInteractionOptions { IsPanEnabled = false };

            // Add a graphics overlay to display the helicopter at the sampling position.
            var graphicsOverlay = new GraphicsOverlay();
            graphicsOverlay.Graphics.Add(_sampleGraphic);
            MyMapView.GraphicsOverlays.Add(graphicsOverlay);

            // Add an analysis overlay to display the elevation and ground-clearance fields.
            var analysisOverlay = new AnalysisOverlay();
            MyMapView.AnalysisOverlays.Add(analysisOverlay);

            try
            {
                // Create the helicopter marker symbol from an embedded image.
                string resourceName = GetType().Assembly.GetManifestResourceNames()
                    .Single(name => name.EndsWith(".helicopter.png"));
                using (Stream image = GetType().Assembly.GetManifestResourceStream(resourceName))
                {
                    var symbol = await PictureMarkerSymbol.CreateAsync(image);
                    symbol.Width = 28;
                    symbol.Height = 28;
                    _sampleGraphic.Symbol = symbol;
                }

                // Get the path to the elevation raster downloaded for this sample.
                string rasterPath = DataManager.GetDataFolder("6bb0ad0e3bd5410fa52191184578f0b2", "Meribel.tif");

                // Create a continuous elevation field from the raster's first band (index 0).
                var elevationField = await ContinuousField.CreateAsync(new[] { rasterPath }, 0);

                // Load the map before creating field analyses so its spatial reference is available.
                await MyMapView.Map.LoadAsync();
                if (!_isActive) return;

                // Wrap the field in a function shared by the rendered analysis and the elevation sampler.
                var elevationFunction = new ContinuousFieldSupplier(elevationField);

                // Create an elevation sampler whose position will be assigned before evaluation.
                _elevationSampleFunction = new ContinuousFieldSampleFunction(elevationFunction, null);

                // True means terrain is below the aircraft. Changing the supplier updates this expression in place.
                var clearanceFunction = elevationFunction.IsLessThan(_altitudeSupplier);

                // Create a sampler for the boolean ground-clearance result.
                _clearanceSampleFunction = new BooleanFieldSampleFunction(clearanceFunction, null);

                // Render elevation with fixed stretch limits and a purple-gray-to-white color ramp.
                var elevationRenderer = new StretchRenderer(
                    new MinMaxStretchParameters(new[] { 1199.0 }, new[] { 2740.0 }),
                    new[] { 1.0 }, false,
                    ColorRamp.Create(new[] { System.Drawing.Color.FromArgb(96, 98, 148), System.Drawing.Color.White }, 256));

                // Boolean fields use index 0 for false (no clearance) and 1 for true (clear).
                // Leave clear areas transparent so the elevation analysis remains visible beneath them.
                var clearanceRenderer = new ColormapRenderer(Colormap.Create(new[]
                {
                    System.Drawing.Color.FromArgb(255, 210, 60, 60),
                    System.Drawing.Color.FromArgb(0, 50, 170, 90)
                }));

                // Track rendering progress and errors independently of the point-sampling results.
                MyMapView.AnalysisViewStateChanged += OnAnalysisViewStateChanged;

                // Field analyses evaluate and redraw automatically when their function inputs change.
                // Display elevation first, then clearance, retaining hillshade detail with half opacity.
                analysisOverlay.Analyses.Add(new FieldAnalysis(elevationFunction, elevationRenderer) { Opacity = 0.5f });
                analysisOverlay.Analyses.Add(new FieldAnalysis(clearanceFunction, clearanceRenderer) { Opacity = 0.5f });

                // Set the initial viewpoint to the center of the elevation field.
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

                // Sample both fields at the center of the data to populate the initial callout.
                await SampleAtAsync(elevationField.Extent.GetCenter());
            }
            catch (Exception ex)
            {
                if (_isActive) ErrorMessage.Text = $"Unable to initialize field sampling: {ex.Message}";
            }
            finally
            {
                LoadingStatus.IsVisible = false;
            }
        }

        private async Task SampleAtAsync(MapPoint location)
        {
            if (!_isReady || location == null) return;

            // Move the sampling graphic to the requested location at the current aircraft altitude.
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

                    // Set the position used by both sampling functions.
                    _elevationSampleFunction.Position = point;
                    _clearanceSampleFunction.Position = point;

                    // Evaluate the functions explicitly to retrieve elevation and the clearance criterion.
                    var elevationSample = await _elevationSampleFunction.EvaluateAsync();
                    var clearanceSample = await _clearanceSampleFunction.EvaluateAsync();
                    if (!_isActive || !_isReady) return;

                    // Discard outdated results and sample the latest position on the next iteration.
                    if (!ReferenceEquals(point, _samplePoint)) continue;

                    // A null sample is NoData, including positions outside the raster extent.
                    string elevation = elevationSample.Value?.ToString("F2") ?? "null";

                    // Calculate numeric clearance from the sampled elevation, preserving null for NoData.
                    string clearance = (altitude - elevationSample.Value)?.ToString("F2") ?? "null";
                    string status = clearanceSample.Value.HasValue
                        ? (clearanceSample.Value.Value ? "Clear" : "Not Clear") : "null";
                    // Native mobile callout details are single-line; use wrapping content for all three readings.
                    var callout = new Label
                    {
                        Text = $"Elevation: {elevation} m\n" +
                               $"Ground clearance: {clearance} m\n" +
                               $"Status: {status}",
                        LineBreakMode = LineBreakMode.WordWrap,
                        WidthRequest = 240
                    };
                    callout.SetAppThemeColor(Label.TextColorProperty, Colors.Black, Colors.White);

                    // Anchor the current values to the sampled map position.
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

            // Update the supplier rather than rebuilding the clearance function or its analysis.
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

                // The two field analyses can finish updating at different times.
                if (e.AnalysisViewState.Status == AnalysisViewStatus.Updating)
                    _updatingAnalyses.Add(analysis);
                else
                    _updatingAnalyses.Remove(analysis);

                // Show updating text only while analysis is active; retain the dot when idle.
                bool isUpdating = _updatingAnalyses.Count > 0;
                string status = isUpdating ? "Analysis updating" : "Analysis idle";
                UpdatingStatus.IsVisible = isUpdating;
                AnalysisStatusDot.TextColor = Microsoft.Maui.Graphics.Color.FromArgb(isUpdating ? "#69F26E" : "#9E9E9E");
                ToolTipProperties.SetText(AnalysisStatusDot, status);
                SemanticProperties.SetDescription(AnalysisStatusDot, status);

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

            // Keep map taps and native point dragging in the same geometry-editing workflow.
            _pointEditor.ReplaceGeometry(e.Location);
            _pointEditor.SelectGeometry();
            e.Handled = true;
        }

        private void OnPointGeometryChanged(object sender, PropertyChangedEventArgs e)
        {
            // Refresh samples after a point edit is committed or the geometry is replaced programmatically.
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
            // Stop view updates and detach input and analysis callbacks when leaving the sample.
            _isActive = false;
            _isReady = false;
            MyMapView.AnalysisViewStateChanged -= OnAnalysisViewStateChanged;
            _pointEditor.PropertyChanged -= OnPointGeometryChanged;
            _pointEditor.InteractionPreviewChanged -= OnPointPreviewChanged;
            if (_pointEditor.IsStarted) _pointEditor.Stop();
        }
    }
}
