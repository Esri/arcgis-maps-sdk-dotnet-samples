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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ArcGISRuntime.WPF.Samples.AddFeaturesWithContingentValues
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Add features with contingent values",
        category: "Data",
        description: "Create and add features whose attribute values satisfy a predefined set of contingencies.",
        instructions: "Tap on the map to add a feature symbolizing a bird's nest. Then choose values describing the nest's status, protection, and buffer size. Notice how different values are available depending on the values of preceding fields. Once the contingent values are validated, tap \"Done\" to add the feature to the map.",
        tags: new[] { "coded values", "contingent values", "feature table", "geodatabase" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("e12b54ea799f4606a2712157cf9f6e41", "b5106355f1634b8996e634c04b6a930a")]
    public partial class AddFeaturesWithContingentValues
    {
        public AddFeaturesWithContingentValues()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
        }
    }
}
