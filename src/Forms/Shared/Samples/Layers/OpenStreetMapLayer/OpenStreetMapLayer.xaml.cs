// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.OpenStreetMapLayer
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "OpenStreetMap layer",
        category: "Layers",
        description: "Add OpenStreetMap as a basemap layer.",
        instructions: "When the sample opens, it will automatically display the OpenStreetMap basemap. Pan and zoom to explore the basemap.",
        tags: new[] { "OSM", "OpenStreetMap", "basemap", "layers", "map", "open", "street" })]
    public partial class OpenStreetMapLayer : ContentPage
    {
        public OpenStreetMapLayer()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create the OpenStreetMap layer.
            var openStreetMapLayer = new Esri.ArcGISRuntime.Mapping.OpenStreetMapLayer();

            try
            {
                await openStreetMapLayer.LoadAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(ex.GetType().Name, ex.Message, "OK");
            }

            // Create a map and set the open street map layer as its basemap.
            MyMapView.Map = new Map { Basemap = new Basemap(openStreetMapLayer) };
        }
    }
}