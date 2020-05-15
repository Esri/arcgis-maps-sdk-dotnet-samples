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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UIKit;

namespace ArcGISRuntime.Samples.SketchOnMap
{
    [Register("SketchOnMap")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Sketch on map",
        category: "GraphicsOverlay",
        description: "Use the Sketch Editor to edit or sketch a new point, line, or polygon geometry on to a map.",
        instructions: "Choose which geometry type to sketch from one of the available buttons. Choose from points, multipoints, polylines, polygons, freehand polylines, and freehand polygons.",
        tags: new[] { "Geometry", "Graphic", "GraphicsOverlay", "SketchCreationMode", "SketchEditor", "draw", "edit" })]
    public class SketchOnMap : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UISegmentedControl _segmentButton;

        // Dictionary associates SketchCreationMode names with values.
        private Dictionary<string, int> _sketchModeDictionary;

        // Graphics overlay to host sketch graphics.
        private GraphicsOverlay _sketchOverlay;

        public SketchOnMap()
        {
            Title = "Sketch on map";
        }

        private void Initialize()
        {
            // Create and show a light gray canvas basemap.
            _myMapView.Map = new Map(Basemap.CreateLightGrayCanvas());

            // Create graphics overlay to display sketch geometry.
            _sketchOverlay = new GraphicsOverlay();
            _myMapView.GraphicsOverlays.Add(_sketchOverlay);
        }

        private void GraphicsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Enable or disable the clear and edit buttons depending on whether or not graphics exist.
            _segmentButton.SetEnabled(_sketchOverlay.Graphics.Count > 0, 1);
            _segmentButton.SetEnabled(_sketchOverlay.Graphics.Count > 0, 5);
        }

        private void CanExecuteChanged(object sender, EventArgs e)
        {
            // Enable or disable the corresponding command for the sketch editor.
            ICommand command = (ICommand)sender;
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

        private async void SegmentButtonClicked(object sender, EventArgs e)
        {
            // Get the segmented button control that raised the event.
            UISegmentedControl buttonControl = (UISegmentedControl)sender;

            // Execute the appropriate action for the control
            switch (buttonControl.SelectedSegment)
            {
                case 0: // Sketch.
                    // Show the sketch modes to choose from.
                    ShowSketchModeList();
                    break;

                case 1:
                    await EditGraphicAsync();
                    break;

                case 2: // Undo.
                    if (_myMapView.SketchEditor.UndoCommand.CanExecute(null))
                    {
                        _myMapView.SketchEditor.UndoCommand.Execute(null);
                    }
                    break;

                case 3: // Redo.
                    if (_myMapView.SketchEditor.RedoCommand.CanExecute(null))
                    {
                        _myMapView.SketchEditor.RedoCommand.Execute(null);
                    }
                    break;

                case 4: // Done.
                    if (_myMapView.SketchEditor.CompleteCommand.CanExecute(null))
                    {
                        _myMapView.SketchEditor.CompleteCommand.Execute(null);
                    }
                    break;

                case 5: // Clear.
                    // Remove all graphics from the graphics overlay.
                    _sketchOverlay.Graphics.Clear();

                    // Cancel any uncompleted sketch.
                    if (_myMapView.SketchEditor.CancelCommand.CanExecute(null))
                    {
                        _myMapView.SketchEditor.CancelCommand.Execute(null);
                    }
                    break;
            }

            // Deselect all segments (user might want to click the same control twice).
            buttonControl.SelectedSegment = -1;
        }

        #region Graphic and symbol helpers

        private Graphic CreateGraphic(Geometry geometry)
        {
            // Create a graphic to display the specified geometry.
            Symbol symbol = null;
            switch (geometry.GeometryType)
            {
                // Symbolize with a fill symbol.
                case GeometryType.Envelope:
                case GeometryType.Polygon:
                    {
                        symbol = new SimpleFillSymbol
                        {
                            Color = Color.Red,
                            Style = SimpleFillSymbolStyle.Solid
                        };
                        break;
                    }
                // Symbolize with a line symbol.
                case GeometryType.Polyline:
                    {
                        symbol = new SimpleLineSymbol
                        {
                            Color = Color.Red,
                            Style = SimpleLineSymbolStyle.Solid,
                            Width = 5d
                        };
                        break;
                    }
                // Symbolize with a marker symbol.
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    {
                        symbol = new SimpleMarkerSymbol
                        {
                            Color = Color.Red,
                            Style = SimpleMarkerSymbolStyle.Circle,
                            Size = 15d
                        };
                        break;
                    }
            }

            // Pass back a new graphic with the appropriate symbol.
            return new Graphic(geometry, symbol);
        }

        private async Task<Graphic> GetGraphicAsync()
        {
            // Wait for the user to click a location on the map.
            MapPoint mapPoint = (MapPoint)await _myMapView.SketchEditor.StartAsync(SketchCreationMode.Point, false);

            // Convert the map point to a screen point.
            var screenCoordinate = _myMapView.LocationToScreen(mapPoint);

            // Identify graphics in the graphics overlay using the point.
            IReadOnlyList<IdentifyGraphicsOverlayResult> results = await _myMapView.IdentifyGraphicsOverlaysAsync(screenCoordinate, 2, false);

            // If results were found, get the first graphic.
            Graphic graphic = null;
            IdentifyGraphicsOverlayResult idResult = results.FirstOrDefault();
            if (idResult != null && idResult.Graphics.Count > 0)
            {
                graphic = idResult.Graphics.FirstOrDefault();
            }

            // Return the graphic (or null if none were found).
            return graphic;
        }

        #endregion Graphic and symbol helpers

        private void ShowSketchModeList()
        {
            // Create a new Alert Controller.
            UIAlertController sketchModeActionSheet = UIAlertController.Create("Sketch Modes", "Choose a sketch mode", UIAlertControllerStyle.ActionSheet);

            // Create a dictionary that associates SketchCreationMode names with values.
            IEnumerable<int> enumValues = Enum.GetValues(typeof(SketchCreationMode)).Cast<int>();
            _sketchModeDictionary = enumValues.ToDictionary(v => Enum.GetName(typeof(SketchCreationMode), v), v => v);

            // Add sketch modes to the action sheet.
            foreach (KeyValuePair<string, int> mode in _sketchModeDictionary)
            {
                UIAlertAction actionItem = UIAlertAction.Create(mode.Key, UIAlertActionStyle.Default, action => SketchGeometry(action.Title));
                sketchModeActionSheet.AddAction(actionItem);
            }

            // Required for iPad - You must specify a source for the Action Sheet since it is displayed as a popover.
            UIPopoverPresentationController presentationPopover = sketchModeActionSheet.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Left;
            }

            // Display the list of sketch modes.
            PresentViewController(sketchModeActionSheet, true, null);
        }

        private async void SketchGeometry(string sketchModeName)
        {
            try
            {
                // Let the user draw on the map view using the chosen sketch mode.
                SketchCreationMode creationMode = (SketchCreationMode)_sketchModeDictionary[sketchModeName];
                Geometry geometry = await _myMapView.SketchEditor.StartAsync(creationMode, true);

                // Create and add a graphic from the geometry the user drew.
                Graphic graphic = CreateGraphic(geometry);
                _sketchOverlay.Graphics.Add(graphic);
            }
            catch (TaskCanceledException)
            {
                // Ignore ... let the user cancel drawing.
            }
            catch (Exception ex)
            {
                // Report exceptions.
                new UIAlertView("Error", "Error drawing graphic shape: " + ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
        }

        private async Task EditGraphicAsync()
        {
            try
            {
                // Allow the user to select a graphic.
                Graphic editGraphic = await GetGraphicAsync();
                if (editGraphic == null)
                {
                    return;
                }

                // Let the user make changes to the graphic's geometry, await the result (updated geometry).
                editGraphic.Geometry = await _myMapView.SketchEditor.StartAsync(editGraphic.Geometry);
            }
            catch (TaskCanceledException)
            {
                // Ignore ... let the user cancel editing.
            }
            catch (Exception ex)
            {
                // Report exceptions.
                new UIAlertView("Error", "Error editing shape: " + ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _segmentButton = new UISegmentedControl("Sketch", "Edit", "Undo", "Redo", "Done", "Clear")
            {
                BackgroundColor = UIColor.White,
                TintColor = UIColor.White,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Clean up borders of segmented control - avoid corner pixels.
            _segmentButton.ClipsToBounds = true;
            _segmentButton.Layer.CornerRadius = 5;

            // Add the views.
            View.AddSubviews(_myMapView, _segmentButton);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),

                _segmentButton.LeadingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.LeadingAnchor),
                _segmentButton.TrailingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.TrailingAnchor),
                _segmentButton.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _segmentButton.ValueChanged += SegmentButtonClicked;
            _myMapView.SketchEditor.UndoCommand.CanExecuteChanged += CanExecuteChanged; // CanExecuteChanged events used to enable/disable controls.
            _myMapView.SketchEditor.RedoCommand.CanExecuteChanged += CanExecuteChanged;
            _myMapView.SketchEditor.CompleteCommand.CanExecuteChanged += CanExecuteChanged;
            _sketchOverlay.Graphics.CollectionChanged += GraphicsChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            try
            {
                // Unsubscribe from events, per best practice.
                if (_myMapView.SketchEditor.CancelCommand.CanExecute(null))
                {
                    _myMapView.SketchEditor.CancelCommand.Execute(null);
                }
                _myMapView.SketchEditor.UndoCommand.CanExecuteChanged -= CanExecuteChanged;
                _myMapView.SketchEditor.RedoCommand.CanExecuteChanged -= CanExecuteChanged;
                _myMapView.SketchEditor.CompleteCommand.CanExecuteChanged -= CanExecuteChanged;
                _sketchOverlay.Graphics.CollectionChanged -= GraphicsChanged;
                _segmentButton.ValueChanged -= SegmentButtonClicked;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}