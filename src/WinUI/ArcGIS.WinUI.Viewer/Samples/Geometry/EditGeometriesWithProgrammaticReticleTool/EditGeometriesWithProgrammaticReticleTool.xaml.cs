// Copyright 2025 Esri.
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

namespace ArcGIS.WinUI.Samples.EditGeometriesWithProgrammaticReticleTool
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Edit geometries with programmatic reticle tool",
        category: "Geometry",
        description: "Use the Programmatic Reticle Tool to edit and create geometries with programmatic operations to facilitate workflows such as those using buttons rather than tap interactions.",
        instructions: "To create a new geometry, select the geometry type you want to create (i.e. points, multipoints, polyline, or polygon) in the settngs view. Press the button to start the geometry editor, pan the map to position the reticle then press the button to place a vertex. To edit an existing geometry, tap the geometry to be edited in the map and perform edits by positioning the reticle over a vertex and pressing the button to pick it up. The vertex can be moved by panning the map and dropped in a new position by pressing the button again.",
        tags: new[] { "draw", "edit", "freehand", "geometry editor", "programmatic", "reticle", "sketch", "vertex" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class EditGeometriesWithProgrammaticReticleTool
    {
        public EditGeometriesWithProgrammaticReticleTool()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
        }
    }
}