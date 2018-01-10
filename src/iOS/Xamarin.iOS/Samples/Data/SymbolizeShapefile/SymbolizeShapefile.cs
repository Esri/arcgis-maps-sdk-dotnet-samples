﻿// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntimeXamarin.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System.IO;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.SymbolizeShapefile
{
    [Register("SymbolizeShapefile")]
    public class SymbolizeShapefile : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Create and hold a reference to a button
        private UIButton _myRendererButton = new UIButton { Enabled = false };

        // Hold reference to the feature layer so that its renderer can be changed when button is pushed
        private FeatureLayer _shapefileFeatureLayer;

        // Hold reference to default renderer to enable switching back
        private Renderer _defaultRenderer;

        // Hold reference to alternate renderer to enable switching
        private SimpleRenderer _alternateRenderer;

        public SymbolizeShapefile()
        {
            Title = "Symbolize a shapefile";
        }

        private async void Initialize()
        {
            // Create the map with topographic basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create the point for the map's initial viewpoint
            MapPoint point = new MapPoint(-11662054, 4818336, SpatialReference.Create(3857));

            // Create a viewpoint for the point
            Viewpoint viewpoint = new Viewpoint(point, 200000);

            // Set the initial viewpoint
            myMap.InitialViewpoint = viewpoint;

            // Create a shapefile feature table from the shapefile path
            ShapefileFeatureTable myFeatureTable = new ShapefileFeatureTable(await GetShapefilePath());

            // Create a layer from the feature table
            _shapefileFeatureLayer = new FeatureLayer(myFeatureTable);

            // Wait for the layer to load
            await _shapefileFeatureLayer.LoadAsync();

            // Add the layer to the map
            myMap.OperationalLayers.Add(_shapefileFeatureLayer);

            // Create the symbology for the alternate renderer
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 1.0);
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.Yellow, lineSymbol);

            // Create the alternate renderer
            _alternateRenderer = new SimpleRenderer(fillSymbol);

            // Hold a reference to the default renderer (to enable switching between the renderers)
            _defaultRenderer = _shapefileFeatureLayer.Renderer;

            // Add the map to the mapview
            _myMapView.Map = myMap;

            // Enable changing symbology now that sample is loaded
            _myRendererButton.Enabled = true;
        }

        private void CreateLayout()
        {
            // Configure the renderer button
            _myRendererButton.SetTitle("Change Renderer", UIControlState.Normal);
            _myRendererButton.SetTitleColor(UIColor.White, UIControlState.Normal);

            // Add MapView to the page
            View.AddSubviews(_myMapView, _myRendererButton);

            // Subscribe to button press events
            _myRendererButton.TouchUpInside += Button_Clicked;
        }

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            // Toggle the renderer
            if (_shapefileFeatureLayer.Renderer == _defaultRenderer)
            {
                _shapefileFeatureLayer.Renderer = _alternateRenderer;
            }
            else
            {
                _shapefileFeatureLayer.Renderer = _defaultRenderer;
            }
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Set up the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height - 50);

            // Set up the visual frame for the button
            _myRendererButton.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 50, View.Bounds.Width, 50);

            base.ViewDidLayoutSubviews();
        }

        private async Task<string> GetShapefilePath()
        {
            #region offlinedata

            // The desired shapefile is expected to be Subdivisions.shp
            string filename = "Subdivisions.shp";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "SymbolizeShapefile", filename);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the shapefile
                await DataManager.GetData("d98b3e5293834c5f852f13c569930caa", "SymbolizeShapefile");
            }
            return filepath;

            #endregion offlinedata
        }
    }
}