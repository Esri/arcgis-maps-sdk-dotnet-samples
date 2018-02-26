// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;

namespace ArcGISRuntime.WPF.Samples.ShowMagnifier
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Show magnifier",
        "MapView",
        "This sample demonstrates how you can tap and hold on a map to get the magnifier. You can also pan while tapping and holding to move the magnifier across the map.",
        "This sample only works on a device with a touch screen. The magnifier will not appear via a mouse click.")]
    public partial class ShowMagnifier
    {
        public ShowMagnifier()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap and initial location
            Map myMap = new Map(BasemapType.Topographic, 34.056295, -117.195800, 10);

            // Enable magnifier
            MyMapView.InteractionOptions.IsMagnifierEnabled = true;

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }
    }
}