// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;

namespace ArcGIS.Samples.SetBasemap
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Set basemap",
        category: "Map",
        description: "Change a map's basemap.",
        instructions: "Use the drop down menu to select the active basemap from the list of available basemaps.",
        tags: new[] { "basemap", "basemap gallery", "basemap style", "map", "toolkit" })]
    public partial class SetBasemap : ContentPage
    {
        public SetBasemap()
        {
            InitializeComponent();

            // Assign a new map to the MapView.
            MyMapView.Map = new Map();
        }
    }
}