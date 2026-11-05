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
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcGIS.Samples.ControlMapAlgebraInputsWithFloatSuppliers
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Control map algebra inputs with float suppliers",
        category: "Analysis",
        description: "Use float suppliers to dynamically update numeric inputs to a map algebra expression and automatically refresh analysis results as input values change.",
        instructions: "When the sample opens, the map shows the result of a slope analysis which displays slopes between 0 to 10 degrees. This range shows gentle hills and flatter areas such as roads and man-made structures in dark purple. Drag the range slider handles in the control panel to adjust the minimum and maximum slope values between 0 and 85 degrees (the maximum slope value of the data). The analysis updates automatically to show only slopes within the selected range. An indicator in the control panel shows when the analysis is updating. The slope results are drawn on top of an elevation field which is rendered with a surface color ramp to provide context for the data. Green areas represent lower lying terrain, and brown to white reflects higher elevations.",
        tags: new[] { "elevation", "map algebra", "raster", "slope", "spatial analysis", "terrain" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("e8fe90cd623b48dfa683f448bef65e70")]
    public partial class ControlMapAlgebraInputsWithFloatSuppliers : ContentPage
    {
        private FloatSupplier _minimumSlopeSupplier;
        private FloatSupplier _maximumSlopeSupplier;
        private readonly HashSet<Analysis> _updatingAnalyses = new HashSet<Analysis>();

        private const double MaximumSlope = 85;
        private const double ThumbSize = 44;
        private double _rangeStart;
        private double _rangeEnd = 10;
        private double _minimumDragStart;
        private double _maximumDragStart;
        private bool _analysisErrorReported;

#if ANDROID
        private double _minimumTouchStartX;
        private double _maximumTouchStartX;
#endif

        public ControlMapAlgebraInputsWithFloatSuppliers()
        {
            InitializeComponent();

#if ANDROID
            foreach (Border thumb in new[] { MinimumSlopeThumb, MaximumSlopeThumb })
            {
                thumb.GestureRecognizers.Clear();
                thumb.HandlerChanging += OnSlopeThumbHandlerChanging;
                thumb.HandlerChanged += OnSlopeThumbHandlerChanged;
            }
#endif

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
                    await Application.Current.Windows[0].Page.DisplayAlertAsync("Error initializing analysis", ex.Message, "OK");
                }
            }
        }

        private void UpdateSlopeRange()
        {
            if (_minimumSlopeSupplier == null || _maximumSlopeSupplier == null || _analysisErrorReported) return;

            SlopeRangeText.Text = $"Slope range: {_rangeStart:0} to {_rangeEnd:0} degrees";
            // FieldAnalysis automatically reevaluates and redraws when either supplier's value changes.
            _minimumSlopeSupplier.Value = (float)_rangeStart;
            _maximumSlopeSupplier.Value = (float)_rangeEnd;
        }

        private void SetSlopeBoundary(bool minimum, double value)
        {
            if (_minimumSlopeSupplier == null || _analysisErrorReported) return;

            value = (int)Math.Clamp(Math.Round(value), 0, MaximumSlope);
            if (minimum)
                _rangeStart = Math.Min(value, _rangeEnd);
            else
                _rangeEnd = Math.Max(value, _rangeStart);

            UpdateSlopeRange();
            PositionSlopeThumbs();
        }

        private void PositionSlopeThumbs()
        {
            double width = Math.Max(0, SlopeRangeSlider.Width - ThumbSize);
            double start = width * _rangeStart / MaximumSlope;
            double end = width * _rangeEnd / MaximumSlope;
            AbsoluteLayout.SetLayoutBounds(SlopeTrack, new Rect(ThumbSize / 2, 20, width, 4));
            AbsoluteLayout.SetLayoutBounds(SelectedSlopeTrack, new Rect(start + ThumbSize / 2, 20, end - start, 4));
            AbsoluteLayout.SetLayoutBounds(MinimumSlopeThumb, new Rect(start, 0, ThumbSize, ThumbSize));
            AbsoluteLayout.SetLayoutBounds(MaximumSlopeThumb, new Rect(end, 0, ThumbSize, ThumbSize));
            SemanticProperties.SetDescription(MinimumSlopeThumb, $"Minimum slope: {_rangeStart:0} degrees");
            SemanticProperties.SetDescription(MaximumSlopeThumb, $"Maximum slope: {_rangeEnd:0} degrees");
        }

        private void OnSlopeRangeSizeChanged(object sender, EventArgs e)
        {
            PositionSlopeThumbs();
        }

        private void OnMinimumSlopePanUpdated(object sender, PanUpdatedEventArgs e) => OnSlopePanUpdated(true, e);

        private void OnMaximumSlopePanUpdated(object sender, PanUpdatedEventArgs e) => OnSlopePanUpdated(false, e);

        private void OnSlopePanUpdated(bool minimum, PanUpdatedEventArgs e)
        {
            if (e.StatusType == GestureStatus.Started)
            {
                if (minimum)
                    _minimumDragStart = _rangeStart;
                else
                    _maximumDragStart = _rangeEnd;
            }
            else if (e.StatusType == GestureStatus.Running && SlopeRangeSlider.Width > ThumbSize)
            {
                double start = minimum ? _minimumDragStart : _maximumDragStart;
                SetSlopeBoundary(minimum, start + e.TotalX / (SlopeRangeSlider.Width - ThumbSize) * MaximumSlope);
            }
            else if (e.StatusType == GestureStatus.Completed || e.StatusType == GestureStatus.Canceled)
            {
                BringSlopeThumbForward(minimum);
            }
        }

        private void BringSlopeThumbForward(bool minimum)
        {
            // Reorder overlapping handles after the gesture, without interrupting native touch capture.
            bool minimumOnTop = _rangeStart == MaximumSlope || (minimum && _rangeEnd > 0);
            MinimumSlopeThumb.ZIndex = minimumOnTop ? 1 : 0;
            MaximumSlopeThumb.ZIndex = minimumOnTop ? 0 : 1;
        }

