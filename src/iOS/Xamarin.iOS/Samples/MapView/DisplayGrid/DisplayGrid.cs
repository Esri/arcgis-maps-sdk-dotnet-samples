// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
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
        name: "Display grid",
        category: "MapView",
        description: "Display coordinate system grids including Latitude/Longitude, MGRS, UTM and USNG on a map view. Also, toggle label visibility and change the color of grid lines and grid labels.",
        instructions: "Select type of grid from the types (LatLong, MGRS, UTM and USNG) and modify its properties like label visibility, grid line color, and grid label color. Press the button to apply these settings.",
        tags: new[] { "MGRS", "USNG", "UTM", "coordinates", "degrees", "graticule", "grid", "latitude", "longitude", "minutes", "seconds" })]
    public class DisplayGrid : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _typeButton;
        private UIBarButtonItem _lineColorButton;
        private UIBarButtonItem _positionButton;
        private UIBarButtonItem _labelColorButton;

        // Fields for storing the user's grid preferences.
        private string _selectedGridType = "LatLong";
        private Colors? _selectedGridColor = Colors.Red;
        private Colors? _selectedLabelColor = Colors.White;
        private GridLabelPosition _selectedLabelPosition = GridLabelPosition.Geographic;

        public DisplayGrid()
        {
            Title = "Display a grid";
        }

        private void Initialize()
        {
            // Set a basemap.
            _myMapView.Map = new Map(Basemap.CreateImageryWithLabelsVector());

            // Apply a grid by default.
            ApplyCurrentSettings();

            // Zoom to a default scale that will show the grid labels if they are enabled.
            _myMapView.SetViewpointCenterAsync(
                new MapPoint(-7702852.905619, 6217972.345771, SpatialReferences.WebMercator), 23227);
        }

        private void ApplyCurrentSettings()
        {
            Grid grid;

            // First, update the grid based on the type selected.
            switch (_selectedGridType)
            {
                case "LatLong":
                    grid = new LatitudeLongitudeGrid();
                    break;

                case "MGRS":
                    grid = new MgrsGrid();
                    break;

                case "UTM":
                    grid = new UtmGrid();
                    break;

                case "USNG":
                default:
                    grid = new UsngGrid();
                    break;
            }

            // Next, apply the label position setting.
            grid.LabelPosition = _selectedLabelPosition;

            // Next, apply the grid color and label color settings for each zoom level.
            for (long level = 0; level < grid.LevelCount; level++)
            {
                // Set the grid color if the grid is selected for display.
                if (_selectedGridColor != null)
                {
                    // Set the line symbol.
                    Symbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, _selectedGridColor.Value, 2);
                    grid.SetLineSymbol(level, lineSymbol);
                }

                // Set the label color if labels are enabled for display.
                if (_selectedLabelColor != null)
                {
                    // Set the text symbol.
                    Symbol textSymbol = new TextSymbol
                    {
                        Color = _selectedLabelColor.Value,
                        Size = 16,
                        FontWeight = FontWeight.Bold
                    };
                    grid.SetTextSymbol(level, textSymbol);
                }
            }

            // Next, hide the grid if it has been hidden.
            if (_selectedGridColor == null)
            {
                grid.IsVisible = false;
            }

            // Next, hide the labels if they have been hidden.
            if (_selectedLabelColor == null)
            {
                grid.IsLabelVisible = false;
            }

            // Apply the updated grid.
            _myMapView.Grid = grid;
        }

        private void LabelPositionButton_Click(object sender, EventArgs e)
        {
            // Create the view controller that will present the list of label positions.
            UIAlertController labelPositionAlert = UIAlertController.Create(null, "Select a label position", UIAlertControllerStyle.ActionSheet);

            // Needed to prevent a crash on iPad.
            UIPopoverPresentationController presentationPopover = labelPositionAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.BarButtonItem = (UIBarButtonItem)sender;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Down;
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

        private void GridTypeButton_Click(object sender, EventArgs e)
        {
            // Create the view controller that will present the list of grid types.
            UIAlertController gridTypeAlert = UIAlertController.Create(null, "Select a grid type", UIAlertControllerStyle.ActionSheet);

            // Needed to prevent a crash on iPad.
            UIPopoverPresentationController presentationPopover = gridTypeAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.BarButtonItem = (UIBarButtonItem)sender;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Down;
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

        private void LabelColorButton_Click(object sender, EventArgs e)
        {
            // Create the view controller that will present the list of label color options.
            UIAlertController labelColorAlert = UIAlertController.Create(null, "Select a label color", UIAlertControllerStyle.ActionSheet);

            // Needed to prevent a crash on iPad.
            UIPopoverPresentationController presentationPopover = labelColorAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.BarButtonItem = (UIBarButtonItem)sender;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Down;
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

        private void GridColorButton_Click(object sender, EventArgs e)
        {
            // Create the view controller that will present the list of grid color options.
            UIAlertController gridColorAlert = UIAlertController.Create(null, "Select a grid color", UIAlertControllerStyle.ActionSheet);

            // Needed to prevent a crash on iPad.
            UIPopoverPresentationController presentationPopover = gridColorAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.BarButtonItem = (UIBarButtonItem)sender;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Down;
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _typeButton = new UIBarButtonItem();
            _typeButton.Title = "Grid type";

            _lineColorButton = new UIBarButtonItem();
            _lineColorButton.Title = "Line color";

            _positionButton = new UIBarButtonItem();
            _positionButton.Title = "Positions";

            _labelColorButton = new UIBarButtonItem();
            _labelColorButton.Title = "Text color";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                _typeButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _lineColorButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _positionButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _labelColorButton
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _typeButton.Clicked += GridTypeButton_Click;
            _lineColorButton.Clicked += GridColorButton_Click;
            _positionButton.Clicked += LabelPositionButton_Click;
            _labelColorButton.Clicked += LabelColorButton_Click;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _typeButton.Clicked -= GridTypeButton_Click;
            _lineColorButton.Clicked -= GridColorButton_Click;
            _positionButton.Clicked -= LabelPositionButton_Click;
            _labelColorButton.Clicked -= LabelColorButton_Click;
        }
    }
}