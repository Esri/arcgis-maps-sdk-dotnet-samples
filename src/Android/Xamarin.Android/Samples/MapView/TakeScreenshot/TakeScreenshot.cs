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

namespace ArcGISRuntimeXamarin.Samples.TakeScreenshot
{
    [Activity]
    public class TakeScreenshot : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Take screenshot";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a new Map instance with the basemap  
            Basemap myBasemap = Basemap.CreateStreets();
            Map myMap = new Map(myBasemap);

            // Assign the map to the MapView
            _myMapView.Map = myMap;
        }

        private async void OnTakeScreenshotClicked(object sender, EventArgs e)
        {
            // Export the image from mapview and assign it to the imageview
            var exportedImage = await _myMapView.ExportImageAsync();

            //var sublayersButton = sender as Button;

            //// Create menu to change sublayer visibility
            //var sublayersMenu = new PopupMenu(this, sublayersButton);
            //sublayersMenu.MenuItemClick += OnSublayersMenuItemClicked;

            //// Create menu options
            //foreach (ArcGISSublayer sublayer in _imageLayer.Sublayers)
            //    sublayersMenu.Menu.Add(sublayer.Name);

            //// Set values to the menu items
            //for (int i = 0; i < sublayersMenu.Menu.Size(); i++)
            //{
            //    var menuItem = sublayersMenu.Menu.GetItem(i);

            //    // Set menu item to contain checkbox
            //    menuItem.SetCheckable(true);
    
            //    // Set default value
            //    menuItem.SetChecked(_imageLayer.Sublayers[i].IsVisible);
            //}

            // Show menu in the view
           // sublayersMenu.Show();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create button to show possible map options
            var takeScreenshotButton = new Button(this);
            takeScreenshotButton.Text = "Capture";
            takeScreenshotButton.Click += OnTakeScreenshotClicked;

            // Add maps button to the layout
            layout.AddView(takeScreenshotButton);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}