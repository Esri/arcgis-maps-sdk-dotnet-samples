// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using ArcGISRuntime.Samples.Managers;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureLayerShapefile
{
    [Register("FeatureLayerShapefile")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("d98b3e5293834c5f852f13c569930caa")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer (shapefile)",
        "Data",
        "This sample demonstrates how to open a shapefile stored on the device and display it as a feature layer with default symbology.",
        "The shapefile will be downloaded from an ArcGIS Online portal automatically.",
        "Featured")]
    public class FeatureLayerShapefile : UIViewController
    {
        // Create a MapView control to display a map
        private MapView _myMapView = new MapView();

        public FeatureLayerShapefile()
        {
            Title = "Feature layer (shapefile)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();

            // Download (if necessary) and add a local shapefile dataset to the map
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // Update the UI to account for new layout
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
        }

        private async void Initialize()
        {
            // Create a new map to display in the map view with a streets basemap
            _myMapView.Map = new Map(Basemap.CreateStreetsVector());

            // Get the path to the downloaded shapefile
            string filepath = GetShapefilePath();

            // Open the shapefile
            ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(filepath);

            // Create a feature layer to display the shapefile
            FeatureLayer newFeatureLayer = new FeatureLayer(myShapefile);

            // Add the feature layer to the map
            _myMapView.Map.OperationalLayers.Add(newFeatureLayer);

            // Zoom the map to the extent of the shapefile
            await _myMapView.SetViewpointGeometryAsync(newFeatureLayer.FullExtent);
        }

        private static string GetShapefilePath()
        {
            return DataManager.GetDataFolder("d98b3e5293834c5f852f13c569930caa", "Public_Art.shp");
        }

        private void CreateLayout()
        {
            // Add MapView to the page
            View.AddSubview(_myMapView);
        }
    }
}