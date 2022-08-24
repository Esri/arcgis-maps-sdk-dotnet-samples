// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.DisplayLayerViewState
{
    [Register("DisplayLayerViewState")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display layer view state",
        category: "MapView",
        description: "Determine if a layer is currently being viewed.",
        instructions: "Pan and zoom around in the map. Each layer's view status is displayed. Notice that some layers configured with a min and max scale change to \"OutOfScale\" at certain scales.",
        tags: new[] { "layer", "map", "status", "view" })]
    public class DisplayLayerViewState : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UITableView _tableView;

        // Reference to list of view status for each layer.
        private readonly List<LayerStatusModel> _layerStatusModels = new List<LayerStatusModel>();

        public DisplayLayerViewState()
        {
            Title = "Display Layer View State";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        private void Initialize()
        {
            // Create a new Map.
            Map myMap = new Map();

            // Create the URL for the tiled layer.
            Uri tiledLayerUri = new Uri("https://services.arcgisonline.com/arcgis/rest/services/World_Topo_Map/MapServer");

            // Create a tiled layer from the URL
            ArcGISTiledLayer tiledLayer = new ArcGISTiledLayer(tiledLayerUri)
            {
                Name = "Tiled Layer"
            };

            // Add the tiled layer to map.
            myMap.OperationalLayers.Add(tiledLayer);

            // Create the URL for the ArcGISMapImage layer.
            Uri imageLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer");

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
            Uri featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Recreation/FeatureServer/0");

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

            _myMapView.LayerViewStateChanged += OnLayerViewStateChanged;

            // Provide used Map to the MapView.
            _myMapView.Map = myMap;
        }

        private void OnLayerViewStateChanged(object sender, LayerViewStateChangedEventArgs e)
        {
            // State changed event is sent by a layer. In the list, find the layer which sends this event. 
            // If it exists then update the status.
            LayerStatusModel model = _layerStatusModels.FirstOrDefault(l => l.LayerName == e.Layer.Name);
            if (model != null)
            {
                model.LayerViewStatus = e.LayerViewState.Status.ToString();
            }

            // Update the table.
            _tableView.ReloadData();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _tableView = new UITableView();
            _tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            _tableView.RowHeight = 40;
            
            // Add the views.
            View.AddSubviews(_myMapView, _tableView);

            // Set a default layout.
            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                applyLandscapeLayout();
            }
            else
            {
                applyPortraitLayout();
            }
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            // Reset constraints.
            _tableView.RemoveFromSuperview();
            _myMapView.RemoveFromSuperview();
            View.AddSubviews(_myMapView, _tableView);

            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                applyLandscapeLayout();
            }
            else
            {
                applyPortraitLayout();
            }
        }

        private void applyPortraitLayout()
        {
            _tableView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _tableView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _tableView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _tableView.HeightAnchor.ConstraintEqualTo(120).Active = true;

            _myMapView.TopAnchor.ConstraintEqualTo(_tableView.BottomAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
        }

        private void applyLandscapeLayout()
        {
            _tableView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _tableView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _tableView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _tableView.WidthAnchor.ConstraintEqualTo(View.Frame.Height).Active = true;

            _myMapView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(_tableView.TrailingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            
            // Subscribe to events, removing any existing subscriptions.
            _myMapView.LayerViewStateChanged -= OnLayerViewStateChanged;
            _myMapView.LayerViewStateChanged += OnLayerViewStateChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.LayerViewStateChanged -= OnLayerViewStateChanged;
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
        private const string CellIdentifier = "TableCell";

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
            UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);

            // Get the model at this current cell index.
            LayerStatusModel model = _layers[indexPath.Row];

            // If there are no cells to reuse-create one.
            // We are specifically using Value1 style so text for both layer name and status can be displayed.
            if (cell == null)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Value1, CellIdentifier);
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