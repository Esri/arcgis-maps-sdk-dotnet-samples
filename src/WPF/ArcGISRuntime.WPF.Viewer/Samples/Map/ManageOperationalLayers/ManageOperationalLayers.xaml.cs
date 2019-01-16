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

namespace ArcGISRuntime.WPF.Samples.ManageOperationalLayers
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Manage operational layers",
        "Map",
        "Add, remove, and reorder operational layers in a map.",
        "")]
    public partial class ManageOperationalLayers
    {
        // The view model manages the data for the sample.
        private MapViewModel _viewModel;

        // During drag and drop, keep track of which list the dragged item came from.
        private ListBoxItem _originatingListBoxItem;

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
            _viewModel = new MapViewModel(new Map(Basemap.CreateStreets()));
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
                ListBoxItem sendingItem = (ListBoxItem) sender;
                _originatingListBoxItem = sendingItem;
                DragDrop.DoDragDrop(sendingItem, sendingItem.DataContext, DragDropEffects.Move);
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

            ListBox destinationBox = FindParentListBox((UIElement) sender);

            // Get the data that is being dropped.
            Layer draggedData = (Layer) e.Data.GetData(typeof(ArcGISMapImageLayer));

            // Find where in the respective lists the items are.
            int indexOfRemoved = sourceBox.Items.IndexOf(draggedData);
            int indexOfInsertion;

            // Sender is the control that the item is being dropped on. Could be a listbox or a listbox item.
            if (sender is ListBoxItem)
            {
                Layer targetData = ((ListBoxItem) sender).DataContext as Layer;
                indexOfInsertion = destinationBox.Items.IndexOf(targetData);
            }
            else if (destinationBox != sourceBox)
            {
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
            while (!(parentElement is ListBox) && parentElement != null)
            {
                parentElement = VisualTreeHelper.GetParent(parentElement) as UIElement;
            }

            return parentElement as ListBox;
        }

        #endregion Drag and drop support
    }

    class MapViewModel
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