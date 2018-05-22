// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using CoreGraphics;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;
using Colors = System.Drawing.Color;

namespace ArcGISRuntime.Samples.DisplayGrid
{
    [Register("DisplayGrid")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display a grid",
        "MapView",
        "Display and work with coordinate grid systems such as Latitude/Longitude, MGRS, UTM and USNG on a map view. This includes toggling labels visibility, changing the color of the grid lines, and changing the color of the grid labels.",
        "Use the buttons in the toolbar to change grid settings. Changes take effect immediately.")]
    public class DisplayGrid : UIViewController
    {
        // Declare the UI controls.
        private UIToolbar _toolbar = new UIToolbar();

        private UIButton _gridTypeButton = new UIButton();
        private UIButton _gridColorButton = new UIButton();
        private UIButton _labelPositionButton = new UIButton();
        private UIButton _labelColorButton = new UIButton();
        private MapView _myMapView = new MapView();

        // Fields for storing the user's grid preferences.
        private string _selectedGridType = "LatLong";

        private Colors? _selectedGridColor = Colors.Red;
        private Colors? _selectedLabelColor = Colors.White;
        private GridLabelPosition _selectedLabelPosition = GridLabelPosition.Geographic;

        public DisplayGrid()
        {
            Title = "Display a grid";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Set a basemap.
            _myMapView.Map = new Map(Basemap.CreateImageryWithLabelsVector());

            // Apply a grid by default.
            ApplyCurrentSettings();
        }

        private void ApplyCurrentSettings()
        {
            // First, update the grid based on the type selected.
            switch (_selectedGridType)
            {
                case "LatLong":
                    _myMapView.Grid = new LatitudeLongitudeGrid();
                    break;

                case "MGRS":
                    _myMapView.Grid = new MgrsGrid();
                    break;

                case "UTM":
                    _myMapView.Grid = new UtmGrid();
                    break;

                case "USNG":
                    _myMapView.Grid = new UsngGrid();
                    break;
            }

            // Next, apply the label position setting.
            _myMapView.Grid.LabelPosition = _selectedLabelPosition;

            // Next, apply the grid color and label color settings for each zoom level.
            for (long level = 0; level < _myMapView.Grid.LevelCount; level++)
            {
                // Set the grid color if the grid is selected for display.
                if (_selectedGridColor != null)
                {
                    // Set the line symbol.
                    Symbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, _selectedGridColor.Value, 2);
                    _myMapView.Grid.SetLineSymbol(level, lineSymbol);
                }

                // Set the label color if labels are enabled for display.
                if (_selectedLabelColor != null)
                {
                    // Set the text symbol.
                    Symbol textSymbol = new TextSymbol
                    {
                        Color = _selectedLabelColor.Value,
                        OutlineColor = Colors.Purple,
                        Size = 16,
                        HaloColor = Colors.Purple,
                        HaloWidth = 3
                    };
                    _myMapView.Grid.SetTextSymbol(level, textSymbol);
                }
            }

            // Next, hide the grid if it has been hidden.
            if (_selectedGridColor == null)
            {
                _myMapView.Grid.IsVisible = false;
            }

            // Next, hide the labels if they have been hidden.
            if (_selectedLabelColor == null)
            {
                _myMapView.Grid.IsLabelVisible = false;
            }
        }

        private void _labelPositionButton_TouchUpInside(object sender, EventArgs e)
        {
            // Create the view controller that will present the list of label positions.
            UIAlertController labelPositionAlert = UIAlertController.Create("Select a label position", "", UIAlertControllerStyle.ActionSheet);

            // Needed to prevent a crash on iPad.
            UIPopoverPresentationController presentationPopover = labelPositionAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Add an option for each label position option.
            foreach (string item in Enum.GetNames(typeof(GridLabelPosition)))
            {
                // Record the selection and re-apply all settings.
                labelPositionAlert.AddAction(UIAlertAction.Create(item, UIAlertActionStyle.Default, action =>
                {
                    _selectedLabelPosition = (GridLabelPosition)Enum.Parse(typeof(GridLabelPosition), item);
                    ApplyCurrentSettings();
                }));
            }

            // Show the alert.
            PresentViewController(labelPositionAlert, true, null);
        }

        private void _gridTypeButton_TouchUpInside(object sender, EventArgs e)
        {
            // Create the view controller that will present the list of grid types.
            UIAlertController gridTypeAlert = UIAlertController.Create("Select a grid type", "", UIAlertControllerStyle.ActionSheet);

            // Needed to prevent a crash on iPad.
            UIPopoverPresentationController presentationPopover = gridTypeAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Add an option for each grid type.
            foreach (string item in new[] { "LatLong", "UTM", "MGRS", "USNG" })
            {
                // Record the selection and re-apply all settings.
                gridTypeAlert.AddAction(UIAlertAction.Create(item, UIAlertActionStyle.Default, action =>
                {
                    _selectedGridType = item;
                    ApplyCurrentSettings();
                }));
            }

            // Show the alert.
            PresentViewController(gridTypeAlert, true, null);
        }

        private void _labelColorButton_TouchUpInside(object sender, EventArgs e)
        {
            // Create the view controller that will present the list of label color options.
            UIAlertController labelColorAlert = UIAlertController.Create("Select a label color", "", UIAlertControllerStyle.ActionSheet);

            // Needed to prevent a crash on iPad.
            UIPopoverPresentationController presentationPopover = labelColorAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Add an option for each color.
            foreach (Colors item in new[] { Colors.Red, Colors.Green, Colors.Blue, Colors.White })
            {
                // Record the selection and re-apply all settings.
                labelColorAlert.AddAction(UIAlertAction.Create(item.Name, UIAlertActionStyle.Default, action =>
                {
                    _selectedLabelColor = item;
                    ApplyCurrentSettings();
                }));
            }

            // Add an option to hide labels.
            labelColorAlert.AddAction(UIAlertAction.Create("Hide labels", UIAlertActionStyle.Default, action =>
            {
                // Record the selection and re-apply all settings.
                _selectedLabelColor = null;
                ApplyCurrentSettings();
            }));

            // Show the alert.
            PresentViewController(labelColorAlert, true, null);
        }

        private void _gridColorButton_TouchUpInside(object sender, EventArgs e)
        {
            // Create the view controller that will present the list of grid color options.
            UIAlertController gridColorAlert = UIAlertController.Create("Select a grid color", "", UIAlertControllerStyle.ActionSheet);

            // Needed to prevent a crash on iPad.
            UIPopoverPresentationController presentationPopover = gridColorAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Add an option for each color.
            foreach (Colors item in new[] { Colors.Red, Colors.Green, Colors.Blue, Colors.White })
            {
                // Record the selection and re-apply all settings.
                gridColorAlert.AddAction(UIAlertAction.Create(item.Name, UIAlertActionStyle.Default, action =>
                {
                    _selectedGridColor = item;
                    ApplyCurrentSettings();
                }));
            }

            // Add an option to hide the grid.
            gridColorAlert.AddAction(UIAlertAction.Create("Hide the grid", UIAlertActionStyle.Default, action =>
            {
                // Record the selection and re-apply all settings.
                _selectedGridColor = null;
                ApplyCurrentSettings();
            }));

            // Show the alert.
            PresentViewController(gridColorAlert, true, null);
        }

        private void CreateLayout()
        {
            // Set the button titles.
            _gridColorButton.SetTitle("Lines", UIControlState.Normal);
            _gridTypeButton.SetTitle("Grid type", UIControlState.Normal);
            _labelColorButton.SetTitle("Text", UIControlState.Normal);
            _labelPositionButton.SetTitle("Positions", UIControlState.Normal);

            // Set the button color.
            _gridColorButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _gridTypeButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _labelColorButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _labelPositionButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Register event handlers.
            _gridColorButton.TouchUpInside += _gridColorButton_TouchUpInside;
            _labelColorButton.TouchUpInside += _labelColorButton_TouchUpInside;
            _gridTypeButton.TouchUpInside += _gridTypeButton_TouchUpInside;
            _labelPositionButton.TouchUpInside += _labelPositionButton_TouchUpInside;

            // Add the controls to the layout.
            View.AddSubviews(_myMapView, _toolbar, _gridColorButton, _gridTypeButton, _labelColorButton, _labelPositionButton);
        }

        public override void ViewDidLayoutSubviews()
        {
            // Helper variables for layout-critical values.
            nfloat toolbarHeight = 30;
            nfloat topMargin = 60;
            nfloat buttonWidth = View.Bounds.Width / 4;
            nfloat toolbarPadding = 5;

            // Apply the MapView frame, adjusting the insets to ensure that critical map
            // elements aren't covered by the UI.
            _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);

            // Apply the toolbar frame.
            _toolbar.Frame = new CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);

            // Apply the button frames.
            int index = 0;
            foreach (UIButton button in new[] { _gridTypeButton, _gridColorButton, _labelPositionButton, _labelColorButton })
            {
                // Apply the frame.
                button.Frame = new CGRect((buttonWidth * index) + toolbarPadding, View.Bounds.Height - toolbarHeight + toolbarPadding, buttonWidth - toolbarPadding, toolbarHeight - (2 * 5));
                // Increment the index.
                index++;
            }
        }
    }
}