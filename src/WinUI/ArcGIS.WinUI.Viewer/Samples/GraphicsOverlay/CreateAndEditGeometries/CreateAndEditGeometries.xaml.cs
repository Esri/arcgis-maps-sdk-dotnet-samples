// Copyright 2023 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.GeometryEditor;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;
using Geometry = Esri.ArcGISRuntime.Geometry.Geometry;

namespace ArcGIS.WinUI.Samples.CreateAndEditGeometries
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        "Create and edit geometries",
        "GraphicsOverlay",
        "Use the Geometry Editor to create new point, multipoint, polyline, or polygon geometries or to edit existing geometries by interacting with a map view.",
        "")]
    public partial class CreateAndEditGeometries
    {
        private GeometryEditor _geometryEditor;
        private SimpleFillSymbol _fillSymbol;
        private SimpleLineSymbol _lineSymbol;
        private SimpleMarkerSymbol _pointSymbol, _multiPointSymbol;
        private List<Button> _geometryButtons;
        private Graphic _selectedGraphic;
        private GraphicsOverlay _graphicsOverlay;
        private bool _geometryCreated = true;

        public CreateAndEditGeometries()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with standard imagery basemap style.
            var map = new Map(BasemapStyle.ArcGISImageryStandard);

            // Set the map to the map view.
            MyMapView.Map = map;

            // Set a viewpoint on the map view.
            MyMapView.SetViewpoint(new Viewpoint(53.08230, -9.5920, 5000));

            // Create a graphics overlay and add it to the map view.
            _graphicsOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Create a geometry editor and set it to the map view.
            _geometryEditor = new GeometryEditor();
            MyMapView.GeometryEditor = _geometryEditor;

            // Create vertex and freehand tools for the geometry editor.
            ToolComboBox.ItemsSource = new Dictionary<string, object>()
            {
                {"Vertex Tool", new VertexTool()}, {"Freehand Tool", new FreehandTool()}
            };

            // Display the tool name in the combo box.
            ToolComboBox.DisplayMemberPath = "Key";

            // Have the vertex tool selected by default.
            ToolComboBox.SelectedIndex = 0;

            // Create symbols for displaying new geometries.
            // Orange-red square for points.
            _pointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Square, Color.OrangeRed, 10);
            // Yellow circle for multipoints.
            _multiPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Yellow, 5);
            // Thin blue line for polylines.
            _lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Blue, 2);
            // Black outline for polygons.
            var polygonLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.Black, 1);
            // Translucent red interior for polygons.
            _fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(70, 255, 0, 0), polygonLineSymbol);

            // Create geometry objects from JSON formatted strings.
            var houseCoordinates = (MapPoint)Geometry.FromJson("{\r\n  \"x\": -9.59309629,\r\n  \"y\": 53.0830063,\r\n  \"spatialReference\":{\"wkid\":4326}\r\n}");
            var outbuildingCoordinates = (Multipoint)Geometry.FromJson("{\r\n  \"points\":[[-9.59386587, 53.08289651], [-9.59370896, 53.08234917],\r\n    [-9.59330546, 53.082564], [-9.59341755, 53.08286662],\r\n    [-9.59326997, 53.08304595], [-9.59246485, 53.08294507],\r\n    [-9.59250034, 53.08286101], [-9.59241815, 53.08284607],\r\n    [-9.59286835, 53.08311506], [-9.59307943, 53.08234731]],\r\n  \"spatialReference\":{\"wkid\":4326}\r\n}");
            var road1Coordinates = (Polyline)Geometry.FromJson("{\r\n  \"paths\":[[[-9.59486423, 53.08169453], [-9.5947812, 53.08175431],\r\n    [-9.59475464, 53.08189379], [-9.59494393, 53.08213622],\r\n    [-9.59464173, 53.08240521], [-9.59413694, 53.08260115],\r\n    [-9.59357903, 53.0829266], [-9.59335984, 53.08311589],\r\n    [-9.59318051, 53.08316903], [-9.59301779, 53.08322216],\r\n    [-9.59264252, 53.08370038], [-9.59250636, 53.08383986]]],\r\n  \"spatialReference\":{\"wkid\":4326}\r\n}");
            var road2Coordinates = (Polyline)Geometry.FromJson("{\r\n  \"paths\":[[[-9.59400079, 53.08136244], [-9.59395761, 53.08149528],\r\n    [-9.59368862, 53.0817045], [-9.59358235, 53.08219267],\r\n    [-9.59331667, 53.08290335], [-9.59314398, 53.08314246],\r\n    [-9.5930676, 53.08330519], [-9.59303439, 53.08351109],\r\n    [-9.59301447, 53.08363728], [-9.59293809, 53.08387307]]],\r\n  \"spatialReference\":{\"wkid\":4326}\r\n}");
            var boundaryCoordinates = (Polygon)Geometry.FromJson("{\r\n  \"rings\": [[[-9.59350122, 53.08320723], [-9.59345177, 53.08333534],\r\n    [-9.59309789, 53.08327198], [-9.59300344, 53.08317992],\r\n    [-9.59221827, 53.08304034], [-9.59220706, 53.08287782],\r\n    [-9.59229486, 53.08280871], [-9.59236398, 53.08268915],\r\n    [-9.59255263, 53.08256769], [-9.59265165, 53.08237906],\r\n    [-9.59287552, 53.08241478], [-9.59292812, 53.0823012],\r\n    [-9.5932294, 53.08235022], [-9.59342188, 53.08260009],\r\n    [-9.59354382, 53.08238728], [-9.59365852, 53.08203535],\r\n    [-9.59408443, 53.08210446], [-9.59448232, 53.08224456],\r\n    [-9.5943609, 53.08243697], [-9.59458319, 53.08245939],\r\n    [-9.59439639, 53.08264619], [-9.59433288, 53.0827975],\r\n    [-9.59404707, 53.08323649], [-9.59350122, 53.08320723]]],\r\n  \"spatialReference\":{\"wkid\":4326}\r\n}");

            // Create example graphics from the geometries and symbols.
            var pointGraphic = new Graphic(houseCoordinates) { Symbol = _pointSymbol };
            var multiPointGraphic = new Graphic(outbuildingCoordinates) { Symbol = _multiPointSymbol };
            var polyline1Graphic = new Graphic(road1Coordinates) { Symbol = _lineSymbol };
            var polyline2Graphic = new Graphic(road2Coordinates) { Symbol = _lineSymbol };
            var polygonGraphic = new Graphic(boundaryCoordinates) { Symbol = _fillSymbol };

            // Add example graphics to the graphics overlay.
            var graphics = new List<Graphic>() { pointGraphic, multiPointGraphic, polyline1Graphic, polyline2Graphic, polygonGraphic };
            foreach (var graphic in graphics)
            {
                _graphicsOverlay.Graphics.Add(graphic);
            }

            // Create a list of geometry buttons for easy enable and disable.
            _geometryButtons = new List<Button>
            {
                PointButton, MultipointButton, PolylineButton, PolygonButton
            };

            // Update the UI to reflect geometry editor property changes.
            _geometryEditor.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(GeometryEditor.IsStarted))
                {
                    // Account for case where there are no graphics to delete and user discards edits.
                    if (_geometryCreated)
                    {
                        // Enable or disable the button when there are graphics created and the geometry editor starts or stops.
                        DeleteAllButton.IsEnabled = !_geometryEditor.IsStarted;
                    }

                    // Disable the save button after the geometry editor stops.
                    if (!_geometryEditor.IsStarted)
                    {
                        SaveButton.IsEnabled = false;
                    }
                }
                else if (e.PropertyName == nameof(GeometryEditor.Geometry))
                {
                    // Enable the save button when the user is editing and the geometry editor can undo.
                    if (_selectedGraphic != null)
                    {
                        SaveButton.IsEnabled = _geometryEditor.CanUndo;
                    }
                    // Enable the save button when the user is creating a geometry and there is geometry to save.
                    else if (_geometryEditor.Geometry != null)
                    {
                        SaveButton.IsEnabled = !_geometryEditor.Geometry.IsEmpty;
                    }
                }
            };
        }

        #region Create controls event handlers

        // Starts the geometry editor with the point geometry type.
        private void PointButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
                // Ensure the geometry editor tool is set to the vertex tool.
                ToolComboBox.SelectedIndex = 0;

                // Disable the combo box as this is always a vertex tool when creating a point.
                ToolComboBox.IsEnabled = false;

                // Disable buttons to reflect that the geometry editor has started.
                DisableOtherGeometryButtons(PointButton);

                _geometryEditor.Start(GeometryType.Point);
            }
        }

        // Start the geometry editor with the multipoint geometry type.
        private void MultipointButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
                // Ensure the geometry editor tool is set to the vertex tool.
                ToolComboBox.SelectedIndex = 0;

                // Disable the combo box as this is always a vertex tool when creating a multipoint.
                ToolComboBox.IsEnabled = false;

                // Disable buttons to reflect that the geometry editor has started.
                DisableOtherGeometryButtons(MultipointButton);

                _geometryEditor.Start(GeometryType.Multipoint);
            }
        }

        // Start the geometry editor with the polyline geometry type.
        private void PolylineButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
                ToolComboBox.IsEnabled = true;

                // Disable buttons to reflect that the geometry editor has started.
                DisableOtherGeometryButtons(PolylineButton);

                _geometryEditor.Start(GeometryType.Polyline);
            }
        }

        // Start the geometry editor with the polygon geometry type.
        private void PolygonButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
                ToolComboBox.IsEnabled = true;

                // Disable buttons to reflect that the geometry editor has started.
                DisableOtherGeometryButtons(PolygonButton);

                _geometryEditor.Start(GeometryType.Polygon);
            }
        }

        // Set the geometry editor tool from the combo box.
        private void ToolComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var keyValuePair = (KeyValuePair<string, object>)ToolComboBox.SelectedItem;
            _geometryEditor.Tool = (GeometryEditorTool)keyValuePair.Value;
        }

        #endregion Create controls event handlers

        #region Edit controls event handlers

        // Undo the last change made to the geometry while editing is active.
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            _geometryEditor.Undo();
        }

        // Redo the last change made to the geometry while editing is active.
        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            _geometryEditor.Redo();
        }

        // Delete the currently selected element of the geometry editor.
        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            _geometryEditor.DeleteSelectedElement();
        }

        // Update an existing graphic or create a new graphic.
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGraphic != null)
            {
                // Update the geometry of the graphic being edited and make it visible again.
                _selectedGraphic.Geometry = _geometryEditor.Stop();
                _selectedGraphic.IsVisible = true;

                // Reset the selected graphic.
                _selectedGraphic.IsSelected = false;
                _selectedGraphic = null;
            }
            else
            {
                _geometryCreated = true;

                // Get the geometry from the geometry editor and create a new graphic for it.
                Geometry geometry = _geometryEditor.Stop();
                var graphic = new Graphic(geometry);

                // Set the graphic style based on geometry type.
                switch (geometry.GeometryType)
                {
                    case GeometryType.Point:
                        graphic.Symbol = _pointSymbol;
                        break;

                    case GeometryType.Multipoint:
                        graphic.Symbol = _multiPointSymbol;
                        break;

                    case GeometryType.Polyline:
                        graphic.Symbol = _lineSymbol;
                        break;

                    case GeometryType.Polygon:
                        graphic.Symbol = _fillSymbol;
                        break;
                }

                _graphicsOverlay.Graphics.Add(graphic);
            }

            // Allow the creation of new graphics.
            EnableGeometryButtons();
            ToolComboBox.IsEnabled = true;
        }

        // Stops the geometry editor without saving the geometry stored within.
        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGraphic != null)
            {
                // Make visisble the pre-existing graphic when editing.
                _selectedGraphic.IsVisible = true;
                _selectedGraphic.IsSelected = false;
                _selectedGraphic = null;

                _geometryEditor.ClearSelection();
            }

            _geometryEditor.Stop();

            // Allow for the creation of new graphics.
            EnableGeometryButtons();
            ToolComboBox.IsEnabled = true;
        }

        // Removes all graphics from the graphics overlay.
        private void DeleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            _graphicsOverlay.Graphics.Clear();

            // Disable the delete all button as there are no graphics to delete.
            DeleteAllButton.IsEnabled = false;
            _geometryCreated = false;

            // Allow for the creation of new graphics.
            EnableGeometryButtons();
            ToolComboBox.IsEnabled = true;
        }

        #endregion Edit controls event handlers

        #region Enable and disable geometry buttons methods

        // Ensure all geometry buttons are enabled.
        private void EnableGeometryButtons()
        {
            foreach (var button in _geometryButtons)
            {
                button.IsEnabled = true;
            }
        }

        // Disable all geometry buttons besides the one that was just clicked.
        private void DisableOtherGeometryButtons(Button keepEnabled)
        {
            foreach (var button in _geometryButtons.Where(value => value != keepEnabled))
            {
                button.IsEnabled = false;
            }
        }

        #endregion Enable and disable geometry buttons methods

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                if (!_geometryEditor.IsStarted)
                {
                    // Identify graphics in the graphics overlay using the mouse point.
                    IReadOnlyList<IdentifyGraphicsOverlayResult> results = await MyMapView.IdentifyGraphicsOverlaysAsync(e.Position, 2, false);

                    // Get the first graphic if results were found.
                    IdentifyGraphicsOverlayResult idResult = results.FirstOrDefault();
                    if (idResult != null && idResult.Graphics.Count > 0)
                    {
                        _selectedGraphic = idResult.Graphics.FirstOrDefault();
                        _selectedGraphic.IsSelected = true;

                        GeometryType geometryType = _selectedGraphic.Geometry.GeometryType;

                        // Configure the UI depending on the geometry type.
                        if (geometryType == GeometryType.Point || geometryType == GeometryType.Multipoint)
                        {
                            ToolComboBox.SelectedIndex = 0;
                            ToolComboBox.IsEnabled = false;
                        }
                        else
                        {
                            ToolComboBox.IsEnabled = true;
                        }
                        switch (geometryType)
                        {
                            case GeometryType.Point:
                                DisableOtherGeometryButtons(PointButton);
                                break;

                            case GeometryType.Multipoint:
                                DisableOtherGeometryButtons(MultipointButton);
                                break;

                            case GeometryType.Polyline:
                                DisableOtherGeometryButtons(PolylineButton);
                                break;

                            case GeometryType.Polygon:
                                DisableOtherGeometryButtons(PolygonButton);
                                break;
                        }

                        // Hide the selected graphic and start an editing session with a copy of it.
                        _geometryEditor.Start(_selectedGraphic.Geometry);
                        _selectedGraphic.IsVisible = false;
                    }
                    else
                    {
                        _selectedGraphic = null;
                    }
                }
            }
            catch (Exception ex)
            {
                // Report exceptions.
                await new MessageDialog2(ex.Message, ex.GetType().Name).ShowAsync();
            }
        }
    }
}