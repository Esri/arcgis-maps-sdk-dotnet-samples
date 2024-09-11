// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using System.Reflection;
using TabControl = System.Windows.Controls.TabControl;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;

namespace ArcGIS.WPF.Samples.StyleGeometryTypesWithSymbols
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Style geometry types with symbols",
        category: "Symbology",
        description: "Use a symbol to display a geometry on a map.",
        instructions: "Tap \"Edit Styles\" and select a geometry to edit with the picker. Use the controls to change the symbol properties for the geometry.",
        tags: new[] { "display", "fill", "graphics", "line", "marker", "overlay", "picture", "point", "symbol", "visualization" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class StyleGeometryTypesWithSymbols
    {
        // Item sources for the combo boxes.
        public List<SimpleMarkerSymbolStyle> SimpleMarkerSymbolStyles => Enum.GetValues(typeof(SimpleMarkerSymbolStyle)).Cast<SimpleMarkerSymbolStyle>().ToList();
        public List<SimpleLineSymbolStyle> SimpleLineSymbolStyles => Enum.GetValues(typeof(SimpleLineSymbolStyle)).Cast<SimpleLineSymbolStyle>().ToList();
        public List<SimpleFillSymbolStyle> SimpleFillSymbolStyles => Enum.GetValues(typeof(SimpleFillSymbolStyle)).Cast<SimpleFillSymbolStyle>().ToList();

        // Hold the selected graphic.
        private Graphic _selectedGraphic;

        // Flag indicating if styling should be applied to a polygon's fill or outline.
        private bool _stylingPolygonFill = true;

        public StyleGeometryTypesWithSymbols()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Set the data context for UI bindings.
            DataContext = this;

            // Create a new map with a topographic basemap initially centered on Woolgarston, England.
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic)
            {
                InitialViewpoint = new Viewpoint(new MapPoint(-225e3, 6_553e3, SpatialReferences.WebMercator), 88e3)
            };

            // A graphics overlay for displaying the geometry graphics on the map view.
            var graphicsOverlay = new GraphicsOverlay();

            // Create the simple marker symbol for styling the point.
            var pointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Purple, 12);

            // Create simple line symbol for styling the polyline.
            var polylineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.Red, 6);

            // Create the simple fill symbol for styling the polygon, including its outline.
            var polygonSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.ForwardDiagonal, Color.Blue,
                new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Green, 3));

            // Create a point graphic and add it to the graphics overlay.
            var point = new MapPoint(-225e3, 6_560e3, SpatialReferences.WebMercator);
            var pointGraphic = new Graphic(point, pointSymbol);
            graphicsOverlay.Graphics.Add(pointGraphic);

            // Create a polyline graphic and add it to the graphics overlay.
            var points = new MapPoint[2]
            {
                new MapPoint(-223e3, 6_559e3, SpatialReferences.WebMercator),
                new MapPoint(-227e3, 6_559e3, SpatialReferences.WebMercator)
            };
            var polyline = new Polyline(points, SpatialReferences.WebMercator);
            var polylineGraphic = new Graphic(polyline, polylineSymbol);
            graphicsOverlay.Graphics.Add(polylineGraphic);

            // Create a polygon graphic and add it to the graphics overlay.
            points = new MapPoint[4]
            {
                new MapPoint(-222e3, 6_558e3, SpatialReferences.WebMercator),
                new MapPoint(-228e3, 6_558e3, SpatialReferences.WebMercator),
                new MapPoint(-228e3, 6_555e3, SpatialReferences.WebMercator),
                new MapPoint(-222e3, 6_555e3, SpatialReferences.WebMercator)
            };
            var polygon = new Polygon(points, SpatialReferences.WebMercator);
            var polygonGraphic = new Graphic(polygon, polygonSymbol);
            graphicsOverlay.Graphics.Add(polygonGraphic);

            // Create a graphic with a picture marker symbol (image resource) and add it to the graphics overlay.
            var pinGraphic = await MakePictureMarkerSymbolFromImage(new MapPoint(-226_770, 6_550_470, SpatialReferences.WebMercator));
            graphicsOverlay.Graphics.Add(pinGraphic);

            // Create a graphic with a picture marker symbol (URL) and add it to the graphics overlay.
            var imageUri = new Uri("https://static.arcgis.com/images/Symbols/OutdoorRecreation/Camping.png");
            var campsiteSymbol = new PictureMarkerSymbol(imageUri)
            {
                Width = 25,
                Height = 25
            };
            var campsitePoint = new MapPoint(-223_560, 6_552_020, SpatialReferences.WebMercator);
            var campsiteGraphic = new Graphic(campsitePoint, campsiteSymbol);
            graphicsOverlay.Graphics.Add(campsiteGraphic);

            // Add the graphics overlay to the map view.
            MyMapView.GraphicsOverlays.Add(graphicsOverlay);
        }

        private async Task<Graphic> MakePictureMarkerSymbolFromImage(MapPoint point)
        {
            // Hold a reference to the picture marker symbol.
            PictureMarkerSymbol pinSymbol;

            // Get current assembly that contains the image.
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Get the resource name of the blue pin star image
            string resourceStreamName = this.GetType().Assembly.GetManifestResourceNames().Single(str => str.EndsWith("pin_star_blue.png"));

            // Load the resource stream
            using (Stream resourceStream = this.GetType().Assembly.GetManifestResourceStream(resourceStreamName))
            {
                // Create new symbol using asynchronous factory method from stream.
                pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
                pinSymbol.Width = 60;
                pinSymbol.Height = 60;
                // The image is a pin; offset the image so that the pinpoint is on the point rather than the image's true center.
                pinSymbol.LeaderOffsetX = 30;
                pinSymbol.OffsetY = 14;
            }

            return new Graphic(point, pinSymbol);
        }

        #region UI event handlers

        private void GeometryTypeTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the tab control.
            TabControl tabControl = sender as TabControl;

            // Update the selected graphic based on the selected tab.
            switch (tabControl.SelectedIndex)
            {
                // Point tab
                case 0:
                    _selectedGraphic = MyMapView.GraphicsOverlays[0].Graphics[0];
                    break;

                // Polyline tab
                case 1:
                    _selectedGraphic = MyMapView.GraphicsOverlays[0].Graphics[1];
                    break;

                // Polygon tab
                case 2:
                    _selectedGraphic = MyMapView.GraphicsOverlays[0].Graphics[2];
                    break;
            }
        }

        private void StyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Selected graphic will be null when initializing the UI.
            if (_selectedGraphic == null) return;

            // Get the selected combo box item.
            var comboBox = sender as System.Windows.Controls.ComboBox;

            // Update the symbol style based on the selected combo box item.
            switch (_selectedGraphic.Geometry.GeometryType)
            {
                case GeometryType.Point:
                    ((SimpleMarkerSymbol)_selectedGraphic.Symbol).Style = (SimpleMarkerSymbolStyle)comboBox.SelectedItem;
                    break;

                case GeometryType.Polyline:
                    ((SimpleLineSymbol)_selectedGraphic.Symbol).Style = (SimpleLineSymbolStyle)comboBox.SelectedItem;
                    break;

                case GeometryType.Polygon:
                    var symbol = (SimpleFillSymbol)_selectedGraphic.Symbol;
                    if (_stylingPolygonFill)
                    {
                        symbol.Style = (SimpleFillSymbolStyle)comboBox.SelectedItem;
                    }
                    else
                    {
                        symbol.Outline = new SimpleLineSymbol((SimpleLineSymbolStyle)comboBox.SelectedItem, symbol.Outline.Color, symbol.Outline.Width);
                    }
                    break;
            }
        }

        private void ColorDialogButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the color preview border.
            var border = (sender as Button).Tag as Border;

            // Create a color dialog with the initial color reflecting the current symbol color.
            var colorDialog = new ColorDialog()
            {
                Color = Viewer.Converters.ColorToSolidBrushConverter.ConvertBack((SolidColorBrush)border.Background)
            };

            // Show the color dialog.
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                border.Background = Viewer.Converters.ColorToSolidBrushConverter.Convert(colorDialog.Color);

                // Update the symbol color based on the selected geometry type.
                switch (_selectedGraphic.Geometry.GeometryType)
                {
                    case GeometryType.Point:
                        ((SimpleMarkerSymbol)_selectedGraphic.Symbol).Color = colorDialog.Color;
                        break;

                    case GeometryType.Polyline:
                        ((SimpleLineSymbol)_selectedGraphic.Symbol).Color = colorDialog.Color;
                        break;

                    case GeometryType.Polygon:
                        var symbol = (SimpleFillSymbol)_selectedGraphic.Symbol;
                        if (_stylingPolygonFill)
                        {
                            symbol.Color = colorDialog.Color;
                        }
                        else
                        {
                            symbol.Outline.Color = colorDialog.Color;
                        }
                        break;
                }
            }
        }

        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Selected graphic will be null when initializing the UI.
            if (_selectedGraphic == null) return;

            // Update the symbol size based on the selected geometry type.
            switch (_selectedGraphic.Geometry.GeometryType)
            {
                case GeometryType.Point:
                    ((SimpleMarkerSymbol)_selectedGraphic.Symbol).Size = e.NewValue;
                    break;

                case GeometryType.Polyline:
                    ((SimpleLineSymbol)_selectedGraphic.Symbol).Width = e.NewValue;
                    break;

                case GeometryType.Polygon:
                    ((SimpleFillSymbol)_selectedGraphic.Symbol).Outline.Width = e.NewValue;
                    break;
            }
        }

        #region Polygon styling

        private void PolygonFillStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _stylingPolygonFill = true;
            StyleComboBox_SelectionChanged(sender, e);
        }

        private void PolygonOutlineStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _stylingPolygonFill = false;
            StyleComboBox_SelectionChanged(sender, e);
        }

        private void PolygonFillColorDialogButton_Click(object sender, RoutedEventArgs e)
        {
            _stylingPolygonFill = true;
            ColorDialogButton_Click(sender, e);
        }

        private void PolygonOutlineColorDialogButton_Click(object sender, RoutedEventArgs e)
        {
            _stylingPolygonFill = false;
            ColorDialogButton_Click(sender, e);
        }

        #endregion Polygon styling

        #endregion UI event handlers
    }
}