// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using ArcGISRuntime.Samples.Managers;

namespace ArcGISRuntime.Samples.FeatureLayerDictionaryRenderer
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("c78b149a1d52414682c86a5feeb13d30", "e0d41b4b409a49a5a7ba11939d8535dc")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Dictionary renderer with feature layer",
        category: "Layers",
        description: "Convert features into graphics to show them with mil2525d symbols.",
        instructions: "Pan and zoom around the map. Observe the displayed military symbology on the map.",
        tags: new[] { "military", "symbol" })]
    public class FeatureLayerDictionaryRenderer : Activity
    {
        // Hold a reference to the map view
        private MapView _myMapView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Feature layer dictionary renderer";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Provide Map to the MapView
            _myMapView.Map = myMap;

            // Get the path to the geodatabase
            string geodbFilePath = GetGeodatabasePath();

            // Load the geodatabase from local storage
            Geodatabase baseGeodatabase = await Geodatabase.OpenAsync(geodbFilePath);

            // Get the path to the symbol dictionary
            string symbolFilepath = GetStyleDictionaryPath();

            try
            {
                // Load the symbol dictionary from local storage
                DictionarySymbolStyle symbolStyle = await DictionarySymbolStyle.CreateFromFileAsync(symbolFilepath);

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

                // Create geometry for the center of the map
                MapPoint centerGeometry = new MapPoint(-13549402.587055, 4397264.96879385, SpatialReference.Create(3857));

                // Set the map's viewpoint to highlight the desired content
                _myMapView.SetViewpoint(new Viewpoint(centerGeometry, 201555));
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }

        // Get the file path for the style dictionary
        private string GetStyleDictionaryPath()
        {
            return DataManager.GetDataFolder("c78b149a1d52414682c86a5feeb13d30", "mil2525d.stylx");
        }

        // Get the file path for the geodatabase
        private string GetGeodatabasePath()
        {
            return DataManager.GetDataFolder("e0d41b4b409a49a5a7ba11939d8535dc", "militaryoverlay.geodatabase");
        }
    }
}