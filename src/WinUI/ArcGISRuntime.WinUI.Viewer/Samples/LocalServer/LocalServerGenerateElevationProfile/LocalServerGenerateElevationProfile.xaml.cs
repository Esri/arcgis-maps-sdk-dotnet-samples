// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace ArcGISRuntime.WinUI.Samples.LocalServerGenerateElevationProfile
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Generate elevation profile with Local Server",
        category: "Local Server",
        description: "Create an elevation profile using a geoprocessing package executed with ArcGIS Runtime Local Server.",
        instructions: "The sample loads at the full extent of the raster dataset. Click the \"Draw Polyline\" button and sketch a polyline along where you'd like the elevation profile to be calculated (the polyline can be any shape). Right-click to save the sketch and draw the polyline. Click \"Generate Elevation Profile\" to interpolate the sketched polyline onto the raster surface in 3D. Once ready, the view will automatically zoom onto the newly drawn elevation profile. Click \"Clear Results\" to reset the sample.",
        tags: new[] { "elevation profile", "geoprocessing", "interpolate shape", "local server", "offline", "parameters", "processing", "raster", "raster function", "scene", "service", "terrain" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("db9cd9beedce4e0987c33c198c8dfb45", "259f420250a444b4944a277eec2c4e42")]
    public partial class LocalServerGenerateElevationProfile
    {
        public LocalServerGenerateElevationProfile()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
        }
    }
}