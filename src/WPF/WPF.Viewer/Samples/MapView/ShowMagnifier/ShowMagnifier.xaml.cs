// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;

namespace ArcGIS.WPF.Samples.ShowMagnifier
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Show magnifier",
        category: "MapView",
        description: "Tap and hold on a map to show a magnifier.",
        instructions: "Tap and hold on the map to show a magnifier, then drag across the map to move the magnifier. You can also pan the map while holding the magnifier, by dragging the magnifier to the edge of the map.",
        tags: new[] { "magnify", "map", "zoom" })]
    public partial class ShowMagnifier
    {
        public ShowMagnifier()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap and initial location.
            Map myMap = new Map(BasemapStyle.ArcGISTopographic);
            myMap.InitialViewpoint = new Viewpoint(34.056295, -117.195800, 50000);

            // The magnifier is enabled by default but will only display on touch screen devices. 
            // To disable the magnifier set IsMagnifierEnabled = false.
            MyMapView.InteractionOptions = new MapViewInteractionOptions()
            {
                IsMagnifierEnabled = true
            };

            // Assign the map to the MapView.
            MyMapView.Map = myMap;
        }
    }
}