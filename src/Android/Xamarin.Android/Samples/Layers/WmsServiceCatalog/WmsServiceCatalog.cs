// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcGISRuntime.Samples.WmsServiceCatalog
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "WMS service catalog",
        category: "Layers",
        description: "Connect to a WMS service and show the available layers and sublayers. ",
        instructions: "",
        tags: new[] { "OGC", "WMS", "catalog", "web map service" })]
    public class WmsServiceCatalog : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView;

        // Hold a reference to the ListView
        private ListView _myDisplayList;

        // Hold the URL to the WMS service providing the US NOAA National Weather Service forecast weather chart
        private readonly Uri _wmsUrl = new Uri("https://idpgis.ncep.noaa.gov/arcgis/services/NWS_Forecasts_Guidance_Warnings/natl_fcst_wx_chart/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Hold a list of LayerDisplayVM; this is the ViewModel
        private readonly List<LayerDisplayVM> _viewModelList = new List<LayerDisplayVM>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "WMS service catalog";

            // Create the UI, setup the control references
            CreateLayout();

            // Initialize the map
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Configuration for having the mapview and webview fill the screen.
            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent,
                1.0f
            );

            // Create the list view
            _myDisplayList = new ListView(this) { LayoutParameters = layoutParams };

            // Create two help labels
            TextView promptLabel = new TextView(this) { Text = "Select a layer" };

            // Create the mapview.
            _myMapView = new MapView(this) { LayoutParameters = layoutParams };

            // Add the views to the layout
            layout.AddView(promptLabel);
            layout.AddView(_myMapView);
            layout.AddView(_myDisplayList);

            // Show the layout in the app
            SetContentView(layout);
        }

        private async void Initialize()
        {
            // Apply an imagery basemap to the map
            _myMapView.Map = new Map(Basemap.CreateDarkGrayCanvasVector());

            // Create the WMS Service
            WmsService service = new WmsService(_wmsUrl);

            try
            {
                // Load the WMS Service
                await service.LoadAsync();

                // Get the service info (metadata) from the service.
                WmsServiceInfo info = service.ServiceInfo;

                // Get the list of layer infos.
                foreach (var layerInfo in info.LayerInfos)
                {
                    LayerDisplayVM.BuildLayerInfoList(new LayerDisplayVM(layerInfo, null), _viewModelList);
                }

                // Create an array adapter for the layer display
                ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, _viewModelList);

                // Apply the adapter
                _myDisplayList.Adapter = adapter;

                // Subscribe to selection change notifications
                _myDisplayList.ItemClick += _myDisplayList_ItemClick;

                // Update the map display based on the viewModel
                UpdateMapDisplay(_viewModelList);
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
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
            List<WmsLayerInfo> selectedLayers = displayList.Where(vm => vm.IsEnabled).Select(vm => vm.Info).ToList();

            // Return if list is empty
            if (!selectedLayers.Any())
            {
                return;
            }

            // Create a new WmsLayer from the selected layers
            WmsLayer myLayer = new WmsLayer(selectedLayers);

            try
            {
                // Load the layer
                await myLayer.LoadAsync();

                // Zoom to the extent of the layer
                _myMapView.SetViewpoint(new Viewpoint(myLayer.FullExtent));

                // Add the layer to the map
                _myMapView.Map.OperationalLayers.Add(myLayer);
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }

        /// <summary>
        /// Takes action once a new layer selection is made
        /// </summary>
        private void _myDisplayList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            // Clear existing selection
            foreach (LayerDisplayVM item in _viewModelList)
            {
                item.Select(false);
            }

            // Update the selection
            _viewModelList[e.Position].Select();

            // Update the map
            UpdateMapDisplay(_viewModelList);
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

        // Format with indentation to simulate a treeview.
        public override string ToString()
        {
            return $"{new String(' ', NestLevel * 8)} {Info.Title}";
        }

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