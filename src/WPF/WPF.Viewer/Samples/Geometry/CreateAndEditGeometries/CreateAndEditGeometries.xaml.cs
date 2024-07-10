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
using Esri.ArcGISRuntime.UI.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Button = System.Windows.Controls.Button;
using Color = System.Drawing.Color;
using Geometry = Esri.ArcGISRuntime.Geometry.Geometry;

namespace ArcGIS.WPF.Samples.CreateAndEditGeometries
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Create and edit geometries",
        category: "Geometry",
        description: "Use the Geometry Editor to create new point, multipoint, polyline, or polygon geometries or to edit existing geometries by interacting with a map view.",
        instructions: "To create a new geometry, press the button appropriate for the geometry type you want to create (i.e. points, multipoints, polyline, or polygon) and interactively tap and drag on the map view to create the geometry. To edit an existing geometry, tap the geometry to be edited in the map and then perform edits by tapping and dragging its elements. When using an appropriate tool to select a whole geometry, you can use the control handles to scale and rotate the geometry. If creating or editing polyline or polygon geometries, choose the desired creation/editing tool (i.e. `VertexTool`, `ReticleVertexTool`, `FreehandTool`, or one of the available `ShapeTool`s).",
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
        private readonly string _houseCoordinatesJson = @"{""x"": -1067898.59, ""y"": 6998366.62,
            ""spatialReference"": {""latestWkid"":3857, ""wkid"":102100}}";

        private readonly string _outbuildingCoordinatesJson = @"{""points"":[[-1067984.26,6998346.28],[-1067966.80,6998244.84],
            [-1067921.88,6998284.65],[-1067934.36,6998340.74],
            [-1067917.93,6998373.97],[-1067828.30,6998355.28],
            [-1067832.25,6998339.70],[-1067823.10,6998336.93],
            [-1067873.22,6998386.78],[-1067896.72,6998244.49]],
            ""spatialReference"":{""latestWkid"":3857,""wkid"":102100}}";

        private readonly string _road1CoordinatesJson = @"{""paths"":[[[-1068095.40,6998123.52],[-1068086.16,6998134.60],
            [-1068083.20,6998160.44],[-1068104.27,6998205.37],
            [-1068070.63,6998255.22],[-1068014.44,6998291.54],
            [-1067952.33,6998351.85],[-1067927.93,6998386.93],
            [-1067907.97,6998396.78],[-1067889.86,6998406.63],
            [-1067848.08,6998495.26],[-1067832.92,6998521.11]]],
            ""spatialReference"":{""latestWkid"":3857,""wkid"":102100}}";

        private readonly string _road2CoordinatesJson = @"{""paths"":[[[-1067999.28,6998061.97],[-1067994.48,6998086.59],
            [-1067964.53,6998125.37],[-1067952.70,6998215.84],
            [-1067923.13,6998347.54],[-1067903.90,6998391.86],
            [-1067895.40,6998422.02],[-1067891.70,6998460.18],
            [-1067889.49,6998483.56],[-1067880.98,6998527.26]]],
            ""spatialReference"":{""latestWkid"":3857,""wkid"":102100}}";

        private readonly string _boundaryCoordinatesJson = @"{ ""rings"": [[[-1067943.67,6998403.86],[-1067938.17,6998427.60],
            [-1067898.77,6998415.86],[-1067888.26,6998398.80],
            [-1067800.85,6998372.93],[-1067799.61,6998342.81],
            [-1067809.38,6998330.00],[-1067817.07,6998307.85],
            [-1067838.07,6998285.34],[-1067849.10,6998250.38],
            [-1067874.02,6998256.00],[-1067879.87,6998235.95],
            [-1067913.41,6998245.03],[-1067934.84,6998291.34],
            [-1067948.41,6998251.90],[-1067961.18,6998186.68],
            [-1068008.59,6998199.49],[-1068052.89,6998225.45],
            [-1068039.37,6998261.11],[-1068064.12,6998265.26],
            [-1068043.32,6998299.88],[-1068036.25,6998327.93],
            [-1068004.43,6998409.28],[-1067943.67,6998403.86]]],
            ""spatialReference"":{""latestWkid"":3857,""wkid"":102100}}";

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

            // Create vertex and freehand tools for the combo box.
            ToolComboBox.ItemsSource = _toolDictionary = new Dictionary<string, GeometryEditorTool>()
            {
                { "Vertex Tool", new VertexTool() },
                { "Reticle Vertex Tool", new ReticleVertexTool() },
                { "Freehand Tool", new FreehandTool() },
                { "Arrow Shape Tool", ShapeTool.Create(ShapeToolType.Arrow) },
                { "Ellipse Shape Tool", ShapeTool.Create(ShapeToolType.Ellipse) },
                { "Rectangle Shape Tool", ShapeTool.Create(ShapeToolType.Rectangle) },
                { "Triangle Shape Tool", ShapeTool.Create(ShapeToolType.Triangle) }
            };

            // Have the vertex tool selected by default.
            ToolComboBox.SelectedIndex = 0;

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
        private void PointButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
                // Disable buttons to reflect that the geometry editor has started.
                DisableOtherGeometryButtons(PointButton);

                // Disable the combo box as this is always a vertex tool when creating a point.
                ToolComboBox.IsEnabled = false;

                // Disable scale checkbox since points don't scale.
                UniformScaleCheckBox.IsEnabled = false;

                _geometryEditor.Start(GeometryType.Point);
            }
        }

        // Start the geometry editor with the multipoint geometry type.
        private void MultipointButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
                // Disable buttons to reflect that the geometry editor has started.
                DisableOtherGeometryButtons(MultipointButton);

                // Disable the combo box as this is always a vertex tool when creating a point.
                ToolComboBox.IsEnabled = false;

                _geometryEditor.Start(GeometryType.Multipoint);
            }
        }

        // Start the geometry editor with the polyline geometry type.
        private void PolylineButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
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
                // Disable buttons to reflect that the geometry editor has started.
                DisableOtherGeometryButtons(PolygonButton);

                _geometryEditor.Start(GeometryType.Polygon);
            }
        }

        // Set the geometry editor tool from the combo box.
        private void ToolComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Set the geometry editor tool based on the new selection.
            _geometryEditor.Tool = ((KeyValuePair<string, GeometryEditorTool>)ToolComboBox.SelectedItem).Value as GeometryEditorTool;

            // Account for case when vertex tool is selected and geometry editor is started with a polyline or polygon geometry type.
            // Ensure point and multipoint buttons are only enabled when the selected tool is a vertex tool.
            PointButton.IsEnabled = MultipointButton.IsEnabled = !_geometryEditor.IsStarted && (_geometryEditor.Tool is VertexTool || _geometryEditor.Tool is ReticleVertexTool);

            // Uniform scale is not compatible with the reticle vertex tool.
            UniformScaleBox.IsEnabled = !(_geometryEditor.Tool is ReticleVertexTool);
        }

        // Set the scale mode for every geometry editor tool.
        private void UniformScaleCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // Determine the newly selected scale mode.
            GeometryEditorScaleMode scaleMode =
                UniformScaleCheckBox.IsChecked == true ? GeometryEditorScaleMode.Uniform : GeometryEditorScaleMode.Stretch;

            // Update the scale mode for every tool.
            foreach (GeometryEditorTool tool in _toolDictionary.Values)
            {
                if (tool is FreehandTool freehandTool)
                {
                    freehandTool.Configuration.ScaleMode = scaleMode;
                }
                else if (tool is VertexTool vertexTool)
                {
                    vertexTool.Configuration.ScaleMode = scaleMode;
                }
                else if (tool is ShapeTool shapeTool)
                {
                    shapeTool.Configuration.ScaleMode = scaleMode;
                }
            }
        }

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

        // Stops the geometry editor without saving the geometry stored within.
        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            _geometryEditor.Stop();
            ResetFromEditingSession();
        }

        // Removes all graphics from the graphics overlay.
        private void DeleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            _graphicsOverlay.Graphics.Clear();
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
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
                MessageBox.Show("Error: " + ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);

                ResetFromEditingSession();
            }

            // Return since no graphic was selected.
            if (_selectedGraphic == null) return;

            _selectedGraphic.IsSelected = true;

            // Configure the UI depending on the geometry type.
            GeometryType geometryType = _selectedGraphic.Geometry.GeometryType;
            if (geometryType == GeometryType.Point)
            {
                ToolComboBox.SelectedIndex = 0;
                UniformScaleCheckBox.IsEnabled = false;
            }
            if (geometryType == GeometryType.Multipoint)
            {
                ToolComboBox.SelectedIndex = 0;
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
            PointButton.IsEnabled = MultipointButton.IsEnabled = _geometryEditor.Tool is VertexTool || _geometryEditor.Tool is ReticleVertexTool;
            PolylineButton.IsEnabled = PolygonButton.IsEnabled = true;
            ToolComboBox.IsEnabled = true;

            UniformScaleCheckBox.IsEnabled = true;
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