// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;

namespace ArcGIS.WPF.Samples.DisplayMap
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display map",
        category: "Map",
        description: "Display a map with an imagery basemap.",
        instructions: "Run the sample to view the map. Pan and zoom to navigate the map.",
        tags: new[] { "basemap", "map" })]
    public partial class DisplayMap
    {
        public DisplayMap()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(BasemapStyle.ArcGISImageryStandard);

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }
    }
}