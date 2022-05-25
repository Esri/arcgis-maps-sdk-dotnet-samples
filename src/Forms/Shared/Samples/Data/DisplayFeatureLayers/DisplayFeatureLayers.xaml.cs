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

namespace ArcGISRuntimeXamarin.Samples.DisplayFeatureLayers
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display feature layers",
        category: "Data",
        description: "Display feature layers from various data sources.",
        instructions: "Tap the button on the toolbar to add feature layers, from different sources, to the map. Pan and zoom the map to view the feature layers.",
        tags: new[] { "feature", "geodatabase", "geopackage", "layers", "service", "shapefile", "table" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("1759fd3e8a324358a0c58d9a687a8578", "2b0f9e17105847809dfeb04e3cad69e0", "68ec42517cdd439e81b036210483e8e7", "15a7cbd3af1e47cfa5d2c6b93dc44fc2")]
    public partial class DisplayFeatureLayers : ContentPage
    {
        public DisplayFeatureLayers()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
        }
    }
}
