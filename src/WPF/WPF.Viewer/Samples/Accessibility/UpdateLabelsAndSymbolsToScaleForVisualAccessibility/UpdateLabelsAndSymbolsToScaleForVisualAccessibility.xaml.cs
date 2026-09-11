// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Labeling;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Windows.UI.ViewManagement;

namespace ArcGIS.WPF.Samples.UpdateLabelsAndSymbolsToScaleForVisualAccessibility
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Update labels and symbols to scale for visual accessibility",
        category: "Accessibility",
        description: "Scale feature labels and symbols according to the system text-size setting.",
        instructions: "Change the text size in the operating system's accessibility settings to see the restaurant labels and symbols resize. Use **Open OS text-size settings** on Windows or Android. On iOS, open **Settings** > **Accessibility** > **Display & Text Size** > **Larger Text**. On Mac Catalyst, open **System Settings** > **Accessibility** > **Display**.",
        tags: new[] { "accessibility", "label", "readability", "scale", "symbol", "text", "visual impairment" })]
    public partial class UpdateLabelsAndSymbolsToScaleForVisualAccessibility
    {
        // Base sizes for the marker and outline in device-independent pixels.
        private const double BaseMarkerSize = 12;
        private const double BaseOutlineWidth = 1.5;

        // Hold references to the Windows text settings and map content.
        private readonly UISettings _uiSettings = new();
        private FeatureLayer _restaurantsLayer;
        private SimpleMarkerSymbol _restaurantMarker;
        private MapPoint _calloutLocation;
        private string _calloutTitle;
        private string _calloutDetails;

        // Flag indicating if the text scale change event is subscribed.
        private bool _eventsSubscribed;
        private int _identifyRequestId;

        public UpdateLabelsAndSymbolsToScaleForVisualAccessibility()
        {
            InitializeComponent();

            // Initialize the sample.
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a map with a light gray basemap and an initial viewpoint.
            Map map = new Map(BasemapStyle.ArcGISLightGray)
            {
                InitialViewpoint = new Viewpoint(
                    new MapPoint(-117.1793, 34.0556, SpatialReferences.Wgs84),
                    2500)
            };

            // Create a marker symbol for the restaurants.
            _restaurantMarker = new SimpleMarkerSymbol(
                SimpleMarkerSymbolStyle.Circle,
                Color.FromArgb(11, 79, 138),
                BaseMarkerSize)
            {
                Outline = new SimpleLineSymbol(
                    SimpleLineSymbolStyle.Solid,
                    Color.White,
                    BaseOutlineWidth)
            };

            // Create the feature layer and apply the symbol with a renderer.
            _restaurantsLayer = new FeatureLayer(
                new Uri("https://services2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/rest/services/redlands_food/FeatureServer/0"))
            {
                Renderer = new SimpleRenderer(_restaurantMarker)
            };

            // Add the feature layer to the map.
            map.OperationalLayers.Add(_restaurantsLayer);

            // Assign the map to the map view.
            MyMapView.Map = map;

            // Enable system text scaling for labels.
            MyMapView.UseSystemTextScale = ApplyTextScaleToLabelsCheckBox.IsChecked == true;

            // Apply the current system text scale to the marker symbol.
            ApplySystemTextScale();

            // Subscribe to the control and lifecycle events.
            ApplyTextScaleToLabelsCheckBox.Checked += OnLabelTextScaleChanged;
            ApplyTextScaleToLabelsCheckBox.Unchecked += OnLabelTextScaleChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            try
            {
                // Load the feature layer before using its service-provided attributes.
                await _restaurantsLayer.LoadAsync();

                // Create a label definition and add it to the feature layer.
                _restaurantsLayer.LabelDefinitions.Add(new LabelDefinition(
                    new SimpleLabelExpression("[name]"),
                    new TextSymbol
                    {
                        Color = Color.FromArgb(31, 35, 40),
                        HaloColor = Color.White,
                        HaloWidth = 2,
                        Size = 12
                    })
                {
                    DeconflictionStrategy = LabelDeconflictionStrategy.None,
                    Placement = Esri.ArcGISRuntime.ArcGISServices.LabelingPlacement.PointAboveCenter
                });

                // Enable labels on the feature layer.
                _restaurantsLayer.LabelsEnabled = true;
                MyMapView.GeoViewTapped += OnMapViewTapped;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading restaurant data");
            }
        }

        private void ApplySystemTextScale()
        {
            // Get the current system text scale.
            double systemTextScale = _uiSettings.TextScaleFactor;

            // Apply the system text scale to the marker and outline.
            _restaurantMarker.Size = BaseMarkerSize * systemTextScale;
            _restaurantMarker.Outline.Width = BaseOutlineWidth * systemTextScale;

            // Update the displayed scale values.
            UpdateScaleStatus();

            // Refresh the visible callout so its text uses the current system text scale.
            if (MyMapView.IsCalloutVisible && _calloutLocation != null)
            {
                MapPoint calloutLocation = _calloutLocation;
                string calloutTitle = _calloutTitle;
                string calloutDetails = _calloutDetails;

                MyMapView.DismissCallout();
                Dispatcher.BeginInvoke(
                    DispatcherPriority.ApplicationIdle,
                    new Action(() =>
                    {
                        if (ReferenceEquals(_calloutLocation, calloutLocation))
                        {
                            MyMapView.ShowCalloutAt(
                                calloutLocation,
                                new CalloutDefinition(calloutTitle, calloutDetails));
                        }
                    }));
            }
        }

        private void OnLabelTextScaleChanged(object sender, RoutedEventArgs e)
        {
            // Enable or disable system text scaling for labels.
            MyMapView.UseSystemTextScale = ApplyTextScaleToLabelsCheckBox.IsChecked == true;

            // Update the displayed scale values.
            UpdateScaleStatus();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Subscribe to system text scale changes.
            if (!_eventsSubscribed)
            {
                _uiSettings.TextScaleFactorChanged += OnTextScaleFactorChanged;
                _eventsSubscribed = true;
            }

            // Apply the current system text scale.
            ApplySystemTextScale();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // Unsubscribe from system text scale changes.
            if (_eventsSubscribed)
            {
                _uiSettings.TextScaleFactorChanged -= OnTextScaleFactorChanged;
                _eventsSubscribed = false;
            }
        }

        private void OnTextScaleFactorChanged(UISettings sender, object args)
        {
            // Apply the text scale change on the UI thread.
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(ApplySystemTextScale));
        }

        private async void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            int identifyRequestId = ++_identifyRequestId;

            _restaurantsLayer.ClearSelection();
            MyMapView.DismissCallout();
            _calloutLocation = null;
            _calloutTitle = null;
            _calloutDetails = null;

            try
            {
                // Identify at most one restaurant near the tapped screen position.
                IdentifyLayerResult result = await MyMapView.IdentifyLayerAsync(
                    _restaurantsLayer, e.Position, 12, false, 1);

                if (identifyRequestId != _identifyRequestId)
                    return;

                if (result.GeoElements.FirstOrDefault() is not Feature restaurant ||
                    restaurant.Geometry is not MapPoint restaurantLocation)
                    return;

                string restaurantName = restaurant.Attributes.TryGetValue("name", out object name)
                    ? name?.ToString()?.Trim()
                    : null;

                // Project the feature location to WGS 84 for latitude and longitude.
                MapPoint wgs84Location = (MapPoint)restaurantLocation.Project(SpatialReferences.Wgs84);

                // Format stable, locale-independent coordinate details for the callout.
                string calloutDetails = string.Format(
                    CultureInfo.InvariantCulture,
                    "Latitude: {0:F5}\nLongitude: {1:F5}",
                    wgs84Location.Y,
                    wgs84Location.X);

                _calloutLocation = restaurantLocation;
                _calloutTitle = string.IsNullOrEmpty(restaurantName) ? "Restaurant" : restaurantName;
                _calloutDetails = calloutDetails;

                _restaurantsLayer.SelectFeature(restaurant);
                MyMapView.ShowCalloutAt(
                    _calloutLocation,
                    new CalloutDefinition(_calloutTitle, _calloutDetails));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error identifying restaurant");
            }
        }

        private void UpdateScaleStatus()
        {
            double systemTextScale = _uiSettings.TextScaleFactor;

            // Display the current text scale, label setting, and marker size.
            WindowsTextScaleValue.Text = $"{systemTextScale:P0}";
            LabelScaleValue.Text = MyMapView.UseSystemTextScale
                ? "scaled by GeoView.UseSystemTextScale."
                : "API text scaling is disabled.";
            MarkerScaleValue.Text = $"{BaseMarkerSize:0.#} DIPs × {systemTextScale:P0} = {_restaurantMarker.Size:0.#} DIPs.";
        }

        private void OpenTextSizeSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open the Windows text-size settings.
                Process.Start(new ProcessStartInfo("ms-settings:easeofaccess-display")
                {
                    UseShellExecute = true
                });
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to open Windows text-size settings");
            }
        }
    }
}
