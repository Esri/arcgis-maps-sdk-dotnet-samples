// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.RasterLayerImageServiceRaster
{
    [Register("RasterLayerImageServiceRaster")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "ArcGIS raster layer (service)",
        "Layers",
        "This sample demonstrates how to show a raster layer on a map based on an image service layer.",
        "")]
    public class RasterLayerImageServiceRaster : UIViewController
    {
        // Create and hold a reference to the MapView.
        private readonly MapView _myMapView = new MapView();

        public RasterLayerImageServiceRaster()
        {
            Title = "ArcGIS raster layer (service)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

                // Reposition controls.
                _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 0, 0);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private async void Initialize()
        {
            // Create new map with the dark gray canvas basemap.
            Map myMap = new Map(Basemap.CreateDarkGrayCanvasVector());

            // Create a URI to the image service raster.
            Uri uri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NLCDLandCover2001/ImageServer");

            // Create new image service raster from the URI.
            ImageServiceRaster imageServiceRaster = new ImageServiceRaster(uri);

            // Load the image service raster.
            await imageServiceRaster.LoadAsync();

            // Get the service information (aka. metadata) about the image service raster.
            ArcGISImageServiceInfo arcGISImageServiceInfo = imageServiceRaster.ServiceInfo;

            // Create a new raster layer from the image service raster.
            RasterLayer rasterLayer = new RasterLayer(imageServiceRaster);

            // Add the raster layer to the maps layer collection.
            myMap.Basemap.BaseLayers.Add(rasterLayer);

            // Assign the map to the map view.
            _myMapView.Map = myMap;

            // Zoom the map to the extent of the image service raster (which also the extent of the raster layer).
            await _myMapView.SetViewpointGeometryAsync(arcGISImageServiceInfo.FullExtent);
        }

        private void CreateLayout()
        {
            // Add MapView to the page.
            View.AddSubviews(_myMapView);
        }
    }
}