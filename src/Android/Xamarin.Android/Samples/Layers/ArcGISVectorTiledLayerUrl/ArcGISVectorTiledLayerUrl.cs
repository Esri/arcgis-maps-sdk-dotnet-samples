// Copyright 2016 Esri.
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
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.ArcGISVectorTiledLayerUrl
{
    [Activity]
    public class ArcGISVectorTiledLayerUrl : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        private string _navigationUrl = "https://www.arcgis.com/home/item.html?id=63c47b7177f946b49902c24129b87252";
        private string _streetUrl = "https://www.arcgis.com/home/item.html?id=de26a3cf4cc9451298ea173c4b324736";
        private string _nightUrl = "https://www.arcgis.com/home/item.html?id=86f556a2d1fd468181855a35e344567f";
        private string _darkGrayUrl = "https://www.arcgis.com/home/item.html?id=5e9b3685f4c24d8781073dd928ebda50";

        private string _vectorTiledLayerUrl;
        private ArcGISVectorTiledLayer _vectorTiledLayer;

        // String array to store some vector layer names.
        private string[] _vectorLayerNames = new string[]
        {
            "Dark gray",
            "Streets",
            "Night",
            "Navigation"
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
            // Create a new ArcGISVectorTiledLayer with the navigation service Url
            _vectorTiledLayer = new ArcGISVectorTiledLayer(new Uri(_navigationUrl));

            // Create new Map with basemap
            Map myMap = new Map(new Basemap(_vectorTiledLayer));

            // Provide used Map to the MapView
            _myMapView.Map = myMap;
        }

        private void OnVectorLayersClicked(object sender, EventArgs e)
        {
            var button = sender as Button;

            // Create menu to show options
            var menu = new PopupMenu(this, button);
            menu.MenuItemClick += OnVectorLayersMenuItemClicked;

            // Create menu options
            foreach (var vectorLayerName in _vectorLayerNames)
                menu.Menu.Add(vectorLayerName);

            // Show menu in the view
            menu.Show();
        }

        private void OnVectorLayersMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Get title from the selected item
            var selected = e.Item.TitleCondensedFormatted.ToString();

            // Get index that is used to get the selected url
            var selectedIndex = _vectorLayerNames.ToList().IndexOf(selected);

            switch (selectedIndex)
            {
                case 0:
                    _vectorTiledLayerUrl = _darkGrayUrl;
                    break;

                case 1:
                    _vectorTiledLayerUrl = _streetUrl;
                    break;

                case 2:
                    _vectorTiledLayerUrl = _nightUrl;
                    break;

                case 3:
                    _vectorTiledLayerUrl = _navigationUrl;
                    break;
            }

            // Create new ArcGISVectorTiled layer with the selected service Url 
            _vectorTiledLayer = new ArcGISVectorTiledLayer(new Uri(_vectorTiledLayerUrl));

            // Create a new map with a basemap that was selected. Assign this to the mapview's map 
            _myMapView.Map = new Map(new Basemap(_vectorTiledLayer));
        }

 
        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create button to show possible options
            var button = new Button(this);
            button.Text = "Select layer";
            button.Click += OnVectorLayersClicked;

            // Add button to the layout
            layout.AddView(button);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}