// Copyright 2017 Esri.
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
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System.IO;
using ArcGISRuntime.Samples.Managers;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureLayerDictionaryRenderer
{
    [Register("FeatureLayerDictionaryRenderer")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("e34835bf5ec5430da7cf16bb8c0b075c","e0d41b4b409a49a5a7ba11939d8535dc")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer dictionary renderer",
        "Layers",
        "Demonstrates how to apply a dictionary renderer to a feature layer and display mil2525d graphics. The dictionary renderer creates these graphics using a mil2525d style file and the attributes attached to each feature within the geodatabase.",
        "")]
    public class FeatureLayerDictionaryRenderer : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        public FeatureLayerDictionaryRenderer()
        {
            Title = "Feature layer dictionary renderer";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
        }

        private void CreateLayout()
        {
            // Create MapView
            _myMapView = new MapView();

            // Add MapView to the page
            View.AddSubviews(_myMapView);
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Provide Map to the MapView
            _myMapView.Map = myMap;

            // Create geometry for the center of the map
            MapPoint centerGeometry = new MapPoint(-13549402.587055, 4397264.96879385, SpatialReference.Create(3857));

            // Set the map's viewpoint to highlight the desired content
            await _myMapView.SetViewpointAsync(new Viewpoint(centerGeometry, 201555));

            // Get the path to the geodatabase
            string geodbFilePath = GetGeodatabasePath();

            // Load the geodatabase from local storage
            Geodatabase baseGeodatabase = await Geodatabase.OpenAsync(geodbFilePath);

            // Get the path to the symbol dictionary
            string symbolFilepath = GetStyleDictionaryPath();

            // Load the symbol dictionary from local storage
            //     Note that the type of the symbol definition must be explicitly provided along with the file name
            DictionarySymbolStyle symbolStyle = await DictionarySymbolStyle.OpenAsync("mil2525d", symbolFilepath);

            // Add geodatabase features to the map, using the defined symbology
            foreach (FeatureTable table in baseGeodatabase.GeodatabaseFeatureTables)
            {
                // Load the table
                await table.LoadAsync();

                // Create the feature layer from the table
                FeatureLayer myLayer = new FeatureLayer(table);

                // Load the layer
                await myLayer.LoadAsync();

                // Create a Dictionary Renderer using the DictionarySymbolStyle
                DictionaryRenderer dictRenderer = new DictionaryRenderer(symbolStyle);

                // Apply the dictionary renderer to the layer
                myLayer.Renderer = dictRenderer;

                // Add the layer to the map
                myMap.OperationalLayers.Add(myLayer);
            }
        }

        // Get the file path for the style dictionary
        private string GetStyleDictionaryPath()
        {
            return DataManager.GetDataFolder("e34835bf5ec5430da7cf16bb8c0b075c", "mil2525d.stylx");
        }

        // Get the file path for the geodatabase
        private string GetGeodatabasePath()
        {
            return DataManager.GetDataFolder("e0d41b4b409a49a5a7ba11939d8535dc", "militaryoverlay.geodatabase");
        }
    }
}