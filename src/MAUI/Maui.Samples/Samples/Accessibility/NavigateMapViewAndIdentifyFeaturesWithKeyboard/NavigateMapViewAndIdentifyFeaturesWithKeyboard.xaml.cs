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
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
#if IOS || MACCATALYST
using Foundation;
using UIKit;
#endif

using Point = Microsoft.Maui.Graphics.Point;

namespace ArcGIS.Samples.NavigateMapViewAndIdentifyFeaturesWithKeyboard
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Navigate map view and identify features with keyboard",
        category: "Accessibility",
        description: "Perform all map navigation operations using only the keyboard.",
        instructions: "When the sample is launched, a fixed area of interest appears centered over the map, and any features inside it are automatically selected and labeled <kbd>1</kbd> – <kbd>9</kbd>. As you navigate, the selection and labels update to match the features currently inside the area of interest. Use the arrow keys to pan and <kbd>+</kbd> / <kbd>-</kbd> to zoom. Use <kbd>Alt</kbd> + <kbd>←</kbd> / <kbd>→</kbd> to rotate, with <kbd>Alt</kbd> + <kbd>↑</kbd> resetting the map to north. Press <kbd>1</kbd> – <kbd>9</kbd> to show a callout for the matching numbered feature, and press <kbd>Esc</kbd> to dismiss the callout.",
        tags: new[] { "WCAG", "accessibility", "accessible", "identify", "inclusive", "input", "interaction", "keyboard", "navigation", "selection" })]
    [ArcGIS.Samples.Shared.Attributes.ClassFile("./MauiProgram.cs")]
    public partial class NavigateMapViewAndIdentifyFeaturesWithKeyboard
    {
        // Attribute used to title and label each feature.
        private const string NameAttribute = "name";

        // Feature layer holding the restaurants displayed and identified by the sample.
        private FeatureLayer _restaurantsLayer;

        // Overlay for the numbered 1-9 labels.
        private readonly GraphicsOverlay _labelOverlay = new GraphicsOverlay();

        // Features currently in the area of interest, indexed by the matching number key.
        private readonly List<Feature> _rectangleFeatures = new List<Feature>();

        private int _selectionRequestVersion;
        private bool _isErrorDialogOpen;

#if WINDOWS || ANDROID || IOS || MACCATALYST
        private Esri.ArcGISRuntime.UI.Controls.MapView _nativeMapView;
#endif

        public NavigateMapViewAndIdentifyFeaturesWithKeyboard()
        {
            InitializeComponent();
#if IOS || MACCATALYST
            MyMapView.AppleKeyDownHandler = OnAppleKeyDown;
#endif
            Initialize();
        }

        private void Initialize()
        {
            // Create a light gray basemap centered on Redlands.
            Map map = new Map(BasemapStyle.ArcGISLightGray)
            {
                InitialViewpoint = new Viewpoint(new MapPoint(-117.1825, 34.0556, SpatialReferences.Wgs84), 2500)
            };

            // Create the restaurants feature layer.
            Uri serviceUri = new Uri("https://services2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/rest/services/redlands_food/FeatureServer/0");
            _restaurantsLayer = new FeatureLayer(serviceUri)
            {
                // Symbolize each restaurant as a filled circle with a white outline.
                Renderer = new SimpleRenderer(new SimpleMarkerSymbol(
                    SimpleMarkerSymbolStyle.Circle,
                    System.Drawing.Color.FromArgb(255, 11, 79, 138),
                    12)
                {
                    Outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.White, 1.5)
                })
            };

            // Add the feature layer to the map.
            map.OperationalLayers.Add(_restaurantsLayer);

            // Display the map and apply the selection halo color.
            MyMapView.Map = map;
            MyMapView.SelectionProperties.Color = System.Drawing.Color.FromArgb(255, 190, 24, 93);

            // Add the label overlay on top of the map.
            MyMapView.GraphicsOverlays.Add(_labelOverlay);

            // Refresh the selection after every pan, zoom, or rotation.
            MyMapView.NavigationCompleted += OnNavigationCompleted;

            // Trigger the initial selection once the view finishes its first draw.
            MyMapView.DrawStatusChanged += OnInitialDrawCompleted;

            // Native key events preserve the MapView's built-in keyboard navigation while adding 1-9 and Escape.
            MyMapView.HandlerChanged += OnMapViewHandlerChanged;
            Loaded += OnPageLoaded;
            Unloaded += OnPageUnloaded;
        }

        private async void OnInitialDrawCompleted(object sender, DrawStatusChangedEventArgs e)
        {
            if (e.Status != DrawStatus.Completed) return;

            MyMapView.DrawStatusChanged -= OnInitialDrawCompleted;
            FocusKeyboardInput();
            await SelectFeaturesInRectangleAsync();
        }

        private async void OnNavigationCompleted(object sender, EventArgs e)
        {
            FocusKeyboardInput();
            await SelectFeaturesInRectangleAsync();
        }

        private async Task SelectFeaturesInRectangleAsync()
        {
            if (_restaurantsLayer?.FeatureTable is not ServiceFeatureTable table) return;

            int requestVersion = ++_selectionRequestVersion;

            // Get the center point of the MapView in map space.
            Point screenCenter = new Point(MyMapView.Width / 2, MyMapView.Height / 2);
            MapPoint mapCenter = MyMapView.ScreenToLocation(screenCenter);
            if (mapCenter == null) return;

            // Measure the rectangle's half-width in map units.
            double rectangleHalfWidth = SelectionRectangle.WidthRequest / 2;
            MapPoint rightMap = MyMapView.ScreenToLocation(new Point(screenCenter.X + rectangleHalfWidth, screenCenter.Y));
            if (rightMap == null) return;

            // Build a square envelope matching the rectangle's footprint in map space.
            double mapHalfWidth = GeometryEngine.Distance(mapCenter, rightMap);
            Envelope envelope = new Envelope(
                mapCenter.X - mapHalfWidth,
                mapCenter.Y - mapHalfWidth,
                mapCenter.X + mapHalfWidth,
                mapCenter.Y + mapHalfWidth,
                mapCenter.SpatialReference);

            // Query for features that intersect the envelope. Normalize for crossings of the antimeridian.
            QueryParameters query = new QueryParameters
            {
                Geometry = GeometryEngine.NormalizeCentralMeridian(envelope),
                SpatialRelationship = SpatialRelationship.Intersects
            };

            try
            {
                FeatureQueryResult results = await table.QueryFeaturesAsync(query, QueryFeatureFields.LoadAll);
                if (requestVersion != _selectionRequestVersion) return;

                // Project each result to its on-screen position so labels can be ordered by reading order.
                List<(Feature Feature, MapPoint Anchor, Point Screen)> ordered = new List<(Feature, MapPoint, Point)>();
                foreach (Feature feature in results)
                {
                    if (feature.Geometry is not MapPoint anchor) continue;
                    ordered.Add((feature, anchor, MyMapView.LocationToScreen(anchor)));
                }

                // Sort top-to-bottom, then left-to-right.
                ordered.Sort((first, second) =>
                {
                    int yComparison = first.Screen.Y.CompareTo(second.Screen.Y);
                    return yComparison != 0 ? yComparison : first.Screen.X.CompareTo(second.Screen.X);
                });

                // Replace the previous selection only after the latest query completes.
                _restaurantsLayer.ClearSelection();
                _labelOverlay.Graphics.Clear();
                _rectangleFeatures.Clear();
                OverflowMessage.IsVisible = ordered.Count > 9;

                int index = 1;
                foreach ((Feature feature, MapPoint anchor, _) in ordered)
                {
                    _restaurantsLayer.SelectFeature(feature);

                    // Only the first nine features have a matching number key.
                    if (index > 9) continue;

                    string name = GetFeatureName(feature, fallback: null);
                    string text = name != null ? $"{index}: {name}" : index.ToString();
                    TextSymbol label = new TextSymbol(
                        text,
                        System.Drawing.Color.FromArgb(255, 31, 35, 40),
                        15,
                        Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center,
                        Esri.ArcGISRuntime.Symbology.VerticalAlignment.Top)
                    {
                        HaloColor = System.Drawing.Color.White,
                        HaloWidth = 2,
                        OffsetY = -14
                    };

                    _labelOverlay.Graphics.Add(new Graphic(anchor, label));
                    _rectangleFeatures.Add(feature);
                    index++;
                }
            }
            catch (Exception ex)
            {
                await ShowIdentifyErrorAsync(ex.Message);
            }
        }

        private bool ShowCalloutForIndex(int featureIndex)
        {
            if (featureIndex < 0 || featureIndex >= _rectangleFeatures.Count) return false;

            ShowCalloutForFeature(_rectangleFeatures[featureIndex]);
            return true;
        }

        private void DismissCallout()
        {
            MyMapView.DismissCallout();
            SelectionRectangle.IsVisible = true;
        }

        private void ShowCalloutForFeature(Feature feature)
        {
            if (feature.Geometry is not MapPoint anchor) return;

            // Project the anchor to WGS84 for the latitude and longitude readout.
            MapPoint wgs84Anchor = (MapPoint)GeometryEngine.Project(anchor, SpatialReferences.Wgs84);
            string name = GetFeatureName(feature, fallback: "Restaurant");
            string detail = $"Lat: {wgs84Anchor.Y:0.000000}\nLon: {wgs84Anchor.X:0.000000}";

            // Offset the callout so the restaurant feature remains unobstructed.
            Point screen = MyMapView.LocationToScreen(anchor);
            MapPoint leaderAnchor = MyMapView.ScreenToLocation(new Point(screen.X, screen.Y - 4)) ?? anchor;

            MyMapView.ShowCalloutAt(leaderAnchor, new CalloutDefinition(name, detail));
            SelectionRectangle.IsVisible = false;
        }

        private static string GetFeatureName(Feature feature, string fallback)
        {
            return feature.Attributes.TryGetValue(NameAttribute, out object value) &&
                   value is string name &&
                   !string.IsNullOrWhiteSpace(name)
                ? name
                : fallback;
        }

        private async Task ShowIdentifyErrorAsync(string message)
        {
            if (_isErrorDialogOpen) return;

            _isErrorDialogOpen = true;
            try
            {
                await Application.Current.Windows[0].Page.DisplayAlertAsync("Identify error", message, "OK");
            }
            finally
            {
                _isErrorDialogOpen = false;
            }
        }

        private void OnPageLoaded(object sender, EventArgs e) => AttachKeyboardInput();

        private void OnPageUnloaded(object sender, EventArgs e)
        {
            DetachKeyboardInput();
            _selectionRequestVersion++;
        }

        private void OnMapViewHandlerChanged(object sender, EventArgs e) => AttachKeyboardInput();

        private void AttachKeyboardInput()
        {
#if WINDOWS || ANDROID || IOS || MACCATALYST
            DetachKeyboardInput();
            if (MyMapView.Handler?.PlatformView is not Esri.ArcGISRuntime.UI.Controls.MapView nativeMapView) return;

            _nativeMapView = nativeMapView;
#if WINDOWS
            _nativeMapView.KeyDown += OnNativeMapViewKeyDown;
#elif ANDROID
            _nativeMapView.Focusable = true;
            _nativeMapView.FocusableInTouchMode = true;
            _nativeMapView.KeyPress += OnNativeMapViewKeyPress;
#endif
            FocusKeyboardInput();
#endif
        }

        private void DetachKeyboardInput()
        {
#if WINDOWS || ANDROID || IOS || MACCATALYST
            if (_nativeMapView == null) return;

#if WINDOWS
            _nativeMapView.KeyDown -= OnNativeMapViewKeyDown;
#elif ANDROID
            _nativeMapView.KeyPress -= OnNativeMapViewKeyPress;
#endif
            _nativeMapView = null;
#endif
        }

        private void FocusKeyboardInput()
        {
#if WINDOWS
            _nativeMapView?.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
#elif ANDROID
            _nativeMapView?.RequestFocus();
#elif IOS || MACCATALYST
            _nativeMapView?.BecomeFirstResponder();
#endif
        }

