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

namespace ArcGISRuntime.WinUI.Samples.QueryFeaturesWithArcadeExpression
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Query features with arcade expression",
        category: "GraphicsOverlay",
        description: "Query features on a map using an arcade expression.",
        instructions: "Click on any neighborhood to see the number of crimes in the last 60 days in a callout.",
        tags: new[] { "arcade evaluator", "arcade expression", "identify layers", "portal", "portal item", "query" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("14562fced3474190b52d315bc19127f6")]
    public partial class QueryFeaturesWithArcadeExpression
    {
        public QueryFeaturesWithArcadeExpression()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
        }
    }
}