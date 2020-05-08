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

namespace ArcGISRuntime.Samples.OpenMapURL
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Open map URL",
        category: "Map",
        description: "Display a web map.",
        instructions: "A web map can be selected from the drop-down list. On selection the web map displays in the map view.",
        tags: new[] { "portal item", "web map" })]
    public class OpenMapURL : Activity
    {
        // Hold a reference to the map view
        private MapView _myMapView;

        // String array to hold urls to publicly available web maps
        private string[] itemURLs = {
            "https://www.arcgis.com/home/item.html?id=392451c381ad4109bf04f7bd442bc038",
            "https://www.arcgis.com/home/item.html?id=01f052c8995e4b9e889d73c3e210ebe3",
            "https://www.arcgis.com/home/item.html?id=92ad152b9da94dee89b9e387dfe21acd"
        };

        // String array to store titles for the webmaps specified above. These titles are in the same order as the urls above
        private string[] titles = {
            "Population Pressure",
            "USA Tapestry Segmentation",
            "Geology of United States"
        };

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Open map (URL)";

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
            Button mapsButton = (Button)sender;

            // Create menu to show map options
            PopupMenu mapsMenu = new PopupMenu(this, mapsButton);
            mapsMenu.MenuItemClick += OnMapsMenuItemClicked;

            // Create menu options
            foreach (string title in titles)
                mapsMenu.Menu.Add(title);

            // Show menu in the view
            mapsMenu.Show();
        }

        private void OnMapsMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Get title from the selected item
            string selectedMapTitle = e.Item.TitleCondensedFormatted.ToString();

            // Get index that is used to get the selected url
            int selectedIndex = titles.ToList().IndexOf(selectedMapTitle);

            // Create a new Map instance with url of the webmap that selected
            _myMapView.Map = new Map(new Uri(itemURLs[selectedIndex]));
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create button to show possible map options
            Button mapsButton = new Button(this)
            {
                Text = "Maps"
            };
            mapsButton.Click += OnMapsClicked;

            // Add maps button to the layout
            layout.AddView(mapsButton);

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}