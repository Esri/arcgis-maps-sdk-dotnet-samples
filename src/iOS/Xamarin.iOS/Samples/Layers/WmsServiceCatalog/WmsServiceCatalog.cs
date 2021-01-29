// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.WmsServiceCatalog
{
    [Register("WmsServiceCatalog")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "WMS service catalog",
        category: "Layers",
        description: "Connect to a WMS service and show the available layers and sublayers. ",
        instructions: "",
        tags: new[] { "OGC", "WMS", "catalog", "web map service" })]
    public class WmsServiceCatalog : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UITableView _layerList;
        private NSLayoutConstraint[] _portraitConstraints;
        private NSLayoutConstraint[] _landscapeConstraints;

        // Hold the URL to the WMS service providing the US NOAA National Weather Service forecast weather chart.
        private readonly Uri _wmsUrl = new Uri("https://idpgis.ncep.noaa.gov/arcgis/services/NWS_Forecasts_Guidance_Warnings/natl_fcst_wx_chart/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Hold a source for the UITableView that shows the available WMS layers.
        private LayerListSource _layerListSource;

        public WmsServiceCatalog()
        {
            Title = "WMS service catalog";
        }

        private async void Initialize()
        {
            // Show dark gray canvas basemap.
            _myMapView.Map = new Map(BasemapStyle.ArcGISDarkGray);

            // Create the WMS Service.
            WmsService service = new WmsService(_wmsUrl);

            try
            {
                // Load the WMS Service.
                await service.LoadAsync();

                // Get the service info (metadata) from the service.
                WmsServiceInfo info = service.ServiceInfo;

                List<LayerDisplayVM> viewModelList = new List<LayerDisplayVM>();

                // Get the list of layer infos.
                foreach (var layerInfo in info.LayerInfos)
                {
                    LayerDisplayVM.BuildLayerInfoList(new LayerDisplayVM(layerInfo, null), viewModelList);
                }

                // Construct the layer list source.
                _layerListSource = new LayerListSource(viewModelList, this);

                // Set the source for the table view (layer list).
                _layerList.Source = _layerListSource;

                // Force an update of the list display.
                _layerList.ReloadData();
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        /// <summary>
        /// Updates the map with the latest layer selection.
        /// </summary>
        private async void UpdateMapDisplay(List<LayerDisplayVM> displayList)
        {
            // Remove all existing layers.
            _myMapView.Map.OperationalLayers.Clear();

            // Get a list of selected LayerInfos.
            IEnumerable<WmsLayerInfo> selectedLayers = displayList.Where(vm => vm.IsEnabled).Select(vm => vm.Info);

            // Create a new WmsLayer from the selected layers.
            WmsLayer myLayer = new WmsLayer(selectedLayers);

            try
            {
                // Wait for the layer to load.
                await myLayer.LoadAsync();

                // Zoom to the extent of the layer.
                await _myMapView.SetViewpointAsync(new Viewpoint(myLayer.FullExtent));

                // Add the layer to the map.
                _myMapView.Map.OperationalLayers.Add(myLayer);
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        /// <summary>
        /// Takes action once a new layer selection is made.
        /// </summary>
        public void LayerSelectionChanged(int selectedIndex)
        {
            // Clear existing selection.
            foreach (LayerDisplayVM item in _layerListSource.ViewModelList)
            {
                item.Select(false);
            }

            // Update the selection.
            _layerListSource.ViewModelList[selectedIndex].Select();

            // Update the map.
            UpdateMapDisplay(_layerListSource.ViewModelList);
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

            _layerList = new UITableView();
            _layerList.TranslatesAutoresizingMaskIntoConstraints = false;
            _layerList.RowHeight = 40;

            UILabel helpLabel = new UILabel
            {
                Text = "Select layers for display.",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, _layerList, helpLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                helpLabel.LeadingAnchor.ConstraintEqualTo(_myMapView.LeadingAnchor),
                helpLabel.TrailingAnchor.ConstraintEqualTo(_myMapView.TrailingAnchor),
                helpLabel.TopAnchor.ConstraintEqualTo(_myMapView.TopAnchor),
                helpLabel.HeightAnchor.ConstraintEqualTo(40),
            });

            _portraitConstraints = new[]
            {
                _layerList.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _layerList.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _layerList.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _layerList.HeightAnchor.ConstraintEqualTo(_layerList.RowHeight * 4),
                _myMapView.TopAnchor.ConstraintEqualTo(_layerList.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            };

            _landscapeConstraints = new[]
            {
                _layerList.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _layerList.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _layerList.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _layerList.TrailingAnchor.ConstraintEqualTo(View.CenterXAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(_layerList.TrailingAnchor),
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            };

            SetLayoutOrientation();
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            // Reset constraints.
            NSLayoutConstraint.DeactivateConstraints(_portraitConstraints);
            NSLayoutConstraint.DeactivateConstraints(_landscapeConstraints);
            SetLayoutOrientation();
        }

        private void SetLayoutOrientation()
        {
            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                NSLayoutConstraint.ActivateConstraints(_landscapeConstraints);
            }
            else
            {
                NSLayoutConstraint.ActivateConstraints(_portraitConstraints);
            }
        }
    }

    /// <summary>
    /// This is a ViewModel class for maintaining the state of a layer selection.
    /// Typically, this would go in a separate file, but it is included here for clarity.
    /// </summary>
    public class LayerDisplayVM
    {
        public WmsLayerInfo Info { get; }

        // True if layer is selected for display.
        public bool IsEnabled { get; private set; }

        // Keeps track of how much indentation should be added (to simulate a tree view in a list).
        private int NestLevel
        {
            get
            {
                if (Parent == null)
                {
                    return 0;
                }

                return Parent.NestLevel + 1;
            }
        }

        private List<LayerDisplayVM> Children { get; set; }

        private LayerDisplayVM Parent { get; }

        public LayerDisplayVM(WmsLayerInfo info, LayerDisplayVM parent)
        {
            Info = info;
            Parent = parent;
        }

        // Select this layer and all child layers.
        public void Select(bool isSelected = true)
        {
            IsEnabled = isSelected;
            if (Children == null)
            {
                return;
            }

            foreach (var child in Children)
            {
                child.Select(isSelected);
            }
        }

        // Name with formatting to simulate treeview.
        public string Name => $"{new string(' ', NestLevel * 8)} {Info.Title}";

        public static void BuildLayerInfoList(LayerDisplayVM root, IList<LayerDisplayVM> result)
        {
            // Add the root node to the result list.
            result.Add(root);

            // Initialize the child collection for the root.
            root.Children = new List<LayerDisplayVM>();

            // Recursively add sublayers.
            foreach (WmsLayerInfo layer in root.Info.LayerInfos)
            {
                // Create the view model for the sublayer.
                LayerDisplayVM layerVM = new LayerDisplayVM(layer, root);

                // Add the sublayer to the root's sublayer collection.
                root.Children.Add(layerVM);

                // Recursively add children.
                BuildLayerInfoList(layerVM, result);
            }
        }
    }

    /// <summary>
    /// Class defines how a UITableView renders its contents.
    /// This implements the list of WMS sublayers.
    /// </summary>
    public class LayerListSource : UITableViewSource
    {
        public readonly List<LayerDisplayVM> ViewModelList = new List<LayerDisplayVM>();

        // Used when re-using cells to ensure that a cell of the right type is used
        private const string CellId = "TableCell";

        // Hold a reference to the owning view controller; this will be the active instance of WmsServiceCatalog.
        [Weak] public WmsServiceCatalog Owner;

        public LayerListSource(List<LayerDisplayVM> items, WmsServiceCatalog owner)
        {
            // Set the items.
            if (items != null)
            {
                ViewModelList = items;
            }

            // Set the owner.
            Owner = owner;
        }

        /// <summary>
        /// This method gets a table view cell for the suggestion at the specified index.
        /// </summary>
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Try to get a re-usable cell (this is for performance).
            UITableViewCell cell = tableView.DequeueReusableCell(CellId);

            // If there are no cells, create a new one.
            if (cell == null)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Default, CellId)
                {
                    BackgroundColor = UIColor.FromWhiteAlpha(0, 0f)
                };
                cell.TextLabel.TextColor = Owner.View.TintColor;
            }

            // Get the specific item to display.
            LayerDisplayVM item = ViewModelList[indexPath.Row];

            // Set the text on the cell.
            cell.TextLabel.Text = item.Name;

            // Return the cell.
            return cell;
        }

        /// <summary>
        /// This method allows the UITableView to know how many rows to render.
        /// </summary>
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return ViewModelList.Count;
        }

        /// <summary>
        /// Method called when a row is selected; notifies the primary view.
        /// </summary>
        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            // Deselect the row.
            tableView.DeselectRow(indexPath, true);

            // Select the layer.
            Owner.LayerSelectionChanged(indexPath.Row);
        }
    }
}