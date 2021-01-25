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
using System;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.EditFeatureLinkedAnnotation
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Edit features with feature-linked annotation",
        category: "Data",
        description: "Edit feature attributes which are linked to annotation through an expression.",
        instructions: "Pan and zoom the map to see that the text on the map is annotation, not labels. Click one of the address points to update the house number (AD_ADDRESS) and street name (ST_STR_NAM). Click one of the dashed parcel polylines and click another location to change its geometry. NOTE: Selection is only enabled for points and straight (single segment) polylines.",
        tags: new[] { "annotation", "attributes", "feature-linked annotation", "features", "fields" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("74c0c9fa80f4498c9739cc42531e9948")]
    public partial class EditFeatureLinkedAnnotation
    {
        public EditFeatureLinkedAnnotation()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
        }
    }
}
