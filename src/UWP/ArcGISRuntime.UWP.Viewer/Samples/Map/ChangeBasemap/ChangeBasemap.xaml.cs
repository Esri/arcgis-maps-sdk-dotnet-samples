// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.ChangeBasemap
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Change basemap",
        category: "Map",
        description: "Change a map's basemap. A basemap is beneath all layers on a `Map` and is used to provide visual reference for the operational layers.",
        instructions: "Use the drop down menu to select the active basemap from the list of available basemaps.",
        tags: new[] { "basemap", "map" })]
    public partial class ChangeBasemap
    {
        // Dictionary that associates names with basemaps
        private readonly Dictionary<string, Basemap> _basemapOptions = new Dictionary<string, Basemap>()
        {
            {"Streets (Raster)", Basemap.CreateStreets()},
            {"Streets (Vector)", Basemap.CreateStreetsVector()},
            {"Streets - Night (Vector)", Basemap.CreateStreetsNightVector()},
            {"Imagery (Raster)", Basemap.CreateImagery()},
            {"Imagery with Labels (Raster)", Basemap.CreateImageryWithLabels()},
            {"Imagery with Labels (Vector)", Basemap.CreateImageryWithLabelsVector()},
            {"Dark Gray Canvas (Vector)", Basemap.CreateDarkGrayCanvasVector()},
            {"Light Gray Canvas (Raster)", Basemap.CreateLightGrayCanvas()},
            {"Light Gray Canvas (Vector)", Basemap.CreateLightGrayCanvasVector()},
            {"Navigation (Vector)", Basemap.CreateNavigationVector()},
            {"OpenStreetMap (Raster)", Basemap.CreateOpenStreetMap()}
        };

        public ChangeBasemap()
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