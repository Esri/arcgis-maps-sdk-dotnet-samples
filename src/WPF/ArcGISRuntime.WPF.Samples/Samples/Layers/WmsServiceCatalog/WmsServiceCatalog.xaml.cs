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
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace ArcGISRuntime.WPF.Samples.WmsServiceCatalog
{
    public partial class WmsServiceCatalog
    {
        // Hold the URL to the WMS service providing the US NOAA National Weather Service forecast weather chart
        private Uri wmsUrl = new Uri("https://idpgis.ncep.noaa.gov/arcgis/services/NWS_Forecasts_Guidance_Warnings/natl_fcst_wx_chart/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Hold a list of DisplayObjects; this is the ViewModel
        public ObservableCollection<DisplayObject> _viewModel = new ObservableCollection<DisplayObject>();

        public WmsServiceCatalog()
        {
            InitializeComponent();

            // Execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Apply an imagery basemap to the map
            MyMapView.Map = new Map(Basemap.CreateDarkGrayCanvasVector());

            // Create the Wms Service
            WmsService service = new WmsService(wmsUrl);

            // Load the WMS Service
            await service.LoadAsync();

            // Get the service info (metadata) from the service
            WmsServiceInfo info = service.ServiceInfo;

            // Get the list of layer names
            IReadOnlyList<WmsLayerInfo> topLevelLayers = info.LayerInfos;

            // Recursively build up a list of all the layers in the service and get their IDs as a flat list
            List<WmsLayerInfo> expandedList = new List<WmsLayerInfo>();
            GetLayerIds(topLevelLayers, expandedList);

            // Build the ViewModel from the expanded list of layer infos
            foreach(WmsLayerInfo layerInfo in expandedList)
            {
                // DisplayObject is a custom type made for this sample to serve as the ViewModel; it is not a part of the ArcGIS Runtime
                _viewModel.Add(new DisplayObject(layerInfo));
            }

            // Update the map display based on the viewModel
            UpdateMapDisplay(_viewModel);

            // Update the list of layers
            MyDisplayList.ItemsSource = _viewModel;
        }

        private void GetLayerIds(IReadOnlyList<WmsLayerInfo> info, List<WmsLayerInfo> result)
        {
            // Return if there are no more layers to explore
            if (info.Count < 1) { return; }

            // Add each layer and each layer's children
            foreach(WmsLayerInfo layer in info)
            {
                // Add this layer if it is a valid display layer
                //if (!String.IsNullOrWhiteSpace(layer.Name))
                {
                    result.Add(layer);
                }

                // Recursively add children
                GetLayerIds(layer.LayerInfos, result);
            }
        }

        /// <summary>
        /// Updates the map layer 
        /// </summary>
        private void UpdateMapDisplay(ObservableCollection<DisplayObject> displayList)
        {
            // Remove all existing layers
            MyMapView.Map.OperationalLayers.Clear();

            // Get a list of selected LayerInfos
            IEnumerable<WmsLayerInfo> selectedLayers = displayList.Where(vm => vm.IsEnabled).Select(vm => vm.Info);

            // Create a new WmsLayer from the selected layers
            WmsLayer myLayer = new WmsLayer(selectedLayers);

            // Add the layer to the map
            MyMapView.Map.OperationalLayers.Add(myLayer);
        }

        private void MyDisplayList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Update items
            foreach(DisplayObject item in e.AddedItems)
            {
                item.IsEnabled = true;
            }

            foreach(DisplayObject item in e.RemovedItems)
            {
                item.IsEnabled = false;
            }

            // Update the map
            UpdateMapDisplay(_viewModel);
        }
    }

    public class DisplayObject
    {
        public WmsLayerInfo Info { get; set; }

        public Boolean IsEnabled { get; set; } = false;

        public DisplayObject(WmsLayerInfo info)
        {
            this.Info = info;
        }

        public override string ToString()
        {
            return Info.Title;
        }
    }
}