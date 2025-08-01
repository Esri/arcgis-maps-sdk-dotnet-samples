// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace ArcGIS.WinUI.Samples.SetBasemap
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Set basemap",
        category: "Map",
        description: "Change a map's basemap.",
        instructions: "Use the drop down menu to select the active basemap from the list of available basemaps.",
        tags: new[] { "basemap", "basemap gallery", "basemap style", "map", "toolkit" })]
    public partial class SetBasemap
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
            {"OpenStreetMap", new Basemap(BasemapStyle.OpenOSMStyle)}
        };

        public SetBasemap()
        {
            InitializeComponent();

            // Setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map
            Map myMap = new Map();

            // Assign the map to the MapView
            MyMapView.Map = myMap;

            // Set titles as an items source and choose the first item
            BasemapChooser.ItemsSource = _basemapOptions.Keys;
            BasemapChooser.SelectedIndex = 0;
        }

        private void OnBasemapListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the title of the selected basemap
            string selectedBasemapTitle = e.AddedItems[0].ToString();

            // Retrieve the basemap from the dictionary
            MyMapView.Map.Basemap = _basemapOptions[selectedBasemapTitle];
        }
    }
}