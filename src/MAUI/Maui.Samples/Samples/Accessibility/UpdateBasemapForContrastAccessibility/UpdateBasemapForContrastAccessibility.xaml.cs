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
using Microsoft.Maui.ApplicationModel;
#if WINDOWS
using System.Linq;
using System.Runtime.InteropServices;
#endif
#if IOS || MACCATALYST
using Foundation;
using UIKit;
#endif
#if ANDROID
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using AndroidUiModeManager = Android.App.UiModeManager;
#endif

namespace ArcGIS.Samples.UpdateBasemapForContrastAccessibility
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Update basemap for contrast accessibility",
        category: "Accessibility",
        description: "Display a map view that updates between authored light, dark, and high-contrast basemaps.",
        instructions: "In automatic mode, change the device theme or toggle the OS high-contrast setting (Windows high contrast, iOS Increase Contrast, Android high contrast) to see the basemap update. Switch to manual mode to pick a basemap directly. Toggle the reference layers switch to show or hide labels and boundaries.",
        tags: new[] { "accessibility", "accessible", "basemap", "colorblind", "contrast", "dark", "enhanced", "high", "inclusive", "legibility", "light", "living atlas", "readability", "vision", "visual impairment", "WCAG" })]
    public partial class UpdateBasemapForContrastAccessibility
    {
        // Portal item ID for the authored high-contrast light web map.
        private const string HighContrastLightItemId = "084291b0ecad4588b8c8853898d72445";

        // Portal item ID for the authored high-contrast dark web map.
        private const string HighContrastDarkItemId = "3e23478909194c54992eaaee78b5f754";

        // Track the last applied basemap choice so duplicate OS notifications are dropped.
        private BasemapChoice? _lastAppliedChoice;

        public UpdateBasemapForContrastAccessibility()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create a new map centered on Southern California.
            MyMapView.Map = new Esri.ArcGISRuntime.Mapping.Map { InitialViewpoint = new Viewpoint(34.05, -117.19, 2e6) };

            // Apply the basemap matching the current OS appearance.
            _ = UpdateBasemapAsync();

            // Show the device-settings shortcuts only on platforms that can deep-link to them.
            OpenSettingsPanel.IsVisible = AutomaticModeRadioButton.IsChecked && CanOpenDeviceSettings();

            // Listen for theme changes from MAUI.
            if (Application.Current != null)
                Application.Current.RequestedThemeChanged += OnAppRequestedThemeChanged;

            // Listen for high-contrast changes from each platform's native API.
            SubscribeToContrastChanges();

            // Detach subscriptions when the page leaves the visual tree.
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, EventArgs e)
        {
            // Detach the MAUI theme listener.
            if (Application.Current != null)
                Application.Current.RequestedThemeChanged -= OnAppRequestedThemeChanged;

            // Detach the platform-specific contrast listeners.
            UnsubscribeFromContrastChanges();
        }

        // Resolve the basemap for the current mode and OS state, and assign it to the map.
        private async Task UpdateBasemapAsync()
        {
            // Pick a basemap choice from the current mode and OS settings.
            BasemapChoice choice = ResolveBasemapChoice();

            // Skip if the same basemap is already applied.
            if (_lastAppliedChoice == choice) return;
            _lastAppliedChoice = choice;

            // Build the new basemap.
            Basemap basemap;
            try
            {
                basemap = await CreateBasemapAsync(choice);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Basemap failed to load: {ex.Message}");
                return;
            }

            // Assign the basemap and refresh reference-layer visibility.
            MyMapView.Map.Basemap = basemap;
            ApplyReferenceLayerVisibility(basemap);
        }

        // Pick a basemap choice from the current contrast mode and OS state.
        private BasemapChoice ResolveBasemapChoice()
        {
            // In automatic mode, derive the choice from the OS theme and high-contrast state.
            if (AutomaticModeRadioButton.IsChecked)
            {
                bool highContrast = IsHighContrastEnabled();
                bool dark = IsDarkThemeEnabled();
                if (highContrast) return dark ? BasemapChoice.HighContrastDark : BasemapChoice.HighContrastLight;
                return dark ? BasemapChoice.Dark : BasemapChoice.Light;
            }

            // In manual mode, return the currently selected radio button.
            if (DarkRadioButton.IsChecked) return BasemapChoice.Dark;
            if (HighContrastLightRadioButton.IsChecked) return BasemapChoice.HighContrastLight;
            if (HighContrastDarkRadioButton.IsChecked) return BasemapChoice.HighContrastDark;
            return BasemapChoice.Light;
        }

        // Build the basemap for the given choice. High-contrast variants come from authored portal items.
        private static async Task<Basemap> CreateBasemapAsync(BasemapChoice choice)
        {
            switch (choice)
            {
                case BasemapChoice.Dark:
                    return new Basemap(BasemapStyle.ArcGISDarkGray);
                case BasemapChoice.HighContrastLight:
                    return new Basemap(await LoadPortalItemAsync(HighContrastLightItemId));
                case BasemapChoice.HighContrastDark:
                    return new Basemap(await LoadPortalItemAsync(HighContrastDarkItemId));
                default:
                    return new Basemap(BasemapStyle.ArcGISLightGray);
            }
        }

        // Load a portal item from the default portal.
        private static async Task<PortalItem> LoadPortalItemAsync(string itemId)
        {
            ArcGISPortal portal = await ArcGISPortal.CreateAsync();
            return await PortalItem.CreateAsync(portal, itemId);
        }

        // Show or hide the basemap's reference layers based on the switch.
        private async Task ApplyReferenceLayerVisibility(Basemap basemap)
        {
            // Skip if the basemap has no reference layers.
            if (basemap?.ReferenceLayers == null) return;

            // Read the desired visibility from the switch.
            bool visible = ReferenceLayersSwitch.IsToggled;
            
            // Ensure the basemap is loaded
            await basemap.LoadAsync();

            // Set each reference layer to that visibility.
            foreach (var layer in basemap.ReferenceLayers)
            {
                layer.IsVisible = visible;
            }
        }

        // Rebuild the basemap when MAUI's resolved app theme changes.
        private void OnAppRequestedThemeChanged(object sender, AppThemeChangedEventArgs e) =>
            _ = UpdateBasemapAsync();

        // Switch between automatic and manual appearance modes.
        private void ContrastModeRadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (ManualAppearancePanel == null) return;
            ManualAppearancePanel.IsVisible = ManualModeRadioButton.IsChecked;
            OpenSettingsPanel.IsVisible = !ManualModeRadioButton.IsChecked && CanOpenDeviceSettings();
            _lastAppliedChoice = null;
            _ = UpdateBasemapAsync();
        }

        // Apply a manually-selected basemap choice.
        private void ManualAppearanceRadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (ManualModeRadioButton.IsChecked && e.Value)
            {
                // Reset the cache so the new choice always applies.
                _lastAppliedChoice = null;
                _ = UpdateBasemapAsync();
            }
        }

        // Apply the reference-layer switch state to the current basemap.
        private void ReferenceLayersSwitch_Toggled(object sender, ToggledEventArgs e) =>
            ApplyReferenceLayerVisibility(MyMapView.Map?.Basemap);

        // Open the OS theme settings page.
        private async void OpenThemeSettingsButton_Clicked(object sender, EventArgs e) =>
            await OpenDeviceSettingsAsync(DeviceSettingsTarget.Theme);

        // Open the OS high-contrast settings page.
        private async void OpenContrastSettingsButton_Clicked(object sender, EventArgs e) =>
            await OpenDeviceSettingsAsync(DeviceSettingsTarget.Contrast);

        private enum BasemapChoice
        {
            Light,
            Dark,
            HighContrastLight,
            HighContrastDark
        }

        private enum DeviceSettingsTarget
        {
            Theme,
            Contrast
        }

        #region Platform scaffolding

