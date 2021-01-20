// Copyright 2021 Esri.
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
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Change basemap",
        category: "Map",
        description: "Change a map's basemap. A basemap is beneath all layers on a `Map` and is used to provide visual reference for the operational layers.",
        instructions: "Use the drop down menu to select the active basemap from the list of available basemaps.",
        tags: new[] { "basemap", "map" })]
    public class ChangeBasemap : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView;

        // Dictionary that associates names with basemaps.
        private readonly Dictionary<string, Basemap> _basemapOptions = new Dictionary<string, Basemap>()
        {
            {"Streets", new Basemap(BasemapStyle.ArcGISStreets)},
            {"Streets - Night", new Basemap(BasemapStyle.ArcGISStreetsNight)},
            {"Imagery", new Basemap(BasemapStyle.ArcGISImageryStandard)},
            {"Imagery with Labels", new Basemap(BasemapStyle.ArcGISImagery)},
            {"Dark Gray Canvas", new Basemap(BasemapStyle.ArcGISDarkGray)},
            {"Light Gray Canvas", new Basemap(BasemapStyle.ArcGISLightGray)},
            {"Navigation", new Basemap(BasemapStyle.ArcGISNavigation)},
            {"OpenStreetMap", new Basemap(BasemapStyle.OSMStandard)}
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
            Button mapsButton = (Button)sender;

            // Create menu to show map options
            PopupMenu mapsMenu = new PopupMenu(this, mapsButton);
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
            string selectedBasemap = e.Item.TitleCondensedFormatted.ToString();

            // Retrieve the basemap from the dictionary
            _myMapView.Map.Basemap = _basemapOptions[selectedBasemap];
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create button to show map options
            Button mapsButton = new Button(this)
            {
                Text = "Choose basemap"
            };
            mapsButton.Click += OnBasemapsClicked;

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