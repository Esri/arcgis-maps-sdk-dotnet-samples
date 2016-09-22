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
using Esri.ArcGISRuntime.UI;
using System;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.OpenExistingMap
{
    [Activity]
    public class OpenExistingMap : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // String array to hold urls to publicly available web maps
        private string[] itemURLs = new string[]
        {
            "http://www.arcgis.com/home/item.html?id=2d6fa24b357d427f9c737774e7b0f977",
            "http://www.arcgis.com/home/item.html?id=01f052c8995e4b9e889d73c3e210ebe3",
            "http://www.arcgis.com/home/item.html?id=74a8f6645ab44c4f82d537f1aa0e375d"
        };

        // String array to store titles for the webmaps specified above. These titles are in the same order as the urls above
        private string[] titles = new string[]
        {
            "Housing with Mortgages",
            "USA Tapestry Segmentation",
            "Geology of United States"
        };

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Open an existing map";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a new Map instance with url of the webmap that is displayed by default
            Map myMap = new Map(new Uri(itemURLs[0]));

            // Provide used Map to the MapView
            _myMapView.Map = myMap;
        }

        private void OnMapsClicked(object sender, EventArgs e)
        {
            var mapsButton = sender as Button;

            // Create menu to show map options
            var mapsMenu = new PopupMenu(this, mapsButton);
            mapsMenu.MenuItemClick += OnMapsMenuItemClicked;

            // Create menu options
            foreach (var title in titles)
                mapsMenu.Menu.Add(title);

            // Show menu in the view
            mapsMenu.Show();
        }

        private void OnMapsMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Get title from the selected item
            var selectedMapTitle = e.Item.TitleCondensedFormatted.ToString();

            // Get index that is used to get the selected url
            var selectedIndex = titles.ToList().IndexOf(selectedMapTitle);

            // Create a new Map instance with url of the webmap that selected
            _myMapView.Map = new Map(new Uri(itemURLs[selectedIndex]));
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create button to show possible map options
            var mapsButton = new Button(this);
            mapsButton.Text = "Maps";
            mapsButton.Click += OnMapsClicked;

            // Add maps button to the layout
            layout.AddView(mapsButton);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}