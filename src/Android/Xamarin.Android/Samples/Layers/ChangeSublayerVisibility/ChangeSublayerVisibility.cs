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

namespace ArcGISRuntime.Samples.ChangeSublayerVisibility
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Map image layer sublayer visibility",
        category: "Layers",
        description: "Change the visibility of sublayers.",
        instructions: "Each sublayer has a check box which can be used to toggle the visibility of the sublayer.",
        tags: new[] { "layers", "sublayers", "visibility" })]
    public class ChangeSublayerVisibility : Activity
    {
        // Hold a reference to the map view
        private MapView _myMapView;

        // Hold reference to image layer for easier access to sublayers 
        private ArcGISMapImageLayer _imageLayer;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Change sublayer visibility";
            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map
            Map myMap = new Map();

            // Create uri to the map image layer
            Uri serviceUri = new Uri(
               "https://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer");

            // Create new image layer from the url
            _imageLayer = new ArcGISMapImageLayer(serviceUri)
            {
                Name = "World Cities Population"
            };

            // Add created layer to the basemaps collection
            myMap.Basemap.BaseLayers.Add(_imageLayer);

            // Assign the map to the MapView
            _myMapView.Map = myMap;
        }

        private async void OnSublayersClicked(object sender, EventArgs e)
        {
            try
            {
                // Make sure that layer and it's sublayers are loaded
                // If layer is already loaded, this returns directly
                await _imageLayer.LoadAsync();

                Button sublayersButton = (Button)sender;

                // Create menu to change sublayer visibility
                PopupMenu sublayersMenu = new PopupMenu(this, sublayersButton);
                sublayersMenu.MenuItemClick += OnSublayersMenuItemClicked;

                // Create menu options
                foreach (ArcGISSublayer sublayer in _imageLayer.Sublayers)
                    sublayersMenu.Menu.Add(sublayer.Name);

                // Set values to the menu items
                for (int i = 0; i < sublayersMenu.Menu.Size(); i++)
                {
                    IMenuItem menuItem = sublayersMenu.Menu.GetItem(i);

                    // Set menu item to contain checkbox
                    menuItem.SetCheckable(true);
    
                    // Set default value
                    menuItem.SetChecked(_imageLayer.Sublayers[i].IsVisible);
                }

                // Show menu in the view
                sublayersMenu.Show();
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private void OnSublayersMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Change the checked value
            e.Item.SetChecked(!e.Item.IsChecked);

            // Get title from the selected item
            string selectedSublayerTitle = e.Item.TitleCondensedFormatted.ToString();

            // Get index that is used to get the selected url
            ArcGISSublayer sublayer = _imageLayer.Sublayers.First(x => x.Name == selectedSublayerTitle);
            sublayer.IsVisible = e.Item.IsChecked;
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create button to show possible map options
            Button mapsButton = new Button(this)
            {
                Text = "Sublayers"
            };
            mapsButton.Click += OnSublayersClicked;

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