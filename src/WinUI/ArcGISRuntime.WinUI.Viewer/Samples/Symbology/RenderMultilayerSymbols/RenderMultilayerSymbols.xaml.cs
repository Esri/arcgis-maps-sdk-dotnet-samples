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

namespace ArcGISRuntime.WinUI.Samples.RenderMultilayerSymbols
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Render multilayer symbols",
        "Symbology",
        "Shows a multilayer symbol on a map.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class RenderMultilayerSymbols
    {
        public RenderMultilayerSymbols()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
        }
    }
}