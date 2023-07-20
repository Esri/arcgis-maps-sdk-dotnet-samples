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
using Esri.ArcGISRuntime.UI.GeometryEditor;
using Color = System.Drawing.Color;

namespace ArcGIS.Samples.CreateAndEditGeometries
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Create and edit geometries",
        category: "GraphicsOverlay",
        description: "Use the Geometry Editor to create new point, multipoint, polyline, or polygon geometries or to edit existing geometries by interacting with a map view.",
        instructions: "To create a new geometry, press the button appropriate for the geometry type you want to create (i.e. points, multipoints, polyline, or polygon) and interactively tap and drag on the map view to create the geometry. To edit an existing geometry, tap the geometry to be edited in the map to select it and then edit the geometry by tapping and dragging elements of the geometry. If creating or editing polyline or polygon geometries, choose the desired creation/editing tool (i.e. `VertexTool` or `FreehandTool`).",
        tags: new[] { "draw", "edit", "freehand", "geometry editor", "sketch", "vertex" })]
    public partial class CreateAndEditGeometries
    {
        private GeometryEditor _geometryEditor;
        private Graphic _selectedGraphic;
        private GraphicsOverlay _graphicsOverlay;

        private SimpleFillSymbol _polygonSymbol;
        private SimpleLineSymbol _polylineSymbol;
        private SimpleMarkerSymbol _pointSymbol, _multiPointSymbol;

        private Dictionary<GeometryType, Button> _geometryButtons;
        private Dictionary<string, GeometryEditorTool> _toolDictionary;

        // Json formatted strings for initial geometries.
        private readonly string _houseCoordinatesJson = @"{""x"": -9.59309629, ""y"": 53.0830063,
            ""spatialReference"":{""wkid"":4326}}";

        private readonly string _outbuildingCoordinatesJson = @"{""points"":[[-9.59386587, 53.08289651], [-9.59370896, 53.08234917],
                [-9.59330546, 53.082564], [-9.59341755, 53.08286662],
                [-9.59326997, 53.08304595], [-9.59246485, 53.08294507],
                [-9.59250034, 53.08286101], [-9.59241815, 53.08284607],
                [-9.59286835, 53.08311506], [-9.59307943, 53.08234731]],
                ""spatialReference"":{""wkid"":4326}}";

        private readonly string _road1CoordinatesJson = @"{""paths"":[[[-9.59486423, 53.08169453], [-9.5947812, 53.08175431],
                [-9.59475464, 53.08189379], [-9.59494393, 53.08213622],
                [-9.59464173, 53.08240521], [-9.59413694, 53.08260115],
                [-9.59357903, 53.0829266], [-9.59335984, 53.08311589],
                [-9.59318051, 53.08316903], [-9.59301779, 53.08322216],
                [-9.59264252, 53.08370038], [-9.59250636, 53.08383986]]],
                ""spatialReference"":{""wkid"":4326}}";

        private readonly string _road2CoordinatesJson = @"{""paths"":[[[-9.59400079, 53.08136244], [-9.59395761, 53.08149528],
                [-9.59368862, 53.0817045], [-9.59358235, 53.08219267],
                [-9.59331667, 53.08290335], [-9.59314398, 53.08314246],
                [-9.5930676, 53.08330519], [-9.59303439, 53.08351109],
                [-9.59301447, 53.08363728], [-9.59293809, 53.08387307]]],
                ""spatialReference"":{""wkid"":4326}}";

        private readonly string _boundaryCoordinatesJson = @"{ ""rings"": [[[-9.59350122, 53.08320723], [-9.59345177, 53.08333534],
                [-9.59309789, 53.08327198], [-9.59300344, 53.08317992],
                [-9.59221827, 53.08304034], [-9.59220706, 53.08287782],
                [-9.59229486, 53.08280871], [-9.59236398, 53.08268915],
                [-9.59255263, 53.08256769], [-9.59265165, 53.08237906],
                [-9.59287552, 53.08241478], [-9.59292812, 53.0823012],
                [-9.5932294, 53.08235022], [-9.59342188, 53.08260009],
                [-9.59354382, 53.08238728], [-9.59365852, 53.08203535],
                [-9.59408443, 53.08210446], [-9.59448232, 53.08224456],
                [-9.5943609, 53.08243697], [-9.59458319, 53.08245939],
                [-9.59439639, 53.08264619], [-9.59433288, 53.0827975],
                [-9.59404707, 53.08323649], [-9.59350122, 53.08320723]]],
                ""spatialReference"":{""wkid"":4326}}";

        public CreateAndEditGeometries()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create a map for the map view and set an inital viewpoint.
            MyMapView.Map = new Map(BasemapStyle.ArcGISImagery)
            {
                InitialViewpoint = new Viewpoint(53.08230, -9.5920, 5000)
            };

            // Create a graphics overlay and add it to the map view.
            _graphicsOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Create a geometry editor and set it to the map view.
            _geometryEditor = new GeometryEditor();
            MyMapView.GeometryEditor = _geometryEditor;

            // Create geometry editor tools for the combo box.
            _toolDictionary = new Dictionary<string, GeometryEditorTool>()
            {
                { "Vertex Tool", new VertexTool() },
                { "Freehand Tool", new FreehandTool() },
                { "Arrow Shape Tool", ShapeTool.Create(ShapeToolType.Arrow) },
                { "Ellipse Shape Tool", ShapeTool.Create(ShapeToolType.Ellipse) },
                { "Rectangle Shape Tool", ShapeTool.Create(ShapeToolType.Rectangle) },
                { "Triangle Shape Tool", ShapeTool.Create(ShapeToolType.Triangle) }
            };
            ToolPicker.ItemsSource = _toolDictionary.Keys.ToList();

            // Have the vertex tool selected by default.
            ToolPicker.SelectedIndex = 0;

            // Create a dictionary to lookup which geometry type corresponds with which button.
            _geometryButtons = new Dictionary<GeometryType, Button>
            {
                { GeometryType.Point, PointButton },
                { GeometryType.Multipoint, MultipointButton },
                { GeometryType.Polyline, PolylineButton },
                { GeometryType.Polygon, PolygonButton }
            };

            CreateInitialGraphics();
        }

        #region Event handlers

        // Starts the geometry editor with the point geometry type.
        private void PointButton_Click(object sender, EventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
                // Disable buttons to reflect that the geometry editor has started.
                DisableOtherGeometryButtons(PointButton);

                // Disable the combo box as this is always a vertex tool when creating a point.
                ToolPicker.IsEnabled = false;

                _geometryEditor.Start(GeometryType.Point);
            }
        }

        // Start the geometry editor with the multipoint geometry type.
        private void MultipointButton_Click(object sender, EventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
                // Disable buttons to reflect that the geometry editor has started.
                DisableOtherGeometryButtons(MultipointButton);

                // Disable the combo box as this is always a vertex tool when creating a point.
                ToolPicker.IsEnabled = false;

                _geometryEditor.Start(GeometryType.Multipoint);
            }
        }

        // Start the geometry editor with the polyline geometry type.
        private void PolylineButton_Click(object sender, EventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
                // Disable buttons to reflect that the geometry editor has started.
                DisableOtherGeometryButtons(PointButton);

                _geometryEditor.Start(GeometryType.Polyline);
            }
        }

        // Start the geometry editor with the polygon geometry type.
        private void PolygonButton_Click(object sender, EventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
                // Disable buttons to reflect that the geometry editor has started.
                DisableOtherGeometryButtons(PointButton);

                _geometryEditor.Start(GeometryType.Polygon);
            }
        }

        // Set the geometry editor tool from the picker.
        private void ToolPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            GeometryEditorTool tool = _toolDictionary[ToolPicker.SelectedItem.ToString()];
            _geometryEditor.Tool = tool;

            // Account for case when vertex tool is selected and geometry editor is started with a polyline or polygon geometry type.
            // Ensure point and multipoint buttons are only enabled when the selected tool is a vertex tool.
            PointButton.IsEnabled = MultipointButton.IsEnabled = !_geometryEditor.IsStarted && tool is VertexTool;
        }

        // Set the scale mode of the geometry editor.
        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            // Determine if shape tools should use uniform scaling.
            GeometryEditorScaleMode shouldBeUniform = (sender as CheckBox).IsChecked ? GeometryEditorScaleMode.Uniform : GeometryEditorScaleMode.Stretch;

            // Update all shape tools scaling options.
            foreach (ShapeTool tool in _toolDictionary.Values.Where(v => v is ShapeTool))
            {
                tool.Configuration.ScaleMode = shouldBeUniform;
            }
        }

        // Undo the last change made to the geometry while editing is active.
        private void UndoButton_Click(object sender, EventArgs e)
        {
            _geometryEditor.Undo();
        }

        // Redo the last change made to the geometry while editing is active.
        private void RedoButton_Click(object sender, EventArgs e)
        {
            _geometryEditor.Redo();
        }

        // Delete the currently selected element of the geometry editor.
        private void DeleteSelectedButton_Click(object sender, EventArgs e)
        {
            _geometryEditor.DeleteSelectedElement();
        }

        // Update an existing graphic or create a new graphic.
        private void SaveButton_Click(object sender, EventArgs e)
        {
            Geometry geometry = _geometryEditor.Stop();

            if (geometry != null)
            {
                if (_selectedGraphic != null)
                {
                    // Update the geometry of the graphic being edited and make it visible again.
                    _selectedGraphic.Geometry = geometry;
                }
                else
                {
                    // Create a new graphic based on the geometry and add it to the graphics overlay.
                    _graphicsOverlay.Graphics.Add(new Graphic(geometry, GetSymbol(geometry.GeometryType)));
                }
            }

            ResetFromEditingSession();
        }

        // Stop the geometry editor without saving the geometry stored within.
        private void DiscardButton_Click(object sender, EventArgs e)
        {
            _geometryEditor.Stop();
            ResetFromEditingSession();
        }

        // Remove all graphics from the graphics overlay.
        private void DeleteAllButton_Click(object sender, EventArgs e)
        {
            _graphicsOverlay.Graphics.Clear();
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            // Return immediately when in an editing session.
            if (_geometryEditor.IsStarted) return;

            try
            {
                // Identify graphics in the graphics overlay using the mouse point.
                IReadOnlyList<IdentifyGraphicsOverlayResult> results = await MyMapView.IdentifyGraphicsOverlaysAsync(e.Position, 5, false);

                // Try to get the first graphic from the first result.
                _selectedGraphic = results.FirstOrDefault()?.Graphics?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Report exceptions.
                await Application.Current.MainPage.DisplayAlert("Error editing", ex.Message, "OK");

                ResetFromEditingSession();
                return;
            }

            // Return since no graphic was selected.
            if (_selectedGraphic == null) return;

            _selectedGraphic.IsSelected = true;

            // Configure the UI depending on the geometry type.
            GeometryType geometryType = _selectedGraphic.Geometry.GeometryType;
            if (geometryType == GeometryType.Point || geometryType == GeometryType.Multipoint)
            {
                ToolPicker.SelectedIndex = 0;
            }
            DisableOtherGeometryButtons(_geometryButtons[geometryType]);

            // Hide the selected graphic and start an editing session with a copy of it.
            _geometryEditor.Start(_selectedGraphic.Geometry);
            _selectedGraphic.IsVisible = false;
        }

        #endregion Event handlers

        #region Helper methods

        private void CreateInitialGraphics()
        {
            // Create symbols for displaying new geometries.
            _pointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Square, Color.OrangeRed, 10);
            _multiPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Yellow, 5);
            _polylineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Blue, 2);
            var polygonLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.Black, 1);
            _polygonSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(70, 255, 0, 0), polygonLineSymbol);

            // Create geometries from Json formatted strings.
            var houseCoordinates = (MapPoint)Geometry.FromJson(_houseCoordinatesJson);
            var outbuildingCoordinates = (Multipoint)Geometry.FromJson(_outbuildingCoordinatesJson);
            var road1Coordinates = (Polyline)Geometry.FromJson(_road1CoordinatesJson);
            var road2Coordinates = (Polyline)Geometry.FromJson(_road2CoordinatesJson);
            var boundaryCoordinates = (Polygon)Geometry.FromJson(_boundaryCoordinatesJson);

            // Add new example graphics from the geometries and symbols to the graphics overlay.
            _graphicsOverlay.Graphics.Add(new Graphic(houseCoordinates) { Symbol = _pointSymbol });
            _graphicsOverlay.Graphics.Add(new Graphic(outbuildingCoordinates) { Symbol = _multiPointSymbol });
            _graphicsOverlay.Graphics.Add(new Graphic(road1Coordinates) { Symbol = _polylineSymbol });
            _graphicsOverlay.Graphics.Add(new Graphic(road2Coordinates) { Symbol = _polylineSymbol });
            _graphicsOverlay.Graphics.Add(new Graphic(boundaryCoordinates) { Symbol = _polygonSymbol });
        }

        // Reset the UI after the editor stops.
        private void ResetFromEditingSession()
        {
            // Reset the selected graphic.
            if (_selectedGraphic != null)
            {
                _selectedGraphic.IsSelected = false;
                _selectedGraphic.IsVisible = true;
            }
            _selectedGraphic = null;

            // Point and multipoint sessions do not support the vertex tool.
            PointButton.IsEnabled = MultipointButton.IsEnabled = _geometryEditor.Tool is VertexTool;
            PolylineButton.IsEnabled = PolygonButton.IsEnabled = true;
            ToolPicker.IsEnabled = true;
        }

        // Return the graphic style based on geometry type.
        private Symbol GetSymbol(GeometryType geometryType)
        {
            switch (geometryType)
            {
                case GeometryType.Point:
                    return _pointSymbol;

                case GeometryType.Multipoint:
                    return _multiPointSymbol;

                case GeometryType.Polyline:
                    return _polylineSymbol;

                case GeometryType.Polygon:
                    return _polygonSymbol;
            }
            return null;
        }

        // Disable all geometry buttons besides the one that was just clicked.
        private void DisableOtherGeometryButtons(Button keepEnabled)
        {
            foreach (Button button in _geometryButtons.Values)
            {
                button.IsEnabled = button == keepEnabled;
            }
        }

        #endregion Helper methods
    }
}