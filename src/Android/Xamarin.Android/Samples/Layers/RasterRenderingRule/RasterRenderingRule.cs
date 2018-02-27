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
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;

namespace ArcGISRuntime.Samples.RasterRenderingRule
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Raster rendering rule",
        "Layers",
        "This sample demonstrates how to create an `ImageServiceRaster`, fetch the `RenderingRule`s from the service info, and use a `RenderingRule` to create an `ImageServiceRaster` and add it to a raster layer.",
        "")]
    public class RasterRenderingRule : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Create a empty read-only list for the various rendering rules of the image service raster
        private IReadOnlyList<RenderingRuleInfo> _myReadOnlyListRenderRuleInfos;

        // Create the Uri for the image server
        private Uri _myUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/CharlotteLAS/ImageServer");

        // Create a list to store the names of the rendering rule info for the image service raster
        private List<string> _names = new List<string>();

        // Hold a reference to a button for the user to change the rendering rule for the image service raster
        private Button _renderingRulesButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Raster rendering rule";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Assign a new map to the MapView
            _myMapView.Map = new Map();

            // Set the basemap to Streets
            _myMapView.Map.Basemap = Basemap.CreateStreets();

            // Create a new image service raster from the Uri
            ImageServiceRaster myImageServiceRaster = new ImageServiceRaster(_myUri);

            // Load the image service raster
            await myImageServiceRaster.LoadAsync();

            // Get the ArcGIS image service info (metadata) from the image service raster
            ArcGISImageServiceInfo myArcGISImageServiceInfo = myImageServiceRaster.ServiceInfo;

            // Get the full extent envelope of the image service raster (the Charlotte, NC area)
            Envelope myEnvelope = myArcGISImageServiceInfo.FullExtent;

            // Define a new view point from the full extent envelope
            Viewpoint myViewPoint = new Viewpoint(myEnvelope);

            // Zoom to the area of the full extent envelope of the image service raster
            await _myMapView.SetViewpointAsync(myViewPoint);

            // Get the rendering rule info (i.e. definitions of how the image should be drawn) info from the image service raster
            _myReadOnlyListRenderRuleInfos = myArcGISImageServiceInfo.RenderingRuleInfos;

            // Loop through each rendering rule info
            foreach (RenderingRuleInfo myRenderingRuleInfo in _myReadOnlyListRenderRuleInfos)
            {
                // Get the name of the rendering rule info
                string myRenderingRuleName = myRenderingRuleInfo.Name;

                // Add the name of the rendering rule info to the list of names
                _names.Add(myRenderingRuleName);
            }

            // Invoke the button for the user to change the rendering rule of the image service raster
            OnChangeRenderingRuleClicked(_renderingRulesButton, null);
        }

        private void OnChangeRenderingRuleClicked(object sender, EventArgs e)
        {
            // Get the rendering rule button
            Button renderingRuleButton = sender as Button;

            // Create menu to show the rendering rule options
            PopupMenu renderingRuleMenu = new PopupMenu(this, renderingRuleButton);
            renderingRuleMenu.MenuItemClick += OnChangeRenderingRuleMenuItemClicked;

            // Create menu options
            foreach (String renderingRuleName in _names)
            {
                renderingRuleMenu.Menu.Add(renderingRuleName);
            }

            // Show menu in the view
            renderingRuleMenu.Show();
        }

        private void OnChangeRenderingRuleMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Get the rendering rule from the selected item
            String selectedRenderingRuleType = e.Item.TitleCondensedFormatted.ToString();

            // Loop through each rendering rule info in the image service raster
            foreach (RenderingRuleInfo myRenderingRuleInfo in _myReadOnlyListRenderRuleInfos)
            {
                // Get the name of the rendering rule info
                string myRenderingRuleName = myRenderingRuleInfo.Name;

                // If the name of the rendering rule info matches what was chosen by the user, proceed
                if (myRenderingRuleName == selectedRenderingRuleType)
                {
                    // Create a new rendering rule from the rendering rule info
                    RenderingRule myRenderingRule = new RenderingRule(myRenderingRuleInfo);

                    // Create a new image service raster
                    ImageServiceRaster myImageServiceRaster = new ImageServiceRaster(_myUri);

                    // Set the image service raster's rendering rule to the rendering rule created earlier
                    myImageServiceRaster.RenderingRule = myRenderingRule;

                    // Create a new raster layer from the image service raster
                    RasterLayer myRasterLayer = new RasterLayer(myImageServiceRaster);

                    // Add the raster layer to the operational layers of the  map view
                    _myMapView.Map.OperationalLayers.Add(myRasterLayer);
                }
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create button to show possible rendering rule options
            _renderingRulesButton = new Button(this);
            _renderingRulesButton.Text = "Change Rendering Rule";
            _renderingRulesButton.Click += OnChangeRenderingRuleClicked;

            // Add rendering rule button to the layout
            layout.AddView(_renderingRulesButton);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}