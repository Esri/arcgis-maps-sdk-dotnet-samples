// Copyright 2026 Esri.
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

namespace ArcGIS.Samples.DisplayGeometryEditorInformationDuringInteraction
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display geometry editor information during interaction",
        category: "Geometry",
        description: "Use the geometry editor to see information about the geometry editor's previewed geometry during an editing interaction.",
        instructions: "Tap a graphic to edit its geometry by moving, rotating, or scaling the geometry. During the interaction, information about the geometry will be displayed to provide feedback to the user.",
        tags: new[] { "draw", "edit", "geometry editor", "interaction preview" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class DisplayGeometryEditorInformationDuringInteraction
    {
        public DisplayGeometryEditorInformationDuringInteraction()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
        }
    }
}