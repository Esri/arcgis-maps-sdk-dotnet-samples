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

namespace ArcGISRuntime.Samples.ChangeBasemap
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change basemap",
        "Map",
        "This sample demonstrates how to dynamically change the basemap displayed in a Map.",
        "")]
    public class ChangeBasemap : Activity
    {
        // Create and hold reference to the used MapView
        private readonly MapView _myMapView = new MapView();

        // Dictionary that associates names with basemaps
        private readonly Dictionary<string, Basemap> _basemapOptions = new Dictionary<string, Basemap>()
        {
            {"Streets (Raster)", Basemap.CreateStreets()},
            {"Streets (Vector)", Basemap.CreateStreetsVector()},
            {"Streets - Night (Vector)", Basemap.CreateStreetsNightVector()},
            {"Imagery (Raster)", Basemap.CreateImagery()},
            {"Imagery with Labels (Raster)", Basemap.CreateImageryWithLabels()},
            {"Imagery with Labels (Vector)", Basemap.CreateImageryWithLabelsVector()},
            {"Dark Gray Canvas (Vector)", Basemap.CreateDarkGrayCanvasVector()},
            {"Light Gray Canvas (Raster)", Basemap.CreateLightGrayCanvas()},
            {"Light Gray Canvas (Vector)", Basemap.CreateLightGrayCanvasVector()},
            {"Navigation (Vector)", Basemap.CreateNavigationVector()},
            {"OpenStreetMap (Raster)", Basemap.CreateOpenStreetMap()}
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
            Map myMap = new Map(_basemapOptions.Values.First());

            // Provide used Map to the MapView
            _myMapView.Map = myMap;
        }

        private void OnBasemapsClicked(object sender, EventArgs e)
        {
            // Get a reference to the button
            var mapsButton = sender as Button;

            // Create menu to show map options
            var mapsMenu = new PopupMenu(this, mapsButton);
            mapsMenu.MenuItemClick += OnBasemapsMenuItemClicked;

            // Create menu options
            foreach (string basemapType in _basemapOptions.Keys)
            {
                mapsMenu.Menu.Add(basemapType);
            }

            // Show menu in the view
            mapsMenu.Show();
        }

        private void OnBasemapsMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Get title from the selected item
            var selectedBasemap = e.Item.TitleCondensedFormatted.ToString();

            // Retrieve the basemap from the dictionary
            _myMapView.Map.Basemap = _basemapOptions[selectedBasemap];
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create button to show map options
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