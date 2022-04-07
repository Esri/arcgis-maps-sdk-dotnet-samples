// Copyright 2017 Esri.
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
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.UI.Xaml;

namespace ArcGISRuntime.WinUI.Samples.SketchOnMap
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Sketch on map",
        category: "GraphicsOverlay",
        description: "Use the Sketch Editor to edit or sketch a new point, line, or polygon geometry on to a map.",
        instructions: "Choose which geometry type to sketch from one of the available buttons. Choose from points, multipoints, polylines, polygons, freehand polylines, freehand polygons, circles, ellipses, triangles, arrows and rectangles.",
        tags: new[] { "draw", "edit" })]
    public sealed partial class SketchOnMap
    {
        // Graphics overlay to host sketch graphics
        private GraphicsOverlay _sketchOverlay;

        public SketchOnMap()
        {
            InitializeComponent();

            // Call a function to set up the map and sketch editor
            Initialize();
        }

        private void Initialize()
        {
            // Create a map
            Map myMap = new Map(BasemapStyle.ArcGISImageryStandard);

            // Create graphics overlay to display sketch geometry
            _sketchOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_sketchOverlay);

            // Assign the map to the MapView
            MyMapView.Map = myMap;

            // set a viewpoint on the map view
            MyMapView.SetViewpoint(new Viewpoint(64.3286, -15.5314, 72223));

            // Set the sketch editor as the page's data context
            DataContext = MyMapView.SketchEditor;
        }

        #region Graphic and symbol helpers
        private Graphic SaveGraphic(Geometry geometry)
        {
            // Create a graphic to display the specified geometry
            Symbol symbol = null;
            if (geometry != null)
            {
                switch (geometry.GeometryType)
                {
                    // Symbolize with a fill symbol
                    case GeometryType.Envelope:
                    case GeometryType.Polygon:
                        {
                            symbol = new SimpleFillSymbol()
                            {
                                Color = Color.Red,
                                Style = SimpleFillSymbolStyle.Solid
                            };
                            break;
                        }
                    // Symbolize with a line symbol
                    case GeometryType.Polyline:
                        {
                            symbol = new SimpleLineSymbol()
                            {
                                Color = Color.Red,
                                Style = SimpleLineSymbolStyle.Solid,
                                Width = 5d
                            };
                            break;
                        }
                    // Symbolize with a marker symbol
                    case GeometryType.Point:
                    case GeometryType.Multipoint:
                        {

                            symbol = new SimpleMarkerSymbol()
                            {
                                Color = Color.Red,
                                Style = SimpleMarkerSymbolStyle.Circle,
                                Size = 15d
                            };
                            break;
                        }
                }

                // Pass back a new graphic with the appropriate symbol
                return new Graphic(geometry, symbol);
            }

            return null;
        }

        private async Task<Graphic> GetGraphicAsync()
        {
            // Wait for the user to click a location on the map
            MapPoint mapPoint = (MapPoint)await MyMapView.SketchEditor.StartAsync(SketchCreationMode.Point, false);

            // Convert the map point to a screen point
            Windows.Foundation.Point screenCoordinate = MyMapView.LocationToScreen(mapPoint);

            // Identify graphics in the graphics overlay using the point
            IReadOnlyList<IdentifyGraphicsOverlayResult> results = await MyMapView.IdentifyGraphicsOverlaysAsync(screenCoordinate, 2, false);

            // If results were found, get the first graphic
            Graphic graphic = null;
            IdentifyGraphicsOverlayResult idResult = results.FirstOrDefault();
            if (idResult != null && idResult.Graphics.Count > 0)
            {
                graphic = idResult.Graphics.FirstOrDefault();
            }

            // Return the graphic (or null if none were found)
            return graphic;
        }
        #endregion

        private void ShapeClick(object sender, RoutedEventArgs e)
        {
            // Get the command parameter from the button press
            string mode = (sender as Microsoft.UI.Xaml.Controls.Button).CommandParameter.ToString();

            // Check if the command parameter is defined in the SketchCreationMode enumerator
            if (Enum.IsDefined(typeof(SketchCreationMode), mode))
            {
                _ = DrawButtonClick((SketchCreationMode)Enum.Parse(typeof(SketchCreationMode), mode));
            }
        }

        private async Task DrawButtonClick(SketchCreationMode creationMode)
        {
            try
            {
                // Let the user draw on the map view using the chosen sketch mode
                Geometry geometry = await MyMapView.SketchEditor.StartAsync(creationMode, true);

                // Create and add a graphic from the geometry the user drew
                Graphic graphic = SaveGraphic(geometry);
                _sketchOverlay.Graphics.Add(graphic);

                // Enable/disable the clear and edit buttons according to whether or not graphics exist in the overlay
                ClearButton.IsEnabled = _sketchOverlay.Graphics.Count > 0;
                EditButton.IsEnabled = _sketchOverlay.Graphics.Count > 0;
            }
            catch (TaskCanceledException)
            {
                // Ignore ... let the user cancel drawing
            }
            catch (Exception ex)
            {
                // Report exceptions
                await new MessageDialog2("Error drawing graphic shape: " + ex.Message, ex.GetType().Name).ShowAsync();
            }
        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            // Remove all graphics from the graphics overlay
            _sketchOverlay.Graphics.Clear();

            // Disable buttons that require graphics
            ClearButton.IsEnabled = false;
            EditButton.IsEnabled = false;
        }

        private async void EditButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Allow the user to select a graphic
                Graphic editGraphic = await GetGraphicAsync();

                if (editGraphic == null)
                {
                    return;
                }

                // Let the user make changes to the graphic's geometry, await the result (updated geometry)
                Geometry newGeometry = await MyMapView.SketchEditor.StartAsync(editGraphic.Geometry);

                // Display the updated geometry in the graphic
                editGraphic.Geometry = newGeometry;
            }
            catch (TaskCanceledException)
            {
                // Ignore ... let the user cancel editing
            }
            catch (Exception ex)
            {
                // Report exceptions
                await new MessageDialog2("Error editing shape: " + ex.Message, ex.GetType().Name).ShowAsync();
            }
        }
    }
}