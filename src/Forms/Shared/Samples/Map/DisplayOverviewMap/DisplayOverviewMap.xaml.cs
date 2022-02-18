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
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.DisplayOverviewMap
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display overview map",
        category: "Map",
        description: "Include an overview or inset map as an additional map view to show the wider context of the primary view. ",
        instructions: "Pan or zoom across the map view to browse through the tourist attractions feature layer and notice the viewpoint and scale of the linked overview map update automatically. When running the sample on a desktop, you can also navigate by panning and zooming on the overview map. However, interactivity of the overview map is disabled on mobile devices.",
        tags: new[] { "context", "inset", "map", "minimap", "overview", "preview", "small scale", "toolkit", "view" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("97ceed5cfc984b4399e23888f6252856")]
    public partial class DisplayOverviewMap : ContentPage
    {
        public DisplayOverviewMap()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
        }
    }
}
