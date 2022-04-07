// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using Xamarin.Forms;
using Esri.ArcGISRuntime.Data;
using System.Threading.Tasks;
using System.Linq;
using System;
using Colors = System.Drawing.Color;
using System.Collections.Generic;

namespace ArcGISRuntime.Samples.SketchOnMap
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Sketch on map",
        category: "GraphicsOverlay",
        description: "Use the Sketch Editor to edit or sketch a new point, line, or polygon geometry on to a map.",
        instructions: "Choose which geometry type to sketch from one of the available buttons. Choose from points, multipoints, polylines, polygons, freehand polylines, freehand polygons, circles, ellipses, triangles, arrows and rectangles.",
        tags: new[] { "draw", "edit" })]
    public partial class SketchOnMap : ContentPage
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

            // Set a viewpoint on the map view
            MyMapView.SetViewpoint(new Viewpoint(64.3286, -15.5314, 72223));

            // Fill the combo box with choices for the sketch modes (shapes)
            Array sketchModes = System.Enum.GetValues(typeof(SketchCreationMode));
            foreach(object mode in sketchModes)
            {
                SketchModePicker.Items.Add(mode.ToString());
            }

            SketchModePicker.SelectedIndex = 0;

            // Set a gray background color for Android
            if (Device.RuntimePlatform == Device.Android)
            {
                DrawToolsGrid.BackgroundColor = Color.Gray;
            }

            // Hack to get around linker being too aggressive - this should be done with binding.
            UndoButton.Command = MyMapView.SketchEditor.UndoCommand;
            RedoButton.Command = MyMapView.SketchEditor.RedoCommand;
            CompleteButton.Command = MyMapView.SketchEditor.CompleteCommand;
        }

        #region Graphic and symbol helpers
        private Graphic CreateGraphic(Esri.ArcGISRuntime.Geometry.Geometry geometry)
        {
            // Create a graphic to display the specified geometry
            Symbol symbol = null;
            switch (geometry.GeometryType)
            {
                // Symbolize with a fill symbol
                case GeometryType.Envelope:
                case GeometryType.Polygon:
                    {
                        symbol = new SimpleFillSymbol()
                        {
                            Color = Colors.Red,
                            Style = SimpleFillSymbolStyle.Solid
                        };
                        break;
                    }
                // Symbolize with a line symbol
                case GeometryType.Polyline:
                    {
                        symbol = new SimpleLineSymbol()
                        {
                            Color = Colors.Red,
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
                            Color = Colors.Red,
                            Style = SimpleMarkerSymbolStyle.Circle,
                            Size = 15d
                        };
                        break;
                    }
            }

            // pass back a new graphic with the appropriate symbol
            return new Graphic(geometry, symbol);
        }

        private async Task<Graphic> GetGraphicAsync()
        {
            // Wait for the user to click a location on the map
            MapPoint mapPoint = (MapPoint)await MyMapView.SketchEditor.StartAsync(SketchCreationMode.Point, false);

            // Convert the map point to a screen point
            var screenCoordinate = MyMapView.LocationToScreen(mapPoint);

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

        private async void DrawButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Hide the draw/edit tools
                DrawToolsGrid.IsVisible = false;

                // Let the user draw on the map view using the chosen sketch mode
                SketchCreationMode creationMode = (SketchCreationMode)SketchModePicker.SelectedIndex;
                Esri.ArcGISRuntime.Geometry.Geometry geometry = await MyMapView.SketchEditor.StartAsync(creationMode, true);
                
                // Create and add a graphic from the geometry the user drew
                Graphic graphic = CreateGraphic(geometry);
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
                await Application.Current.MainPage.DisplayAlert("Error", "Error drawing graphic shape: " + ex.Message, "OK");
            }
        }

        private void ClearButtonClick(object sender, EventArgs e)
        {
            // Remove all graphics from the graphics overlay
            _sketchOverlay.Graphics.Clear();

            // Cancel any uncompleted sketch
            if (MyMapView.SketchEditor.CancelCommand.CanExecute(null))
            {
                MyMapView.SketchEditor.CancelCommand.Execute(null);
            }

            // Disable buttons that require graphics
            ClearButton.IsEnabled = false;
            EditButton.IsEnabled = false;
        }

        private async void EditButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Hide the draw/edit tools
                DrawToolsGrid.IsVisible = false;

                // Allow the user to select a graphic
                Graphic editGraphic = await GetGraphicAsync();
                if (editGraphic == null) { return; }

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
                await Application.Current.MainPage.DisplayAlert("Error", "Error editing shape: " + ex.Message,"OK");
            }
        }

        private void ShowDrawTools(object sender, EventArgs e)
        {
            // Show the grid that contains the sketch mode, draw, and edit controls
            DrawToolsGrid.IsVisible = true;
        }
    }
}
