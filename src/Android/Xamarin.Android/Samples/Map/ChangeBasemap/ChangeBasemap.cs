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

namespace ArcGISRuntimeXamarin.Samples.ChangeBasemap
{
    [Activity]
    public class ChangeBasemap : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // String array to store basemap constructor types
        private string[] _basemapTypes = new string[]
        {
            "Topographic",
            "Streets",
            "Imagery",
            "Oceans"
        };

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Change basemap";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());
            
            // Provide used Map to the MapView
            _myMapView.Map = myMap;
        }

        private void OnBasemapsClicked(object sender, EventArgs e)
        {
            var mapsButton = sender as Button;

            // Create menu to show map options
            var mapsMenu = new PopupMenu(this, mapsButton);
            mapsMenu.MenuItemClick += OnBasemapsMenuItemClicked;

            // Create menu options
            foreach (var basemapType in _basemapTypes)
                mapsMenu.Menu.Add(basemapType);

            // Show menu in the view
            mapsMenu.Show();
        }

        private void OnBasemapsMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Get title from the selected item
            var selectedBasemapType = e.Item.TitleCondensedFormatted.ToString();

            // Get index that is used to get the selected url
            var selectedIndex = _basemapTypes.ToList().IndexOf(selectedBasemapType);

            switch (selectedIndex)
            {
                case 0:
   
                    // Set the basemap to Topographic
                    _myMapView.Map.Basemap = Basemap.CreateTopographic();
                    break;

                case 1:
                
                    // Set the basemap to Streets
                    _myMapView.Map.Basemap = Basemap.CreateStreets();
                    break;

                case 2:
                
                    // Set the basemap to Imagery
                    _myMapView.Map.Basemap = Basemap.CreateImagery();
                    break;

                case 3:
                
                    // Set the basemap to Oceans
                    _myMapView.Map.Basemap = Basemap.CreateOceans();
                    break;
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create button to show possible map options
            var mapsButton = new Button(this);
            mapsButton.Text = "Basemaps";
            mapsButton.Click += OnBasemapsClicked;
    
            // Add maps button to the layout
            layout.AddView(mapsButton);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}