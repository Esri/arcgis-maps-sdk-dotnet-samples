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
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.WmsServiceCatalog
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "WMS service catalog",
        category: "Layers",
        description: "Connect to a WMS service and show the available layers and sublayers. ",
        instructions: "",
        tags: new[] { "OGC", "WMS", "catalog", "web map service" })]
    public partial class WmsServiceCatalog : ContentPage
    {
        // Hold the URL to the WMS service providing the US NOAA National Weather Service forecast weather chart
        private readonly Uri _wmsUrl = new Uri(
            "https://idpgis.ncep.noaa.gov/arcgis/services/NWS_Forecasts_Guidance_Warnings/natl_fcst_wx_chart/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Hold a list of LayerDisplayVM; this is the ViewModel
        private readonly ObservableCollection<LayerDisplayVM> _viewModelList = new ObservableCollection<LayerDisplayVM>();

        public WmsServiceCatalog()
        {
            InitializeComponent();

            // Initialize the map
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Apply an imagery basemap to the map.
            MyMapView.Map = new Map(BasemapStyle.ArcGISDarkGray);

            // Create the WMS Service.
            WmsService service = new WmsService(_wmsUrl);

            try
            {
                // Load the WMS Service.
                await service.LoadAsync();

                // Get the service info (metadata) from the service.
                WmsServiceInfo info = service.ServiceInfo;

                // Get the list of layer infos.
                foreach (var layerInfo in info.LayerInfos)
                {
                    LayerDisplayVM.BuildLayerInfoList(new LayerDisplayVM(layerInfo, null), _viewModelList);
                }

                // Update the map display based on the viewModel.
                _ = UpdateMapDisplay(_viewModelList);

                // Update the list of layers.
                MyDisplayList.ItemsSource = _viewModelList;
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            }
        }

        /// <summary>
        /// Updates the map with the latest layer selection.
        /// </summary>
        private async Task UpdateMapDisplay(ObservableCollection<LayerDisplayVM> displayList)
        {
            // Remove all existing layers.
            MyMapView.Map.OperationalLayers.Clear();

            // Get a list of selected LayerInfos.
            List<WmsLayerInfo> selectedLayers = displayList.Where(vm => vm.IsEnabled).Select(vm => vm.Info).ToList();

            // Only WMS layer infos without sub layers can be used to construct a WMS layer. Group layers that have sub layers must be excluded.
            selectedLayers = selectedLayers.Where(info => info.LayerInfos.Count == 0).ToList();

            // Return if no layers selected.
            if (!selectedLayers.Any())
            {
                return;
            }

            // Create a new WmsLayer from the selected layers.
            WmsLayer myLayer = new WmsLayer(selectedLayers);

            try
            {
                // Load the layer.
                await myLayer.LoadAsync();

                // Add the layer to the map.
                MyMapView.Map.OperationalLayers.Add(myLayer);

                // Update the viewpoint.
                await MyMapView.SetViewpointAsync(new Viewpoint(myLayer.FullExtent));
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            }
        }

        /// <summary>
        /// Takes action once a new layer selection is made.
        /// </summary>
        private void MyDisplayList_SelectionChanged(object sender, SelectedItemChangedEventArgs e)
        {
            // Deselect all layers.
            foreach (LayerDisplayVM item in _viewModelList)
            {
                item.Select(false);
            }

            // Hold a reference to the selected item
            LayerDisplayVM selectedItem = (LayerDisplayVM)e.SelectedItem;

            // Update the selection
            selectedItem.Select();

            // Update the map
            _ = UpdateMapDisplay(_viewModelList);
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

        public string Name => $"{new String(' ', NestLevel * 8)} {Info.Title}";

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
}