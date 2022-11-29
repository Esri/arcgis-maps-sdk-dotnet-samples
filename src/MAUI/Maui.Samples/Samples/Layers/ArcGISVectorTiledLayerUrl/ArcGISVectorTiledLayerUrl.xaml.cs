// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;

namespace ArcGIS.Samples.ArcGISVectorTiledLayerUrl
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "ArcGIS vector tiled layer URL",
        category: "Layers",
        description: "Load an ArcGIS Vector Tiled Layer from a URL.",
        instructions: "Use the drop down menu to load different vector tile basemaps.",
        tags: new[] { "tiles", "vector", "vector basemap", "vector tiled layer", "vector tiles" })]
    public partial class ArcGISVectorTiledLayerUrl : ContentPage
    {
        private readonly Dictionary<string, Uri> _layerUrls = new Dictionary<string, Uri>()
        {
            {"Mid-Century", new Uri("https://www.arcgis.com/home/item.html?id=7675d44bb1e4428aa2c30a9b68f97822")},
            {"Colored Pencil", new Uri("https://www.arcgis.com/home/item.html?id=4cf7e1fb9f254dcda9c8fbadb15cf0f8")},
            {"Newspaper", new Uri("https://www.arcgis.com/home/item.html?id=dfb04de5f3144a80bc3f9f336228d24a")},
            {"Nova", new Uri("https://www.arcgis.com/home/item.html?id=75f4dfdff19e445395653121a95a85db")},
            {"World Street Map (Night)", new Uri("https://www.arcgis.com/home/item.html?id=86f556a2d1fd468181855a35e344567f")}
        };

        public ArcGISVectorTiledLayerUrl()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create a new ArcGISVectorTiledLayer with the navigation service URL
            ArcGISVectorTiledLayer vectorTiledLayer = new ArcGISVectorTiledLayer(_layerUrls.Values.First());

            // Create new Map with basemap
            Map myMap = new Map(new Basemap(vectorTiledLayer));

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }

        private async void OnChangeLayerButtonClicked(object sender, EventArgs e)
        {
            // Get list of layer names as array to show in an action sheet
            string[] layerNames = _layerUrls.Keys.ToArray();

            try
            {
                // Show sheet and get title from the selection
                string selectedLayer = await Application.Current.MainPage.DisplayActionSheet("Select layer", "Cancel", null, layerNames);

                // If selected cancel do nothing
                if (selectedLayer == "Cancel") return;

                // Create a new ArcGISVectorTiledLayer with the URL Selected by the user
                ArcGISVectorTiledLayer vectorTiledLayer = new ArcGISVectorTiledLayer(_layerUrls[selectedLayer]);

                // Create new Map with basemap and assigning to the MapView's Map
                MyMapView.Map = new Map(new Basemap(vectorTiledLayer));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}