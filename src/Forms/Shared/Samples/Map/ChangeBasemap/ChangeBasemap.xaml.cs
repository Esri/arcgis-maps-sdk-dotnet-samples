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
        instructions: "When the basemap gallery appears, select a basemap to be displayed.",
        tags: new[] { "basemap", "map" })]
    public partial class ChangeBasemap : ContentPage
    {
        public ChangeBasemap()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void BasemapSelected(object sender, Esri.ArcGISRuntime.Toolkit.UI.BasemapGalleryItem e)
        {
            MyMapView.Map.Basemap = MyBasemapGallery.SelectedBasemap.Basemap;
        }

        private void OnChangeBasemapButtonClicked(object sender, EventArgs e)
        {
            // Toggles the basemap gallery on and off.
            if (MyBasemapGallery.IsVisible)
            {
                MyBasemapGallery.IsVisible = false;
            }
            else
            {
                MyBasemapGallery.IsVisible = true;
            }
        }

        private async void Initialize()
        {
            // Creates a portal for the BasemapGallery to get the collection of basemaps.
            MyBasemapGallery.Portal = await ArcGISPortal.CreateAsync();
            await MyBasemapGallery.Portal.GetBasemapsAsync();

            // Assign the map to the MapView
            MyMapView.Map = new Map();
        }
    }
}