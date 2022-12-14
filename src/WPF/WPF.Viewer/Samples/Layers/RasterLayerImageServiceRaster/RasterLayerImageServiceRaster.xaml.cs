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

namespace ArcGIS.WPF.Samples.RasterLayerImageServiceRaster
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Raster layer (service)",
        category: "Layers",
        description: "Create a raster layer from a raster image service.",
        instructions: "Simply launch the sample to see a raster from an image service being used on a map.",
        tags: new[] { "image service", "raster" })]
    public partial class RasterLayerImageServiceRaster
    {
        public RasterLayerImageServiceRaster()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create new map with the dark gray canvas basemap.
            Map myMap = new Map(BasemapStyle.ArcGISDarkGray);

            // Create a Uri to the image service raster.
            Uri myUri = new Uri("https://gis.ngdc.noaa.gov/arcgis/rest/services/bag_hillshades_subsets/ImageServer");

            // Create new image service raster from the Uri.
            ImageServiceRaster myImageServiceRaster = new ImageServiceRaster(myUri);

            // Create a new raster layer from the image service raster.
            RasterLayer myRasterLayer = new RasterLayer(myImageServiceRaster);

            // Add the raster layer to the maps layer collection.
            myMap.OperationalLayers.Add(myRasterLayer);

            // Assign the map to the map view.
            MyMapView.Map = myMap;

            // Zoom in to the San Francisco Bay.
            MyMapView.SetViewpointCenterAsync(new MapPoint(-13643095.660131, 4550009.846004, SpatialReferences.WebMercator), 100000);
        }
    }
}