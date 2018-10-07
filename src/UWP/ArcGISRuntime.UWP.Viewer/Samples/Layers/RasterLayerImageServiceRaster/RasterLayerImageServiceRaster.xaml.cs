// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using System;

namespace ArcGISRuntime.UWP.Samples.RasterLayerImageServiceRaster
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "ArcGIS raster layer (service)",
        "Layers",
        "Add a raster layer from an image service to a map.",
        "")]
    public partial class RasterLayerImageServiceRaster
    {
        public RasterLayerImageServiceRaster()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Create new map with the dark gray canvas basemap.
            Map myMap = new Map(Basemap.CreateDarkGrayCanvasVector());

            // Create a Uri to the image service raster.
            Uri myUri = new Uri("https://gis.ngdc.noaa.gov/arcgis/rest/services/bag_hillshades/ImageServer");

            // Create new image service raster from the Uri.
            ImageServiceRaster myImageServiceRaster = new ImageServiceRaster(myUri);

            // Load the image service raster.
            await myImageServiceRaster.LoadAsync();

            // Create a new raster layer from the image service raster.
            RasterLayer myRasterLayer = new RasterLayer(myImageServiceRaster);

            // Add the raster layer to the maps layer collection.
            myMap.Basemap.BaseLayers.Add(myRasterLayer);

            // Assign the map to the map view.
            MyMapView.Map = myMap;

            // zoom in to the San Francisco Bay.
            await MyMapView.SetViewpointCenterAsync(new MapPoint(-13643095.660131, 4550009.846004, SpatialReferences.WebMercator), 100000);
        }
    }
}