#if WINDOWS
        private void OnNativeMapViewKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                DismissCallout();
                e.Handled = true;
                return;
            }

            int featureIndex = e.Key switch
            {
                >= Windows.System.VirtualKey.Number1 and <= Windows.System.VirtualKey.Number9 =>
                    (int)e.Key - (int)Windows.System.VirtualKey.Number1,
                >= Windows.System.VirtualKey.NumberPad1 and <= Windows.System.VirtualKey.NumberPad9 =>
                    (int)e.Key - (int)Windows.System.VirtualKey.NumberPad1,
                _ => -1
            };

            if (ShowCalloutForIndex(featureIndex))
            {
                e.Handled = true;
            }
        }
#elif ANDROID
        private void OnNativeMapViewKeyPress(object sender, Android.Views.View.KeyEventArgs e)
        {
            if (e.Event?.Action != Android.Views.KeyEventActions.Down) return;

            if (e.KeyCode == Android.Views.Keycode.Escape)
            {
                DismissCallout();
                e.Handled = true;
                return;
            }

            int featureIndex = e.KeyCode switch
            {
                >= Android.Views.Keycode.Num1 and <= Android.Views.Keycode.Num9 =>
                    (int)e.KeyCode - (int)Android.Views.Keycode.Num1,
                >= Android.Views.Keycode.Numpad1 and <= Android.Views.Keycode.Numpad9 =>
                    (int)e.KeyCode - (int)Android.Views.Keycode.Numpad1,
                _ => -1
            };

            if (ShowCalloutForIndex(featureIndex))
            {
                e.Handled = true;
            }
        }
