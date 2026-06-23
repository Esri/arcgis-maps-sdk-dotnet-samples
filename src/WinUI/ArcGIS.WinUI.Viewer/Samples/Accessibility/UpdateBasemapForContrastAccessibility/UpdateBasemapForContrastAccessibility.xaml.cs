// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.
using Esri.ArcGISRuntime.Mapping;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace ArcGIS.WinUI.Samples.UpdateBasemapForContrastAccessibility
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Update basemap for contrast accessibility",
        category: "Accessibility",
        description: "Display a map view that updates between light, dark, and high-contrast basemaps.",
        instructions: "In automatic mode, change the Windows app theme or toggle high contrast to see the basemap update. Switch to manual mode to pick a basemap directly. Toggle the reference layers switch to show or hide labels and boundaries.",
        tags: new[] { "accessibility", "accessible", "basemap", "colorblind", "contrast", "dark", "enhanced", "high", "inclusive", "legibility", "light", "living atlas", "readability", "vision", "visual impairment", "WCAG" })]
    public partial class UpdateBasemapForContrastAccessibility
    {
        // Portal item URL for the authored high-contrast light web map.
        private const string HighContrastLightUrl =
            "https://www.arcgis.com/home/item.html?id=084291b0ecad4588b8c8853898d72445";

        // Portal item URL for the authored high-contrast dark web map.
        private const string HighContrastDarkUrl =
            "https://www.arcgis.com/home/item.html?id=3e23478909194c54992eaaee78b5f754";

        // Source of Windows theme and high-contrast change notifications.
        private readonly UISettings _uiSettings = new();

        // Track the last applied basemap choice to skip redundant rebuilds.
        private BasemapChoice? _lastAppliedChoice;

        public UpdateBasemapForContrastAccessibility()
        {
            InitializeComponent();

            // Setup the control references and execute initialization.
            Initialize();
        }

        private void Initialize()
        {
            // Create a new map with no basemap; the first appearance update will assign one.
            MyMapView.Map = new Map
            {
                InitialViewpoint = new Viewpoint(34.05, -117.19, 2e6)
            };

            // Hook the page lifecycle so OS appearance subscriptions are only active while loaded.
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Subscribe to UISettings.ColorValuesChanged for theme and high-contrast changes.
            _uiSettings.ColorValuesChanged += OnColorValuesChanged;

            // Rebuild the basemap to match the current Windows appearance.
            _ = UpdateBasemapAsync();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // Unsubscribe from OS appearance notifications.
            _uiSettings.ColorValuesChanged -= OnColorValuesChanged;
        }

        private async Task UpdateBasemapAsync()
        {
            // Resolve the basemap choice from the current mode and Windows appearance.
            BasemapChoice choice = ResolveBasemapChoice();

            // Skip if the same basemap is already applied.
            if (_lastAppliedChoice == choice)
                return;

            // Remember the new choice so duplicate notifications skip the rebuild.
            _lastAppliedChoice = choice;

            // Build the new basemap and assign it to the existing map.
            Basemap basemap = CreateBasemap(choice);
            MyMapView.Map.Basemap = basemap;

            try
            {
                // Wait for the new basemap to finish loading before applying reference layer visibility.
                await basemap.LoadAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Basemap failed to load: {ex.Message}");
                return;
            }

            // Apply the current reference-layer toggle state to the newly loaded basemap.
            ApplyReferenceLayerVisibility(basemap);
        }

        private BasemapChoice ResolveBasemapChoice()
        {
            // In automatic mode, pick a basemap from the current Windows theme and high-contrast state.
            if (AutomaticModeRadioButton.IsChecked == true)
            {
                // Read the current Windows high-contrast and background-brightness state.
                bool highContrast = IsHighContrastEnabled();
                bool light = IsBackgroundLight(highContrast, _uiSettings);

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
            if (DarkRadioButton.IsChecked == true)
                return BasemapChoice.Dark;

            if (HighContrastLightRadioButton.IsChecked == true)
                return BasemapChoice.HighContrastLight;

            if (HighContrastDarkRadioButton.IsChecked == true)
                return BasemapChoice.HighContrastDark;

            // Default to the light basemap.
            return BasemapChoice.Light;
        }

        private static Basemap CreateBasemap(BasemapChoice choice)
        {
            // Build the basemap for the requested choice. High-contrast variants come from authored portal items.
            return choice switch
            {
                BasemapChoice.Dark =>
                    new Basemap(BasemapStyle.ArcGISDarkGray),

                BasemapChoice.HighContrastLight =>
                    new Basemap(new Uri(HighContrastLightUrl)),

                BasemapChoice.HighContrastDark =>
                    new Basemap(new Uri(HighContrastDarkUrl)),

                _ =>
                    new Basemap(BasemapStyle.ArcGISLightGray)
            };
        }

        private void ApplyReferenceLayerVisibility(Basemap basemap)
        {
            if (basemap?.ReferenceLayers == null)
                return;

            // Read the desired visibility from the toggle switch.
            bool visible = ReferenceLayersToggle.IsOn;

            // Toggle each reference layer to match.
            foreach (Layer layer in basemap.ReferenceLayers)
            {
                layer.IsVisible = visible;
            }
        }

        // Switch between automatic and manual appearance modes.
        private void ContrastModeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ManualAppearancePanel == null)
                return;

            // Show the manual radio buttons only in manual mode.
            bool manual = ManualModeRadioButton.IsChecked == true;

            ManualAppearancePanel.Visibility = manual
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Show the Open Settings shortcuts only in automatic mode.
            OpenSettingsPanel.Visibility = manual
                ? Visibility.Collapsed
                : Visibility.Visible;

            // Force a rebuild because the resolved choice may differ even if the cached value matches.
            _lastAppliedChoice = null;
            _ = UpdateBasemapAsync();
        }

        // Apply a manually-selected basemap choice.
        private void ManualAppearanceRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ManualModeRadioButton.IsChecked == true)
            {
                // Reset the cache so the new manual choice always applies.
                _lastAppliedChoice = null;
                _ = UpdateBasemapAsync();
            }
        }

        // Toggle reference layer visibility on the current basemap.
        private void ReferenceLayersToggle_Toggled(object sender, RoutedEventArgs e)
        {
            ApplyReferenceLayerVisibility(MyMapView.Map?.Basemap);
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
        // High-contrast palettes are read from the system window color (via Win32 GetSysColor)
        // and classified using a weighted RGB sum that approximates Rec. 601 perceived luma
        // ((2*R + 5*G + B) / 8) compared against mid-gray (128).
        // Outside high contrast the Windows app theme is read from
        // UISettings.GetColorValue(UIColorType.Background): white = light, black = dark.
        private static bool IsBackgroundLight(bool highContrast, UISettings uiSettings)
        {
            if (highContrast)
            {
                // Read the system window color (returned as 0x00BBGGRR).
                uint color = GetSysColor(COLOR_WINDOW);

                byte r = (byte)(color & 0xFF);
                byte g = (byte)((color >> 8) & 0xFF);
                byte b = (byte)((color >> 16) & 0xFF);

                // Weighted RGB compared against mid-gray.
                return (5 * g + 2 * r + b) > 8 * 128;
            }

            // In dark mode UISettings returns black for Background; in light mode it returns white.
            Color background = uiSettings.GetColorValue(UIColorType.Background);
            return background.R > 127;
        }

        // Query the Win32 high-contrast accessibility flag via SystemParametersInfoW(SPI_GETHIGHCONTRAST).
        private static bool IsHighContrastEnabled()
        {
            var highContrast = new HIGHCONTRAST
            {
                cbSize = (uint)Marshal.SizeOf<HIGHCONTRAST>()
            };

            return SystemParametersInfoW(
                       SPI_GETHIGHCONTRAST,
                       highContrast.cbSize,
                       ref highContrast,
                       0)
                   && (highContrast.dwFlags & HCF_HIGHCONTRASTON) != 0;
        }

        // Windows theme or high-contrast changed; rebuild on the UI thread.
        // UISettings raises this on a background thread, so marshal to the UI queue
        // before touching any XAML property (IsChecked throws RPC_E_WRONG_THREAD off-thread).
        private void OnColorValuesChanged(UISettings sender, object args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (AutomaticModeRadioButton.IsChecked == true)
                    _ = UpdateBasemapAsync();
            });
        }

        // Open the Windows theme settings page.
        private async void OpenThemeSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:colors"));
        }

        // Open the Windows high-contrast settings page.
        private async void OpenContrastSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(
                new Uri("ms-settings:easeofaccess-highcontrast"));
        }

        // Win32 P/Invoke bindings used for high-contrast detection and system colors.
        private const uint SPI_GETHIGHCONTRAST = 0x0042;
        private const uint HCF_HIGHCONTRASTON = 0x00000001;
        private const int COLOR_WINDOW = 5;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfoW(
            uint uiAction,
            uint uiParam,
            ref HIGHCONTRAST pvParam,
            uint fWinIni);

        [DllImport("user32.dll")]
        private static extern uint GetSysColor(int nIndex);

        [StructLayout(LayoutKind.Sequential)]
        private struct HIGHCONTRAST
        {
            public uint cbSize;
            public uint dwFlags;
            public IntPtr lpszDefaultScheme;
        }

        #endregion
    }
}
