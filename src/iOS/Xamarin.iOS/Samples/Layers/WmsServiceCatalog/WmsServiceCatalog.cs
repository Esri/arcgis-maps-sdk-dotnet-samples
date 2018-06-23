// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using UIKit;

namespace ArcGISRuntime.Samples.WmsServiceCatalog
{
    [Register("WmsServiceCatalog")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "WMS service catalog",
        "Layers",
        "This sample demonstrates how to enable and disable the display of layers discovered from a WMS service.",
        "")]
    public class WmsServiceCatalog : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();

        private readonly UIToolbar _toolbar = new UIToolbar();

        private readonly UILabel _myHelpLabel = new UILabel
        {
            Text = "Select a layer from the list above.",
            TextAlignment = UITextAlignment.Center,
            AdjustsFontSizeToFitWidth = true
        };

        private readonly UITableView _myDisplayList = new UITableView
        {
            RowHeight = 30,
            BackgroundColor = UIColor.FromWhiteAlpha(0, 0)
        };

        // Hold the URL to the WMS service providing the US NOAA National Weather Service forecast weather chart.
        private readonly Uri _wmsUrl = new Uri("https://idpgis.ncep.noaa.gov/arcgis/services/NWS_Forecasts_Guidance_Warnings/natl_fcst_wx_chart/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Hold a source for the UITableView that shows the available WMS layers.
        private LayerListSource _layerListSource;

        public WmsServiceCatalog()
        {
            Title = "WMS service catalog";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        private void CreateLayout()
        {
            // Add the controls to the view.
            View.AddSubviews(_myMapView, _toolbar, _myDisplayList, _myHelpLabel);
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat listHeight = 150;
                nfloat controlHeight = 30;
                nfloat margin = 5;
                nfloat toolbarHeight = listHeight + controlHeight + margin * 2;

                // Reposition the views
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myDisplayList.Frame = new CGRect(0, topMargin, View.Bounds.Width, listHeight);
                _myHelpLabel.Frame = new CGRect(margin, _myDisplayList.Frame.Bottom + margin, View.Bounds.Width - (2 * margin), controlHeight);
                _toolbar.Frame = new CGRect(0, topMargin, View.Bounds.Width, toolbarHeight);
                _myMapView.ViewInsets = new UIEdgeInsets(_toolbar.Frame.Bottom, 0, 0, 0);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private async void Initialize()
        {
            // Show dark gray canvas basemap.
            _myMapView.Map = new Map(Basemap.CreateDarkGrayCanvasVector());

            // Create the WMS Service.
            WmsService service = new WmsService(_wmsUrl);

            // Load the WMS Service.
            await service.LoadAsync();

            // Get the service info (metadata) from the service.
            WmsServiceInfo info = service.ServiceInfo;

            // Get the list of layer infos.
            IReadOnlyList<WmsLayerInfo> topLevelLayers = info.LayerInfos;

            // Recursively build up a list of all the layers in the service and get their IDs as a flat list.
            List<WmsLayerInfo> expandedList = new List<WmsLayerInfo>();
            BuildLayerInfoList(topLevelLayers, expandedList);

            List<LayerDisplayVm> displayList = new List<LayerDisplayVm>();

            // Build the ViewModel from the expanded list of layer infos.
            foreach (WmsLayerInfo layerInfo in expandedList)
            {
                // LayerDisplayVM is a custom type made for this sample to serve as the ViewModel; it is not a part of the ArcGIS Runtime.
                displayList.Add(new LayerDisplayVm(layerInfo));
            }

            // Construct the layer list source.
            _layerListSource = new LayerListSource(displayList, this);

            // Set the source for the table view (layer list).
            _myDisplayList.Source = _layerListSource;

            // Force an update of the list display.
            _myDisplayList.ReloadData();
        }

        /// <summary>
        /// Recursively builds a list of WmsLayerInfo metadata starting from a collection of root WmsLayerInfo.
        /// </summary>
        /// <remarks>
        /// For simplicity, this sample doesn't show the layer hierarchy (tree), instead collapsing it to a list.
        /// A tree view would be more appropriate, but would complicate the sample greatly.
        /// </remarks>
        /// <param name="info">Collection of starting WmsLayerInfo object.</param>.
        /// <param name="result">Result list to build.</param>
        private void BuildLayerInfoList(IReadOnlyList<WmsLayerInfo> info, List<WmsLayerInfo> result)
        {
            // Return if there are no more layers to explore.
            if (info.Count < 1)
            {
                return;
            }

            // Add each layer and each layer's children.
            foreach (WmsLayerInfo layer in info)
            {
                // Add the layer.
                result.Add(layer);

                // Recursively add children.
                BuildLayerInfoList(layer.LayerInfos, result);
            }
        }

        /// <summary>
        /// Updates the map with the latest layer selection.
        /// </summary>
        private async void UpdateMapDisplay(List<LayerDisplayVm> displayList)
        {
            // Remove all existing layers.
            _myMapView.Map.OperationalLayers.Clear();

            // Get a list of selected LayerInfos.
            IEnumerable<WmsLayerInfo> selectedLayers = displayList.Where(vm => vm.IsEnabled).Select(vm => vm.Info);

            // Create a new WmsLayer from the selected layers.
            WmsLayer myLayer = new WmsLayer(selectedLayers);

            // Wait for the layer to load.
            await myLayer.LoadAsync();

            // Zoom to the extent of the layer.
            await _myMapView.SetViewpointAsync(new Viewpoint(myLayer.FullExtent));

            // Add the layer to the map.
            _myMapView.Map.OperationalLayers.Add(myLayer);
        }

        /// <summary>
        /// Takes action once a new layer selection is made.
        /// </summary>
        public void LayerSelectionChanged(int selectedIndex)
        {
            // Clear existing selection.
            foreach (LayerDisplayVm item in _layerListSource.ViewModelList)
            {
                item.IsEnabled = false;
            }

            // Update the selection.
            _layerListSource.ViewModelList[selectedIndex].IsEnabled = true;

            // Update the map.
            UpdateMapDisplay(_layerListSource.ViewModelList);
        }
    }

    /// <summary>
    /// This is a ViewModel class for maintaining the state of a layer selection.
    /// Typically, this would go in a separate file, but it is included here for clarity.
    /// </summary>
    public class LayerDisplayVm
    {
        /// <summary>
        /// Metadata for the individual selected layer.
        /// </summary>
        public WmsLayerInfo Info { get; }

        /// <summary>
        /// True if the layer is selected for display.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Title property to facilitate binding.
        /// </summary>
        public string Title => Info.Title;

        public LayerDisplayVm(WmsLayerInfo info)
        {
            Info = info;
        }
    }

    /// <summary>
    /// Class defines how a UITableView renders its contents.
    /// This implements the list of WMS sublayers.
    /// </summary>
    public class LayerListSource : UITableViewSource
    {
        public readonly List<LayerDisplayVm> ViewModelList = new List<LayerDisplayVm>();

        // Used when re-using cells to ensure that a cell of the right type is used
        private const string CellId = "TableCell";

        // Hold a reference to the owning view controller; this will be the active instance of WmsServiceCatalog.
        private WmsServiceCatalog Owner { get; }

        public LayerListSource(List<LayerDisplayVm> items, WmsServiceCatalog owner)
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
            LayerDisplayVm item = ViewModelList[indexPath.Row];

            // Set the text on the cell.
            cell.TextLabel.Text = item.Title;

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