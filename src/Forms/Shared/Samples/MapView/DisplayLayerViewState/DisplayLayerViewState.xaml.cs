// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.DisplayLayerViewState
{
    public partial class DisplayLayerViewState : ContentPage
    {
        // Reference to list of view status for each layer
        private List<LayerStatusModel> _layerStatusModels = new List<LayerStatusModel>();

        public DisplayLayerViewState()
        {
            InitializeComponent();

            Title = "Display layer view state";

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map
            Map myMap = new Map();

            // Create the uri for the tiled layer
            var tiledLayerUri = new Uri(
                "http://sampleserver6.arcgisonline.com/arcgis/rest/services/WorldTimeZones/MapServer");

            // Create a tiled layer using url
            ArcGISTiledLayer tiledLayer = new ArcGISTiledLayer(tiledLayerUri);
            tiledLayer.Name = "Tiled Layer";

            // Add the tiled layer to map
            myMap.OperationalLayers.Add(tiledLayer);

            // Create the uri for the ArcGISMapImage layer
            var imageLayerUri = new Uri(
                "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer");

            // Create ArcGISMapImage layer using a url
            ArcGISMapImageLayer imageLayer = new ArcGISMapImageLayer(imageLayerUri);
            imageLayer.Name = "Image Layer";

            // Set the visible scale range for the image layer
            imageLayer.MinScale = 40000000;
            imageLayer.MaxScale = 2000000;

            // Add the image layer to map
            myMap.OperationalLayers.Add(imageLayer);

            // Create Uri for feature layer
            var featureLayerUri = new Uri(
                "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Recreation/FeatureServer/0");

            // Create a feature layer using url
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

            // Set models list as a itemssource
            layerStatusListView.ItemsSource = _layerStatusModels;

            // Event for layer view state changed
            MyMapView.LayerViewStateChanged += OnLayerViewStateChanged;

            // Provide used Map to the MapView
            MyMapView.Map = myMap;
        }

        private void OnLayerViewStateChanged(object sender, LayerViewStateChangedEventArgs e)
        {
            // State changed event is sent by a layer. In the list, find the layer which sends this event. 
            // If it exists then update the status
            var model = _layerStatusModels.FirstOrDefault(l => l.LayerName == e.Layer.Name);
            if (model != null)
                model.LayerViewStatus = e.LayerViewState.Status.ToString();
        }

        /// <summary>
        /// This is a custom class that holds information for layer name and status
        /// </summary>
        public class LayerStatusModel : INotifyPropertyChanged
        {
            private string layerViewStatus;

            public string LayerName { get; private set; }

            public string LayerViewStatus
            {
                get { return layerViewStatus; }
                set { layerViewStatus = value; NotifyPropertyChanged(); }
            }

            public LayerStatusModel(string layerName, string layerStatus)
            {
                LayerName = layerName;
                LayerViewStatus = layerStatus;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}
