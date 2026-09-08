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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Point = System.Windows.Point;

namespace ArcGIS.WPF.Samples.NavigateMapViewAndIdentifyFeaturesWithKeyboard
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Navigate map view and identify features with keyboard",
        category: "Accessibility",
        description: "Perform all map navigation operations using only the keyboard.",
        instructions: "When the sample is launched, a fixed area of interest appears centered over the map, and any features inside it are automatically selected and labeled <kbd>1</kbd> – <kbd>9</kbd>. As you navigate, the selection and labels update to match the features currently inside the area of interest. Use the arrow keys to pan and <kbd>+</kbd> / <kbd>-</kbd> to zoom. Use <kbd>Alt</kbd> + <kbd>←</kbd> / <kbd>→</kbd> to rotate, with <kbd>Alt</kbd> + <kbd>↑</kbd> resetting the map to north. Press <kbd>1</kbd> – <kbd>9</kbd> to show a callout for the matching numbered feature, and press <kbd>Esc</kbd> to dismiss the callout.",
        tags: new[] { "WCAG", "accessibility", "accessible", "identify", "inclusive", "input", "interaction", "keyboard", "navigation", "selection" })]
    public partial class NavigateMapViewAndIdentifyFeaturesWithKeyboard
    {
        private const string NameAttribute = "name";

        private FeatureLayer _restaurantsLayer;
        private readonly GraphicsOverlay _labelOverlay = new GraphicsOverlay();
        private readonly List<Feature> _rectangleFeatures = new List<Feature>();

        private int _selectionRequestVersion;

        private static readonly Color MarkerFill = Color.FromArgb(255, 11, 79, 138);
        private static readonly Color SelectionHalo = Color.FromArgb(255, 190, 24, 93);
        private static readonly Color LabelText = Color.FromArgb(255, 31, 35, 40);

        public NavigateMapViewAndIdentifyFeaturesWithKeyboard()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create a light gray basemap centered on Redlands.
            Map map = new Map(BasemapStyle.ArcGISLightGray)
            {
                InitialViewpoint = new Viewpoint(new MapPoint(-117.1825, 34.0556, SpatialReferences.Wgs84), 2500)
            };

            // Create and symbolize the restaurants feature layer.
            Uri serviceUri = new Uri("https://services2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/rest/services/redlands_food/FeatureServer/0");
            _restaurantsLayer = new FeatureLayer(serviceUri)
            {
                Renderer = new SimpleRenderer(new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, MarkerFill, 12)
                {
                    Outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.White, 1.5)
                })
            };
            map.OperationalLayers.Add(_restaurantsLayer);

            // Display the map, selection color, and numbered-label overlay.
            MyMapView.Map = map;
            MyMapView.SelectionProperties.Color = SelectionHalo;
            MyMapView.GraphicsOverlays.Add(_labelOverlay);

            // Refresh the selection after navigation and handle number or Escape keys.
            MyMapView.NavigationCompleted += OnNavigationCompleted;
            MyMapView.PreviewKeyDown += OnMapPreviewKeyDown;
            MyMapView.DrawStatusChanged += OnInitialDrawCompleted;
        }

        private async void OnInitialDrawCompleted(object sender, DrawStatusChangedEventArgs e)
        {
            if (e.Status != DrawStatus.Completed) return;

            MyMapView.DrawStatusChanged -= OnInitialDrawCompleted;
            MyMapView.Focus();
            await SelectFeaturesInRectangleAsync();
        }

        private async void OnNavigationCompleted(object sender, EventArgs e)
        {
            MyMapView.Focus();
            await SelectFeaturesInRectangleAsync();
        }

        private async Task SelectFeaturesInRectangleAsync()
        {
            if (_restaurantsLayer?.FeatureTable is not ServiceFeatureTable table) return;

            int requestVersion = ++_selectionRequestVersion;

            // Convert the fixed rectangle from screen space to a map-space envelope.
            Point screenCenter = new Point(MyMapView.ActualWidth / 2, MyMapView.ActualHeight / 2);
            MapPoint mapCenter = MyMapView.ScreenToLocation(screenCenter);
            if (mapCenter == null) return;

            double rectangleHalfWidth = SelectionRectangle.Width / 2;
            MapPoint rightMap = MyMapView.ScreenToLocation(new Point(screenCenter.X + rectangleHalfWidth, screenCenter.Y));
            if (rightMap == null) return;

            double mapHalfWidth = GeometryEngine.Distance(mapCenter, rightMap);
            Envelope envelope = new Envelope(
                mapCenter.X - mapHalfWidth,
                mapCenter.Y - mapHalfWidth,
                mapCenter.X + mapHalfWidth,
                mapCenter.Y + mapHalfWidth,
                mapCenter.SpatialReference);

            QueryParameters query = new QueryParameters
            {
                Geometry = GeometryEngine.NormalizeCentralMeridian(envelope),
                SpatialRelationship = SpatialRelationship.Intersects
            };

            try
            {
                FeatureQueryResult results = await table.QueryFeaturesAsync(query, QueryFeatureFields.LoadAll);
                if (requestVersion != _selectionRequestVersion) return;

                // Order features by their screen position so numbering follows reading order.
                List<(Feature Feature, MapPoint Anchor, Point Screen)> ordered = new List<(Feature, MapPoint, Point)>();
                foreach (Feature feature in results)
                {
                    if (feature.Geometry is not MapPoint anchor) continue;
                    ordered.Add((feature, anchor, MyMapView.LocationToScreen(anchor)));
                }

                ordered.Sort((first, second) =>
                {
                    int yComparison = first.Screen.Y.CompareTo(second.Screen.Y);
                    return yComparison != 0 ? yComparison : first.Screen.X.CompareTo(second.Screen.X);
                });

                // Replace the previous selection only after the latest query completes.
                _restaurantsLayer.ClearSelection();
                _labelOverlay.Graphics.Clear();
                _rectangleFeatures.Clear();
                OverflowMessage.Visibility = ordered.Count > 9 ? Visibility.Visible : Visibility.Collapsed;

                int index = 1;
                foreach ((Feature feature, MapPoint anchor, _) in ordered)
                {
                    _restaurantsLayer.SelectFeature(feature);
                    if (index > 9) continue;

                    string name = GetFeatureName(feature, fallback: null);
                    string text = name != null ? $"{index}: {name}" : index.ToString();
                    TextSymbol label = new TextSymbol(
                        text,
                        LabelText,
                        15,
                        Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center,
                        Esri.ArcGISRuntime.Symbology.VerticalAlignment.Top)
                    {
                        HaloColor = Color.White,
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
                MessageBox.Show(ex.Message, "Identify error");
            }
        }

        private void OnMapPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                MyMapView.DismissCallout();
                SelectionRectangle.Visibility = Visibility.Visible;
                e.Handled = true;
                return;
            }

            int featureIndex;
            if (e.Key >= Key.D1 && e.Key <= Key.D9)
            {
                featureIndex = (int)e.Key - (int)Key.D1;
            }
            else if (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad9)
            {
                featureIndex = (int)e.Key - (int)Key.NumPad1;
            }
            else
            {
                return;
            }

            if (featureIndex < _rectangleFeatures.Count)
            {
                ShowCalloutForFeature(_rectangleFeatures[featureIndex]);
                e.Handled = true;
            }
        }

        private void ShowCalloutForFeature(Feature feature)
        {
            if (feature.Geometry is not MapPoint anchor) return;

            MapPoint wgs84Anchor = (MapPoint)GeometryEngine.Project(anchor, SpatialReferences.Wgs84);
            string name = GetFeatureName(feature, fallback: "Restaurant");
            string detail = $"Lat: {wgs84Anchor.Y:0.000000}\nLon: {wgs84Anchor.X:0.000000}";

            Point screen = MyMapView.LocationToScreen(anchor);
            MapPoint leaderAnchor = MyMapView.ScreenToLocation(new Point(screen.X, screen.Y - 4)) ?? anchor;

            MyMapView.ShowCalloutAt(leaderAnchor, new CalloutDefinition(name, detail));
            SelectionRectangle.Visibility = Visibility.Collapsed;
        }

        private static string GetFeatureName(Feature feature, string fallback)
        {
            return feature.Attributes.TryGetValue(NameAttribute, out object value) &&
                   value is string name &&
                   !string.IsNullOrWhiteSpace(name)
                ? name
                : fallback;
        }
    }
}
