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
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Point = System.Windows.Point;

namespace ArcGIS.WPF.Samples.ShowInteractiveViewshedInAnalysisOverlay
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
        private ViewshedFunction _viewshedFunction;
        private GraphicsOverlay _graphicsOverlay;
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
            _graphicsOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

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
                _graphicsOverlay.Graphics.Add(_observerGraphic);

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
                _viewshedFunction = new ViewshedFunction(continuousFieldFunction, _viewshedParameters);
                var discreteViewshed = _viewshedFunction.ToDiscreteFieldFunction();

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

                // Clean up event handlers when the sample is unloaded.
                Unloaded += SampleUnloaded;

                // Register mouse events for click-and-drag observer placement.
                MyMapView.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent,
                    new MouseButtonEventHandler(MyMapView_MouseLeftButtonDown), true);
                MyMapView.AddHandler(UIElement.PreviewMouseMoveEvent,
                    new MouseEventHandler(MyMapView_MouseMove), true);
                MyMapView.AddHandler(UIElement.PreviewMouseLeftButtonUpEvent,
                    new MouseButtonEventHandler(MyMapView_MouseLeftButtonUp), true);
                MyMapView.MouseLeave += MyMapView_MouseLeave;
                MyMapView.LostMouseCapture += MyMapView_LostMouseCapture;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error initializing viewshed");
            }
        }

        #region Mouse interaction handlers

        // Start dragging and capture mouse to track movement outside the control.
        private void MyMapView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isInitialized) return;
            _isDragging = true;
            MyMapView.CaptureMouse();
            UpdateObserverFromScreenPoint(e.GetPosition(MyMapView));
            e.Handled = true;
        }

        // Update the observer position as the mouse moves while dragging.
        private void MyMapView_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !_isInitialized) return;

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                ResetDragState();
                return;
            }

            UpdateObserverFromScreenPoint(e.GetPosition(MyMapView));
        }

        // Finalize observer placement when the mouse button is released.
        private void MyMapView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isInitialized) return;

            if (_isDragging)
            {
                UpdateObserverFromScreenPoint(e.GetPosition(MyMapView));
            }

            ResetDragState();
        }

        // End dragging if the mouse leaves the map view.
        private void MyMapView_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !_isInitialized) return;
            UpdateObserverFromScreenPoint(e.GetPosition(MyMapView));
            ResetDragState();
        }

        // Reset the drag flag if mouse capture is lost.
        private void MyMapView_LostMouseCapture(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        // Stop dragging and release the mouse capture.
        private void ResetDragState()
        {
            _isDragging = false;
            if (MyMapView.IsMouseCaptured)
            {
                MyMapView.ReleaseMouseCapture();
            }
        }

        #endregion Mouse interaction handlers

        #region Observer position

        // Convert a screen point to a map location and update the observer position.
        private void UpdateObserverFromScreenPoint(Point screenPoint)
        {
            MapPoint mapPoint = MyMapView.ScreenToLocation(screenPoint);
            if (mapPoint == null) return;
            if (double.IsNaN(mapPoint.X) || double.IsNaN(mapPoint.Y)) return;

            SetObserverPosition(mapPoint.X, mapPoint.Y);
        }

        // Update the observer position and viewshed parameters with the new coordinates.
        private void SetObserverPosition(double x, double y)
        {
            _observerPosition = new MapPoint(x, y, _observerElevation, SpatialReferences.WebMercator);
            _viewshedParameters.ObserverPosition = _observerPosition;
            _observerGraphic.Geometry = _observerPosition;
        }

        #endregion Observer position

        #region Parameter change handlers

        // Update the observer elevation and reposition the observer when the slider value changes.
        private void OnObserverElevationChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized || _observerPosition == null) return;
            _observerElevation = e.NewValue;
            SetObserverPosition(_observerPosition.X, _observerPosition.Y);
        }

        // Update the target height viewshed parameter when the slider value changes.
        private void OnTargetHeightChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized) return;
            _viewshedParameters.TargetHeight = e.NewValue;
        }

        // Update the maximum radius viewshed parameter when the slider value changes.
        private void OnMaxRadiusChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized) return;
            _viewshedParameters.MaxRadius = e.NewValue;
        }

        // Update the field of view viewshed parameter when the slider value changes.
        private void OnFieldOfViewChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized) return;
            _viewshedParameters.FieldOfView = e.NewValue;
        }

        // Update the heading viewshed parameter when the slider value changes.
        private void OnHeadingChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized) return;
            _viewshedParameters.Heading = e.NewValue;
        }

        #endregion Parameter change handlers

        private void SampleUnloaded(object sender, RoutedEventArgs e)
        {
            MyMapView.RemoveHandler(UIElement.PreviewMouseLeftButtonDownEvent,
                new MouseButtonEventHandler(MyMapView_MouseLeftButtonDown));
            MyMapView.RemoveHandler(UIElement.PreviewMouseMoveEvent,
                new MouseEventHandler(MyMapView_MouseMove));
            MyMapView.RemoveHandler(UIElement.PreviewMouseLeftButtonUpEvent,
                new MouseButtonEventHandler(MyMapView_MouseLeftButtonUp));
            MyMapView.MouseLeave -= MyMapView_MouseLeave;
            MyMapView.LostMouseCapture -= MyMapView_LostMouseCapture;
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