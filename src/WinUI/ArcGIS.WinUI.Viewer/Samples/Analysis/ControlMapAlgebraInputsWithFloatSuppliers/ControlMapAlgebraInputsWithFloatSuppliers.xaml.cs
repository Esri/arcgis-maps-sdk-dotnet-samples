// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using CommunityToolkit.WinUI.UI.Controls;
using Esri.ArcGISRuntime.Analysis;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcGIS.WinUI.Samples.ControlMapAlgebraInputsWithFloatSuppliers
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Control map algebra inputs with float suppliers",
        category: "Analysis",
        description: "Use float suppliers to dynamically update numeric inputs to a map algebra expression and automatically refresh analysis results as input values change.",
        instructions: "When the sample opens, the map shows the result of a slope analysis which displays slopes between 0 to 10 degrees. This range shows gentle hills and flatter areas such as roads and man-made structures in dark purple. Drag the range slider handles in the control panel to adjust the minimum and maximum slope values between 0 and 85 degrees (the maximum slope value of the data). The analysis updates automatically to show only slopes within the selected range. An indicator in the control panel shows when the analysis is updating. The slope results are drawn on top of an elevation field which is rendered with a surface color ramp to provide context for the data. Green areas represent lower lying terrain, and brown to white reflects higher elevations.",
        tags: new[] { "elevation", "map algebra", "raster", "slope", "spatial analysis", "terrain" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("e8fe90cd623b48dfa683f448bef65e70")]
    public partial class ControlMapAlgebraInputsWithFloatSuppliers
    {
        private FloatSupplier _minimumSlopeSupplier;
        private FloatSupplier _maximumSlopeSupplier;
        private readonly HashSet<Analysis> _updatingAnalyses = new HashSet<Analysis>();

        private double _rangeStart;
        private double _rangeEnd = 10;
        private bool _analysisErrorReported;

        public ControlMapAlgebraInputsWithFloatSuppliers()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Slope requires a conformal projection, and the map and field must use the same spatial reference.
            var spatialReference = SpatialReference.Create(32632);
            MyMapView.Map = new Map(spatialReference);

            try
            {
                await MyMapView.Map.LoadAsync();

                // Create a continuous field from the elevation raster in UTM zone 32N.
                string rasterPath = DataManager.GetDataFolder("e8fe90cd623b48dfa683f448bef65e70", "Meribel_32N.tif");
                var continuousField = await ContinuousField.CreateAsync(new[] { rasterPath }, 0, spatialReference);

                // Supply the elevation field and derive a function that returns slope in degrees.
                var elevationFunction = new ContinuousFieldSupplier(continuousField);
                var slopeFunction = elevationFunction.Slope();

                // Suppliers let the numeric inputs change without rebuilding the map algebra expression.
                _minimumSlopeSupplier = new FloatSupplier(0);
                _maximumSlopeSupplier = new FloatSupplier(10);

                // Compare against the suppliers themselves, not their current values, to keep the expression dynamic.
                BooleanFieldFunction slopeSelection =
                    (slopeFunction >= _minimumSlopeSupplier) & (slopeFunction <= _maximumSlopeSupplier);
                var maskedSlope = slopeFunction.Mask(slopeSelection);

                // Keep the color scale fixed as the selected range changes.
                var slopeRenderer = new StretchRenderer(
                    new MinMaxStretchParameters(new[] { 0.0 }, new[] { 85.0 }),
                    gammas: new[] { 1.0 },
                    estimateStatistics: false,
                    colorRamp: ColorRamp.Create(PresetColorRampType.Viridis, 256));

                var elevationRenderer = new StretchRenderer(
                    new MinMaxStretchParameters(new[] { 1199.0 }, new[] { 2740.0 }),
                    gammas: new[] { 1.0 },
                    estimateStatistics: false,
                    colorRamp: ColorRamp.Create(PresetColorRampType.Surface, 256));

                // Set the initial viewpoint before displaying the analyses.
                await MyMapView.SetViewpointCenterAsync(continuousField.Extent.GetCenter(), 10000);
                MyMapView.AnalysisViewStateChanged += OnAnalysisViewStateChanged;

                // Draw elevation first to provide context beneath the selected slopes.
                var analysisOverlay = new AnalysisOverlay();
                analysisOverlay.Analyses.Add(new FieldAnalysis(elevationFunction, elevationRenderer) { Opacity = 0.6f });
                analysisOverlay.Analyses.Add(new FieldAnalysis(maskedSlope, slopeRenderer));
                MyMapView.AnalysisOverlays.Add(analysisOverlay);

                SlopeRangeSlider.IsEnabled = !_analysisErrorReported;
            }
            catch (Exception ex)
            {
                if (!_analysisErrorReported)
                {
                    _analysisErrorReported = true;
                    AnalysisStatusText.Text = "Analysis failed.";
                    await new MessageDialog2(ex.Message, "Error initializing analysis").ShowAsync();
                }
            }
        }

        private void OnSlopeRangeChanged(object sender, RangeChangedEventArgs e)
        {
            if (_minimumSlopeSupplier == null || _maximumSlopeSupplier == null || _analysisErrorReported) return;

            _rangeStart = SlopeRangeSlider.RangeStart;
            _rangeEnd = SlopeRangeSlider.RangeEnd;
            UpdateSlopeRange();
        }

        private void UpdateSlopeRange()
        {
            if (_minimumSlopeSupplier == null || _maximumSlopeSupplier == null || _analysisErrorReported) return;

            SlopeRangeText.Text = $"Slope range: {_rangeStart:0} to {_rangeEnd:0} degrees";
            // FieldAnalysis automatically reevaluates and redraws when either supplier's value changes.
            _minimumSlopeSupplier.Value = (float)_rangeStart;
            _maximumSlopeSupplier.Value = (float)_rangeEnd;
        }

        private async void OnAnalysisViewStateChanged(object sender, AnalysisViewStateChangedEventArgs e)
        {
            // Either the elevation analysis or the slope analysis can still be updating.
            if (e.AnalysisViewState.Status == AnalysisViewStatus.Updating)
                _updatingAnalyses.Add(e.Analysis);
            else
                _updatingAnalyses.Remove(e.Analysis);

            if (_analysisErrorReported) return;

            AnalysisProgressBar.Visibility = _updatingAnalyses.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            AnalysisStatusText.Text = _updatingAnalyses.Count > 0 ? "Analysis updating..." : "Analysis up to date.";

            // Report the first unrecoverable error and prevent further updates to the failed analysis.
            if (e.AnalysisViewState.Status == AnalysisViewStatus.Error)
            {
                _analysisErrorReported = true;
                SlopeRangeSlider.IsEnabled = false;
                AnalysisProgressBar.Visibility = Visibility.Collapsed;
                AnalysisStatusText.Text = "Analysis failed.";
                await new MessageDialog2(e.AnalysisViewState.Error?.Message ?? "The field analysis could not be displayed.", "Error displaying analysis").ShowAsync();
            }
        }
    }
}