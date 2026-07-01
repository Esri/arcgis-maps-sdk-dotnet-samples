// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.ViewManagement;

namespace ArcGIS.WPF.Samples.UpdateBasemapForContrastAccessibility
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Update basemap for contrast accessibility",
        category: "Accessibility",
        description: "Display a map view that updates between authored light, dark, and high-contrast basemaps.",
        instructions: "In automatic mode, change the Windows app theme or toggle high contrast to see the basemap update. Switch to manual mode to pick a basemap directly. Toggle the reference layers checkbox to show or hide labels and boundaries.",
        tags: new[] { "accessibility", "accessible", "basemap", "colorblind", "contrast", "dark", "enhanced", "high", "inclusive", "legibility", "light", "living atlas", "readability", "vision", "visual impairment", "WCAG" })]
    public partial class UpdateBasemapForContrastAccessibility
    {
        // Portal item ID for the authored high-contrast light web map.
        private const string HighContrastLightItemId = "084291b0ecad4588b8c8853898d72445";

        // Portal item ID for the authored high-contrast dark web map.
        private const string HighContrastDarkItemId = "3e23478909194c54992eaaee78b5f754";

        // Hold a reference to UISettings for reading the Windows app background color.
        private readonly UISettings _uiSettings = new();

        // Track the last applied basemap choice to skip redundant rebuilds.
        private BasemapChoice? _lastAppliedChoice;

        // Flag indicating the page has been unloaded so in-flight work can bail out.
        private bool _isUnloaded;

        // Track whether the OS appearance event handlers are currently subscribed.
        private bool _eventsSubscribed;

        public UpdateBasemapForContrastAccessibility()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create a new map with a light gray basemap centered over Southern California.
            MyMapView.Map = new Map(BasemapStyle.ArcGISLightGray)
            {
                InitialViewpoint = new Viewpoint(34.05, -117.19, 2e6)
            };

            // Hook the page lifecycle so OS appearance events are only subscribed while loaded.
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Mark the page as loaded.
            _isUnloaded = false;

            // Subscribe to Windows theme and high-contrast change notifications.
            if (!_eventsSubscribed)
            {
                _uiSettings.ColorValuesChanged += OnUISettingsChanged;
                SystemParameters.StaticPropertyChanged += OnSystemParametersChanged;
                _eventsSubscribed = true;
            }

            // Build the initial basemap matching the current Windows appearance.
            _ = UpdateMapForAppearanceAsync();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // Mark the page as unloaded so any in-flight update bails out.
            _isUnloaded = true;

            // The WPF sample viewer's Sample_Unloaded handler nulls MyMapView.Map on Unloaded.
            // Drop the cached choice so a later reattach rebuilds the basemap.
            _lastAppliedChoice = null;

            // Unsubscribe from OS appearance notifications.
            if (_eventsSubscribed)
            {
                _uiSettings.ColorValuesChanged -= OnUISettingsChanged;
                SystemParameters.StaticPropertyChanged -= OnSystemParametersChanged;
                _eventsSubscribed = false;
            }
        }

        private async Task UpdateMapForAppearanceAsync()
        {
            // Bail out if the page has been unloaded.
            if (_isUnloaded)
                return;

            try
            {
                // Resolve the basemap choice from the current mode and Windows appearance.
                BasemapChoice choice = ResolveBasemapChoice();

                // Skip rebuilding if the same basemap is already applied.
                if (_lastAppliedChoice == choice)
                    return;

                // Capture the current viewpoint so the new map opens at the same location.
                Viewpoint currentViewpoint =
                    MyMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale)
                    ?? new Viewpoint(34.05, -117.19, 2e6);

                // Build the new map with the captured viewpoint pre-seeded.
                Map newMap = await CreateMapAsync(choice, currentViewpoint);

                // Bail out if the page was unloaded during the async load.
                if (_isUnloaded)
                    return;

                // Swap the new map into the MapView; it opens at newMap.InitialViewpoint.
                MyMapView.Map = newMap;

                // Remember the applied choice and refresh reference layer visibility.
                _lastAppliedChoice = choice;
                ApplyReferenceLayerVisibility(newMap.Basemap);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateMapForAppearanceAsync failed: {ex}");
            }
        }

        private BasemapChoice ResolveBasemapChoice()
        {
            // In automatic mode, pick a basemap from the current Windows theme and high-contrast state.
            if (AutomaticModeRadioButton?.IsChecked == true)
            {
                // Read the current Windows high-contrast and background-brightness state.
                bool highContrast = SystemParameters.HighContrast;
                bool light = IsBackgroundLight();

                // Map the (highContrast, light) pair to a basemap choice.
                return (highContrast, light) switch
                {
                    (true, true) => BasemapChoice.HighContrastLight,
                    (true, false) => BasemapChoice.HighContrastDark,
                    (false, true) => BasemapChoice.Light,
                    (false, false) => BasemapChoice.Dark
                };
            }

            // In manual mode, return whichever radio button the user has selected.
            if (DarkRadioButton?.IsChecked == true)
                return BasemapChoice.Dark;

            if (HighContrastLightRadioButton?.IsChecked == true)
                return BasemapChoice.HighContrastLight;

            if (HighContrastDarkRadioButton?.IsChecked == true)
                return BasemapChoice.HighContrastDark;

            // Default to the light basemap.
            return BasemapChoice.Light;
        }

        private static async Task<Map> CreateMapAsync(BasemapChoice choice, Viewpoint viewpoint)
        {
            // Build the map for the requested choice; high-contrast variants come from authored portal items.
            Map map = choice switch
            {
                BasemapChoice.HighContrastLight => await CreatePortalMapAsync(HighContrastLightItemId),
                BasemapChoice.HighContrastDark => await CreatePortalMapAsync(HighContrastDarkItemId),
                BasemapChoice.Dark => new Map(BasemapStyle.ArcGISDarkGray),
                _ => new Map(BasemapStyle.ArcGISLightGray)
            };

            // Make sure the map is loaded before assigning a viewpoint.
            await map.LoadAsync();

            // Set the viewpoint after LoadAsync so the portal-item load doesn't overwrite it.
            // Pre-seeding the viewpoint avoids a flash through the authored/default view when the map is swapped in.
            map.InitialViewpoint = viewpoint;
            return map;
        }

        private static async Task<Map> CreatePortalMapAsync(string itemId)
        {
            // Connect to the default portal and load the authored web map by item ID.
            ArcGISPortal portal = await ArcGISPortal.CreateAsync();
            PortalItem portalItem = await PortalItem.CreateAsync(portal, itemId);

            // Build the map from the portal item and load it.
            Map map = new Map(portalItem);
            await map.LoadAsync();
            return map;
        }

        private void ApplyReferenceLayerVisibility(Basemap basemap)
        {
            // Nothing to do if the basemap has no reference layers.
            if (basemap?.ReferenceLayers == null)
                return;

            // Read the desired visibility from the checkbox.
            bool visible = ReferenceLayersCheckBox?.IsChecked == true;

            // Toggle each reference layer to match.
            foreach (Layer layer in basemap.ReferenceLayers)
            {
                layer.IsVisible = visible;
            }
        }

        // Switch between automatic and manual appearance modes.
        private void ContrastModeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // The manual appearance panel may not exist yet during early initialization.
            if (ManualAppearancePanel == null)
                return;

            // Show the manual radio buttons only in manual mode.
            bool manual = ManualModeRadioButton.IsChecked == true;

            ManualAppearancePanel.Visibility = manual
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Show the Open Settings shortcuts only in automatic mode.
            if (OpenSettingsPanel != null)
            {
                OpenSettingsPanel.Visibility = manual
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }

            // Force a rebuild because the resolved choice may differ even if the cached value matches.
            _lastAppliedChoice = null;
            _ = UpdateMapForAppearanceAsync();
        }

        // Apply a manually-selected basemap choice.
        private void ManualAppearanceRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ManualModeRadioButton?.IsChecked == true)
            {
                // Reset the cache so the new manual choice always applies.
                _lastAppliedChoice = null;
                _ = UpdateMapForAppearanceAsync();
            }
        }

        // Toggle reference layer visibility on the current basemap.
        private void ReferenceLayersCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            ApplyReferenceLayerVisibility(MyMapView?.Map?.Basemap);
        }

        private enum BasemapChoice
        {
            Light, 
            Dark, 
            HighContrastLight, 
            HighContrastDark 
        }

        #region Windows scaffolding

        // Returns true when the current app background reads as light.
        // High-contrast palettes live in SystemColors; the normal app background lives in WinRT UISettings.
        // The weighted RGB sum is the standard integer approximation of Rec. 601 perceived luma (2*R + 5*G + B) / 8,
        // compared against mid-gray (128) via the un-divided form.
        private bool IsBackgroundLight()
        {
            // In high contrast, use the system window color from SystemColors.
            if (SystemParameters.HighContrast)
            {
                var background = SystemColors.WindowColor;
                return (5 * background.G + 2 * background.R + background.B) > 8 * 128;
            }

            // Otherwise, read the app background color from UISettings.
            var appBackground = _uiSettings.GetColorValue(UIColorType.Background);
            return (5 * appBackground.G + 2 * appBackground.R + appBackground.B) > 8 * 128;
        }

        // Windows app color values changed. Rebuild the basemap if automatic mode is active.
        // ColorValuesChanged fires off the UI thread, so marshal back to the dispatcher first.
        private void OnUISettingsChanged(UISettings sender, object args)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!_isUnloaded && AutomaticModeRadioButton?.IsChecked == true)
                {
                    _ = UpdateMapForAppearanceAsync();
                }
            }));
        }

        // Windows system parameters changed. Rebuild if the high-contrast state flipped and automatic mode is active.
        private void OnSystemParametersChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SystemParameters.HighContrast))
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (!_isUnloaded && AutomaticModeRadioButton?.IsChecked == true)
                    {
                        _ = UpdateMapForAppearanceAsync();
                    }
                }));
            }
        }

        // Open the Windows theme settings page.
        private void OpenThemeSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:colors")
            {
                UseShellExecute = true
            });
        }

        // Open the Windows high-contrast settings page.
        private void OpenContrastSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:easeofaccess-highcontrast")
            {
                UseShellExecute = true
            });
        }

        #endregion
    }
}
