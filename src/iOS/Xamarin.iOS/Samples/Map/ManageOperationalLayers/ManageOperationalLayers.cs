// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using CoreGraphics;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ManageOperationalLayers
{
    [Register("ManageOperationalLayers")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Manage operational layers",
        category: "Map",
        description: "Add, remove, and reorder operational layers in a map.",
        instructions: "When the app starts, a list displays the operational layers that are currently displayed in the map. Right-tap on the list item to remove the layer, or left-tap to move it to the top. The map will be updated automatically.",
        tags: new[] { "add", "delete", "layer", "map", "remove" })]
    public class ManageOperationalLayers : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UITableViewController _tableController;
        private MapViewModel _viewModel;
        private UIBarButtonItem _manageLayersButton;

        // Some URLs of layers to add to the map.
        private readonly string[] _layerUrls = new[]
        {
            "https://sampleserver5.arcgisonline.com/arcgis/rest/services/Elevation/WorldElevations/MapServer",
            "https://sampleserver5.arcgisonline.com/arcgis/rest/services/Census/MapServer",
            "https://sampleserver5.arcgisonline.com/arcgis/rest/services/DamageAssessment/MapServer"
        };

        public ManageOperationalLayers()
        {
            Title = "Manage operational layers";
        }

        private void Initialize()
        {
            _viewModel = new MapViewModel(new Map(BasemapStyle.ArcGISStreets));
            _myMapView.Map = _viewModel.Map;

            // Add the layers.
            foreach (string layerUrl in _layerUrls)
            {
                _viewModel.AddLayerFromUrl(layerUrl);
            }

            // Create the table view controller.
            _tableController = new UITableViewController(UITableViewStyle.Plain);

            // The table view content is managed by the view model.
            _tableController.TableView.Source = _viewModel;
        }

        private void ManageLayers_Clicked(object sender, EventArgs e)
        {
            // Show the layer list popover. Note: most behavior is managed by the table view & its source. See MapViewModel.
            var controller = new UINavigationController(_tableController);
            // Show an edit button in the top left of the popover.
            controller.NavigationBar.Items[0].SetLeftBarButtonItem(_tableController.EditButtonItem, false);
            // Show a close button in the top right.
            var closeButton = new UIBarButtonItem("Close", UIBarButtonItemStyle.Plain, (o, ea) => controller.DismissViewController(true, null));
            controller.NavigationBar.Items[0].SetRightBarButtonItem(closeButton, false);
            // Show the table view in a popover.
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

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = ApplicationTheme.BackgroundColor};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _manageLayersButton = new UIBarButtonItem();
            _manageLayersButton.Title = "Manage layers";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _manageLayersButton
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _manageLayersButton.Clicked += ManageLayers_Clicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _manageLayersButton.Clicked -= ManageLayers_Clicked;
        }
    }

    class MapViewModel : UITableViewSource
    {
        public Map Map { get; }
        public LayerCollection IncludedLayers => Map.OperationalLayers;
        public LayerCollection ExcludedLayers { get; } = new LayerCollection();
        private const string CellIdentifier = "LayerTableCell";

        public MapViewModel(Map map)
        {
            Map = map;
        }

        public void AddLayerFromUrl(string layerUrl)
        {
            ArcGISMapImageLayer layer = new ArcGISMapImageLayer(new Uri(layerUrl));
            Map.OperationalLayers.Add(layer);
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Gets a cell for the specified section and row within that section.
            var cell = new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier);
            // The first section is Map.OperationalLayers, the other section is excluded layers.
            switch (indexPath.Section)
            {
                case 0:
                    cell.TextLabel.Text = IncludedLayers[indexPath.Row].Name;
                    break;
                case 1:
                    cell.TextLabel.Text = ExcludedLayers[indexPath.Row].Name;
                    break;
            }

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            // Get the number of layers in each section - sections are how the two lists are represented.
            switch (section)
            {
                case 0:
                    return IncludedLayers.Count;
                case 1:
                    return ExcludedLayers.Count;
                default:
                    return 0;
            }
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            // Two sections - layers in the map and layers not in the map.
            return 2;
        }

        public override string TitleForHeader(UITableView tableView, nint section)
        {
            // The first section is for included layers, the second section is excluded layers.
            return section == 0 ? "Layers in map" : "Layers not in map";
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            // All rows are editable - they can be reordered or moved to another list via insertion/deletion.
            return true;
        }

        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
        {
            // The message shown when you drag to the side when editing.
            switch (indexPath.Section)
            {
                case 0:
                    return "Remove from map";
                case 1:
                    return "Add to map";
            }

            return "This shouldn't happen.";
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            // Commit editing - in this case, editing means insertion or deletion.

            Layer selectedLayer;
            switch (indexPath.Section)
            {
                case 0:
                    selectedLayer = IncludedLayers[indexPath.Row];
                    IncludedLayers.Remove(selectedLayer);
                    ExcludedLayers.Add(selectedLayer);
                    break;
                case 1:
                    selectedLayer = ExcludedLayers[indexPath.Row];
                    ExcludedLayers.Remove(selectedLayer);
                    IncludedLayers.Add(selectedLayer);
                    break;
            }

            // Force the view to reload its data.
            tableView.ReloadData();
        }

        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            // Layers in the first section can be removed from the map (looks like deletion).
            // Layers in the second section can be added to the map (looks like insertion).
            switch (indexPath.Section)
            {
                case 0:
                    return UITableViewCellEditingStyle.Delete;
                default:
                    return UITableViewCellEditingStyle.Insert;
            }
        }

        public override bool CanMoveRow(UITableView tableView, NSIndexPath indexPath)
        {
            // All rows can be moved.
            return true;
        }

        public override void MoveRow(UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
        {
            // Find the source and destination lists (based on the section of the source and destination index).
            LayerCollection source = sourceIndexPath.Section == 0 ? IncludedLayers : ExcludedLayers;
            LayerCollection destination = destinationIndexPath.Section == 0 ? IncludedLayers : ExcludedLayers;

            // Find the layer that is being moved.
            Layer movedLayer = source[sourceIndexPath.Row];

            // Remove the layer from the source list and insert into the destination list.
            source.RemoveAt(sourceIndexPath.Row);
            destination.Insert(destinationIndexPath.Row, movedLayer);

            // Reload the table now that the data has changed.
            tableView.ReloadData();
        }
    }
}