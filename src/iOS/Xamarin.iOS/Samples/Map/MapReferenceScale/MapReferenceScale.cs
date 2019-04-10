// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using CoreGraphics;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.MapReferenceScale
{
    [Register("MapReferenceScale")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Map reference scale",
        "Map",
        "Set a map's reference scale and control which feature layers honor that scale.",
        "")]
    public class MapReferenceScale : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UILabel _scaleLabel;
        private UITableViewController _layerTableController;
        private UIBarButtonItem _layerSelectionButton;

        // List of reference scale options.
        private readonly double[] _referenceScales =
        {
            50000,
            100000,
            250000,
            500000
        };

        public MapReferenceScale()
        {
            Title = "Map reference scale";
        }

        private async void Initialize()
        {
            // Create a portal and an item; the map will be loaded from portal item.
            ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri("https://runtime.maps.arcgis.com"));
            PortalItem mapItem = await PortalItem.CreateAsync(portal, "3953413f3bd34e53a42bf70f2937a408");

            // Create the map from the item.
            Map webMap = new Map(mapItem);

            // Update the UI when the map navigates.
            _myMapView.ViewpointChanged += (o, e) => _scaleLabel.Text = $"Current map scale: 1:{_myMapView.MapScale:n0}";

            // Display the map.
            _myMapView.Map = webMap;

            // Wait for the map to load.
            await webMap.LoadAsync();

            // Configure the tableview controller to enable managing layers.
            _layerTableController = new UITableViewController();
            _layerTableController.TableView.Source = new LayerViewModel(webMap);

            // Enable the button now that the map is ready.
            _layerSelectionButton.Enabled = true;
        }

        private void ShowLayerOptions_Click(object sender, EventArgs e)
        {
            // Show the layer list popover. Note: most behavior is managed by the table view & its source. See LayerViewModel.
            var controller = new UINavigationController(_layerTableController);
            var closeButton = new UIBarButtonItem("Close", UIBarButtonItemStyle.Plain, (o, ea) => controller.DismissViewController(true, null));
            controller.NavigationBar.Items[0].SetRightBarButtonItem(closeButton, false);
            controller.ModalPresentationStyle = UIModalPresentationStyle.Popover;
            controller.PreferredContentSize = new CGSize(300, 250);
            UIPopoverPresentationController pc = controller.PopoverPresentationController;
            if (pc != null)
            {
                pc.BarButtonItem = (UIBarButtonItem) sender;
                pc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                pc.Delegate = new ppDelegate();
            }

            PresentViewController(controller, true, null);
        }

        private void ShowScaleOptions_Click(object sender, EventArgs e)
        {
            // Create the view controller that will present the list of scales.
            UIAlertController basemapSelectionAlert = UIAlertController.Create("Select a reference scale", "", UIAlertControllerStyle.ActionSheet);

            // Add an option for each basemap.
            foreach (double scale in _referenceScales)
            {
                // Selecting a scale will call the lambda method, which will apply the chosen reference scale.
                basemapSelectionAlert.AddAction(UIAlertAction.Create($"1:{scale:n0}", UIAlertActionStyle.Default, action => _myMapView.Map.ReferenceScale = scale));
            }

            // Fix to prevent crash on iPad.
            var popoverPresentationController = basemapSelectionAlert.PopoverPresentationController;
            if (popoverPresentationController != null)
            {
                popoverPresentationController.BarButtonItem = (UIBarButtonItem) sender;
            }

            // Show the alert.
            PresentViewController(basemapSelectionAlert, true, null);
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            _layerSelectionButton = new UIBarButtonItem("Configure layers", UIBarButtonItemStyle.Plain, ShowLayerOptions_Click) {Enabled = false};
            toolbar.Items = new[]
            {
                new UIBarButtonItem("Set reference scale", UIBarButtonItemStyle.Plain, ShowScaleOptions_Click),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _layerSelectionButton
            };

            _scaleLabel = new UILabel
            {
                Text = "Current map scale:",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar, _scaleLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _scaleLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _scaleLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _scaleLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _scaleLabel.HeightAnchor.ConstraintEqualTo(40)
            });
        }

        // Force popover to display on iPhone.
        private class ppDelegate : UIPopoverPresentationControllerDelegate
        {
            public override UIModalPresentationStyle GetAdaptivePresentationStyle(
                UIPresentationController forPresentationController) => UIModalPresentationStyle.None;

            public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller,
                UITraitCollection traitCollection) => UIModalPresentationStyle.None;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }

    class LayerViewModel : UITableViewSource
    {
        private Map _map;
        private const string CellIdentifier = "LayerTableCell";

        public LayerViewModel(Map map)
        {
            _map = map;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Gets a cell for the specified section and row.
            var cell = new UITableViewCell(UITableViewCellStyle.Subtitle, CellIdentifier);
            Layer selectedLayer = _map.OperationalLayers[indexPath.Row];

            // Show the layer name.
            cell.TextLabel.Text = selectedLayer.Name;

            // Show a toggle if this is a feature layer whose visibility can be toggled.
            FeatureLayer fLayer = selectedLayer as FeatureLayer;
            if (fLayer != null)
            {
                // Get the layer as a feature layer.

                // Create a switch.
                UISwitch scaleSwitch = new UISwitch();
                scaleSwitch.On = fLayer.ScaleSymbols;

                // Update the feature layer scaleSymbols setting when the switch is toggled.
                scaleSwitch.ValueChanged += (o, e) => { fLayer.ScaleSymbols = scaleSwitch.On; };

                // Add the switch to the cell.
                cell.AccessoryView = scaleSwitch;
            }

            return cell;
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return false;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _map.OperationalLayers.Count;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return 1;
        }
    }
}