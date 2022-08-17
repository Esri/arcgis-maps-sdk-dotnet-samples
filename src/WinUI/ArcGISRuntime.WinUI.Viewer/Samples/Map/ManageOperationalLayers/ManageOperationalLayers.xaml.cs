// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;

namespace ArcGISRuntime.WinUI.Samples.ManageOperationalLayers
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Manage operational layers",
        category: "Map",
        description: "Add, remove, and reorder operational layers in a map.",
        instructions: "When the app starts, a list displays the operational layers that are currently displayed in the map. Right-click on the list item to remove the layer, or left-click to move it to the top. The map will be updated automatically.",
        tags: new[] { "add", "delete", "layer", "map", "remove" })]
    public partial class ManageOperationalLayers
    {
        // The view model manages the data for the sample.
        private MapViewModel _viewModel;

        // Hold a reference to the originating listview when dragging and dropping.
        private ListView _originListView;

        // Some URLs of layers to add to the map.
        private readonly string[] _layerUrls = new[]
        {
            "http://sampleserver5.arcgisonline.com/arcgis/rest/services/Elevation/WorldElevations/MapServer",
            "http://sampleserver5.arcgisonline.com/arcgis/rest/services/Census/MapServer",
            "http://sampleserver5.arcgisonline.com/arcgis/rest/services/DamageAssessment/MapServer"
        };

        public ManageOperationalLayers()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            _viewModel = new MapViewModel(new Map(BasemapStyle.ArcGISStreets));

            // Configure the bindings to point to the view model.
            this.DataContext = _viewModel;

            // Add the layers.
            foreach (string layerUrl in _layerUrls)
            {
                _viewModel.AddLayerFromUrl(layerUrl);
            }
        }

        private void ListBox_OnDragOver(object sender, DragEventArgs e)
        {
            // Specify that the listview accepts dropping and that the operation is a move (rather than a copy or link)
            e.AcceptedOperation = DataPackageOperation.Move;
        }

        private void ListBox_OnDrop(object sender, DragEventArgs e)
        {
            // If the item being dropped is a layer...
            if (e.DataView != null && e.DataView.Properties != null && e.DataView.Properties.Any(x => x.Key == "item" && x.Value is Layer))
            {
                try
                {
                    // Start doing work for the drop.
                    DragOperationDeferral deferral = e.GetDeferral();

                    // Get the layer that is being moved.
                    KeyValuePair<string, object> draggedItem = e.Data.Properties.FirstOrDefault(x => x.Key == "item");
                    Layer draggedLayer = draggedItem.Value as Layer;

                    // Find the source and destination views.
                    ListView destinationList = sender as ListView;
                    ListView sourceList = _originListView;

                    // Remove the layer and re-add it.
                    ((LayerCollection)sourceList.ItemsSource).Remove(draggedLayer);
                    ((LayerCollection)destinationList.ItemsSource).Add(draggedLayer);

                    // Finish the drop.
                    deferral.Complete();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
            else
            {
                // Don't allow other things to be dropped (e.g. files from the desktop).
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }

        private void ListBox_OnDragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            // Store the originating list view for the drag operation.
            _originListView = sender as ListView;

            // Specify the type of drag and drop.
            e.Data.RequestedOperation = DataPackageOperation.Move;

            if (e.Items != null && e.Items.Any())
            {
                // Store the layer in the data package.
                e.Data.Properties.Add("item", e.Items.FirstOrDefault());
            }
        }
    }

    internal class MapViewModel
    {
        public Map Map { get; }
        public LayerCollection IncludedLayers => Map.OperationalLayers;
        public LayerCollection ExcludedLayers { get; } = new LayerCollection();

        public MapViewModel(Map map)
        {
            Map = map;
        }

        public void AddLayerFromUrl(string layerUrl)
        {
            ArcGISMapImageLayer layer = new ArcGISMapImageLayer(new Uri(layerUrl));
            Map.OperationalLayers.Add(layer);
        }
    }
}