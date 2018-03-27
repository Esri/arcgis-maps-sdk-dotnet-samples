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
using UIKit;

namespace ArcGISRuntime.Samples.WmsServiceCatalog
{
    /// <summary>
    /// Class defines how a UITableView renders its contents.
    /// This implements the list of WMS sublayers
    /// </summary>
    public class LayerListSource : UITableViewSource
    {
        // List of strings; these will be the suggestions
        public List<LayerDisplayVM> _viewModelList = new List<LayerDisplayVM>();

        // Used when re-using cells to ensure that a cell of the right type is used
        private string CellId = "TableCell";

        // Hold a reference to the owning view controller; this will be the active instance of WmsServiceCatalog 
        public WmsServiceCatalog Owner { get; set; }

        public LayerListSource(List<LayerDisplayVM> items, WmsServiceCatalog owner)
        {
            // Set the items
            if (items != null)
            {
                _viewModelList = items;
            }

            // Set the owner
            Owner = owner;
        }

        /// <summary>
        /// This method gets a table view cell for the suggestion at the specified index
        /// </summary>
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Try to get a re-usable cell (this is for performance)
            UITableViewCell cell = tableView.DequeueReusableCell(CellId);

            // If there are no cells, create a new one
            if (cell == null)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Default, CellId);
            }

            // Get the specific item to display
            LayerDisplayVM item = _viewModelList[indexPath.Row];

            // Set the text on the cell
            cell.TextLabel.Text = item.Title;

            // Return the cell
            return cell;
        }

        /// <summary>
        /// This method allows the UITableView to know how many rows to render
        /// </summary>
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _viewModelList.Count;
        }

        /// <summary>
        /// Method called when a row is selected; notifies the primary view
        /// </summary>
        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            // Deselect the row
            tableView.DeselectRow(indexPath, true);

            // Accept the suggestion
            Owner.LayerSelectionChanged(indexPath.Row);
        }
    }

    [Register("WmsServiceCatalog")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "WMS service catalog",
        "Layers",
        "This sample demonstrates how to enable and disable the display of layers discovered from a WMS service.",
        "")]
    public class WmsServiceCatalog : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Hold the URL to the WMS service providing the US NOAA National Weather Service forecast weather chart
        private Uri wmsUrl = new Uri("https://idpgis.ncep.noaa.gov/arcgis/services/NWS_Forecasts_Guidance_Warnings/natl_fcst_wx_chart/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Hold a source for the UITableView that shows the available WMS layers
        private LayerListSource _layerListSource;

        // Create the view for the layer list display
        private UITableView _myDisplayList = new UITableView();

        // Create and hold the help label
        private UILabel _myHelpLabel = new UILabel();

        public WmsServiceCatalog()
        {
            Title = "WMS service catalog";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references
            CreateLayout();

            // Initialize the map
            Initialize();
        }

        private void CreateLayout()
        {
            // Set the help label text and color
            _myHelpLabel.Text = "Select a layer from above list.";
            _myHelpLabel.TextColor = UIColor.Red;

            // Add the MapView to the view
            View.AddSubviews(_myMapView, _myDisplayList, _myHelpLabel);
        }

        public override void ViewDidLayoutSubviews()
        {
            // Variable holding the top bound (for code clarity)
            nfloat pageOffset = NavigationController.TopLayoutGuide.Length;

            // Set up the visual frame for the layer display list
            _myDisplayList.Frame = new CoreGraphics.CGRect(0, pageOffset, View.Bounds.Width, 150);

            // Set up the visual frame for the help label
            _myHelpLabel.Frame = new CoreGraphics.CGRect(10, pageOffset + 150, View.Bounds.Width - 20, 60);

            // Set up the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
        }

        private async void Initialize()
        {
            // Apply an imagery basemap to the map
            _myMapView.Map = new Map(Basemap.CreateDarkGrayCanvasVector());

            // Create the WMS Service
            WmsService service = new WmsService(wmsUrl);

            // Load the WMS Service
            await service.LoadAsync();

            // Get the service info (metadata) from the service
            WmsServiceInfo info = service.ServiceInfo;

            // Get the list of layer infos
            IReadOnlyList<WmsLayerInfo> topLevelLayers = info.LayerInfos;

            // Recursively build up a list of all the layers in the service and get their IDs as a flat list
            List<WmsLayerInfo> expandedList = new List<WmsLayerInfo>();
            BuildLayerInfoList(topLevelLayers, expandedList);

            List<LayerDisplayVM> displayList = new List<LayerDisplayVM>();

            // Build the ViewModel from the expanded list of layer infos
            foreach (WmsLayerInfo layerInfo in expandedList)
            {
                // LayerDisplayVM is a custom type made for this sample to serve as the ViewModel; it is not a part of the ArcGIS Runtime
                displayList.Add(new LayerDisplayVM(layerInfo));
            }

            // Construct the layer list source
            _layerListSource = new LayerListSource(displayList, this);

            // Set the source for the table view (layer list)
            _myDisplayList.Source = _layerListSource;

            // Force an update of the list display
            _myDisplayList.ReloadData();
        }

        /// <summary>
        /// Recursively builds a list of WmsLayerInfo metadata starting from a collection of root WmsLayerInfo
        /// </summary>
        /// <remarks>
        /// For simplicity, this sample doesn't show the layer hierarchy (tree), instead collapsing it to a list.
        /// A tree view would be more appropriate, but would complicate the sample greatly.
        /// </remarks>
        /// <param name="info">Collection of starting WmsLayerInfo object</param>
        /// <param name="result">Result list to build</param>
        private void BuildLayerInfoList(IReadOnlyList<WmsLayerInfo> info, List<WmsLayerInfo> result)
        {
            // Return if there are no more layers to explore
            if (info.Count < 1) { return; }

            // Add each layer and each layer's children
            foreach (WmsLayerInfo layer in info)
            {
                // Add the layer
                result.Add(layer);

                // Recursively add children
                BuildLayerInfoList(layer.LayerInfos, result);
            }
        }

        /// <summary>
        /// Updates the map with the latest layer selection
        /// </summary>
        private async void UpdateMapDisplay(List<LayerDisplayVM> displayList)
        {
            // Remove all existing layers
            _myMapView.Map.OperationalLayers.Clear();

            // Get a list of selected LayerInfos
            IEnumerable<WmsLayerInfo> selectedLayers = displayList.Where(vm => vm.IsEnabled).Select(vm => vm.Info);

            // Create a new WmsLayer from the selected layers
            WmsLayer myLayer = new WmsLayer(selectedLayers);

            // Wait for the layer to load
            await myLayer.LoadAsync();

            // Zoom to the extent of the layer
            await _myMapView.SetViewpointAsync(new Viewpoint(myLayer.FullExtent));

            // Add the layer to the map
            _myMapView.Map.OperationalLayers.Add(myLayer);
        }

        /// <summary>
        /// Takes action once a new layer selection is made
        /// </summary>
        public void LayerSelectionChanged(int selectedIndex)
        {
            // Clear existing selection
            foreach (LayerDisplayVM item in _layerListSource._viewModelList)
            {
                item.IsEnabled = false;
            }

            // Update the selection
            _layerListSource._viewModelList[selectedIndex].IsEnabled = true;

            // Update the map
            UpdateMapDisplay(_layerListSource._viewModelList);
        }
    }

    /// <summary>
    /// This is a ViewModel class for maintaining the state of a layer selection.
    /// Typically, this would go in a separate file, but it is included here for clarity
    /// </summary>
    public class LayerDisplayVM
    {
        /// <summary>
        /// Metadata for the individual selected layer
        /// </summary>
        public WmsLayerInfo Info { get; set; }

        /// <summary>
        /// True if the layer is selected for display
        /// </summary>
        public Boolean IsEnabled { get; set; }

        /// <summary>
        /// Title property to facilitate binding
        /// </summary>
        public String Title { get { return Info.Title; } }

        public LayerDisplayVM(WmsLayerInfo info)
        {
            Info = info;
        }
    }
}