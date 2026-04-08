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
using Esri.ArcGISRuntime.Analysis.Visibility;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace ArcGIS.WinUI.Samples.ShowInteractiveViewshedInAnalysisOverlay
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Show interactive viewshed with analysis overlay",
        category: "Analysis",
        description: "Perform an interactive viewshed analysis to determine visible and non-visible areas from a given observer position.",
        instructions: "The sample loads with a viewshed analysis initialized from an elevation raster covering the Isle of Arran, Scotland. Transparent green shows the area visible from the observer position, and grey shows the non-visible areas. Move the observer position by clicking and dragging over the island to interactively evaluate the viewshed result and display it in the analysis overlay. Alternatively, click on the map to see the viewshed from the clicked location. Use the control panel to explore how the viewshed analysis results change when adjusting the observer elevation, target height, maximum radius, field of view, heading and elevation sampling interval. As you move the observer and update the viewshed parameters, the analysis overlay refreshes to show the evaluated viewshed result.",
        tags: new[] { "analysis overlay", "elevation", "field analysis", "interactive", "raster", "spatial analysis", "terrain", "viewshed", "visibility" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("aa97788593e34a32bcaae33947fdc271")]
    public partial class ShowInteractiveViewshedInAnalysisOverlay
    {
        private ViewshedParameters _viewshedParameters;
        private Graphic _observerGraphic;
        private MapPoint _observerPosition;
        private bool _isDragging;
        private bool _isInitialized;
        private double _observerElevation = 20.0;

        private readonly SimpleMarkerSymbol _observerSymbol = new SimpleMarkerSymbol(
            SimpleMarkerSymbolStyle.Circle, Color.FromArgb(255, 0, 94, 255), 10);

        public ShowInteractiveViewshedInAnalysisOverlay()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a map with the imagery basemap style.
            MyMapView.Map = new Map(BasemapStyle.ArcGISImagery)
            {
                InitialViewpoint = new Viewpoint(55.610000, -5.200346, 100000)
            };

            // Disable panning to allow click-and-drag interaction for observer placement.
            MyMapView.InteractionOptions = new MapViewInteractionOptions { IsPanEnabled = false };

            // Create and add a graphics overlay for the observer marker.
            var graphicsOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(graphicsOverlay);

            // Create and add an analysis overlay for the viewshed.
            var analysisOverlay = new AnalysisOverlay();
            MyMapView.AnalysisOverlays.Add(analysisOverlay);

            try
            {
                // Get the path to the locally stored elevation raster file.
                string rasterPath = DataManager.GetDataFolder("aa97788593e34a32bcaae33947fdc271", "arran.tif");

                // Create a continuous field from the elevation raster file.
                var continuousField = await ContinuousField.CreateAsync(new[] { rasterPath }, 0);

                // Set the initial observer position over the Isle of Arran.
                _observerPosition = new MapPoint(-579246.504, 7479619.947, _observerElevation, SpatialReferences.WebMercator);

                // Add the observer graphic.
                _observerGraphic = new Graphic(_observerPosition, _observerSymbol);
                graphicsOverlay.Graphics.Add(_observerGraphic);

                // Configure the viewshed parameters.
                _viewshedParameters = new ViewshedParameters
                {
                    ObserverPosition = _observerPosition,
                    TargetHeight = 20.0,
                    MaxRadius = 8000,
                    FieldOfView = 150,
                    Heading = 10,
                    ElevationSamplingInterval = 0
                };

                // Create a ContinuousFieldFunction from the continuous field.
                var continuousFieldFunction = ContinuousFieldFunction.Create(continuousField);

                // Create a ViewshedFunction and convert to a DiscreteFieldFunction for visible/not-visible classification.
                var viewshedFunction = new ViewshedFunction(continuousFieldFunction, _viewshedParameters);
                var discreteViewshed = viewshedFunction.ToDiscreteFieldFunction();

                // Create a colormap renderer with visible/not-visible colors.
                var colors = new[]
                {
                    Color.Gray,                         // Not visible
                    Color.FromArgb(128, 136, 204, 132)  // Visible (translucent green)
                };
                var colormap = Colormap.Create(colors);
                var colormapRenderer = new ColormapRenderer(colormap);

                // Create a field analysis with the discrete viewshed function and renderer.
                var fieldAnalysis = new FieldAnalysis(discreteViewshed, colormapRenderer);

                // Add the field analysis to the overlay.
                analysisOverlay.Analyses.Add(fieldAnalysis);

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog2(ex.Message, "Error initializing viewshed");
                await dialog.ShowAsync();
            }
        }

        // Start dragging and update the observer position on pointer press.
        private void MyMapView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!_isInitialized) return;
            _isDragging = true;
            UpdateObserverFromScreenPoint(e.GetCurrentPoint(MyMapView).Position);
        }

        // Update the observer position as the pointer moves while dragging.
        private void MyMapView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDragging || !_isInitialized) return;
            UpdateObserverFromScreenPoint(e.GetCurrentPoint(MyMapView).Position);
        }

        // Finalize observer placement when the pointer is released.
        private void MyMapView_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDragging || !_isInitialized) return;
            UpdateObserverFromScreenPoint(e.GetCurrentPoint(MyMapView).Position);
            _isDragging = false;
        }

        // Convert a screen point to a map location and update the observer position.
        private void UpdateObserverFromScreenPoint(Windows.Foundation.Point screenPoint)
        {
            MapPoint mapPoint = MyMapView.ScreenToLocation(screenPoint);
            if (mapPoint == null) return;

            SetObserverPosition(mapPoint.X, mapPoint.Y);
        }

        // Update the observer position and viewshed parameters with the new coordinates.
        private void SetObserverPosition(double x, double y)
        {
            _observerPosition = new MapPoint(x, y, _observerElevation, SpatialReferences.WebMercator);
            _viewshedParameters.ObserverPosition = _observerPosition;
            _observerGraphic.Geometry = _observerPosition;
        }

        // Update the observer elevation and reposition the observer when the slider value changes.
        private void OnObserverElevationChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (!_isInitialized || _observerPosition == null) return;
            ObserverElevationValue.Text = $"{ObserverElevationSlider.Value:0} m";
            _observerElevation = e.NewValue;
            SetObserverPosition(_observerPosition.X, _observerPosition.Y);
        }

        // Update the viewshed parameters when slider values change.
        private void OnViewshedParameterChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (!_isInitialized) return;

            TargetHeightValue.Text = $"{TargetHeightSlider.Value:0} m";
            MaxRadiusValue.Text = $"{MaxRadiusSlider.Value:0} m";
            FieldOfViewValue.Text = $"{FieldOfViewSlider.Value:0}\u00b0";
            HeadingValue.Text = $"{HeadingSlider.Value:0}\u00b0";

            _viewshedParameters.TargetHeight = TargetHeightSlider.Value;
            _viewshedParameters.MaxRadius = MaxRadiusSlider.Value;
            _viewshedParameters.FieldOfView = FieldOfViewSlider.Value;
            _viewshedParameters.Heading = HeadingSlider.Value;
        }

        // Update the elevation sampling interval when a radio button is selected.
        private void OnSamplingIntervalChanged(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;

            if (SamplingInterval0Radio.IsChecked == true)
                _viewshedParameters.ElevationSamplingInterval = 0;
            else if (SamplingInterval10Radio.IsChecked == true)
                _viewshedParameters.ElevationSamplingInterval = 10;
            else if (SamplingInterval20Radio.IsChecked == true)
                _viewshedParameters.ElevationSamplingInterval = 20;
        }
    }
}
