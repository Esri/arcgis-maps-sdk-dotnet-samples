// Copyright 2021 Esri.
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

namespace ArcGISRuntime.WinUI.Samples.ApplyMosaicRule
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Apply mosaic rule to rasters",
        category: "Layers",
        description: "Apply mosaic rule to a mosaic dataset of rasters.",
        instructions: "When the rasters are loaded, choose from a list of preset mosaic rules to apply to the rasters.",
        tags: new[] { "image service", "mosaic method", "mosaic rule", "raster" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class ApplyMosaicRule
    {
        public ApplyMosaicRule()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
        }
    }
}