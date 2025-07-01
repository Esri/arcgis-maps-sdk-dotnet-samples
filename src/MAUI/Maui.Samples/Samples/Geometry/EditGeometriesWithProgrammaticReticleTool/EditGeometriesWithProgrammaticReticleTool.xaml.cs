// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.Editing;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Map = Esri.ArcGISRuntime.Mapping.Map;
using Color = System.Drawing.Color;

namespace ArcGIS.Samples.EditGeometriesWithProgrammaticReticleTool
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        "Edit geometries with programmatic reticle tool",
        "Geometry",
        "Edit geometries with programmatic operations to facilitate button driven workflows",
        "")]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class EditGeometriesWithProgrammaticReticleTool : INotifyPropertyChanged
    {
        private GeometryEditor _geometryEditor = new GeometryEditor();
        private ProgrammaticReticleTool _programmaticReticleTool = new ProgrammaticReticleTool();
        private Graphic _selectedGraphic;
        private GraphicsOverlay _graphicsOverlay;
        private bool _allowVertexCreation = true;

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

        private readonly Dictionary<string, GeometryType> _geometryTypes = new Dictionary<string, GeometryType>()
        {
            {"Point", GeometryType.Point },
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
                InitialViewpoint = new Viewpoint(51.523806, -0.775395, 1.4e4)
            };

            // Create a graphics overlay and add it to the map view.
            _graphicsOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

            MyMapView.GeometryEditor = _geometryEditor;
            MyMapView.GeometryEditor.Tool = _programmaticReticleTool;

            MyMapView.GeometryEditor.HoveredElementChanged += GeometryEditor_HoveredElementChanged;
            MyMapView.GeometryEditor.PickedUpElementChanged += GeometryEditor_PickedUpElementChanged;

            GeometryTypePicker.ItemsSource = _geometryTypes.Keys.ToList();
            GeometryTypePicker.SelectedIndex = 0;
            AllowVertexCreationSwitch.IsToggled = true;
            AllowVertexCreationSwitch.Toggled += AllowVertexCreationSwitch_Toggled;

            CreateInitialGraphics();
        }

        private void GeometryEditor_PickedUpElementChanged(object sender, PickedUpElementChangedEventArgs e)
        {
            SetButtonText();
        }

        private void GeometryEditor_HoveredElementChanged(object sender, HoveredElementChangedEventArgs e)
        {
            SetButtonText();
        }

        private void SetButtonText()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_allowVertexCreation)
                {
                    MultifunctionButton.IsEnabled = true;

                    if (MyMapView.GeometryEditor.HoveredElement != null)
                    {
                        if (MyMapView.GeometryEditor.HoveredElement is GeometryEditorGeometry || MyMapView.GeometryEditor.HoveredElement is GeometryEditorPart)
                        {
                            if (MyMapView.GeometryEditor.PickedUpElement != null)
                            {
                                MultifunctionButton.Text = "Drop element";
                            }
                            else
                            {
                                MultifunctionButton.Text = "Insert element";
                            }
                        }
                        else
                        {
                            MultifunctionButton.Text = "Pick up element";
                        }
                    }
                    else
                    {
                        if (MyMapView.GeometryEditor.PickedUpElement == null)
                        {
                            MultifunctionButton.Text = "Insert element";

                        }
                        else
                        {
                            MultifunctionButton.Text = "Drop element";

                        }
                    }
                }
                else
                {
                    if (MyMapView.GeometryEditor.HoveredElement == null && MyMapView.GeometryEditor.PickedUpElement == null)
                    {
                        MultifunctionButton.Text = "Pick up element";
                        MultifunctionButton.IsEnabled = false;
                    }
                    else if (MyMapView.GeometryEditor.HoveredElement != null)
                    {
                        MultifunctionButton.Text = "Pick up element";
                        MultifunctionButton.IsEnabled = MyMapView.GeometryEditor.HoveredElement is GeometryEditorVertex;
                    }
                    else if (MyMapView.GeometryEditor.PickedUpElement != null)
                    {
                        MultifunctionButton.Text = "Drop element";
                        MultifunctionButton.IsEnabled = true;
                    }
                }
            });
        }

        private void MultifunctionButton_Clicked(object sender, EventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
                _geometryEditor.Start(_geometryTypes[(string)GeometryTypePicker.SelectedItem]);
                SetButtonText();
                return;
            }

            if (_allowVertexCreation)
            {
                if (MyMapView.GeometryEditor.HoveredElement != null)
                {
                    if (MyMapView.GeometryEditor.HoveredElement is GeometryEditorGeometry || MyMapView.GeometryEditor.HoveredElement is GeometryEditorPart)
                    {
                        _programmaticReticleTool.PlaceElementAtReticle();
                    }
                    else
                    {
                        _programmaticReticleTool.SelectElementAtReticle();
                        _programmaticReticleTool.PickUpSelectedElement();
                    }
                }
                else
                {
                    _programmaticReticleTool.PlaceElementAtReticle();
                }
            }
            else
            {
                if (MyMapView.GeometryEditor.HoveredElement != null && MyMapView.GeometryEditor.HoveredElement is GeometryEditorVertex && MyMapView.GeometryEditor.PickedUpElement == null)
                {
                    _programmaticReticleTool.SelectElementAtReticle();
                    _programmaticReticleTool.PickUpSelectedElement();
                }
                else if (MyMapView.GeometryEditor.PickedUpElement != null)
                {
                    _programmaticReticleTool.PlaceElementAtReticle();
                }
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            try
            {
                if (_geometryEditor.IsStarted)
                {
                    var result = await MyMapView.IdentifyGeometryEditorResultAsync(e.Position, 5);

                    if (result != null && result.Elements.Count > 0)
                    {
                        var firstElement = result.Elements[0];
                        if (firstElement is GeometryEditorVertex vertex)
                        {
                            MyMapView.SetViewpoint(new Viewpoint(new MapPoint(vertex.Point.X, vertex.Point.Y, vertex.Point.SpatialReference)));
                            _geometryEditor.SelectVertex(vertex.PartIndex, vertex.VertexIndex);
                        }
                        else if (firstElement is GeometryEditorMidVertex midVertex && _allowVertexCreation)
                        {
                            MyMapView.SetViewpoint(new Viewpoint(new MapPoint(midVertex.Point.X, midVertex.Point.Y, midVertex.Point.SpatialReference)));
                            _geometryEditor.SelectMidVertex(midVertex.PartIndex, midVertex.SegmentIndex);
                        }
                    }

                    return;
                }

                // Identify graphics in the graphics overlay using the mouse point.
                IReadOnlyList<IdentifyGraphicsOverlayResult> results = await MyMapView.IdentifyGraphicsOverlaysAsync(e.Position, 5, false);

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
            SetButtonText();
            if (_allowVertexCreation)
            {
                MyMapView.SetViewpoint(new Viewpoint(_selectedGraphic.Geometry.Extent.GetCenter(), MyMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale).TargetScale));
            }
            else
            {
                if (_selectedGraphic.Geometry is Polygon polygon)
                {
                    MyMapView.SetViewpoint(new Viewpoint(polygon.Parts[0].EndPoint, MyMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale).TargetScale));
                }
                else if (_selectedGraphic.Geometry is Polyline polyline)
                {
                    MyMapView.SetViewpoint(new Viewpoint(polyline.Parts[0].EndPoint, MyMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale).TargetScale));
                }
                else if (_selectedGraphic.Geometry is Multipoint multiPoint)
                {
                    MyMapView.SetViewpoint(new Viewpoint(multiPoint.Points.Last(), MyMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale).TargetScale));
                }
                else if (_selectedGraphic.Geometry is MapPoint point)
                {
                    MyMapView.SetViewpoint(new Viewpoint(point, MyMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale).TargetScale));
                }
            }
                _selectedGraphic.IsVisible = false;
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

            MultifunctionButton.Text = "Start geometry editor";
            MultifunctionButton.IsEnabled = true;
        }

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

        // Starts the geometry editor with the point geometry type.
        private void PointButton_Click(object sender, EventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
                _geometryEditor.Start(GeometryType.Point);
            }
        }

        // Start the geometry editor with the multipoint geometry type.
        private void MultipointButton_Click(object sender, EventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
                _geometryEditor.Start(GeometryType.Multipoint);
            }
        }

        // Start the geometry editor with the polyline geometry type.
        private void PolylineButton_Click(object sender, EventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
                _geometryEditor.Start(GeometryType.Polyline);
            }
        }

        // Start the geometry editor with the polygon geometry type.
        private void PolygonButton_Click(object sender, EventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
                _geometryEditor.Start(GeometryType.Polygon);
            }
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
            _programmaticReticleTool.VertexCreationPreviewEnabled = e.Value;
            _programmaticReticleTool.Style.GrowEffect.ApplyToMidVertices = e.Value;
            _allowVertexCreation = e.Value;
            SetButtonText();
        }

        // Delete the currently selected element of the geometry editor.
        private void DeleteSelectedButton_Click(object sender, EventArgs e)
        {
            _geometryEditor.DeleteSelectedElement();
        }
    }
}