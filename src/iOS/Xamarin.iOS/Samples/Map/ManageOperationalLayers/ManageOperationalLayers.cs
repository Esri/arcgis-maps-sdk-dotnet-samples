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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ManageOperationalLayers
{
    [Register("ManageOperationalLayers")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Manage operational layers",
        "Map",
        "Add, remove, and reorder operational layers in a map.",
        "")]
    public class ManageOperationalLayers : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UITableViewController _tableController;
        private MapViewModel _viewModel;

        // Some URLs of layers to add to the map.
        private readonly string[] _layerUrls = new[]
        {
            "http://sampleserver5.arcgisonline.com/arcgis/rest/services/Elevation/WorldElevations/MapServer",
            "http://sampleserver5.arcgisonline.com/arcgis/rest/services/Census/MapServer",
            "http://sampleserver5.arcgisonline.com/arcgis/rest/services/DamageAssessment/MapServer"
        };

        public ManageOperationalLayers()
        {
            Title = "Manage operational layers";
        }

        private void Initialize()
        {
            _viewModel = new MapViewModel(new Map(Basemap.CreateStreets()));
            _myMapView.Map = _viewModel.Map;

            // Add the layers.
            foreach (string layerUrl in _layerUrls)
            {
                _viewModel.AddLayerFromUrl(layerUrl);
            }

            // Create the table view controller.
            _tableController = new UITableViewController(UITableViewStyle.Plain);
            _tableController.TableView.Source = _viewModel;
        }

        private void ManageLayers_Clicked(object sender, EventArgs e)
        {
            var controller = new UINavigationController(_tableController);
            controller.NavigationBar.Items[0].SetRightBarButtonItem(_tableController.EditButtonItem, false);
            controller.ModalPresentationStyle = UIModalPresentationStyle.Popover;
            controller.PreferredContentSize = new CGSize(300, 320);
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
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem("Manage layers", UIBarButtonItemStyle.Plain, ManageLayers_Clicked)
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
            var cell = new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier);
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

        public override string TitleForHeader(UITableView tableView, nint section)
        {
            return section == 0 ? "Layers in map" : "Layers not in map";
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true;
        }

        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
        {
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
            // Note: this assumes deletion, so doesn't check editingStyle.

            Layer selectedLayer;
            tableView.BeginUpdates();
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

            tableView.EndUpdates();
        }

        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return UITableViewCellEditingStyle.Delete;
        }

        public override bool CanMoveRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true;
        }

        public override void MoveRow(UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
        {
            LayerCollection source = sourceIndexPath.Section == 0 ? IncludedLayers : ExcludedLayers;
            LayerCollection destination = destinationIndexPath.Section == 0 ? IncludedLayers : ExcludedLayers;

            Layer movedLayer = source[sourceIndexPath.Row];

            source.RemoveAt(sourceIndexPath.Row);
            if (source == destination && destinationIndexPath.Row < sourceIndexPath.Row && destinationIndexPath.Row > 0)
            {
                destination.Insert(destinationIndexPath.Row - 1, movedLayer);
            }
            else
            {
                destination.Insert(destinationIndexPath.Row, movedLayer);
            }
        }
    }
}