#if ANDROID
        private void OnSlopeThumbHandlerChanging(object sender, HandlerChangingEventArgs e)
        {
            if (e.OldHandler?.PlatformView is Android.Views.View view)
            {
                view.Touch -= OnMinimumSlopeTouch;
                view.Touch -= OnMaximumSlopeTouch;
            }
        }

        private void OnSlopeThumbHandlerChanged(object sender, EventArgs e)
        {
            if (sender is Border thumb && thumb.Handler?.PlatformView is Android.Views.View view)
            {
                if (thumb == MinimumSlopeThumb)
                    view.Touch += OnMinimumSlopeTouch;
                else
                    view.Touch += OnMaximumSlopeTouch;
            }
        }

        private void OnMinimumSlopeTouch(object sender, Android.Views.View.TouchEventArgs e) => OnSlopeTouch(true, (Android.Views.View)sender, e);

        private void OnMaximumSlopeTouch(object sender, Android.Views.View.TouchEventArgs e) => OnSlopeTouch(false, (Android.Views.View)sender, e);

        private void OnSlopeTouch(bool minimum, Android.Views.View view, Android.Views.View.TouchEventArgs e)
        {
            if (e.Event is not Android.Views.MotionEvent touch) return;

            switch (touch.ActionMasked)
            {
                case Android.Views.MotionEventActions.Down:
                    // Capture screen coordinates once; moving the thumb must not move the gesture's origin.
                    if (minimum)
                    {
                        _minimumTouchStartX = touch.RawX;
                        _minimumDragStart = _rangeStart;
                    }
                    else
                    {
                        _maximumTouchStartX = touch.RawX;
                        _maximumDragStart = _rangeEnd;
                    }
                    view.Parent?.RequestDisallowInterceptTouchEvent(true);
                    break;

                case Android.Views.MotionEventActions.Move:
                case Android.Views.MotionEventActions.Up:
                    if (SlopeRangeSlider.Width > ThumbSize)
                    {
                        double startX = minimum ? _minimumTouchStartX : _maximumTouchStartX;
                        double startValue = minimum ? _minimumDragStart : _maximumDragStart;
                        double distance = (touch.RawX - startX) / Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Density;
                        SetSlopeBoundary(minimum, startValue + distance / (SlopeRangeSlider.Width - ThumbSize) * MaximumSlope);
                    }
                    if (touch.ActionMasked == Android.Views.MotionEventActions.Up)
                    {
                        view.Parent?.RequestDisallowInterceptTouchEvent(false);
                        BringSlopeThumbForward(minimum);
                        view.PerformClick();
                    }
                    break;

                case Android.Views.MotionEventActions.Cancel:
                    view.Parent?.RequestDisallowInterceptTouchEvent(false);
                    BringSlopeThumbForward(minimum);
                    break;
            }

            e.Handled = true;
        }
#endif

        private async void OnAnalysisViewStateChanged(object sender, AnalysisViewStateChangedEventArgs e)
        {
            // Either the elevation analysis or the slope analysis can still be updating.
            if (e.AnalysisViewState.Status == AnalysisViewStatus.Updating)
                _updatingAnalyses.Add(e.Analysis);
            else
                _updatingAnalyses.Remove(e.Analysis);

            if (_analysisErrorReported) return;

            AnalysisActivityIndicator.IsVisible = _updatingAnalyses.Count > 0;
            AnalysisStatusText.Text = _updatingAnalyses.Count > 0 ? "Analysis updating..." : "Analysis up to date.";

            // Report the first unrecoverable error and prevent further updates to the failed analysis.
            if (e.AnalysisViewState.Status == AnalysisViewStatus.Error)
            {
                _analysisErrorReported = true;
                SlopeRangeSlider.IsEnabled = false;
                AnalysisActivityIndicator.IsVisible = false;
                AnalysisStatusText.Text = "Analysis failed.";
                await Application.Current.Windows[0].Page.DisplayAlertAsync("Error displaying analysis", e.AnalysisViewState.Error?.Message ?? "The field analysis could not be displayed.", "OK");
            }
        }
    }
}