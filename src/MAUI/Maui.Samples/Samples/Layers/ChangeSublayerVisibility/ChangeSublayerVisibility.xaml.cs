// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;

namespace ArcGIS.Samples.ChangeSublayerVisibility
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
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

                // Header showing the image layer name above the sublayer rows.
                Label header = new Label
                {
                    Text = _imageLayer.Name,
                    FontAttributes = FontAttributes.Bold,
                    Padding = new Thickness(10)
                };

                // Collection view of sublayers, each row showing a label and a toggle switch.
                CollectionView sublayersView = new CollectionView
                {
                    ItemsSource = _imageLayer.Sublayers,
                    SelectionMode = Microsoft.Maui.Controls.SelectionMode.None,
                    ItemTemplate = new DataTemplate(() =>
                    {
                        Label nameLabel = new Label { VerticalOptions = LayoutOptions.Center };
                        nameLabel.SetBinding(Label.TextProperty, nameof(ArcGISSublayer.Name));

                        Switch toggle = new Switch { VerticalOptions = LayoutOptions.Center };
                        toggle.SetBinding(Switch.IsToggledProperty, new Binding(nameof(ArcGISSublayer.IsVisible), BindingMode.OneWay));
                        toggle.Toggled += OnSublayerVisibilityToggled;

                        Grid row = new Grid
                        {
                            Padding = new Thickness(10, 5),
                            ColumnDefinitions =
                            {
                                new ColumnDefinition(GridLength.Star),
                                new ColumnDefinition(GridLength.Auto)
                            }
                        };
                        row.Add(nameLabel, 0, 0);
                        row.Add(toggle, 1, 0);
                        return row;
                    })
                };

                StackLayout layout = new StackLayout { Children = { header, sublayersView } };

                // Create internal page for the navigation page
                ContentPage sublayersPage = new ContentPage()
                {
                    Content = layout,
                    Title = "Sublayers"
                };

                // Navigate to the sublayers page
                await Shell.Current.Navigation.PushAsync(sublayersPage);
            }
            catch (Exception ex)
            {
                await Application.Current.Windows[0].Page.DisplayAlertAsync("Error", ex.ToString(), "OK");
            }
        }

        private void OnSublayerVisibilityToggled(object sender, ToggledEventArgs e)
        {
            if (sender is Switch toggle && toggle.BindingContext is ArcGISSublayer sublayer)
            {
                sublayer.IsVisible = e.Value;
            }
        }
    }
}