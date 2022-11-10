// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;

namespace ArcGISMapsSDKMaui.Samples.ChangeSublayerVisibility
{
    [ArcGISMapsSDK.Samples.Shared.Attributes.Sample(
        name: "Map image layer sublayer visibility",
        category: "Layers",
        description: "Change the visibility of sublayers.",
        instructions: "Each sublayer has a check box which can be used to toggle the visibility of the sublayer.",
        tags: new[] { "layers", "sublayers", "visibility" })]
    public partial class ChangeSublayerVisibility : ContentPage
    {
        private ArcGISMapImageLayer _imageLayer;

        public ChangeSublayerVisibility()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map
            Map myMap = new Map();

            // Create uri to the map image layer
            Uri serviceUri = new Uri(
               "https://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer");

            // Create new image layer from the url
            _imageLayer = new ArcGISMapImageLayer(serviceUri)
            {
                Name = "World Cities Population"
            };

            // Add created layer to the basemaps collection
            myMap.Basemap.BaseLayers.Add(_imageLayer);

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }

        private async void OnSublayersClicked(object sender, EventArgs e)
        {
            try
            {
                // Make sure that layer and it's sublayers are loaded
                // If layer is already loaded, this returns directly
                await _imageLayer.LoadAsync();

                // Create layout for sublayers page
                // Create root layout
                StackLayout layout = new StackLayout();

                // Create list for layers
                TableView sublayersTableView = new TableView();

                // Create section for basemaps sublayers
                TableSection sublayersSection = new TableSection(_imageLayer.Name);

                // Create cells for each of the sublayers
                foreach (ArcGISSublayer sublayer in _imageLayer.Sublayers)
                {
                    // Using switch cells that provides on/off functionality
                    SwitchCell cell = new SwitchCell()
                    {
                        Text = sublayer.Name,
                        On = sublayer.IsVisible
                    };

                    // Hook into the On/Off changed event
                    cell.OnChanged += OnCellOnOffChanged;

                    // Add cell into the table view
                    sublayersSection.Add(cell);
                }

                // Add section to the table view
                sublayersTableView.Root.Add(sublayersSection);

                // Add table to the root layout
                layout.Children.Add(sublayersTableView);

                // Create internal page for the navigation page
                ContentPage sublayersPage = new ContentPage()
                {
                    Content = layout,
                    Title = "Sublayers"
                };

                // Navigate to the sublayers page
                await Application.Current.MainPage.Navigation.PushAsync(sublayersPage);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void OnCellOnOffChanged(object sender, ToggledEventArgs e)
        {
            SwitchCell cell = (SwitchCell)sender;

            // Find the layer from the image layer
            ArcGISSublayer sublayer = _imageLayer.Sublayers.First(x => x.Name == cell.Text);

            // Change sublayers visibility
            sublayer.IsVisible = e.Value;
        }
    }
}