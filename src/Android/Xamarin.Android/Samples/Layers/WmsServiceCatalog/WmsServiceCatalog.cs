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
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;

using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.WmsServiceCatalog
{
    [Activity]
    public class WmsServiceCatalog : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Hold a reference to the listview
        private ListView _myDisplayList;

		// Hold the URL to the WMS service providing the US NOAA National Weather Service forecast weather chart
		private Uri wmsUrl = new Uri("https://idpgis.ncep.noaa.gov/arcgis/services/NWS_Forecasts_Guidance_Warnings/natl_fcst_wx_chart/MapServer/WMSServer?request=GetCapabilities&service=WMS");

		// Hold a list of LayerDisplayVM; this is the ViewModel
		private List<LayerDisplayVM> _viewModel = new List<LayerDisplayVM>();

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
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the list view
            _myDisplayList = new ListView(this);

            // Add the listview and mapview to the layout
            layout.AddView(_myDisplayList);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

		private async void Initialize()
		{
			// Apply an imagery basemap to the map
			_myMapView.Map = new Map(Basemap.CreateDarkGrayCanvasVector());

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
			foreach (WmsLayerInfo layerInfo in expandedList)
			{
				// LayerDisplayVM is a custom type made for this sample to serve as the ViewModel; it is not a part of the ArcGIS Runtime
				_viewModel.Add(new LayerDisplayVM(layerInfo));
			}

			// Create an array adapter for the layer display
			ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, _viewModel);

			// Apply the adapter
			_myDisplayList.Adapter = adapter;

            // Subscribe to seleciton change notifications
            _myDisplayList.ItemClick += _myDisplayList_ItemClick;;

			// Update the map display based on the viewModel
			UpdateMapDisplay(_viewModel);
		}

		private void GetLayerIds(IReadOnlyList<WmsLayerInfo> info, List<WmsLayerInfo> result)
		{
			// Return if there are no more layers to explore
			if (info.Count < 1) { return; }

			// Add each layer and each layer's children
			foreach (WmsLayerInfo layer in info)
			{
				// Add the layer
				result.Add(layer);

				// Recursively add children
				GetLayerIds(layer.LayerInfos, result);
			}
		}

		/// <summary>
		/// Updates the map layer
		/// </summary>
		private void UpdateMapDisplay(List<LayerDisplayVM> displayList)
		{
			// Remove all existing layers
            _myMapView.Map.OperationalLayers.Clear();

			// Get a list of selected LayerInfos
			IEnumerable<WmsLayerInfo> selectedLayers = displayList.Where(vm => vm.IsEnabled).Select(vm => vm.Info);

			// Create a new WmsLayer from the selected layers
			WmsLayer myLayer = new WmsLayer(selectedLayers);

			// Add the layer to the map
			_myMapView.Map.OperationalLayers.Add(myLayer);
		}

        void _myDisplayList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
			// Clear existing selection
			foreach (LayerDisplayVM item in _viewModel)
			{
				item.IsEnabled = false;
			}

			// Update the selection
			_viewModel[e.Position].IsEnabled = true;

			// Update the map
			UpdateMapDisplay(_viewModel);
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
		public Boolean IsEnabled { get; set; } = false;

		/// <summary>
		/// Title property to facilitate binding
		/// </summary>
		public String Title { get { return Info.Title; } }

		public LayerDisplayVM(WmsLayerInfo info)
		{
			this.Info = info;
		}

        public override string ToString()
        {
            return Title;
        }
	}
}