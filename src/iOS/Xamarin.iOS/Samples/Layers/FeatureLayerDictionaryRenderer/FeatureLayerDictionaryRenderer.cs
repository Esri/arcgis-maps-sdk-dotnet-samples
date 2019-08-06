// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureLayerDictionaryRenderer
{
    [Register("FeatureLayerDictionaryRenderer")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("e34835bf5ec5430da7cf16bb8c0b075c", "e0d41b4b409a49a5a7ba11939d8535dc")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer dictionary renderer",
        "Layers",
        "Demonstrates how to apply a dictionary renderer to a feature layer and display mil2525d graphics. The dictionary renderer creates these graphics using a mil2525d style file and the attributes attached to each feature within the geodatabase.",
        "",
        "Military", "Symbology", "Military symbology")]
    public class FeatureLayerDictionaryRenderer : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        public FeatureLayerDictionaryRenderer()
        {
            Title = "Feature layer dictionary renderer";
        }

        private async void Initialize()
        {
            // Create new Map with basemap.
            Map map = new Map(Basemap.CreateTopographic());

            // Provide Map to the MapView.
            _myMapView.Map = map;

            // Get the path to the geodatabase.
            string geodbFilePath = DataManager.GetDataFolder("e0d41b4b409a49a5a7ba11939d8535dc", "militaryoverlay.geodatabase");

            // Load the geodatabase from local storage.
            Geodatabase baseGeodatabase = await Geodatabase.OpenAsync(geodbFilePath);

            // Get the path to the symbol dictionary.
            string symbolFilepath = DataManager.GetDataFolder("e34835bf5ec5430da7cf16bb8c0b075c", "mil2525d.stylx");

            try
            {
                // Load the symbol dictionary from local storage.
                //     Note that the type of the symbol definition must be explicitly provided along with the file name.
                DictionarySymbolStyle symbolStyle = await DictionarySymbolStyle.OpenAsync("mil2525d", symbolFilepath);

                // Add geodatabase features to the map, using the defined symbology.
                foreach (GeodatabaseFeatureTable table in baseGeodatabase.GeodatabaseFeatureTables)
                {
                    // Load the table.
                    await table.LoadAsync();

                    // Create the feature layer from the table.
                    FeatureLayer layer = new FeatureLayer(table);

                    // Load the layer.
                    await layer.LoadAsync();

                    // Create and use a Dictionary Renderer using the DictionarySymbolStyle.
                    layer.Renderer = new DictionaryRenderer(symbolStyle);

                    // Add the layer to the map.
                    map.OperationalLayers.Add(layer);
                }

                // Create geometry for the center of the map.
                MapPoint centerGeometry = new MapPoint(-13549402.587055, 4397264.96879385, SpatialReference.Create(3857));

                // Set the map's viewpoint to highlight the desired content.
                _myMapView.SetViewpoint(new Viewpoint(centerGeometry, 201555));
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView();

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }
    }
}