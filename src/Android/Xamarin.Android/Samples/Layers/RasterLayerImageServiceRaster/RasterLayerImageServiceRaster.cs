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
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntime.Samples.RasterLayerImageServiceRaster
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Raster layer (service)",
        "Layers",
        "Create a raster layer from a raster image service.",
        "Simply launch the sample to see a raster from an image service being used on a map.",
        "image service", "raster")]
    public class RasterLayerImageServiceRaster : Activity
    {
        // Create and hold reference to the used MapView.
        private MapView _myMapView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "ArcGIS raster layer (service)";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new map with the dark gray canvas basemap.
            Map myMap = new Map(Basemap.CreateDarkGrayCanvasVector());

            // Create a Uri to the image service raster.
            Uri myUri = new Uri("https://gis.ngdc.noaa.gov/arcgis/rest/services/bag_hillshades/ImageServer");

            // Create new image service raster from the Uri.
            ImageServiceRaster myImageServiceRaster = new ImageServiceRaster(myUri);

            // Create a new raster layer from the image service raster.
            RasterLayer myRasterLayer = new RasterLayer(myImageServiceRaster);

            // Add the raster layer to the maps layer collection.
            myMap.Basemap.BaseLayers.Add(myRasterLayer);

            // Assign the map to the map view.
            _myMapView.Map = myMap;

            // zoom in to the San Francisco Bay.
            _myMapView.SetViewpointCenterAsync(new MapPoint(-13643095.660131, 4550009.846004, SpatialReferences.WebMercator), 100000);
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}