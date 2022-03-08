// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace ArcGISRuntime.WPF.Samples.ChangeBasemap
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Change basemap",
        category: "Map",
        description: "Change a map's basemap. A basemap is beneath all layers on a `Map` and is used to provide visual reference for the operational layers.",
        instructions: "Use the drop down menu to select the active basemap from the list of available basemaps.",
        tags: new[] { "basemap", "map" })]
    public partial class ChangeBasemap
    {
        public ChangeBasemap()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            testInitialize();
        }

        private async void testInitialize()
        {
            // Assign a new map to the MapView
            MyMapView.Map = new Map(Basemap.CreateLightGrayCanvas());

            MyBasemapGallery.Portal = await ArcGISPortal.CreateAsync();

            await MyBasemapGallery.Portal.GetBasemapsAsync();
        }

        private void Initialize()
        {
            // Assign a new map to the MapView
            //MyMapView.Map = new Map(_basemapOptions.Values.First());

            //// Set basemap titles as a items source
            //BasemapChooser.ItemsSource = _basemapOptions.Keys;

            //// Show the first basemap in the list
            //BasemapChooser.SelectedIndex = 0;
        }

        private void BasemapSelected(object sender, Esri.ArcGISRuntime.Toolkit.UI.BasemapGalleryItem e)
        {
            MyMapView.Map.Basemap = MyBasemapGallery.SelectedBasemap.Basemap;
        }
    }
}