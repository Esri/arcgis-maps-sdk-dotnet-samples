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
using Esri.ArcGISRuntime.UI;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.DisplayLayerViewState
{
    [Register("DisplayLayerViewState")]
    public class DisplayLayerViewState : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Create and hold reference to tableview
        private UITableView _tableView;

        // Reference to list of view status for each layer
        private List<LayerStatusModel> _layerStatusModels = new List<LayerStatusModel>();

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

        private void Initialize()
        {
            // Create new Map
            Map myMap = new Map();

            // Create the uri for the tiled layer
            Uri tiledLayerUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/WorldTimeZones/MapServer");

            // Create a tiled layer using url
            ArcGISTiledLayer tiledLayer = new ArcGISTiledLayer(tiledLayerUri);
            tiledLayer.Name = "Tiled Layer";

            // Add the tiled layer to map
            myMap.OperationalLayers.Add(tiledLayer);

            // Create the uri for the ArcGISMapImage layer
            var imageLayerUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer");

            // Create ArcGISMapImage layer using a url
            ArcGISMapImageLayer imageLayer = new ArcGISMapImageLayer(imageLayerUri);
            imageLayer.Name = "Image Layer";

            // Set the visible scale range for the image layer
            imageLayer.MinScale = 40000000;
            imageLayer.MaxScale = 2000000;

            // Add the image layer to map
            myMap.OperationalLayers.Add(imageLayer);

            //Create Uri for feature layer
            var featureLayerUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Recreation/FeatureServer/0");

            //Create a feature layer using url
            FeatureLayer myFeatureLayer = new FeatureLayer(featureLayerUri);
            myFeatureLayer.Name = "Feature Layer";

            // Add the feature layer to map
            myMap.OperationalLayers.Add(myFeatureLayer);

            // Create a mappoint the map should zoom to
            MapPoint mapPoint = new MapPoint(-11000000, 4500000, SpatialReferences.WebMercator);

            // Set the initial viewpoint for map
            myMap.InitialViewpoint = new Viewpoint(mapPoint, 50000000);

            // Initialize the model list with unknown status for each layer
            foreach (Layer layer in myMap.OperationalLayers)
            {
                _layerStatusModels.Add(new LayerStatusModel(layer.Name, "Unknown"));
            }

            // Create the tableview source and pass the list of models to it
            _tableView.Source = new LayerViewStatusTableSource(_layerStatusModels);

            // Event for layer view state changed
            _myMapView.LayerViewStateChanged += OnLayerViewStateChanged;

            // Provide used Map to the MapView
            _myMapView.Map = myMap;
        }

        private void OnLayerViewStateChanged(object sender, LayerViewStateChangedEventArgs e)
        {
            // State changed event is sent by a layer. In the list, find the layer which sends this event. 
            // If it exists then update the status
            var model = _layerStatusModels.FirstOrDefault(l => l.LayerName == e.Layer.Name);
            if (model != null)
                model.LayerViewStatus = e.LayerViewState.Status.ToString();

            // Update the table
            _tableView.ReloadData();
        }

        private void CreateLayout()
        {
            nfloat height = 80;

            //set up UIStackView for laying out controls
            var stackView = new UIStackView(new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height));
            stackView.Axis = UILayoutConstraintAxis.Vertical;
            stackView.Alignment = UIStackViewAlignment.Fill;
            stackView.Distribution = UIStackViewDistribution.FillProportionally;
            stackView.BackgroundColor = UIColor.Gray;

            // Setup the visual frame for the MapView
            _myMapView = new MapView()
            {
                Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height-80)
            };

            stackView.AddArrangedSubview(_myMapView);

            // Create a tableview for displaying layer view status for each layer
            _tableView = new UITableView(new CoreGraphics.CGRect(0, _myMapView.Frame.Height, View.Bounds.Width, height));
            stackView.AddArrangedSubview(_tableView);

            // Add MapView to the page
            View.AddSubviews(stackView);
        }
    }

    /// <summary>
    /// Class that is used by the table view to populate itself
    /// </summary>
    internal class LayerViewStatusTableSource: UITableViewSource
    {
        // List of layer status model
        protected List<LayerStatusModel> _layers;

        // Identifier for the table cell
        protected string cellIdentifier = "TableCell";

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
            return _layers != null?_layers.Count:0;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Get the reused cell from the table view
            UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);

            // Get the model at this current cell index
            LayerStatusModel model = _layers[indexPath.Row];

            // If there are no cells to reuse-create one
            // We are specifically using Value1 style so text for both layer name and status can be displayed
            if (cell == null)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Value1, cellIdentifier);
            }

            cell.TextLabel.Text = model.LayerName?? "Layer " + indexPath.Row;
            cell.DetailTextLabel.Text = model.LayerViewStatus;
            return cell;
        }
    }

    /// <summary>
    /// This is a custom class that holds information for layer name and status
    /// </summary>
    internal class LayerStatusModel
    {
        internal string LayerName { get; private set; }
        internal string LayerViewStatus { get; set; }

        public LayerStatusModel(string layerName, string layerStatus)
        {
            LayerName = layerName;
            LayerViewStatus = layerStatus;
        }
    }
}