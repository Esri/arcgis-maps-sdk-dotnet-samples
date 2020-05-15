// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;

namespace ArcGISRuntime.Samples.WMTSLayer
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "WMTS layer",
        category: "Layers",
        description: "Display a layer from a Web Map Tile Service.",
        instructions: "The layer will be displayed automatically. Use the buttons to choose a different method of loading the layer.",
        tags: new[] { "OGC", "layer", "raster", "tiled", "web map tile service" })]
    public class WMTSLayer : Activity
    {
        // Hold a reference to the map view
        private MapView _myMapView;

        // Button for loading layer using Uri constructor.
        private Button _uriButton;

        // Button for loading layer using WmtsService.
        private Button _infoButton;
        

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "WMTS layer";

            // Create the UI, setup the control references
            CreateLayout();

            // Load the map using Uri to the WMTS service.
            LoadWMTSLayer(true);
        }

        private void UriButton_Clicked(object sender, EventArgs e)
        {
            //Load the WMTS layer using Uri method.
            LoadWMTSLayer(true);

            // Disable and enable the appropriate buttons.
            _uriButton.Enabled = false;
            _infoButton.Enabled = true;
        }

        private void InfoButton_Clicked(object sender, EventArgs e)
        {
            //Load the WMTS layer using layer info.
            LoadWMTSLayer(false);

            // Disable and enable the appropriate buttons.
            _uriButton.Enabled = true;
            _infoButton.Enabled = false;
        }

        private async void LoadWMTSLayer(bool uriMode)
        {
            try
            {
                // Create a new map.
                Map myMap = new Map();

                // Get the basemap from the map.
                Basemap myBasemap = myMap.Basemap;

                // Get the layer collection for the base layers.
                LayerCollection myLayerCollection = myBasemap.BaseLayers;

                // Create an instance for the WMTS layer.
                WmtsLayer myWmtsLayer;

                // Define the Uri to the WMTS service.
                Uri wmtsUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/WorldTimeZones/MapServer/WMTS");

                if (uriMode)
                {
                    // Create a WMTS layer using a Uri and provide an Id value.
                    myWmtsLayer = new WmtsLayer(wmtsUri, "WorldTimeZones");
                }
                else
                {
                    // Define a new instance of the WMTS service.
                    WmtsService myWmtsService = new WmtsService(wmtsUri);

                    // Load the WMTS service.
                    await myWmtsService.LoadAsync();

                    // Get the service information (i.e. metadata) about the WMTS service.
                    WmtsServiceInfo myWmtsServiceInfo = myWmtsService.ServiceInfo;

                    // Obtain the read only list of WMTS layer info objects.
                    IReadOnlyList<WmtsLayerInfo> myWmtsLayerInfos = myWmtsServiceInfo.LayerInfos;

                    // Create a WMTS layer using the first item in the read only list of WMTS layer info objects.
                    myWmtsLayer = new WmtsLayer(myWmtsLayerInfos[0]);
                }

                // Add the WMTS layer to the layer collection of the map.
                myLayerCollection.Add(myWmtsLayer);

                // Assign the map to the MapView.
                _myMapView.Map = myMap;
            }
            catch (Exception ex)
            {
                AlertDialog alert = new AlertDialog.Builder(this).Create();
                alert.SetMessage(ex.Message);
                alert.Show();
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create Button
            _uriButton = new Button(this)
            {
                Text = "WMTSLayer via Uri"
            };
            _uriButton.Click += UriButton_Clicked;
            _uriButton.Enabled = false;

            // Add Button to the layout
            layout.AddView(_uriButton);

            // Create Button
            _infoButton = new Button(this)
            {
                Text = "WMTSLayer via WmtsLayerInfo"
            };
            _infoButton.Click += InfoButton_Clicked;

            // Add Button to the layout
            layout.AddView(_infoButton);

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}