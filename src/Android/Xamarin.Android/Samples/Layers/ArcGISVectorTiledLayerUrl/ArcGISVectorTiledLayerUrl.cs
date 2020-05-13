// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcGISRuntime.Samples.ArcGISVectorTiledLayerUrl
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "ArcGIS vector tiled layer URL",
        category: "Layers",
        description: "Load an ArcGIS Vector Tiled Layer from a URL.",
        instructions: "Use the drop down menu to load different vector tile basemaps.",
        tags: new[] { "tiles", "vector", "vector basemap", "vector tiled layer", "vector tiles" })]
    public class ArcGISVectorTiledLayerUrl : Activity
    {
        // Hold a reference to the map view
        private MapView _myMapView;

        // Dictionary associates layer names with URIs
        private readonly Dictionary<string, Uri> _layerUrls = new Dictionary<string, Uri>()
        {
            {"Mid-Century", new Uri("https://www.arcgis.com/home/item.html?id=7675d44bb1e4428aa2c30a9b68f97822")},
            {"Colored Pencil", new Uri("https://www.arcgis.com/home/item.html?id=4cf7e1fb9f254dcda9c8fbadb15cf0f8")},
            {"Newspaper", new Uri("https://www.arcgis.com/home/item.html?id=dfb04de5f3144a80bc3f9f336228d24a")},
            {"Nova", new Uri("https://www.arcgis.com/home/item.html?id=75f4dfdff19e445395653121a95a85db")},
            {"World Street Map (Night)", new Uri("https://www.arcgis.com/home/item.html?id=86f556a2d1fd468181855a35e344567f")}
        };

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "ArcGIS vector tiled Layer";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a new ArcGISVectorTiledLayer with the navigation service URL
            ArcGISVectorTiledLayer vectorTiledLayer = new ArcGISVectorTiledLayer(_layerUrls.Values.First());

            // Create new Map with basemap
            Map myMap = new Map();
            myMap.OperationalLayers.Add(vectorTiledLayer);

            // Provide used Map to the MapView
            _myMapView.Map = myMap;
        }

        private void OnVectorLayersClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            // Create menu to show options
            PopupMenu menu = new PopupMenu(this, button);
            menu.MenuItemClick += OnVectorLayersMenuItemClicked;

            // Create menu options
            foreach (string vectorLayerName in _layerUrls.Keys)
            {
                menu.Menu.Add(vectorLayerName);
            }

            // Show menu in the view
            menu.Show();
        }

        private void OnVectorLayersMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Get title from the selected item
            string selected = e.Item.TitleCondensedFormatted.ToString();

            // Create new ArcGISVectorTiled layer with the selected service Url
            ArcGISVectorTiledLayer vectorTiledLayer = new ArcGISVectorTiledLayer(_layerUrls[selected]);

            // Create a new map with a basemap that was selected. Assign this to the mapview's map
            _myMapView.Map.OperationalLayers.Clear();
            _myMapView.Map.OperationalLayers.Add(vectorTiledLayer);
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create button to show options
            Button button = new Button(this)
            {
                Text = "Select layer"
            };
            button.Click += OnVectorLayersClicked;

            // Add button to the layout
            layout.AddView(button);

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}