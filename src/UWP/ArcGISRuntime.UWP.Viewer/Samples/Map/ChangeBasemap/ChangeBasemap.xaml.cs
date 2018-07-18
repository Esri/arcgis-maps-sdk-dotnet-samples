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
        "Change basemap",
        "Map",
        "This sample demonstrates how to dynamically change the basemap displayed in a Map.",
        "")]
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
            // Create new Map with basemap
            Map myMap = new Map(_basemapOptions.Values.First());

            // Assign the map to the MapView
            MyMapView.Map = myMap;

            // Set titles as a items source
            basemapChooser.ItemsSource = _basemapOptions.Keys;
        }

        private void OnBasemapListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the title of the selected basemap
            string selectedBasemapTtile = e.AddedItems[0].ToString();

            // Retrieve the basemap from the dictionary
            MyMapView.Map.Basemap = _basemapOptions[selectedBasemapTtile];
        }
    }
}