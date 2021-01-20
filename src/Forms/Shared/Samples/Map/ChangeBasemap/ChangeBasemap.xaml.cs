// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.ChangeBasemap
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Change basemap",
        category: "Map",
        description: "Change a map's basemap. A basemap is beneath all layers on a `Map` and is used to provide visual reference for the operational layers.",
        instructions: "Use the drop down menu to select the active basemap from the list of available basemaps.",
        tags: new[] { "basemap", "map" })]
    public partial class ChangeBasemap : ContentPage
    {
        // Dictionary that associates names with basemaps.
        private readonly Dictionary<string, Basemap> _basemapOptions = new Dictionary<string, Basemap>()
        {
            {"Streets", new Basemap(BasemapStyle.ArcGISStreets)},
            {"Streets - Night", new Basemap(BasemapStyle.ArcGISStreetsNight)},
            {"Imagery", new Basemap(BasemapStyle.ArcGISImageryStandard)},
            {"Imagery with Labels", new Basemap(BasemapStyle.ArcGISImagery)},
            {"Dark Gray Canvas", new Basemap(BasemapStyle.ArcGISDarkGray)},
            {"Light Gray Canvas", new Basemap(BasemapStyle.ArcGISLightGray)},
            {"Navigation", new Basemap(BasemapStyle.ArcGISNavigation)},
            {"OpenStreetMap", new Basemap(BasemapStyle.OSMStandard)}
        };

        public ChangeBasemap()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void OnChangeBasemapButtonClicked(object sender, EventArgs e)
        {
            // Show sheet and get title from the selection
            string selectedBasemap =
                await ((Page)Parent).DisplayActionSheet("Select basemap", "Cancel", null, _basemapOptions.Keys.ToArray());

            // Verify the user did not cancel the operation
            if (!selectedBasemap.ToLower().Equals("cancel"))
            {
                // Retrieve the basemap from the dictionary
                MyMapView.Map.Basemap = _basemapOptions[selectedBasemap];
            }
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(BasemapStyle.ArcGISTopographic);

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }
    }
}
