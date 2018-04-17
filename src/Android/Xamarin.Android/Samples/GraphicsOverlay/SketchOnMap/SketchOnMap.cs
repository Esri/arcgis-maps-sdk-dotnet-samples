// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.SketchOnMap
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Sketch graphics on the map",
        "GraphicsOverlay",
        "This sample demonstrates how to interactively sketch and edit graphics in the map view and display them in a graphics overlay. You can sketch a variety of geometry types and undo or redo operations.",
        "1. Click the 'Sketch' button.\n2. Choose a sketch type from the drop down list.\n3. While sketching, you can undo/redo operations.\n4. Click 'Done' to finish the sketch.\n5. Click 'Edit', then click a graphic to start editing.\n6. Make edits then click 'Done' or 'Cancel' to finish editing.")]
    public class SketchOnMap : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Dictionary to hold sketch mode enum names and values
        private Dictionary<string, int> _sketchModeDictionary;

        // Graphics overlay to host sketch graphics
        private GraphicsOverlay _sketchOverlay;

        // Buttons for interacting with the SketchEditor
        private Button _editButton;
        private Button _undoButton;
        private Button _redoButton;
        private Button _doneButton;
        private Button _clearButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Sketch on map";

            // Create the UI 
            CreateLayout();

            // Initialize controls, set up event handlers, etc.
            Initialize();
        }

        private void Initialize()
        {
            // Create a light gray canvas map
            Map myMap = new Map(Basemap.CreateLightGrayCanvas());

            // Create graphics overlay to display sketch geometry
            _sketchOverlay = new GraphicsOverlay();
            _myMapView.GraphicsOverlays.Add(_sketchOverlay);

            // Assign the map to the MapView
            _myMapView.Map = myMap;

            // Set the sketch editor configuration to allow vertex editing, resizing, and moving
            var config = _myMapView.SketchEditor.EditConfiguration;
            config.AllowVertexEditing = true;
            config.ResizeMode = SketchResizeMode.Uniform;
            config.AllowMove = true;

            // Listen to the sketch editor tools CanExecuteChange so controls can be enabled/disabled
            _myMapView.SketchEditor.UndoCommand.CanExecuteChanged += CanExecuteChanged;
            _myMapView.SketchEditor.RedoCommand.CanExecuteChanged += CanExecuteChanged;
            _myMapView.SketchEditor.CompleteCommand.CanExecuteChanged += CanExecuteChanged;

            // Listen to collection changed event on the graphics overlay to enable/disable controls that require a graphic
            _sketchOverlay.Graphics.CollectionChanged += GraphicsChanged;
        }

        private void CreateLayout()
        {
            // Create horizontal layouts for the buttons at the top
            var buttonLayoutOne = new LinearLayout(this) { Orientation = Orientation.Horizontal };
            var buttonLayoutTwo = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Button to sketch a selected geometry type on the map view
            var sketchButton = new Button(this);
            sketchButton.Text = "Sketch";
            sketchButton.Click += OnSketchClicked;

            // Button to edit an existing graphic's geometry
            _editButton = new Button(this);
            _editButton.Text = "Edit";
            _editButton.Click += OnEditClicked;
            _editButton.Enabled = false;

            // Buttons to Undo/Redo sketch and edit operations
            _undoButton = new Button(this);
            _undoButton.Text = "Undo";
            _undoButton.Click += OnUndoClicked;
            _undoButton.Enabled = false;
            _redoButton = new Button(this);
            _redoButton.Text = "Redo";
            _redoButton.Click += OnRedoClicked;
            _redoButton.Enabled = false;

            // Button to complete the sketch or edit
            _doneButton = new Button(this);
            _doneButton.Text = "Done";
            _doneButton.Click += OnCompleteClicked;
            _doneButton.Enabled = false;

            // Button to clear all graphics and sketches
            _clearButton = new Button(this);
            _clearButton.Text = "Clear";
            _clearButton.Click += OnClearClicked;
            _clearButton.Enabled = false;

            // Add all sketch controls (buttons) to the button bars
            buttonLayoutOne.AddView(sketchButton);
            buttonLayoutOne.AddView(_editButton);
            buttonLayoutOne.AddView(_clearButton);
            // Second button bar
            buttonLayoutTwo.AddView(_undoButton);
            buttonLayoutTwo.AddView(_redoButton);
            buttonLayoutTwo.AddView(_doneButton);

            // Create a new vertical layout for the app (buttons followed by map view)
            var mainLayout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the button layouts
            mainLayout.AddView(buttonLayoutOne);
            mainLayout.AddView(buttonLayoutTwo);

            // Add the map view to the layout
            mainLayout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(mainLayout);
        }

        private void OnSketchClicked(object sender, EventArgs e)
        {
            var sketchButton = sender as Button;

            // Create a dictionary of enum names and values
            var enumValues = Enum.GetValues(typeof(SketchCreationMode)).Cast<int>();
            _sketchModeDictionary = enumValues.ToDictionary(v => Enum.GetName(typeof(SketchCreationMode), v), v => v);

            // Create a menu to show sketch modes
            var sketchModesMenu = new PopupMenu(sketchButton.Context, sketchButton);
            sketchModesMenu.MenuItemClick += OnSketchModeItemClicked;

            // Create a menu option for each basemap type
            foreach (var mode in _sketchModeDictionary)
            {
                sketchModesMenu.Menu.Add(mode.Key);
            }

            // Show menu in the view
            sketchModesMenu.Show();
        }

        private async void OnSketchModeItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            try
            {
                // Get the title of the selected menu item (sketch mode)
                var sketchModeName = e.Item.TitleCondensedFormatted.ToString();

                // Let the user draw on the map view using the chosen sketch mode
                SketchCreationMode creationMode = (SketchCreationMode)_sketchModeDictionary[sketchModeName];
                Geometry geometry = await _myMapView.SketchEditor.StartAsync(creationMode, true);

                // Create and add a graphic from the geometry the user drew
                Graphic graphic = CreateGraphic(geometry);
                _sketchOverlay.Graphics.Add(graphic);
            }
            catch (TaskCanceledException)
            {
                // Ignore ... let the user cancel drawing
            }
            catch (Exception ex)
            {
                // Report exceptions
                var alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Error drawing graphic shape");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            try
            {
                // Allow the user to select a graphic
                Graphic editGraphic = await GetGraphicAsync();
                if (editGraphic == null) { return; }

                // Let the user make changes to the graphic's geometry, await the result (updated geometry)
                Geometry newGeometry = await _myMapView.SketchEditor.StartAsync(editGraphic.Geometry);

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
                var alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Error editing shape");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            // Remove all graphics from the graphics overlay
            _sketchOverlay.Graphics.Clear();

            // Cancel any uncompleted sketch
            if (_myMapView.SketchEditor.CancelCommand.CanExecute(null))
            {
                _myMapView.SketchEditor.CancelCommand.Execute(null);
            }
        }

        private void OnCompleteClicked(object sender, EventArgs e)
        {
            // Execute the CompleteCommand on the sketch editor
            if (_myMapView.SketchEditor.CompleteCommand.CanExecute(null))
            {
                _myMapView.SketchEditor.CompleteCommand.Execute(null);
            }
        }

        private void OnRedoClicked(object sender, EventArgs e)
        {
            // Execute the RedoCommand on the sketch editor
            if (_myMapView.SketchEditor.RedoCommand.CanExecute(null))
            {
                _myMapView.SketchEditor.RedoCommand.Execute(null);
            }
        }

        private void OnUndoClicked(object sender, EventArgs e)
        {
            // Execute the UndoCommand on the sketch editor
            if (_myMapView.SketchEditor.UndoCommand.CanExecute(null))
            {
                _myMapView.SketchEditor.UndoCommand.Execute(null);
            }
        }

        private void GraphicsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Enable or disable the clear and edit buttons depending on whether or not graphics exist
            bool haveGraphics = _sketchOverlay.Graphics.Count > 0;
            _editButton.Enabled = haveGraphics;
            _clearButton.Enabled = haveGraphics;
        }

        private void CanExecuteChanged(object sender, EventArgs e)
        {
            // Enable or disable the corresponding command for the sketch editor
            var command = sender as System.Windows.Input.ICommand;
            if (command == _myMapView.SketchEditor.UndoCommand)
            {
                _undoButton.Enabled = command.CanExecute(null);
            }
            else if (command == _myMapView.SketchEditor.RedoCommand)
            {
                _redoButton.Enabled = command.CanExecute(null);
            }
            else if (command == _myMapView.SketchEditor.CompleteCommand)
            {
                _doneButton.Enabled = command.CanExecute(null);
            }
        }

        #region Graphic and symbol helpers
        private Graphic CreateGraphic(Geometry geometry)
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
                            Color = Color.Red,
                            Style = SimpleFillSymbolStyle.Solid,
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

            // pass back a new graphic with the appropriate symbol
            return new Graphic(geometry, symbol);
        }

        private async Task<Graphic> GetGraphicAsync()
        {
            // Wait for the user to click a location on the map
            var mapPoint = (MapPoint)await _myMapView.SketchEditor.StartAsync(SketchCreationMode.Point, false);

            // Convert the map point to a screen point
            var screenCoordinate = _myMapView.LocationToScreen(mapPoint);

            // Identify graphics in the graphics overlay using the point
            var results = await _myMapView.IdentifyGraphicsOverlaysAsync(screenCoordinate, 2, false);

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

        private async void SketchGeometry(string sketchModeName)
        {
            try
            {
                // Let the user draw on the map view using the chosen sketch mode
                SketchCreationMode creationMode = (SketchCreationMode)_sketchModeDictionary[sketchModeName];
                Geometry geometry = await _myMapView.SketchEditor.StartAsync(creationMode, true);

                // Create and add a graphic from the geometry the user drew
                Graphic graphic = CreateGraphic(geometry);
                _sketchOverlay.Graphics.Add(graphic);
            }
            catch (TaskCanceledException)
            {
                // Ignore ... let the user cancel drawing
            }
            catch (Exception ex)
            {
                // Report exceptions
                var alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Error drawing graphic shape");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }
        }

        private async void EditGraphic()
        {
            try
            {
                // Allow the user to select a graphic
                Graphic editGraphic = await GetGraphicAsync();
                if (editGraphic == null) { return; }

                // Let the user make changes to the graphic's geometry, await the result (updated geometry)
                Geometry newGeometry = await _myMapView.SketchEditor.StartAsync(editGraphic.Geometry);

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
                var alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Error editing shape");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }
        }
    }
}