#if WINDOWS
        // ThemeSettings exposes the OS high-contrast state and a Changed event that fires for both
        // theme and high-contrast transitions. Created lazily once the page has a platform window.
        private Microsoft.UI.System.ThemeSettings _themeSettings;
#endif
#if IOS || MACCATALYST
        // Observer token for the iOS "Increase Contrast" notification.
        private NSObject _darkerColorsObserver;
#endif
#if ANDROID
        // Fallback Settings.Secure key used by older Android versions and some OEM builds
        // for the high-contrast text accessibility setting.
        private const string HighContrastTextKey = "high_text_contrast_enabled";

        // Content observer for the legacy Secure key above.
        private HighContrastObserver _highContrastObserver;

        // Listener for the Android 14+ system "Color contrast" slider.
        private ContrastChangeListener _contrastChangeListener;
#endif

        // Subscribe to the platform's native high-contrast notifications.
        private void SubscribeToContrastChanges()
        {
#if WINDOWS
            // ThemeSettings is created lazily once the page is attached to a window.
            TrySubscribeWindowsThemeSettings();
            Loaded += OnPageLoadedWindows;
#endif
#if IOS || MACCATALYST
            // Listen for the iOS "Increase Contrast" toggle.
            _darkerColorsObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                UIApplication.DarkerSystemColorsStatusDidChangeNotification,
                notification => Dispatcher.Dispatch(() => _ = UpdateBasemapAsync()));
