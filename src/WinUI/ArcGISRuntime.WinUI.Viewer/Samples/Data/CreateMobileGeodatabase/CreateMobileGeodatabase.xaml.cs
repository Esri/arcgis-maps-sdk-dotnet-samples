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

namespace ArcGISRuntime.WinUI.Samples.CreateMobileGeodatabase
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Create mobile geodatabase",
        category: "Data",
        description: "Create and share a mobile geodatabase.",
        instructions: "Tap on the map to add a feature symbolizing the user's location. Tap \"View table\" to view the contents of the geodatabase feature table. Once you have added the location points to the map, click on \"Close geodatabase\" to retrieve the `.geodatabase` file which can then be imported into ArcGIS Pro or opened in an ArcGIS Runtime application.",
        tags: new[] { "arcgis pro", "database", "feature", "feature table", "geodatabase", "mobile geodatabase", "sqlite" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class CreateMobileGeodatabase
    {
        public CreateMobileGeodatabase()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
        }
    }
}