// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;

namespace ArcGIS.WPF.Samples.OpenStreetMapLayer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "OpenStreetMap layer",
        category: "Layers",
        description: "Add OpenStreetMap as a basemap layer.",
        instructions: "When the sample opens, it will automatically display the map with the OpenStreetMap basemap. Pan and zoom to observe the basemap.",
        tags: new[] { "OSM", "OpenStreetMap", "basemap", "layers", "map", "open", "street" })]
    public partial class OpenStreetMapLayer
    {
        public OpenStreetMapLayer()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create the OpenStreetMap basemap.
            Basemap osmBasemap = new Basemap(BasemapStyle.OSMStandard);

            // Create the map with the OpenStreetMap basemap.
            Map osmMap = new Map(osmBasemap);

            // Show the map in the view.
            MyMapView.Map = osmMap;
        }
    }
}