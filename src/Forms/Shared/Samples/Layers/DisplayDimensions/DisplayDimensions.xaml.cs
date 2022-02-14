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
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.DisplayDimensions
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display dimensions",
        category: "Layers",
        description: "Display dimension features from a mobile map package.",
        instructions: "When the sample loads, it will automatically display the map containing dimension features from the mobile map package. The name of the dimension layer containing the dimension features is displayed in the controls box. Control the visibility of the dimension layer with the \"Dimension Layer visibility\" check box, and apply a definition expression to show dimensions of greater than or equal to 450m in length using the \"Definition Expression\" checkbox.",
        tags: new[] { "dimension", "layer", "mmpk", "mobile map package", "utility" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("f5ff6f5556a945bca87ca513b8729a1e")]
    public partial class DisplayDimensions : ContentPage
    {
        public DisplayDimensions()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
        }
    }
}
