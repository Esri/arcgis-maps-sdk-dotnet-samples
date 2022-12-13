// Copyright 2022 Esri.
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
using Colors = System.Drawing.Color;

namespace ArcGIS.Samples.SketchOnMap
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Sketch on map",
        category: "GraphicsOverlay",
        description: "Use the Sketch Editor to edit or sketch a new point, line, or polygon geometry on to a map.",
        instructions: "Choose which geometry type to sketch from one of the available buttons. Choose from points, multipoints, polylines, polygons, freehand polylines, freehand polygons, circles, ellipses, triangles, arrows and rectangles.",
        tags: new[] { "draw", "edit" })]
    public partial class SketchOnMap : ContentPage
    {
        // Graphics overlay to host sketch graphics.
        private GraphicsOverlay _sketchOverlay;

        // Background colors for tool icons.
        private static SolidColorBrush ThemedColor;
        private static SolidColorBrush Red;

        // Button for keeping track of the currently enabled tool.
        private static Button EnabledTool;

        private TaskCompletionSource<Graphic> _graphicCompletionSource;

        private string[] _sketchModes;

        private static int _drawModeIndex;

        public SketchOnMap()
        {
            InitializeComponent();

            // Call a function to set up the map and sketch editor.
            Initialize();
        }

        private void Initialize()
        {
            // Create a map.
            Map myMap = new Map(BasemapStyle.ArcGISImageryStandard);

            // Create graphics overlay to display sketch geometry.
            _sketchOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_sketchOverlay);

            // Assign the map to the MapView.
            MyMapView.Map = myMap;

            // Set a viewpoint on the map view.
            MyMapView.SetViewpoint(new Viewpoint(64.3286, -15.5314, 72223));

            // Create a list of draw modes.
            _sketchModes = Enum.GetNames(typeof(SketchCreationMode));

            // Hack to get around linker being too aggressive - this should be done with binding.
            UndoButton.Command = MyMapView.SketchEditor.UndoCommand;
            RedoButton.Command = MyMapView.SketchEditor.RedoCommand;
            CompleteButton.Command = MyMapView.SketchEditor.CompleteCommand;
            CancelButton.Command = MyMapView.SketchEditor.CancelCommand;

            // Ensure colors are consistent with XAML colors.
            ThemedColor = (SolidColorBrush)Sketch.Background;
            Red = new SolidColorBrush(Microsoft.Maui.Graphics.Color.FromRgb(255, 0, 0));

            // No tool currently selected, so simply instantiate the button.
            EnabledTool = new Button();
        }

        #region Graphic and symbol helpers

        private Graphic CreateGraphic(Esri.ArcGISRuntime.Geometry.Geometry geometry)
        {
            // Create a graphic to display the specified geometry.
            Symbol symbol = null;
            switch (geometry.GeometryType)
            {
                // Symbolize with a fill symbol.
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
                // Symbolize with a line symbol.
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
                // Symbolize with a marker symbol.
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

            // Pass back a new graphic with the appropriate symbol.
            return new Graphic(geometry, symbol);
        }

        #endregion Graphic and symbol helpers

        private async void StartSketch(object sender, EventArgs e)
        {
            try
            {
                // Let the user draw on the map view using the chosen sketch mode.
                SketchCreationMode creationMode = (SketchCreationMode)_drawModeIndex;
                Esri.ArcGISRuntime.Geometry.Geometry geometry = await MyMapView.SketchEditor.StartAsync(creationMode, true);

                // Create and add a graphic from the geometry the user drew.
                Graphic graphic = CreateGraphic(geometry);
                _sketchOverlay.Graphics.Add(graphic);

                // Enable/disable the clear and edit buttons according to whether or not graphics exist in the overlay.
                ClearButton.IsEnabled = _sketchOverlay.Graphics.Count > 0;
                EditButton.IsEnabled = _sketchOverlay.Graphics.Count > 0;
            }
            catch (TaskCanceledException)
            {
                // Ignore ... let the user cancel drawing.
            }
            catch (Exception ex)
            {
                // Report exceptions.
                await Application.Current.MainPage.DisplayAlert("Error", "Error drawing graphic shape: " + ex.Message, "OK");
            }
        }

        private void ClearButtonClick(object sender, EventArgs e)
        {
            // Update UI
            UnselectTool(sender, e);

            // Remove all graphics from the graphics overlay.
            _sketchOverlay.Graphics.Clear();

            // Cancel any uncompleted sketch.
            if (MyMapView.SketchEditor.CancelCommand.CanExecute(null))
            {
                MyMapView.SketchEditor.CancelCommand.Execute(null);
            }

            // Disable buttons that require graphics.
            ClearButton.IsEnabled = false;
            EditButton.IsEnabled = false;
        }

        private async void EditButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Update UI.
                SelectTool(sender as Button);

                // Create a TaskCompletionSource object to wait for a graphic.
                _graphicCompletionSource = new TaskCompletionSource<Graphic>();

                // Wait for the user to select a graphic.
                Graphic editGraphic = await _graphicCompletionSource.Task;

                // Let the user make changes to the graphic's geometry, await the result (updated geometry).
                Geometry newGeometry = await MyMapView.SketchEditor.StartAsync(editGraphic.Geometry);

                // Display the updated geometry in the graphic.
                editGraphic.Geometry = newGeometry;
            }
            catch (TaskCanceledException)
            {
                // Ignore ... let the user cancel editing.
            }
            catch (Exception ex)
            {
                // Report exceptions.
                await Application.Current.MainPage.DisplayAlert("Error", "Error editing shape: " + ex.Message, "OK");
            }
        }

        private async void ShowDrawTools(object sender, EventArgs e)
        {
            try
            {
                // Update UI.
                SelectTool(sender as Button);

                // Show draw tools and wait for the user to select a mode.
                string choice = await Application.Current.MainPage.DisplayActionSheet("Set draw mode:", "Cancel", null, _sketchModes);

                // Set the selected index for the draw mode.
                if (_sketchModes.Contains(choice))
                {
                    _drawModeIndex = Array.IndexOf(_sketchModes, choice);
                    StartSketch(sender, e);
                }
                else
                {
                    // Update UI
                    UnselectTool(sender, e);
                }
            }
            catch (Exception ex)
            {
                // Report exceptions.
                await Application.Current.MainPage.DisplayAlert("Error", "Error editing shape: " + ex.Message, "OK");
            }
        }

        #region Tool selection UI helpers

        private void SelectTool(Button selectedButton)
        {
            // Gray out the background of the currently selected tool.
            if (EnabledTool is not null)
                EnabledTool.Background = ThemedColor;

            // Set the static variable to whichever button that was just clicked.
            EnabledTool = selectedButton;

            // Set the background of the currently selected tool to red.
            EnabledTool.Background = Red;
        }

        private void UnselectTool(object sender, EventArgs e)
        {
            // Gray out the background of the currently selected tool.
            if (EnabledTool is not null)
                EnabledTool.Background = ThemedColor;

            // Dereference the unselected tool's button.
            EnabledTool = null;
        }

        #endregion Tool selection UI helpers

        private async void OnGeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            try
            {
                if (_graphicCompletionSource is not null && !_graphicCompletionSource.Task.IsCompleted)
                {
                    // Identify graphics in the graphics overlay using the point.
                    IReadOnlyList<IdentifyGraphicsOverlayResult> results = await MyMapView.IdentifyGraphicsOverlaysAsync(e.Position, 2, false);

                    // If results were found, get the first graphic.
                    IdentifyGraphicsOverlayResult idResult = results.FirstOrDefault();
                    if (idResult != null && idResult.Graphics.Count > 0)
                    {
                        Graphic graphic = idResult.Graphics.FirstOrDefault();
                        _graphicCompletionSource.TrySetResult(graphic);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Ignore ... let the user cancel drawing.
            }
            catch (Exception ex)
            {
                // Report exceptions.
                await Application.Current.MainPage.DisplayAlert("Error", "Error editing shape: " + ex.Message, "OK");
            }
        }
    }
}