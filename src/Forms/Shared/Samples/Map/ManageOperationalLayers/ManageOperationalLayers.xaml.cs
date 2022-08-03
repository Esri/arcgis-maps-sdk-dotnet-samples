// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.ManageOperationalLayers
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Manage operational layers",
        category: "Map",
        description: "Add, remove, and reorder operational layers in a map.",
        instructions: "When the app starts, a list displays the operational layers that are currently displayed in the map. Right-tap on the list item to remove the layer, or left-tap to move it to the top. The map will be updated automatically.",
        tags: new[] { "add", "delete", "layer", "map", "remove" })]
    public partial class ManageOperationalLayers : ContentPage
    {
        private MapViewModel _viewModel;

        // Some URLs of layers to add to the map.
        private readonly string[] _layerUrls = new[]
        {
            "https://sampleserver5.arcgisonline.com/arcgis/rest/services/Elevation/WorldElevations/MapServer",
            "https://sampleserver5.arcgisonline.com/arcgis/rest/services/Census/MapServer",
            "https://sampleserver5.arcgisonline.com/arcgis/rest/services/DamageAssessment/MapServer"
        };

        public ManageOperationalLayers()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Set up the view model and bindings.
            _viewModel = new MapViewModel(new Map(BasemapStyle.ArcGISStreets));
            MyMapView.Map = _viewModel.Map;
            IncludedListView.ItemsSource = _viewModel.IncludedLayers;
            ExcludedListView.ItemsSource = _viewModel.ExcludedLayers;

            // Add the layers.
            foreach (string layerUrl in _layerUrls)
            {
                _viewModel.AddLayerFromUrl(layerUrl);
            }
        }

        private void MoveButton_OnClicked(object sender, EventArgs e)
        {
            try
            {
                // Get the clicked button.
                Button clickedButton = (Button)sender;

                // Get the clicked layer.
                Layer clickedLayer = (Layer)clickedButton.BindingContext;

                // Move the layer.
                _viewModel.MoveLayer(clickedLayer);
            }
            // Sometimes if the user clicks too quickly NREs will occur.
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void DemoteButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Get the clicked button.
                Button clickedButton = (Button)sender;

                // Get the clicked layer.
                Layer clickedLayer = (Layer)clickedButton.BindingContext;

                // Move the layer.
                _viewModel.DemoteLayer(clickedLayer);
            }
            // Sometimes if the user clicks too quickly NREs will occur.
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void PromoteButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Get the clicked button.
                Button clickedButton = (Button)sender;

                // Get the clicked layer.
                Layer clickedLayer = (Layer)clickedButton.BindingContext;

                // Move the layer.
                _viewModel.PromoteLayer(clickedLayer);
            }
            // Sometimes if the user clicks too quickly NREs will occur.
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }

    class MapViewModel
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

        public void DemoteLayer(Layer selectedLayer)
        {
            // Find the collection the layer is in.
            LayerCollection owningCollection;
            if (IncludedLayers.Contains(selectedLayer))
            {
                owningCollection = IncludedLayers;
            }
            else
            {
                owningCollection = ExcludedLayers;
            }

            // Get the current index (position) of the layer.
            int layerIndex = owningCollection.IndexOf(selectedLayer);

            // Skip if the layer can't be moved down because it is already at the bottom.
            if (layerIndex == owningCollection.Count - 1)
            {
                return;
            }

            // Move the layer by removing it and re-adding it at its old position plus 1.
            owningCollection.Remove(selectedLayer);
            owningCollection.Insert(layerIndex + 1, selectedLayer);
        }

        public void PromoteLayer(Layer selectedLayer)
        {
            // Find the collection the layer is in.
            LayerCollection owningCollection = IncludedLayers.Contains(selectedLayer) ? IncludedLayers : ExcludedLayers;

            // Get the current index (position) of the layer.
            int layerIndex = owningCollection.IndexOf(selectedLayer);

            // Skip if the layer can't be moved because it is already at the top.
            if (layerIndex < 1)
            {
                return;
            }

            // Move the layer by removing it and re-adding it at its old position minus 1.
            owningCollection.Remove(selectedLayer);
            owningCollection.Insert(layerIndex - 1, selectedLayer);
        }

        public void MoveLayer(Layer selectedLayer)
        {
            // Remove the layer from the list it is currently in and add it to the other list.
            if (IncludedLayers.Contains(selectedLayer))
            {
                IncludedLayers.Remove(selectedLayer);
                ExcludedLayers.Add(selectedLayer);
            }
            else
            {
                ExcludedLayers.Remove(selectedLayer);
                IncludedLayers.Add(selectedLayer);
            }
        }
    }
}