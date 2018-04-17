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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.WmsServiceCatalog
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "WMS service catalog",
        "Layers",
        "This sample demonstrates how to enable and disable the display of layers discovered from a WMS service.",
        "")]
    public partial class WmsServiceCatalog : ContentPage
    {
        // Hold the URL to the WMS service providing the US NOAA National Weather Service forecast weather chart
        private Uri wmsUrl = new Uri("https://idpgis.ncep.noaa.gov/arcgis/services/NWS_Forecasts_Guidance_Warnings/natl_fcst_wx_chart/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Hold a list of LayerDisplayVM; this is the ViewModel
        private ObservableCollection<LayerDisplayVM> _viewModelList = new ObservableCollection<LayerDisplayVM>();

        public WmsServiceCatalog()
        {
            InitializeComponent();

            Title = "WMS service catalog";

            // Initialize the map
            Initialize();
        }

        private async void Initialize()
        {
            // Apply an imagery basemap to the map
            MyMapView.Map = new Map(Basemap.CreateDarkGrayCanvasVector());

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

            // Build the ViewModel from the expanded list of layer infos
            foreach (WmsLayerInfo layerInfo in expandedList)
            {
                // LayerDisplayVM is a custom type made for this sample to serve as the ViewModel; it is not a part of the ArcGIS Runtime
                _viewModelList.Add(new LayerDisplayVM(layerInfo));
            }

            // Update the map display based on the viewModel
            UpdateMapDisplay(_viewModelList);

            // Update the list of layers
            MyDisplayList.ItemsSource = _viewModelList;
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
        private async void UpdateMapDisplay(ObservableCollection<LayerDisplayVM> displayList)
        {
            // Remove all existing layers
            MyMapView.Map.OperationalLayers.Clear();

            // Get a list of selected LayerInfos
            IEnumerable<WmsLayerInfo> selectedLayers = displayList.Where(vm => vm.IsEnabled).Select(vm => vm.Info);

            // Return if no layers selected
            if (selectedLayers.Count() < 1) { return; }

            // Create a new WmsLayer from the selected layers
            WmsLayer myLayer = new WmsLayer(selectedLayers);

            // Load the layer
            await myLayer.LoadAsync();

            // Add the layer to the map
            MyMapView.Map.OperationalLayers.Add(myLayer);

            // Update the viewpoint
            await MyMapView.SetViewpointAsync(new Viewpoint(myLayer.FullExtent));
        }

        /// <summary>
        /// Takes action once a new layer selection is made
        /// </summary>
        private void MyDisplayList_SelectionChanged(object sender, SelectedItemChangedEventArgs e)
        {
            // Clear existing selection
            foreach (LayerDisplayVM item in _viewModelList)
            {
                item.IsEnabled = false;
            }

            // Hold a reference to the selected item
            LayerDisplayVM selectedItem = e.SelectedItem as LayerDisplayVM;

            // Update the selection
            selectedItem.IsEnabled = true;

            // Update the map
            UpdateMapDisplay(_viewModelList);
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