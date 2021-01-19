// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.RasterLayerRasterFunction
{
    [Register("RasterLayerRasterFunction")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Apply raster function to raster from service",
        category: "Layers",
        description: "Load a raster from a service, then apply a function to it.",
        instructions: "The raster function is applied automatically when the sample starts and the result is displayed.",
        tags: new[] { "function", "layer", "raster", "raster function", "service" })]
    public class RasterLayerRasterFunction : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        public RasterLayerRasterFunction()
        {
            Title = "ArcGIS raster function (service)";
        }

        private async void Initialize()
        {
            // Create new map with the streets basemap.
            Map myMap = new Map(BasemapStyle.ArcGISStreets);

            // Create a URI to the image service raster.
            Uri rasterUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/NLCDLandCover2001/ImageServer");

            // Create new image service raster from the URI.
            ImageServiceRaster imageServiceRaster = new ImageServiceRaster(rasterUri);

            try
            {
                // Load the image service raster.
                await imageServiceRaster.LoadAsync();

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
                const string theJsonString = @"{
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

                // Create a raster function from the JSON string using the static/Shared method called: RasterFunction.FromJson(JSON as String).
                RasterFunction rasterFunction = RasterFunction.FromJson(theJsonString);

                // NOTE: You could have alternatively created the raster function via a JSON string that is contained in a 
                // file on disk (ex: hillshade_simplified.json) via the constructor: Esri.ArcGISRuntime.Rasters.RasterFunction(path as String).

                // Get the raster function arguments.
                RasterFunctionArguments rasterFunctionArguments = rasterFunction.Arguments;

                // Get the list of names from the raster function arguments.
                IReadOnlyList<string> myRasterNames = rasterFunctionArguments.GetRasterNames();

                // Apply the first raster name and image service raster in the raster function arguments.
                rasterFunctionArguments.SetRaster(myRasterNames[0], imageServiceRaster);

                // Create a new raster based on the raster function.
                Raster raster = new Raster(rasterFunction);

                // Create a new raster layer from the raster.
                RasterLayer rasterLayer = new RasterLayer(raster);

                // Add the raster layer to the maps layer collection.
                myMap.Basemap.BaseLayers.Add(rasterLayer);

                // Assign the map to the map view.
                _myMapView.Map = myMap;

                // Get the service information (aka. metadata) about the image service raster.
                ArcGISImageServiceInfo arcGISImageServiceInfo = imageServiceRaster.ServiceInfo;

                // Zoom the map to the extent of the image service raster (which also the extent of the raster layer).
                await _myMapView.SetViewpointGeometryAsync(arcGISImageServiceInfo.FullExtent);
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }
    }
}