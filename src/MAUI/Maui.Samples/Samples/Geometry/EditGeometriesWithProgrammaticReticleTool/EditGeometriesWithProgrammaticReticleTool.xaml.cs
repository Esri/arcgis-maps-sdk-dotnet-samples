// Copyright 2025 Esri.
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
using Esri.ArcGISRuntime.UI.Editing;
using Microsoft.Maui.ApplicationModel;
using Color = System.Drawing.Color;
using Map = Esri.ArcGISRuntime.Mapping.Map;

namespace ArcGIS.Samples.EditGeometriesWithProgrammaticReticleTool
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Edit geometries with programmatic reticle tool",
        category: "Geometry",
        description: "Use the Programmatic Reticle Tool to edit and create geometries with programmatic operations to facilitate customized workflows such as those using buttons rather than tap interactions.",
        instructions: "To create a new geometry, select the geometry type you want to create (i.e. points, multipoints, polyline, or polygon) in the settings view. Press the button to start the geometry editor, pan the map to position the reticle then press the button to place a vertex. To edit an existing geometry, tap the geometry to be edited in the map and perform edits by positioning the reticle over a vertex and pressing the button to pick it up. The vertex can be moved by panning the map and dropped in a new position by pressing the button again.",
        tags: new[] { "draw", "edit", "freehand", "geometry editor", "programmatic", "reticle", "sketch", "vertex" })]
    public partial class EditGeometriesWithProgrammaticReticleTool
    {
        // Hold references to the geometry editor and programmatic reticle tool.
        private GeometryEditor _geometryEditor = new GeometryEditor();
        private ProgrammaticReticleTool _programmaticReticleTool = new ProgrammaticReticleTool();

        // Hold references to the graphics overlay and selected graphic.
        private Graphic _selectedGraphic;
        private GraphicsOverlay _graphicsOverlay;

        // Flag to indicate whether vertex creation is allowed.
        private bool _allowVertexCreation = true;

        // Reticle state to determine the current action of the programmatic reticle tool.
        private ReticleState _reticleState = ReticleState.Default;

        // Hold references to the symbols used for displaying  geometries.
        private SimpleFillSymbol _polygonSymbol;
        private SimpleLineSymbol _polylineSymbol;
        private SimpleMarkerSymbol _pointSymbol, _multiPointSymbol;

        private string _pinkneysGreenJson = @"{""rings"":[[[-84843.262719916485,6713749.9329888355],[-85833.376589175183,6714679.7122141244],
                    [-85406.822347959576,6715063.9827222107],[-85184.329997390232,6715219.6195847588],
                    [-85092.653857582554,6715119.5391713539],[-85090.446872787768,6714792.7656492386],
                    [-84915.369168906298,6714297.8798246197],[-84854.295522911285,6714080.907587287],
                    [-84843.262719916485,6713749.9329888355]]],""spatialReference"":{""wkid"":102100,""latestWkid"":3857}}";

        private string _beechLodgeBoundaryJson = @"{""paths"":[[[-87090.652708065536,6714158.9244240439],[-87247.362370337316,6714232.880689906],
                    [-87226.314032974493,6714605.4697726099],[-86910.499335316243,6714488.006312645],
                    [-86750.82198052686,6714401.1768307304],[-86749.846825938366,6714305.8450344801]]],""spatialReference"":{""wkid"":102100,""latestWkid"":3857}}";

        private string _treeMarkersJson = @"{""points"":[[-86750.751150056443,6713749.4529355941],[-86879.381793060631,6713437.3335486846],
                    [-87596.503104619667,6714381.7342108283],[-87553.257569537804,6714402.0910389507],
                    [-86831.019903597829,6714398.4128562529],[-86854.105933315877,6714396.1957954112],
                    [-86800.624094892439,6713992.3374453448]],""spatialReference"":{""wkid"":102100,""latestWkid"":3857}}";

        // Dictionary to map geometry type names to GeometryType enum values.
        private readonly Dictionary<string, GeometryType> _geometryTypes = new Dictionary<string, GeometryType>()
        {
            { "Point", GeometryType.Point },
            { "Multipoint", GeometryType.Multipoint },
            { "Polygon", GeometryType.Polygon },
            { "Polyline", GeometryType.Polyline },
        };

        public EditGeometriesWithProgrammaticReticleTool()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            MyMapView.Map = new Map()
            {
                Basemap = new Basemap(BasemapStyle.ArcGISImagery),
                InitialViewpoint = new Viewpoint(51.523806, -0.775395, 2e4)
            };

            // Create the geometry editor and add it to the map view.
            _geometryEditor = new GeometryEditor();
            MyMapView.GeometryEditor = _geometryEditor;

            // Create the programmatic reticle tool and set it as the geometry editor tool.
            _programmaticReticleTool = new ProgrammaticReticleTool();
            _geometryEditor.Tool = _programmaticReticleTool;

            // Add event handlers for geometry editor events.
            MyMapView.GeometryEditor.HoveredElementChanged += GeometryEditor_HoveredElementChanged;
            MyMapView.GeometryEditor.PickedUpElementChanged += GeometryEditor_PickedUpElementChanged;

            // Enable vertex creation by default and set up the switch.
            AllowVertexCreationSwitch.IsToggled = true;
            AllowVertexCreationSwitch.Toggled += AllowVertexCreationSwitch_Toggled;

            // Create a graphics overlay and add it to the map view.
            _graphicsOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Set the geometry type picker and its default value.
            GeometryTypePicker.ItemsSource = _geometryTypes.Keys.ToList();
            GeometryTypePicker.SelectedIndex = 0;

            // Create the initial graphics for the sample.
            CreateInitialGraphics();
        }

        private void SetButtonText()
        {
            // Update the multifunction button text based on the geometry editor state and hovered/picked up elements.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Picked up elements can be dropped, so the button text indicates that a point can be dropped.
                if (MyMapView.GeometryEditor.PickedUpElement != null)
                {
                    // If a vertex is picked up, the button text indicates that it can be dropped.
                    MultifunctionButton.Text = "Drop point";
                    MultifunctionButton.IsEnabled = true;
                    _reticleState = ReticleState.PickedUp;
                    return;
                }

                // When vertex creation is allowed, the button text changes based on the hovered or picked up element. Vertices and mid-vertices can be picked up.
                if (_allowVertexCreation)
                {
                    MultifunctionButton.IsEnabled = true;

                    if (MyMapView.GeometryEditor.PickedUpElement != null)
                    {
                        MultifunctionButton.Text = "Drop point";
                        _reticleState = ReticleState.PickedUp;
                    }
                    else if (MyMapView.GeometryEditor.HoveredElement != null && (MyMapView.GeometryEditor.HoveredElement is GeometryEditorVertex || MyMapView.GeometryEditor.HoveredElement is GeometryEditorMidVertex))
                    {
                        MultifunctionButton.Text = "Pick up point";
                        _reticleState = MyMapView.GeometryEditor.HoveredElement is GeometryEditorVertex ? ReticleState.HoveringVertex : ReticleState.HoveringMidVertex;
                    }
                    else
                    {
                        MultifunctionButton.Text = "Insert point";
                        _reticleState = ReticleState.Default;
                    }
                }
                // When vertex creation is not allowed, the button text changes based on the picked up element only. Only vertices can be picked up, mid-vertices cannot be picked up.
                else
                {
                    // If no vertex is picked up, the button text indicates that a point can be picked up if the hovered element is a geometry editor vertex.
                    MultifunctionButton.Text = "Pick up point";
                    MultifunctionButton.IsEnabled = MyMapView.GeometryEditor.HoveredElement is GeometryEditorVertex;
                    _reticleState = MyMapView.GeometryEditor.HoveredElement is GeometryEditorVertex ? ReticleState.HoveringVertex : ReticleState.HoveringMidVertex;
                }
            });
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

            // Update the multifunction button text and enable it.
            MultifunctionButton.Text = "Start geometry editor";
            MultifunctionButton.IsEnabled = true;
        }

        private void CreateInitialGraphics()
        {
            // Create symbols for displaying new geometries.
            _pointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Square, Color.OrangeRed, 10);
            _multiPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Yellow, 5);
            _polylineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Blue, 2);
            var polygonLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.Black, 1);
            _polygonSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(70, 255, 0, 0), polygonLineSymbol);

            // Create geometries from Json formatted strings.
            var pinkneysGreen = (Polygon)Geometry.FromJson(_pinkneysGreenJson);
            var beechLodgeBoundary = (Polyline)Geometry.FromJson(_beechLodgeBoundaryJson);
            var treeMarkers = (Multipoint)Geometry.FromJson(_treeMarkersJson);

            // Add new example graphics from the geometries and symbols to the graphics overlay.
            _graphicsOverlay.Graphics.Add(new Graphic(pinkneysGreen) { Symbol = _polygonSymbol });
            _graphicsOverlay.Graphics.Add(new Graphic(beechLodgeBoundary) { Symbol = _polylineSymbol });
            _graphicsOverlay.Graphics.Add(new Graphic(treeMarkers) { Symbol = _multiPointSymbol });
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

        #region Event handlers
        private void GeometryEditor_PickedUpElementChanged(object sender, PickedUpElementChangedEventArgs e)
        {
            // Update the button text based on the picked up element.
            SetButtonText();
        }

        private void GeometryEditor_HoveredElementChanged(object sender, HoveredElementChangedEventArgs e)
        {
            // Update the button text based on the hovered element.
            SetButtonText();
        }

        private void MultifunctionButton_Clicked(object sender, EventArgs e)
        {
            // If the geometry editor is not started, start it with the selected geometry type.
            if (!_geometryEditor.IsStarted)
            {
                _geometryEditor.Start(_geometryTypes[(string)GeometryTypePicker.SelectedItem]);

                // Set the button text to indicate that the editor is active.
                SetButtonText();
                return;
            }

            if (_allowVertexCreation)
            {
                // When vertex creation is allowed vertices and mid-vertices can be picked up, new vertices can be inserted.
                switch (_reticleState)
                {
                    case ReticleState.Default:
                    case ReticleState.PickedUp:
                        _programmaticReticleTool.PlaceElementAtReticle();
                        break;
                    case ReticleState.HoveringVertex:
                    case ReticleState.HoveringMidVertex:
                        _programmaticReticleTool.SelectElementAtReticle();
                        _programmaticReticleTool.PickUpSelectedElement();
                        break;
                }
            }
            else
            {
                // When vertex creation is not allowed functionality is limited to picking up and moving existing vertices, mid-vertices cannot be picked up.
                switch (_reticleState)
                {
                    case ReticleState.PickedUp:
                        _programmaticReticleTool.PlaceElementAtReticle();
                        break;
                    case ReticleState.HoveringVertex:
                        _programmaticReticleTool.SelectElementAtReticle();
                        _programmaticReticleTool.PickUpSelectedElement();
                        break;
                    default:
                        // In edit mode, only picked up vertices can be placed and only vertices can be picked up.
                        break;
                }
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            try
            {
                // If the geometry editor is started, identify the geometry editor result at the tapped position.
                if (_geometryEditor.IsStarted)
                {
                    // Identify the geometry editor result at the tapped position.
                    IdentifyGeometryEditorResult result = await MyMapView.IdentifyGeometryEditorResultAsync(e.Position, 10);

                    if (result != null && result.Elements.Count > 0)
                    {
                        // Get the first element from the result.
                        GeometryEditorElement element = result.Elements.FirstOrDefault();

                        // If the element is a vertex or mid-vertex, set the viewpoint to its position and select it.
                        if (element is GeometryEditorVertex vertex)
                        {
                            await MyMapView.SetViewpointAsync(new Viewpoint(new MapPoint(vertex.Point.X, vertex.Point.Y, vertex.Point.SpatialReference)), TimeSpan.FromSeconds(0.3));
                            _geometryEditor.SelectVertex(vertex.PartIndex, vertex.VertexIndex);
                        }
                        else if (element is GeometryEditorMidVertex midVertex && _allowVertexCreation)
                        {
                            await MyMapView.SetViewpointAsync(new Viewpoint(new MapPoint(midVertex.Point.X, midVertex.Point.Y, midVertex.Point.SpatialReference)), TimeSpan.FromSeconds(0.3));
                            _geometryEditor.SelectMidVertex(midVertex.PartIndex, midVertex.SegmentIndex);
                        }
                    }

                    return;
                }

                // Identify graphics in the graphics overlay using the tapped position.
                IReadOnlyList<IdentifyGraphicsOverlayResult> results = await MyMapView.IdentifyGraphicsOverlaysAsync(e.Position, 10, false);

                // Try to get the first graphic from the first result.
                _selectedGraphic = results.FirstOrDefault()?.Graphics?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Report exceptions.
                await Application.Current.Windows[0].Page.DisplayAlert("Error editing", ex.Message, "OK");

                ResetFromEditingSession();
                return;
            }

            // Return since no graphic was selected.
            if (_selectedGraphic == null) return;

            _selectedGraphic.IsSelected = true;

            // Hide the selected graphic and start an editing session with a copy of it.
            _geometryEditor.Start(_selectedGraphic.Geometry);

            // Set the button text to indicate that the editor is active.
            SetButtonText();

            // If vertex creation is allowed, set the viewpoint to the center of the selected graphic's geometry.
            // Otherwise, set the viewpoint to the end point of the first part of the geometry.
            if (_allowVertexCreation)
            {
                await MyMapView.SetViewpointAsync(new Viewpoint(_selectedGraphic.Geometry.Extent.GetCenter(), MyMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale).TargetScale), TimeSpan.FromSeconds(0.3));
            }
            else
            {
                switch (_selectedGraphic.Geometry)
                {
                    case Polygon polygon:
                        await MyMapView.SetViewpointAsync(new Viewpoint(polygon.Parts[0].EndPoint, MyMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale).TargetScale), TimeSpan.FromSeconds(0.3));
                        break;
                    case Polyline polyline:
                        await MyMapView.SetViewpointAsync(new Viewpoint(polyline.Parts[0].EndPoint, MyMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale).TargetScale), TimeSpan.FromSeconds(0.3));
                        break;
                    case Multipoint multiPoint:
                        await MyMapView.SetViewpointAsync(new Viewpoint(multiPoint.Points.Last(), MyMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale).TargetScale), TimeSpan.FromSeconds(0.3));
                        break;
                    case MapPoint point:
                        await MyMapView.SetViewpointAsync(new Viewpoint(point, MyMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale).TargetScale), TimeSpan.FromSeconds(0.3));
                        break;
                }
            }

            // Hide the selected graphic while editing.
            _selectedGraphic.IsVisible = false;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Stop the geometry editor and get the resulting geometry.
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
            // Stop the geometry editor and discard the geometry.
            _geometryEditor.Stop();
            ResetFromEditingSession();
        }

        // Undo the last change made to the geometry while editing is active.
        private void UndoButton_Click(object sender, EventArgs e)
        {
            if (_geometryEditor.PickedUpElement != null)
            {
                _geometryEditor.CancelCurrentAction();
            }
            else
            {
                _geometryEditor.Undo();
            }
        }

        // Redo the last change made to the geometry while editing is active.
        private void RedoButton_Click(object sender, EventArgs e)
        {
            _geometryEditor.Redo();
        }

        private void AllowVertexCreationSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            // Update the programmatic reticle tool and geometry editor settings based on the switch state.
            _programmaticReticleTool.VertexCreationPreviewEnabled = _programmaticReticleTool.Style.GrowEffect.ApplyToMidVertices = _allowVertexCreation = e.Value;

            // If the geometry editor is started, update the button text to reflect the new state.
            if (_geometryEditor.IsStarted)
            {
                SetButtonText();
            }
        }

        private void SettingsButton_Clicked(object sender, EventArgs e)
        {
            SettingsPopup.IsVisible = true;
        }

        private void ClosePopupButton_Clicked(object sender, EventArgs e)
        {
            SettingsPopup.IsVisible = false;
        }

        // Delete the currently selected element of the geometry editor.
        private void DeleteSelectedButton_Click(object sender, EventArgs e)
        {
            _geometryEditor.DeleteSelectedElement();
        }
        #endregion
    }

    public enum ReticleState
    {
        Default,
        PickedUp,
        HoveringVertex,
        HoveringMidVertex,
    }
}