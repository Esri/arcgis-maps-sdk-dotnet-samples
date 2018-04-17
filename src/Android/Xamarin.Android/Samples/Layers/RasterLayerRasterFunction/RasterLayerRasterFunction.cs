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
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;

namespace ArcGISRuntime.Samples.RasterLayerRasterFunction
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "ArcGIS raster function (service)",
        "Layers",
        "This sample demonstrates how to show a raster layer on a map based on an image service layer that has a raster function applied.",
        "")]
    public class RasterLayerRasterFunction : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "ArcGIS raster function (service)";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Create new map with the streets basemap
            Map myMap = new Map(Basemap.CreateStreets());

            // Create a Uri to the image service raster
            var myUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NLCDLandCover2001/ImageServer");

            // Create new image service raster from the Uri
            ImageServiceRaster myImageServiceRaster = new ImageServiceRaster(myUri);

            // Load the image service raster
            await myImageServiceRaster.LoadAsync();

            // NOTE: This is the ASCII text for actual raw JSON string:
            // ========================================================
            //{
            //  "raster_function_arguments":
            //  {
            //    "z_factor":{"double":25.0,"type":"Raster_function_variable"},
            //    "slope_type":{"raster_slope_type":"none","type":"Raster_function_variable"},
            //    "azimuth":{"double":315,"type":"Raster_function_variable"},
            //    "altitude":{"double":45,"type":"Raster_function_variable"},
            //    "type":"Raster_function_arguments",
            //    "raster":{"name":"raster","is_raster":true,"type":"Raster_function_variable"},
            //    "nbits":{"int":8,"type":"Raster_function_variable"}
            //  },
            //  "raster_function":{"type":"Hillshade_function"},
            //  "type":"Raster_function_template"
            //}

            // Define the JSON string needed for the raster function
            String theJSON_String =
             @"{
                ""raster_function_arguments"":
                {
                  ""z_factor"":{ ""double"":25.0,""type"":""Raster_function_variable""},
                  ""slope_type"":{ ""raster_slope_type"":""none"",""type"":""Raster_function_variable""},
                  ""azimuth"":{ ""double"":315,""type"":""Raster_function_variable""},
                  ""altitude"":{ ""double"":45,""type"":""Raster_function_variable""},
                  ""type"":""Raster_function_arguments"",
                  ""raster"":{ ""name"":""raster"",""is_raster"":true,""type"":""Raster_function_variable""},
                  ""nbits"":{ ""int"":8,""type"":""Raster_function_variable""}
                },
              ""raster_function"":{ ""type"":""Hillshade_function""},
              ""type"":""Raster_function_template""
            }";

            // Create a raster function from the JSON string using the static/Shared method called: RasterFunction.FromJson(json as String)
            RasterFunction myRasterFunction = RasterFunction.FromJson(theJSON_String);

            // NOTE: Depending on your platform/device, you could have alternatively created the raster function via a JSON string that is contained in a 
            // file on disk (ex: hillshade_simplified.json) via the constructor: Esri.ArcGISRuntime.Rasters.RasterFunction(path as String)

            // Get the raster function arguments
            RasterFunctionArguments myRasterFunctionArguments = myRasterFunction.Arguments;

            // Get the list of names from the raster function arguments
            IReadOnlyList<string> myRasterNames = myRasterFunctionArguments.GetRasterNames();

            // Apply the first raster name and image service raster in the raster function arguments
            myRasterFunctionArguments.SetRaster(myRasterNames[0], myImageServiceRaster);

            // Create a new raster based on the raster function
            Raster myRaster = new Raster(myRasterFunction);

            // Create a new raster layer from the raster
            RasterLayer myRasterLayer = new RasterLayer(myRaster);

            // Add the raster layer to the maps layer collection
            myMap.Basemap.BaseLayers.Add(myRasterLayer);

            // Assign the map to the map view
            _myMapView.Map = myMap;

            // Get the service information (aka. metadata) about the image service raster
            ArcGISImageServiceInfo myArcGISImageServiceInfo = myImageServiceRaster.ServiceInfo;

            // Zoom the map to the extent of the image service raster (which also the extent of the raster layer)
            await _myMapView.SetViewpointGeometryAsync(myArcGISImageServiceInfo.FullExtent);

            // NOTE: The sample zooms to the extent of the ImageServiceRaster. Currently the ArcGIS Runtime does not 
            // support zooming a RasterLayer out beyond 4 times it's published level of detail. The sample uses 
            // MapView.SetViewpointCenterAsync() method to ensure the image shows when the app starts. You can see 
            // the effect of the image service not showing when you zoom out to the full extent of the image and beyond.
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}