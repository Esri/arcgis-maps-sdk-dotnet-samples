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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ArcGIS.WPF.Samples.ManageOperationalLayers
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Manage operational layers",
        category: "Map",
        description: "Add, remove, and reorder operational layers in a map.",
        instructions: "When the app starts, a list displays the operational layers that are currently displayed in the map. Right-click on the list item to remove the layer, or left-click to move it to the top. The map will be updated automatically.",
        tags: new[] { "add", "delete", "layer", "map", "remove" })]
    public partial class ManageOperationalLayers
    {
        // The view model manages the data for the sample.
        private MapViewModel _viewModel;

        // During drag and drop, keep track of which list the dragged item came from.
        private ListBoxItem _originatingListBoxItem;

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
            _viewModel = new MapViewModel(new Map(BasemapStyle.ArcGISStreets));

            // Configure bindings to point to the view model.
            this.DataContext = _viewModel;

            // Add the layers.
            foreach (string layerUrl in _layerUrls)
            {
                _viewModel.AddLayerFromUrl(layerUrl);
            }
        }

        #region Drag and drop support

        private void ListBox_DragPreviewMove(object sender, MouseButtonEventArgs e)
        {
            // This method is called when the user clicks and starts dragging a listbox item.

            if (sender is ListBoxItem)
            {
                // Get the listbox item that is being moved.
                ListBoxItem sendingItem = (ListBoxItem)sender;

                // Record that this item was being dragged - used later when drag ends to determine which item to move.
                _originatingListBoxItem = sendingItem;

                // Register the start of the drag & drop operation with the system.
                DragDrop.DoDragDrop(sendingItem, sendingItem.DataContext, DragDropEffects.Move);

                // Mark the dragged item as selected.
                sendingItem.IsSelected = true;
            }
        }

        private void ListBoxItem_OnDrop(object sender, DragEventArgs e)
        {
            // This method is called when the user finishes dragging while over the listbox.

            // Find the source and destination list boxes.
            ListBox sourceBox = FindParentListBox(_originatingListBoxItem);
            if (sourceBox == null)
            {
                // Return if the source isn't valid - happens when duplicate events are raised.
                return;
            }

            // Find the list box that the item was dropped on (i.e. dragged to).
            ListBox destinationBox = FindParentListBox((UIElement)sender);

            // Get the data that is being dropped.
            Layer draggedData = (Layer)e.Data.GetData(typeof(ArcGISMapImageLayer));

            // Find where in the respective lists the items are.
            int indexOfRemoved = sourceBox.Items.IndexOf(draggedData);
            int indexOfInsertion;

            // Sender is the control that the item is being dropped on. Could be a listbox or a listbox item.
            if (sender is ListBoxItem)
            {
                // Find the layer that the item represents.
                Layer targetData = ((ListBoxItem)sender).DataContext as Layer;

                // Find the position of the layer in the listbox.
                indexOfInsertion = destinationBox.Items.IndexOf(targetData);
            }
            else if (destinationBox != sourceBox)
            {
                // Drop the item at the end of the list if the user let go of the item on the empty space in the box rather than the list item.
                // This works because both the listbox and its individual listbox items participate in drag and drop.
                indexOfInsertion = destinationBox.Items.Count - 1;
            }
            else
            {
                return;
            }

            // Find the appropriate source and destination boxes.
            LayerCollection sourceList = sourceBox == IncludedListBox ? _viewModel.IncludedLayers : _viewModel.ExcludedLayers;
            LayerCollection destinationList = destinationBox == IncludedListBox ? _viewModel.IncludedLayers : _viewModel.ExcludedLayers;

            // Return if there is nothing to do.
            if (sourceList == destinationList && indexOfRemoved == indexOfInsertion)
            {
                return;
            }

            if (sourceBox == destinationBox && indexOfRemoved < indexOfInsertion)
            {
                indexOfInsertion -= 1;
            }

            // Perform the move.
            sourceList.RemoveAt(indexOfRemoved);
            destinationList.Insert(indexOfInsertion + 1, draggedData);
        }

        private static ListBox FindParentListBox(UIElement source)
        {
            // This is needed because it is hard to tell which listbox an item belongs to.

            // Walk up the visual element tree until a ListBox is found.
            UIElement parentElement = source;
            // While the parent element is not a listbox and the parent element is not null,
            while (!(parentElement is ListBox) && parentElement != null)
            {
                // find the next parent.
                parentElement = VisualTreeHelper.GetParent(parentElement) as UIElement;
            }

            return parentElement as ListBox;
        }

        #endregion Drag and drop support
    }

    internal class MapViewModel
    {
        public Map Map { get; set; }

        public LayerCollection IncludedLayers
        {
            get { return Map.OperationalLayers; }
        }

        public LayerCollection ExcludedLayers { get; set; }

        public MapViewModel(Map map)
        {
            Map = map;
            ExcludedLayers = new LayerCollection();
        }

        public void AddLayerFromUrl(string layerUrl)
        {
            ArcGISMapImageLayer layer = new ArcGISMapImageLayer(new Uri(layerUrl));
            Map.OperationalLayers.Add(layer);
        }
    }
}