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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System.Collections.Generic;
using UIKit;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Drawing;

namespace ArcGISRuntime.Samples.SketchOnMap
{
    [Register("SketchOnMap")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Sketch graphics on the map",
        "GraphicsOverlay",
        "This sample demonstrates how to interactively sketch and edit graphics in the map view and display them in a graphics overlay. You can sketch a variety of geometry types and undo or redo operations.",
        "1. Click the 'Sketch' button.\n2. Choose a sketch type from the drop down list.\n3. While sketching, you can undo/redo operations.\n4. Click 'Done' to finish the sketch.\n5. Click 'Edit', then click a graphic to start editing.\n6. Make edits then click 'Done' or 'Cancel' to finish editing.")]
    public class SketchOnMap : UIViewController
    {
        // Constant holding offset where the MapView control should start

        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Dictionary to hold sketch mode enum names and values
        private Dictionary<string, int> _sketchModeDictionary;

        // Graphics overlay to host sketch graphics
        private GraphicsOverlay _sketchOverlay;

        // Segmented control to show sketch controls
        private UISegmentedControl _segmentButton;
        
        public SketchOnMap()
        {
            Title = "Sketch on map";
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			// Initialize controls, set up event handlers, etc.
			Initialize();

            // Create the UI 
            CreateLayout();


        }

        public override void ViewDidLayoutSubviews()
        {
           // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _segmentButton.Frame = new CoreGraphics.CGRect(0, _myMapView.Bounds.Height - 60, View.Bounds.Width, 30);
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
            // Create a new MapView control


            // Add a segmented button control
            _segmentButton = new UISegmentedControl();
            _segmentButton.BackgroundColor = UIColor.White;
            _segmentButton.Frame = new CoreGraphics.CGRect(0, _myMapView.Bounds.Height - 60, View.Bounds.Width, 30);
            _segmentButton.InsertSegment("Sketch", 0, false);
            _segmentButton.InsertSegment("Edit", 1, false);
            _segmentButton.InsertSegment("Undo", 2, false);
            _segmentButton.InsertSegment("Redo", 3, false);
            _segmentButton.InsertSegment("Done", 4, false);
            _segmentButton.InsertSegment("Clear", 5, false);

            // Disable all segment buttons except "Sketch"
            _segmentButton.SetEnabled(false, 1);
            _segmentButton.SetEnabled(false, 2);
            _segmentButton.SetEnabled(false, 3);
            _segmentButton.SetEnabled(false, 4);
            _segmentButton.SetEnabled(false, 5);

            // Handle the "click" for each segment (new segment is selected)
            _segmentButton.ValueChanged += SegmentButtonClicked;

            // Add the MapView and UIButton to the page
            View.AddSubviews(_myMapView, _segmentButton);
        }

        private void GraphicsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Enable or disable the clear and edit buttons depending on whether or not graphics exist
            _segmentButton.SetEnabled(_sketchOverlay.Graphics.Count > 0, 1);
            _segmentButton.SetEnabled(_sketchOverlay.Graphics.Count > 0, 5);
        }

        private void CanExecuteChanged(object sender, EventArgs e)
        {
            // Enable or disable the corresponding command for the sketch editor
            var command = sender as System.Windows.Input.ICommand;
            if (command == _myMapView.SketchEditor.UndoCommand)
            {
                _segmentButton.SetEnabled(command.CanExecute(null), 2);
            }
            else if (command == _myMapView.SketchEditor.RedoCommand)
            {
                _segmentButton.SetEnabled(command.CanExecute(null), 3);
            }
            else if (command == _myMapView.SketchEditor.CompleteCommand)
            {
                _segmentButton.SetEnabled(command.CanExecute(null), 4);
            }
        }

        private void SegmentButtonClicked(object sender, EventArgs e)
        {
            // Get the segmented button control that raised the event
            var buttonControl = sender as UISegmentedControl;

            // Get the selected segment in the control
            var selectedSegmentId = buttonControl.SelectedSegment;

            // Execute the appropriate action for the control
            switch (selectedSegmentId)
            {
                case 0: // Sketch
                    // Show the sketch modes to choose from
                    ShowSketchModeList();
                    break;
                case 1:
                    EditGraphic();
                    break;
                case 2: // Undo
                    if (_myMapView.SketchEditor.UndoCommand.CanExecute(null))
                    {
                        _myMapView.SketchEditor.UndoCommand.Execute(null);
                    }
                    break;
                case 3: // Redo
                    if (_myMapView.SketchEditor.RedoCommand.CanExecute(null))
                    {
                        _myMapView.SketchEditor.RedoCommand.Execute(null);
                    }
                    break;
                case 4: // Done
                    if (_myMapView.SketchEditor.CompleteCommand.CanExecute(null))
                    {
                        _myMapView.SketchEditor.CompleteCommand.Execute(null);
                    }
                    break;
                case 5: // Clear
                        // Remove all graphics from the graphics overlay
                    _sketchOverlay.Graphics.Clear();

                    // Cancel any uncompleted sketch
                    if (_myMapView.SketchEditor.CancelCommand.CanExecute(null))
                    {
                        _myMapView.SketchEditor.CancelCommand.Execute(null);
                    }
                    break;
            }

            // Unselect all segments (user might want to click the same control twice)
            buttonControl.SelectedSegment = -1;
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

        private void ShowSketchModeList()
        {
            // Create a new Alert Controller
            UIAlertController sketchModeActionSheet = UIAlertController.Create("Sketch Modes", "Choose a sketch mode", UIAlertControllerStyle.ActionSheet);

            // Create an action for drawing the selected sketch type
            var sketchAction = new Action<UIAlertAction>((axun) => { SketchGeometry(axun.Title); });

            // Create a dictionary of enum names and values
            var enumValues = Enum.GetValues(typeof(SketchCreationMode)).Cast<int>();
            _sketchModeDictionary = enumValues.ToDictionary(v => Enum.GetName(typeof(SketchCreationMode), v), v => v);
            
            // Add sketch modes to the action sheet
            foreach (var mode in _sketchModeDictionary)
            {
                UIAlertAction actionItem = UIAlertAction.Create(mode.Key, UIAlertActionStyle.Default, sketchAction);
                sketchModeActionSheet.AddAction(actionItem);
            }

            // Required for iPad - You must specify a source for the Action Sheet since it is displayed as a popover
            UIPopoverPresentationController presentationPopover = sketchModeActionSheet.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the list of sketch modes
            PresentViewController(sketchModeActionSheet, true, null);
        }

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
                UIAlertView alert = new UIAlertView("Error", "Error drawing graphic shape: " + ex.Message, null, "OK", null);
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
                UIAlertView alert = new UIAlertView("Error", "Error editing shape: " + ex.Message, null, "OK", null);
            }
        }
    }
}