#endif
#if ANDROID
            var androidCtx = Android.App.Application.Context;

            // Listen for changes to the legacy "high contrast text" Secure key.
            _highContrastObserver = new HighContrastObserver(() => Dispatcher.Dispatch(() => _ = UpdateBasemapAsync()));
            androidCtx.ContentResolver?.RegisterContentObserver(
                Settings.Secure.GetUriFor(HighContrastTextKey)!,
                false,
                _highContrastObserver);

            // Also listen for the Android 14+ system "Color contrast" slider when the device exposes it.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.UpsideDownCake &&
                androidCtx.GetSystemService(Context.UiModeService) is AndroidUiModeManager uiModeManager &&
                androidCtx.MainExecutor is { } mainExecutor)
            {
                _contrastChangeListener = new ContrastChangeListener(() => Dispatcher.Dispatch(() => _ = UpdateBasemapAsync()));
                uiModeManager.AddContrastChangeListener(mainExecutor, _contrastChangeListener);
            }
#endif
        }

        // Detach the listeners registered in SubscribeToContrastChanges.
        private void UnsubscribeFromContrastChanges()
        {
#if WINDOWS
            Loaded -= OnPageLoadedWindows;
            if (_themeSettings != null)
            {
                _themeSettings.Changed -= OnThemeSettingsChanged;
                _themeSettings = null;
            }
#endif
#if IOS || MACCATALYST
            if (_darkerColorsObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_darkerColorsObserver);
                _darkerColorsObserver.Dispose();
                _darkerColorsObserver = null;
            }
#endif
#if ANDROID
            if (_highContrastObserver != null)
            {
                Android.App.Application.Context.ContentResolver?.UnregisterContentObserver(_highContrastObserver);
                _highContrastObserver.Dispose();
                _highContrastObserver = null;
            }

            if (_contrastChangeListener != null)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.UpsideDownCake &&
                    Android.App.Application.Context.GetSystemService(Context.UiModeService) is AndroidUiModeManager uiModeManager)
                {
                    uiModeManager.RemoveContrastChangeListener(_contrastChangeListener);
                }
                _contrastChangeListener.Dispose();
                _contrastChangeListener = null;
            }
#endif
        }

        // Return true when the OS reports dark mode. Windows in high-contrast reports the HC scheme's
        // background color instead of the user's light/dark preference.
        private bool IsDarkThemeEnabled()
        {
#if ANDROID
            // Read the configuration's UI night-mode flag.
            var ctx = Android.App.Application.Context;
            var nightMode = (ctx.Resources?.Configuration?.UiMode ?? 0) & Android.Content.Res.UiMode.NightMask;
            return nightMode == Android.Content.Res.UiMode.NightYes;
#elif WINDOWS
            if (IsHighContrastEnabled())
            {
                // In high contrast, classify the HC scheme's window color by perceived luma (Rec. 601 approximation).
                uint c = GetSysColor(COLOR_WINDOW);
                byte r = (byte)(c & 0xFF);
                byte g = (byte)((c >> 8) & 0xFF);
                byte b = (byte)((c >> 16) & 0xFF);
                return (5 * g + 2 * r + b) <= 8 * 128;
            }
            // Outside high contrast, defer to MAUI's resolved app theme.
            return Application.Current?.RequestedTheme == AppTheme.Dark;
#else
            return Application.Current?.RequestedTheme == AppTheme.Dark;
#endif
        }

        // Return true when the OS reports high-contrast mode.
        private bool IsHighContrastEnabled()
        {
#if WINDOWS
            // ThemeSettings is null until the page is loaded and the platform window is available.
            return _themeSettings?.HighContrast ?? false;
#elif IOS || MACCATALYST
            // iOS exposes "Increase Contrast" as DarkerSystemColorsEnabled.
            return UIAccessibility.DarkerSystemColorsEnabled;
#elif ANDROID
            var androidCtx = Android.App.Application.Context;

            // Check the Android 14+ "Color contrast" slider when the device exposes it.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.UpsideDownCake &&
                androidCtx.GetSystemService(Context.UiModeService) is AndroidUiModeManager uiModeManager &&
                uiModeManager.Contrast > 0f)
            {
                return true;
            }

            // Fall back to the legacy "high contrast text" Secure key.
            var resolver = androidCtx.ContentResolver;
            return resolver != null &&
                   Settings.Secure.GetInt(resolver, HighContrastTextKey, 0) == 1;
