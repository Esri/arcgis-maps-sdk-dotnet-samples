﻿// Copyright 2025 Esri.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGIS.Samples.SnapGeometryEditsWithUtilityNetworkRules
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Snap geometry edits with utility network rules",
        category: "Utility network",
        description: "Use the Geometry Editor to edit geometries using utility network connectivity rules.",
        instructions: "To edit a geometry, tap a point geometry to be edited in the map to select it. Then edit the geometry by clicking the button to start the geometry editor.",
        tags: new[] { "edit", "feature", "geometry editor", "graphics", "layers", "map", "snapping", "utility network" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class SnapGeometryEditsWithUtilityNetworkRules
    {
        public SnapGeometryEditsWithUtilityNetworkRules()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
        }
    }
}