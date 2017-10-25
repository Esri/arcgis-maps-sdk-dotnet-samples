﻿// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.RasterRenderingRule
{
    public partial class RasterRenderingRule
    {
        public RasterRenderingRule()
        {
            InitializeComponent();

            // Setup the control references and execute initialization
            Initialize();
        }

        // Create an empty read-only list for the various rendering rules of the image service raster
        private IReadOnlyList<RenderingRuleInfo> _myReadOnlyListRenderRuleInfos;

        // Create a Uri for the image server
        private Uri _myUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/CharlotteLAS/ImageServer");

        private async void Initialize()
        {
            // Assign a new map to the MapView
            MyMapView.Map = new Map();

            // Set the basemap to Streets
            MyMapView.Map.Basemap = Basemap.CreateStreets();

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
            await MyMapView.SetViewpointAsync(myViewPoint);

            // Get the rendering rule info (i.e. definitions of how the image should be drawn) info from the image service raster
            _myReadOnlyListRenderRuleInfos = myArcGISImageServiceInfo.RenderingRuleInfos;

            // Loop through each rendering rule info
            foreach (RenderingRuleInfo myRenderingRuleInfo in _myReadOnlyListRenderRuleInfos)
            {
                // Get the name of the rendering rule info
                string myRenderingRuleName = myRenderingRuleInfo.Name;

                // Add the name of the rendering rule info to the combo box
                comboBox_RenderingRuleChooser.Items.Add(myRenderingRuleName);
            }

            // Set the combo box index to the first rendering rule info name
            comboBox_RenderingRuleChooser.SelectedIndex = 0;
        }

        /// <summary>
        /// Called when a rendering rule info name is changed in the combo box
        /// </summary>
        private void comboBox_RenderingRuleChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Loop through each rendering rule info in the image service raster
            foreach (RenderingRuleInfo myRenderingRuleInfo in _myReadOnlyListRenderRuleInfos)
            {
                // Get the name of the rendering rule info
                string myRenderingRuleName = myRenderingRuleInfo.Name;

                // If the name of the rendering rule info matches what was chosen in the combo box, proceed
                if (myRenderingRuleName == (string)comboBox_RenderingRuleChooser.SelectedItem)
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
                    MyMapView.Map.OperationalLayers.Add(myRasterLayer);
                }
            }
        }
    }
}