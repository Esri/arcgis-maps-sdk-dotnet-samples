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
using Esri.ArcGISRuntime.Maui;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Microsoft.Maui.ApplicationModel;
#if WINDOWS
using Windows.UI.ViewManagement;
#endif
#if IOS || MACCATALYST
using UIKit;
#endif
#if ANDROID
using Android.Content;
using Android.Provider;
#endif
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGIS.Samples.UpdateLabelsAndSymbolsToScaleForVisualAccessibility
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Update labels and symbols to scale for visual accessibility",
        category: "Accessibility",
        description: "Scale feature labels and symbols according to the system text-size setting.",
        instructions: "Change the text size in the operating system's accessibility settings to see the restaurant labels and symbols resize. Use **Open OS text-size settings** on Windows or Android. On iOS, open **Settings** > **Accessibility** > **Display & Text Size** > **Larger Text**. On Mac Catalyst, open **System Settings** > **Accessibility** > **Display**.",
        tags: new[] { "accessibility", "label", "readability", "scale", "symbol", "text", "visual impairment" })]
    public partial class UpdateLabelsAndSymbolsToScaleForVisualAccessibility : ContentPage
    {
        // Base sizes for the marker and outline in device-independent pixels.
        private const double BaseMarkerSize = 12;
        private const double BaseOutlineWidth = 1.5;

        // Hold references to the device text settings and map content.
        private FeatureLayer _restaurantsLayer;
        private SimpleMarkerSymbol _restaurantMarker;
        private MapPoint _calloutLocation;
        private string _calloutTitle;
        private string _calloutDetails;

#if WINDOWS
        private readonly UISettings _uiSettings = new();
#endif
#if ANDROID
        private AndroidConfigurationCallback _androidConfigurationCallback;
#endif
#if IOS || MACCATALYST
        private IDisposable _contentSizeCategoryObserver;
#endif
        private bool _isActive;
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
            Esri.ArcGISRuntime.Mapping.Map map = new Esri.ArcGISRuntime.Mapping.Map(BasemapStyle.ArcGISLightGray)
            {
                InitialViewpoint = new Viewpoint(
                    new MapPoint(-117.1793, 34.0556, SpatialReferences.Wgs84),
                    2500)
            };

            // Create a marker symbol for the restaurants.
            _restaurantMarker = new SimpleMarkerSymbol(
                SimpleMarkerSymbolStyle.Circle,
                System.Drawing.Color.FromArgb(11, 79, 138),
                BaseMarkerSize)
            {
                Outline = new SimpleLineSymbol(
                    SimpleLineSymbolStyle.Solid,
                    System.Drawing.Color.White,
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
            MyMapView.UseSystemTextScale = ApplyTextScaleToLabelsCheckBox.IsChecked;

            // Apply the current system text scale to the marker symbol.
            ApplySystemTextScale();

            try
            {
                // Load the feature layer before using its service-provided attributes.
                await _restaurantsLayer.LoadAsync();

                // Create a label definition and add it to the feature layer.
                _restaurantsLayer.LabelDefinitions.Add(new LabelDefinition(
                    new SimpleLabelExpression("[name]"),
                    new TextSymbol
                    {
                        Color = System.Drawing.Color.FromArgb(31, 35, 40),
                        HaloColor = System.Drawing.Color.White,
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
                await Application.Current.Windows[0].Page.DisplayAlertAsync("Error loading restaurant data", ex.Message, "OK");
            }
        }

        private void ApplySystemTextScale()
        {
            if (_restaurantMarker == null)
                return;

            // Get the current platform text scale.
            double systemTextScale = GetSystemTextScaleFactor();

            // Apply the system text scale to the marker and outline.
            _restaurantMarker.Size = BaseMarkerSize * systemTextScale;
            _restaurantMarker.Outline.Width = BaseOutlineWidth * systemTextScale;

            // Update the displayed scale values.
            UpdateScaleStatus(systemTextScale);

            // Refresh the visible callout so its text uses the current system text scale.
            if (MyMapView.IsCalloutVisible && _calloutLocation != null)
            {
                MapPoint calloutLocation = _calloutLocation;
                string calloutTitle = _calloutTitle;
                string calloutDetails = _calloutDetails;

                MyMapView.DismissCallout();
                Dispatcher.Dispatch(() =>
                {
                    if (ReferenceEquals(_calloutLocation, calloutLocation))
                    {
                        MyMapView.ShowCalloutAt(
                            calloutLocation,
                            new CalloutDefinition(calloutTitle, calloutDetails));
                    }
                });
            }
        }

        private void OnLabelTextScaleChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_restaurantMarker == null)
                return;

            // Enable or disable system text scaling for labels.
            MyMapView.UseSystemTextScale = e.Value;
            UpdateScaleStatus(GetSystemTextScaleFactor());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _isActive = true;

#if WINDOWS
            _uiSettings.TextScaleFactorChanged += OnTextScaleFactorChanged;
#elif ANDROID
            _androidConfigurationCallback = new AndroidConfigurationCallback(QueueSystemTextScaleUpdate);
            Android.App.Application.Context.RegisterComponentCallbacks(_androidConfigurationCallback);
#elif IOS || MACCATALYST
            _contentSizeCategoryObserver = UIApplication.Notifications.ObserveContentSizeCategoryChanged(
                (_, _) => QueueSystemTextScaleUpdate());
#endif

            // Apply the current system text scale when the page is attached.
            ApplySystemTextScale();
        }

        protected override void OnDisappearing()
        {
            _isActive = false;

#if WINDOWS
            _uiSettings.TextScaleFactorChanged -= OnTextScaleFactorChanged;
#elif ANDROID
            Android.App.Application.Context.UnregisterComponentCallbacks(_androidConfigurationCallback);
            _androidConfigurationCallback.Dispose();
            _androidConfigurationCallback = null;
#elif IOS || MACCATALYST
            _contentSizeCategoryObserver.Dispose();
            _contentSizeCategoryObserver = null;
#endif

            base.OnDisappearing();
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
                await Application.Current.Windows[0].Page.DisplayAlertAsync("Error identifying restaurant", ex.Message, "OK");
            }
        }

        private void UpdateScaleStatus(double systemTextScale)
        {
            // Display the current text scale, label setting, and marker size.
            WindowsTextScaleValue.Text = $"{systemTextScale:P0}";
            LabelScaleValue.Text = ApplyTextScaleToLabelsCheckBox.IsChecked
                ? "scaled by GeoView.UseSystemTextScale."
                : "API text scaling is disabled.";
            MarkerScaleValue.Text = $"{BaseMarkerSize:0.#} DIPs × {systemTextScale:P0} = {_restaurantMarker.Size:0.#} DIPs.";
        }

        private async void OpenTextSizeSettingsButton_Clicked(object sender, EventArgs e)
        {
            try
            {
#if WINDOWS
                await Launcher.OpenAsync(new Uri("ms-settings:easeofaccess-display"));
#elif ANDROID
                Intent intent = new Intent(Settings.ActionAccessibilitySettings);
                intent.AddFlags(ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intent);
                await Task.CompletedTask;
#elif IOS
                await Application.Current.Windows[0].Page.DisplayAlertAsync(
                    "System text-size settings",
                    "Open Settings > Accessibility > Display & Text Size > Larger Text to change Dynamic Type. iOS does not provide a stable public link to this setting.",
                    "OK");
#elif MACCATALYST
                await Application.Current.Windows[0].Page.DisplayAlertAsync(
                    "System text-size settings",
                    "Open System Settings > Accessibility > Display to change the text size. Mac Catalyst does not provide a stable public link to this setting.",
                    "OK");
#else
                await Task.CompletedTask;
#endif
            }
            catch (Exception ex)
            {
                await Application.Current.Windows[0].Page.DisplayAlertAsync("Unable to open text-size settings", ex.Message, "OK");
            }
        }

        private double GetSystemTextScaleFactor()
        {
#if WINDOWS
            return _uiSettings.TextScaleFactor;
#elif ANDROID
            return Android.App.Application.Context.Resources.Configuration.FontScale;
#elif IOS || MACCATALYST
            return (double)UIFontMetrics.GetMetrics(UIFontTextStyle.Body.GetConstant().ToString())
                .GetScaledValue((System.Runtime.InteropServices.NFloat)BaseMarkerSize) / BaseMarkerSize;
#else
            return 1.0;
#endif
        }

#if WINDOWS
        private void OnTextScaleFactorChanged(UISettings sender, object args)
        {
            QueueSystemTextScaleUpdate();
        }
#endif

        private void QueueSystemTextScaleUpdate() =>
            Dispatcher.Dispatch(() =>
            {
                if (_isActive)
                    ApplySystemTextScale();
            });

#if ANDROID
        private sealed class AndroidConfigurationCallback(Action configurationChanged) : Java.Lang.Object, IComponentCallbacks
        {
            public void OnConfigurationChanged(Android.Content.Res.Configuration _) =>
                configurationChanged();

            public void OnLowMemory() { }
        }
#endif
    }
}
