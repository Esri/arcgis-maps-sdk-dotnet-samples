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
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace ArcGISRuntime.WinUI.Samples.EditBranchVersioning
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Edit with branch versioning",
        category: "Data",
        description: "Create, query and edit a specific server version using service geodatabase.",
        instructions: "Once loaded, the map will zoom to the extent of the feature layer. The current version is indicated at the top of the map. Click \"Create Version\" to open a dialog to specify the version information (name, access, and description). See the *Additional information* section for restrictions on the version name.",
        tags: new[] { "branch versioning", "edit", "version control", "version management server" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class EditBranchVersioning
    {
        public EditBranchVersioning()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
        }
    }
}