#else
            return false;
#endif
        }

#if WINDOWS
        // Rebuild the basemap when the OS theme or high-contrast state changes.
        private void OnThemeSettingsChanged(Microsoft.UI.System.ThemeSettings sender, object args) =>
            _ = UpdateBasemapAsync();

        // Retry the lazy ThemeSettings subscription now that the page has a platform window.
        private void OnPageLoadedWindows(object sender, EventArgs e) => TrySubscribeWindowsThemeSettings();

        // Resolve the platform WindowId and create ThemeSettings for it.
        // Goes through the Window/HWND because the page's XamlRoot returns a WindowId from inside
        // MAUI's hosted tree that ThemeSettings doesn't bind to.
        private void TrySubscribeWindowsThemeSettings()
        {
            if (_themeSettings != null) return;

            // Walk the MAUI handler chain to the native Microsoft.UI.Xaml.Window.
            var mauiWindow = this.Window
                ?? Microsoft.Maui.Controls.Application.Current?.Windows?.FirstOrDefault();
            var nativeWindow = mauiWindow?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (nativeWindow == null) return;

            // Convert the window to a WindowId via its HWND.
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);

            // Create the ThemeSettings instance and start listening for changes.
            _themeSettings = Microsoft.UI.System.ThemeSettings.CreateForWindowId(windowId);
            _themeSettings.Changed += OnThemeSettingsChanged;

            // Re-evaluate now that the HC state is finally readable.
            _lastAppliedChoice = null;
            _ = UpdateBasemapAsync();
        }
#endif

        // Return true on platforms that can deep-link to OS settings pages.
        // iOS and Mac Catalyst are excluded because Apple's supported public API only
        // opens this app's own Settings entry.
        private static bool CanOpenDeviceSettings()
        {
#if WINDOWS || ANDROID
            return true;
#else
            return false;
#endif
        }

        // Launch the OS settings page for the requested target.
        private static async Task OpenDeviceSettingsAsync(DeviceSettingsTarget target)
        {
            try
            {
#if WINDOWS
                // Windows ms-settings deep-link URIs.
                string uri = target == DeviceSettingsTarget.Theme
                    ? "ms-settings:colors"
                    : "ms-settings:easeofaccess-highcontrast";
                await Launcher.OpenAsync(new Uri(uri));
#elif ANDROID
                // Android system settings intents.
                string action = target == DeviceSettingsTarget.Theme
                    ? Settings.ActionDisplaySettings
                    : Settings.ActionAccessibilitySettings;
                var intent = new Intent(action);
                intent.AddFlags(ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intent);
                await Task.CompletedTask;
#else
                await Task.CompletedTask;
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Open settings failed: {ex}");
            }
        }

#if WINDOWS
        // Win32 P/Invoke for the high-contrast scheme's window color.
        private const int COLOR_WINDOW = 5;

        [DllImport("user32.dll")]
        private static extern uint GetSysColor(int nIndex);
#endif

#if ANDROID
        // Content observer that fires when an Android Settings.Secure value changes.
        private class HighContrastObserver : ContentObserver
        {
            private readonly Action _onChange;

            public HighContrastObserver(Action onChange) : base(new Handler(Looper.MainLooper!))
            {
                _onChange = onChange;
            }

            public override void OnChange(bool selfChange) => _onChange();
        }

        // Listener for Android 14+ "Color contrast" slider changes.
        private class ContrastChangeListener : Java.Lang.Object, AndroidUiModeManager.IContrastChangeListener
        {
            private readonly Action _onChange;

            public ContrastChangeListener(Action onChange) { _onChange = onChange; }

            public void OnContrastChanged(float contrast) => _onChange();
        }
#endif

        #endregion
    }
}
