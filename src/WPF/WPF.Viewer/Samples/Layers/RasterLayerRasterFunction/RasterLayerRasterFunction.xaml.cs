// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.RasterLayerRasterFunction
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Apply raster function to raster from service",
        category: "Layers",
        description: "Load a raster from a service, then apply a function to it.",
        instructions: "The raster function is applied automatically when the sample starts and the result is displayed.",
        tags: new[] { "function", "layer", "raster", "raster function", "service" })]
    public partial class RasterLayerRasterFunction
    {
        public RasterLayerRasterFunction()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create new map with the streets basemap
            Map myMap = new Map(BasemapStyle.ArcGISStreets);

            // Create a Uri to the image service raster
            Uri myUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NLCDLandCover2001/ImageServer");

            // Create new image service raster from the Uri
            ImageServiceRaster myImageServiceRaster = new ImageServiceRaster(myUri);

            try
            {
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
                string theJSON_String =
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
                myMap.OperationalLayers.Add(myRasterLayer);

                // Assign the map to the map view
                MyMapView.Map = myMap;

                // Get the service information (aka. metadata) about the image service raster
                ArcGISImageServiceInfo myArcGISImageServiceInfo = myImageServiceRaster.ServiceInfo;

                // Zoom the map to the extent of the image service raster (which also the extent of the raster layer)
                await MyMapView.SetViewpointGeometryAsync(myArcGISImageServiceInfo.FullExtent);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }
    }
}