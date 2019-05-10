// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using RuntimeImageExtensions = Esri.ArcGISRuntime.Xamarin.Forms.RuntimeImageExtensions;

namespace ArcGISRuntime.Samples.SymbolsFromMobileStyle
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Read symbols from a mobile style",
        "Symbology",
        "Open a local mobile style file (.stylx) and read its contents.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("1bd036f221f54a99abc9e46ff3511cbf")]
    public partial class SymbolsFromMobileStyle : ContentPage
    {
        // A mobile style containing symbols.
        private SymbolStyle _emojiStyle;

        // The unique identifier (key) for the background face symbol in the mobile style.
        private readonly string _baseSymbolKey = "Face1";

        public SymbolsFromMobileStyle()
        {
            InitializeComponent();

            // Call a function that will create the map and fill UI controls with symbols in a mobile style.
            Initialize();
        }

        private async Task Initialize()
        {
            SelectSymbolGrid.IsVisible = false;

            // Create a new topographic basemap and assign it to the map view.
            Map map = new Map(Basemap.CreateTopographic());
            MyMapView.Map = map;

            // Create a graphics overlay for showing point graphics and add it to the map view.
            GraphicsOverlay overlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(overlay);

            // Fill the symbol color combo box with some color choices.
            List<Color> faceColors = new List<Color>
            {
                Color.Yellow, Color.LightGreen, Color.LightPink
            };
            ColorListView.ItemsSource = faceColors;

            // Get the full path to the downloaded mobile style file (.stylx).
            string mobileStyleFilePath = DataManager.GetDataFolder("1bd036f221f54a99abc9e46ff3511cbf", "emoji-mobile.stylx");

            // Call a function that will read the mobile style file and populate list boxes with symbol layers.
            await ReadMobileStyle(mobileStyleFilePath);

            // Handle the tapped event on the map view to draw point graphics with the chosen symbol.
            MyMapView.GeoViewTapped += GeoViewTapped;
        }

        public void ChooseSymbolButtonClicked(object sender, EventArgs e)
        {
            // Show or hide the symbol selection controls.
            SelectSymbolGrid.IsVisible = !SelectSymbolGrid.IsVisible;
        }

        private async Task ReadMobileStyle(string stylePath)
        {
            try
            {
                // Open the mobile style file at the provided path.
                _emojiStyle = await SymbolStyle.OpenAsync(stylePath);

                // Get the default style search parameters.
                SymbolStyleSearchParameters searchParams = await _emojiStyle.GetDefaultSearchParametersAsync();

                // Search the style with the default parameters to return all symbol results.
                IList<SymbolStyleSearchResult> styleResults = await _emojiStyle.SearchSymbolsAsync(searchParams);
                
                // Create lists to contain the available symbol layers for each category of symbol and add an empty entry as default.
                List<SymbolLayerInfo> eyeSymbolInfos = new List<SymbolLayerInfo> { new SymbolLayerInfo("", null, "") };
                List<SymbolLayerInfo> mouthSymbolInfos = new List<SymbolLayerInfo> { new SymbolLayerInfo("", null, "") };
                List<SymbolLayerInfo> hatSymbolInfos = new List<SymbolLayerInfo>() { new SymbolLayerInfo("", null, "") };

                // Loop through the results and put symbols into the appropriate list according to category.
                foreach (SymbolStyleSearchResult result in styleResults)
                {
                    // Get the symbol for this result.
                    MultilayerPointSymbol multiLayerSym = result.Symbol as MultilayerPointSymbol;

                    // Create a swatch image from the symbol.
                    RuntimeImage swatch = await multiLayerSym.CreateSwatchAsync(30, 30, 96, Color.White);
                    ImageSource symbolImage = await RuntimeImageExtensions.ToImageSourceAsync(swatch);

                    // Create a symbol layer info object to represent the symbol in the list.
                    // The symbol key will be used to retrieve the symbol from the style.
                    SymbolLayerInfo symbolInfo = new SymbolLayerInfo(result.Name, symbolImage, result.Key);

                    // Add the symbol layer info to the correct list for its category.
                    switch (result.Category)
                    {
                        case "Eyes":
                            {
                                eyeSymbolInfos.Add(symbolInfo);
                                break;
                            }
                        case "Mouth":
                            {
                                mouthSymbolInfos.Add(symbolInfo);
                                break;
                            }
                        case "Hat":
                            {
                                hatSymbolInfos.Add(symbolInfo);
                                break;
                            }
                    }
                }

                // Show the symbols in the category list boxes.
                EyesListView.ItemsSource = eyeSymbolInfos;
                MouthListView.ItemsSource = mouthSymbolInfos;
                HatListView.ItemsSource = hatSymbolInfos;

                // Call a function to construct the current symbol (default yellow circle).
                Symbol faceSymbol = await GetCurrentSymbol();

                // Call a function to show a preview image of the symbol.
                await UpdateSymbolPreview(faceSymbol);
            }
            catch (Exception ex)
            {
                // Report the exception.
                await DisplayAlert("Error reading style", ex.Message, "OK");
            }
        }

        // Handler for the tapped event on the map view.
        private async void GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Call a function to get the currently defined multilayer point symbol.
            MultilayerPointSymbol faceSymbol = await GetCurrentSymbol();

            // Create a graphic for the tapped location using the current symbol and add it to the map view.
            Graphic graphic = new Graphic(e.Location, faceSymbol);
            MyMapView.GraphicsOverlays.First().Graphics.Add(graphic);
        }

        // An event handler for list box and combo box selection changes that will update the current symbol.
        private async void SymbolLayerSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // Call a function that will construct the current symbol.
            Symbol faceSymbol = await GetCurrentSymbol();
            
            // Call a function to update the symbol preview.
            await UpdateSymbolPreview(faceSymbol);
        }

        private async Task<MultilayerPointSymbol> GetCurrentSymbol()
        {
            // If the style hasn't been opened, return.
            if (_emojiStyle == null) { return null; }

            MultilayerPointSymbol faceSymbol = null;
            try
            {
                // Get the key that identifies the selected eye symbol (or an empty string if none selected).
                SymbolLayerInfo eyeLayerInfo = (SymbolLayerInfo)EyesListView.SelectedItem;
                string eyeLayerKey = eyeLayerInfo != null ? eyeLayerInfo.Key : string.Empty;

                // Get the key that identifies the selected mouth symbol (or an empty string if none selected).
                SymbolLayerInfo mouthLayerInfo = (SymbolLayerInfo)MouthListView.SelectedItem;
                string mouthLayerKey = mouthLayerInfo != null ? mouthLayerInfo.Key : string.Empty;

                // Get the key that identifies the selected hat symbol (or an empty string if none selected).
                SymbolLayerInfo hatLayerInfo = (SymbolLayerInfo)HatListView.SelectedItem;
                string hatLayerKey = hatLayerInfo != null ? hatLayerInfo.Key : string.Empty;

                // Create a list of the symbol keys that identify the selected symbol layers, including the base (circle) symbol.
                List<string> symbolKeys = new List<string>
                {
                    _baseSymbolKey, eyeLayerKey, mouthLayerKey, hatLayerKey
                };

                // Get a multilayer point symbol from the style that contains the selected symbol layers.
                faceSymbol = await _emojiStyle.GetSymbolAsync(symbolKeys) as MultilayerPointSymbol;

                // Loop through all symbol layers and lock the color.
                foreach (SymbolLayer lyr in faceSymbol.SymbolLayers)
                {
                    // Changing the color of the symbol will not affect this layer.
                    lyr.IsColorLocked = true;
                }

                // Unlock the color for the base (first) layer. Changing the symbol color will change this layer's color.
                faceSymbol.SymbolLayers.First().IsColorLocked = false;

                // Set the symbol color from the combo box.
                if (ColorListView.SelectedItem != null)
                {
                    faceSymbol.Color = (Color)ColorListView.SelectedItem;
                }

                // Set the symbol size from the slider.
                faceSymbol.Size = SizeSlider.Value;
            }
            catch (Exception ex)
            {
                // Report the exception.
                await DisplayAlert("Error creating symbol", ex.Message, "OK");
            }

            // Return the multilayer point symbol.
            return faceSymbol;
        }

        private async Task UpdateSymbolPreview(Symbol symbolToShow)
        {
            if (symbolToShow == null) { return; }

            // Create a swatch from the symbol with a white background.
            RuntimeImage swatch = await symbolToShow.CreateSwatchAsync(80, 80, 96, Color.White);

            // Convert the swatch to an image source and show it in the Image control.
            ImageSource symbolImage = await RuntimeImageExtensions.ToImageSourceAsync(swatch);
            SymbolPreviewImage.Source = symbolImage;
        }

        // A handler for the click event on the clear graphics button.
        private void ClearGraphicsButtonClicked(object sender, EventArgs e)
        {
            // Clear all graphics from the first (only) graphics overlay in the map view.
            MyMapView.GraphicsOverlays.FirstOrDefault()?.Graphics.Clear();
        }
    }

    // A class for storing information about a symbol layer.
    public class SymbolLayerInfo
    {
        // An image source for a preview image of the symbol.
        public ImageSource ImageSrc { get; private set; }

        // The name of the symbol as it appears in the mobile style.
        public string Name { get; private set; }

        // A key that uniquely identifies the symbol in the style.
        public string Key { get; private set; }

        // Take all symbol info in the constructor.
        public SymbolLayerInfo(string name, ImageSource source, string key)
        {
            Name = name;
            ImageSrc = source;
            Key = key;
        }
    }
}