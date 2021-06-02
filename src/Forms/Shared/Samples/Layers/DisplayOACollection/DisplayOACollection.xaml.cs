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
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.DisplayOACollection
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display OGC API collection",
        category: "Layers",
        description: "Display an OGC API feature collection and query features while navigating the map view.",
        instructions: "Pan the map and observe how new features are loaded from the OGC API feature service.",
        tags: new[] { "OGC", "OGC API", "feature", "feature layer", "feature table", "service", "table", "web" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class DisplayOACollection : ContentPage
    {
        public DisplayOACollection()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
        }
    }
}
