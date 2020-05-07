// Copyright 2019 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.ManageOperationalLayers
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Manage operational layers",
        "Map",
        "Add, remove, and reorder operational layers in a map.",
        "When the app starts, a list displays the operational layers that are currently displayed in the map. Right-tap on the list item to remove the layer, or left-tap to move it to the top. The map will be updated automatically.",
        "add", "delete", "layer", "map", "remove")]
    public class ManageOperationalLayers : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private ListView _includedListView;
        private ListView _excludedListView;
        private PopupMenu _menu;
        private MapViewModel _viewModel;

        // Some URLs of layers to add to the map.
        private readonly string[] _layerUrls = new[]
        {
            "http://sampleserver5.arcgisonline.com/arcgis/rest/services/Elevation/WorldElevations/MapServer",
            "http://sampleserver5.arcgisonline.com/arcgis/rest/services/Census/MapServer",
            "http://sampleserver5.arcgisonline.com/arcgis/rest/services/DamageAssessment/MapServer"
        };

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Manage operational layers";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Configure the view model and the map.
            _viewModel = new MapViewModel(new Map(Basemap.CreateStreets()));
            _myMapView.Map = _viewModel.Map;

            // Add the layers.
            foreach (string layerUrl in _layerUrls)
            {
                _viewModel.AddLayerFromUrl(layerUrl);
            }

            // Configure the list views to show the layer lists.
            UpdateLayerListViews();

            // Listen for taps to enable reconfiguring.
            _includedListView.ItemClick += ListItem_Click;
            _excludedListView.ItemClick += ListItem_Click;
        }

        private void UpdateLayerListViews()
        {
            // Configure array adapters - these convert the layer lists into arrays of strings that can be displayed in a list view.
            ArrayAdapter includedLayerAdapter = new ArrayAdapter<string>(
                this,
                Android.Resource.Layout.SimpleListItem1,
                _viewModel.IncludedLayers.Select(layer => layer.Name).ToArray());
            ArrayAdapter excludedLayerAdapter = new ArrayAdapter<string>(
                this,
                Android.Resource.Layout.SimpleListItem1,
                _viewModel.ExcludedLayers.Select(layer => layer.Name).ToArray());

            _includedListView.Adapter = includedLayerAdapter;
            _excludedListView.Adapter = excludedLayerAdapter;
        }

        private void ListItem_Click(object sender, AdapterView.ItemClickEventArgs e)
        {
            // Find the list the item belongs to.
            LayerCollection sendingList = sender == _includedListView ? _viewModel.IncludedLayers : _viewModel.ExcludedLayers;

            // Constants for command names.
            const string moveUpCommand = "Move up";
            const string moveDownCommand = "Move down";
            const string addToMapCommand = "Add to map";
            const string removeFromMapCommand = "Remove from map";

            // Create menu to show options.
            _menu = new PopupMenu(this, (ListView) sender);

            // Handle the click, calling the right method depending on the command.
            _menu.MenuItemClick += (o, menuArgs) =>
            {
                _menu.Dismiss();
                switch (menuArgs.Item.ToString())
                {
                    case moveUpCommand:
                        _viewModel.PromoteLayer(sendingList, e.Position);
                        break;
                    case moveDownCommand:
                        _viewModel.DemoteLayer(sendingList, e.Position);
                        break;
                    case addToMapCommand:
                    case removeFromMapCommand:
                        _viewModel.MoveLayer(sendingList, e.Position);
                        break;
                }

                // Update the lists in the view.
                UpdateLayerListViews();
            };

            // Add the menu commands.
            _menu.Menu.Add(moveUpCommand);
            _menu.Menu.Add(moveDownCommand);
            _menu.Menu.Add(sender == _includedListView ? removeFromMapCommand : addToMapCommand);

            // Show menu in the view.
            _menu.Show();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) {Orientation = Orientation.Vertical};

            // Create and add a help label.
            TextView helpLabel = new TextView(this);
            helpLabel.Text = "Tap to reorder or add/remove layers.";
            helpLabel.Gravity = GravityFlags.Center;
            layout.AddView(helpLabel);

            // Create and add layer lists and their labels.
            TextView inMapLabel = new TextView(this);
            inMapLabel.Text = "Layers in map";
            inMapLabel.Gravity = GravityFlags.Center;
            layout.AddView(inMapLabel);

            _includedListView = new ListView(this);
            layout.AddView(_includedListView);

            TextView outOfMapLabel = new TextView(this);
            outOfMapLabel.Text = "Layers not in map";
            outOfMapLabel.Gravity = GravityFlags.Center;
            layout.AddView(outOfMapLabel);

            _excludedListView = new ListView(this);
            layout.AddView(_excludedListView);

            // Create the map view.
            _myMapView = new MapView(this);

            // Add the map view to the layout.
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
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

        public void DemoteLayer(LayerCollection owningCollection, int position)
        {
            // Skip if the layer can't be moved because its already at the bottom.
            if (position == owningCollection.Count - 1)
            {
                return;
            }

            // Move the layer by removing it from its current position and inserting it at the next higher position.
            Layer selectedLayer = owningCollection[position];
            owningCollection.RemoveAt(position);
            owningCollection.Insert(position + 1, selectedLayer);
        }

        public void PromoteLayer(LayerCollection owningCollection, int position)
        {
            // Skip if the layer can't be moved up because it is already at the top.
            if (position < 1)
            {
                return;
            }

            // Move the layer by removing it from its current position and adding it at the next lower position.
            Layer selectedLayer = owningCollection[position];
            owningCollection.RemoveAt(position);
            owningCollection.Insert(position - 1, selectedLayer);
        }

        public void MoveLayer(LayerCollection owningCollection, int position)
        {
            // Find the selected layer.
            Layer selectedLayer = owningCollection[position];

            // Move the layer from one list to another by removing it from the source list and adding it to the destination list.
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