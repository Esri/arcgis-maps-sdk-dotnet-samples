// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using UIKit;

namespace ArcGISRuntime.Samples.DisplayLayerViewState
{
    [Register("DisplayLayerViewState")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display layer view state",
        "MapView",
        "This sample demonstrates how to get view status for layers in a map.",
        "")]
    public class DisplayLayerViewState : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();

        private readonly UITableView _tableView = new UITableView
        {
            RowHeight = 40
        };

        // Reference to list of view status for each layer.
        private readonly List<LayerStatusModel> _layerStatusModels = new List<LayerStatusModel>();

        public DisplayLayerViewState()
        {
            Title = "Display Layer View State";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat tableViewHeight = 120;

                // Reposition the views.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height - tableViewHeight);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 0, 0);
                _tableView.Frame = new CGRect(0, _myMapView.Frame.Height, View.Bounds.Width, tableViewHeight);

                base.ViewDidLayoutSubviews();
            }
            catch (NullReferenceException)
            {
            }
        }

        private void Initialize()
        {
            // Create a new Map.
            Map myMap = new Map();

            // Create the URL for the tiled layer.
            Uri tiledLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/WorldTimeZones/MapServer");

            // Create a tiled layer from the URL
            ArcGISTiledLayer tiledLayer = new ArcGISTiledLayer(tiledLayerUri)
            {
                Name = "Tiled Layer"
            };

            // Add the tiled layer to map.
            myMap.OperationalLayers.Add(tiledLayer);

            // Create the URL for the ArcGISMapImage layer.
            var imageLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer");

            // Create ArcGISMapImage layer using a URL.
            ArcGISMapImageLayer imageLayer = new ArcGISMapImageLayer(imageLayerUri)
            {
                Name = "Image Layer",
                // Set the visible scale range for the image layer.
                MinScale = 40000000,
                MaxScale = 2000000
            };

            // Add the image layer to map.
            myMap.OperationalLayers.Add(imageLayer);

            // Create Uri for feature layer.
            var featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Recreation/FeatureServer/0");

            // Create a feature layer using URL.
            FeatureLayer myFeatureLayer = new FeatureLayer(featureLayerUri)
            {
                Name = "Feature Layer"
            };

            // Add the feature layer to map.
            myMap.OperationalLayers.Add(myFeatureLayer);

            // Create a point the map should zoom to.
            MapPoint mapPoint = new MapPoint(-11000000, 4500000, SpatialReferences.WebMercator);

            // Set the initial viewpoint for map.
            myMap.InitialViewpoint = new Viewpoint(mapPoint, 50000000);

            // Initialize the model list with unknown status for each layer.
            foreach (Layer layer in myMap.OperationalLayers)
            {
                _layerStatusModels.Add(new LayerStatusModel(layer.Name, "Unknown"));
            }

            // Create the table view source and pass the list of models to it.
            _tableView.Source = new LayerViewStatusTableSource(_layerStatusModels);

            // Event for layer view state changed.
            _myMapView.LayerViewStateChanged += OnLayerViewStateChanged;

            // Provide used Map to the MapView.
            _myMapView.Map = myMap;
        }

        private void OnLayerViewStateChanged(object sender, LayerViewStateChangedEventArgs e)
        {
            // State changed event is sent by a layer. In the list, find the layer which sends this event. 
            // If it exists then update the status.
            var model = _layerStatusModels.FirstOrDefault(l => l.LayerName == e.Layer.Name);
            if (model != null)
            {
                model.LayerViewStatus = e.LayerViewState.Status.ToString();
            }

            // Update the table.
            _tableView.ReloadData();
        }

        private void CreateLayout()
        {
            View.AddSubviews(_myMapView, _tableView);
        }
    }

    /// <summary>
    /// Class that is used by the table view to populate itself
    /// </summary>
    internal class LayerViewStatusTableSource : UITableViewSource
    {
        // List of layer status model.
        private readonly List<LayerStatusModel> _layers;

        // Identifier for the table cell.
        private readonly string cellIdentifier = "TableCell";

        public LayerViewStatusTableSource(List<LayerStatusModel> layers)
        {
            _layers = layers;
        }

        /// <summary>
        /// Called by the TableView to determine how many sections(groups) there are.
        /// </summary>
        public override nint NumberOfSections(UITableView tableView)
        {
            return 1;
        }

        /// <summary>
        /// Called by the TableView to determine how many cells to create for that particular section.
        /// </summary>
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _layers?.Count ?? 0;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Get the reused cell from the table view.
            UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);

            // Get the model at this current cell index.
            LayerStatusModel model = _layers[indexPath.Row];

            // If there are no cells to reuse-create one.
            // We are specifically using Value1 style so text for both layer name and status can be displayed.
            if (cell == null)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Value1, cellIdentifier);
            }

            cell.TextLabel.Text = model.LayerName ?? "Layer " + indexPath.Row;
            cell.DetailTextLabel.Text = model.LayerViewStatus;
            return cell;
        }
    }

    /// <summary>
    /// This is a custom class that holds information for layer name and status.
    /// </summary>
    internal class LayerStatusModel
    {
        internal string LayerName { get; }
        internal string LayerViewStatus { get; set; }

        public LayerStatusModel(string layerName, string layerStatus)
        {
            LayerName = layerName;
            LayerViewStatus = layerStatus;
        }
    }
}