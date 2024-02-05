// Copyright 2024 Esri.
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

namespace ArcGIS.WinUI.Samples.ValidateUtilityNetworkTopology
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        "Validate utility network topology",
        "UtilityNetwork",
        "Demonstrates the workflow of getting the network state and validating the topology of a utility network.",
        "")]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("6e3fc6db3d0b4e6589eb4097eb3e5b9b")]
    public partial class ValidateUtilityNetworkTopology
    {
        public ValidateUtilityNetworkTopology()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
        }
    }
}