#elif IOS || MACCATALYST
        private bool OnAppleKeyDown(string input)
        {
            if (input == "\u001b")
            {
                DismissCallout();
                return true;
            }

            if (input.Length == 1 && input[0] >= '1' && input[0] <= '9')
            {
                return ShowCalloutForIndex(input[0] - '1');
            }

            return false;
        }
#endif
    }

    // Custom MAUI view used to register the sample-specific native Apple handler.
    public sealed class KeyboardMapView : Esri.ArcGISRuntime.Maui.MapView
    {
#if IOS || MACCATALYST
        internal Func<string, bool> AppleKeyDownHandler { get; set; }
#endif
    }

    // Create a native MapView subclass on Apple platforms without changing the handler for other samples.
    public sealed class KeyboardMapViewHandler : Esri.ArcGISRuntime.Maui.Handlers.MapViewHandler
    {
#if IOS || MACCATALYST
        protected override Esri.ArcGISRuntime.UI.Controls.MapView CreatePlatformView() =>
            new AppleKeyboardMapView();

        protected override void ConnectHandler(Esri.ArcGISRuntime.UI.Controls.MapView platformView)
        {
            base.ConnectHandler(platformView);

            if (platformView is AppleKeyboardMapView appleMapView &&
                VirtualView is KeyboardMapView keyboardMapView)
            {
                appleMapView.KeyDownHandler = input =>
                    keyboardMapView.AppleKeyDownHandler?.Invoke(input) == true;
            }
        }

        protected override void DisconnectHandler(Esri.ArcGISRuntime.UI.Controls.MapView platformView)
        {
            if (platformView is AppleKeyboardMapView appleMapView)
            {
                appleMapView.KeyDownHandler = null;
            }

            base.DisconnectHandler(platformView);
        }

        private sealed class AppleKeyboardMapView : Esri.ArcGISRuntime.UI.Controls.MapView
        {
            internal Func<string, bool> KeyDownHandler { get; set; }

            public override void PressesBegan(NSSet<UIPress> presses, UIPressesEvent evt)
            {
                // Consume only sample shortcuts and leave navigation keys with the ArcGIS MapView.
                List<UIPress> unhandledPresses = new List<UIPress>();
                foreach (UIPress press in presses)
                {
                    string input = GetInput(press.Key);
                    if (input == null || KeyDownHandler?.Invoke(input) != true)
                    {
                        unhandledPresses.Add(press);
                    }
                }

                if (unhandledPresses.Count == (int)presses.Count)
                {
                    base.PressesBegan(presses, evt);
                }
                else if (unhandledPresses.Count > 0)
                {
                    using NSSet<UIPress> remainingPresses = new NSSet<UIPress>(unhandledPresses.ToArray());
                    base.PressesBegan(remainingPresses, evt);
                }
            }

            public override void MovedToWindow()
            {
                base.MovedToWindow();
                if (Window != null)
                {
                    BecomeFirstResponder();
                }
            }

            private static string GetInput(UIKey key)
            {
                if (key == null) return null;

                int keyCode = (int)key.KeyCode;

                // Number-row and numpad HID usages are independent of the active keyboard layout.
                if (keyCode is >= 30 and <= 38)
                {
                    return ((char)('1' + keyCode - 30)).ToString();
                }
                if (keyCode is >= 89 and <= 97)
                {
                    return ((char)('1' + keyCode - 89)).ToString();
                }

                return keyCode == 41 ? "\u001b" : key.CharactersIgnoringModifiers;
            }
        }
#endif
    }
}