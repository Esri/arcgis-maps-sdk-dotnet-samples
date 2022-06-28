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
using Esri.ArcGISRuntime.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Symbol = Esri.ArcGISRuntime.Symbology.Symbol;

namespace ArcGISRuntime.WinUI.Samples.SymbolsFromMobileStyle
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Read symbols from mobile style",
        category: "Symbology",
        description: "Combine multiple symbols from a mobile style file into a single symbol.",
        instructions: "Select a symbol and a color from each of the category lists to create an emoji. A preview of the symbol is updated as selections are made. The size of the symbol can be set using the slider. Click the map to create a point graphic using the customized emoji symbol, and click \"Reset\" to clear all graphics from the display.",
        tags: new[] { "advanced symbology", "mobile style", "multilayer", "stylx" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("1bd036f221f54a99abc9e46ff3511cbf")]
    public partial class SymbolsFromMobileStyle
    {
        // A mobile style containing symbols.
        private SymbolStyle _emojiStyle;

        // The unique identifier (key) for the background face symbol in the mobile style.
        private readonly string _baseSymbolKey = "Face1";

        public SymbolsFromMobileStyle()
        {
            InitializeComponent();

            // Call a function that will create the map and fill UI controls with symbols in a mobile style.
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a new topographic basemap and assign it to the map view.
            Map map = new Map(BasemapStyle.ArcGISTopographic);
            MyMapView.Map = map;

            // Create a graphics overlay for showing point graphics and add it to the map view.
            GraphicsOverlay overlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(overlay);

            // Fill the symbol color combo box with some color choices.
            List<Color> faceColors = new List<Color>
            {
                Color.Yellow, Color.LightGreen, Color.LightPink
            };
            FaceColorComboBox.ItemsSource = faceColors;
            FaceColorComboBox.SelectedIndex = 0;

            // Get the full path to the downloaded mobile style file (.stylx).
            string mobileStyleFilePath = DataManager.GetDataFolder("1bd036f221f54a99abc9e46ff3511cbf", "emoji-mobile.stylx");
            try
            {
                // Call a function that will read the mobile style file and populate list boxes with symbol layers.
                await ReadMobileStyle(mobileStyleFilePath);

                // Handle the tapped event on the map view to draw point graphics with the chosen symbol.
                MyMapView.GeoViewTapped += GeoViewTapped;
            }
            catch (Exception ex)
            {
                _ = new MessageDialog2(ex.Message, "Error").ShowAsync();
            }
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

                // Create an empty placeholder image to represent "no symbol" for each category.
                ImageSource emptyImage = null;

                // Create lists to contain the available symbol layers for each category of symbol and add an empty entry as default.
                List<SymbolLayerInfo> eyeSymbolInfos = new List<SymbolLayerInfo> { new SymbolLayerInfo("", emptyImage, "") };
                List<SymbolLayerInfo> mouthSymbolInfos = new List<SymbolLayerInfo> { new SymbolLayerInfo("", emptyImage, "") };
                List<SymbolLayerInfo> hatSymbolInfos = new List<SymbolLayerInfo>() { new SymbolLayerInfo("", emptyImage, "") };

                // Loop through the results and put symbols into the appropriate list according to category.
                foreach (SymbolStyleSearchResult result in styleResults)
                {
                    // Get the symbol for this result.
                    MultilayerPointSymbol multiLayerSym = await result.GetSymbolAsync() as MultilayerPointSymbol;

                    // Create a swatch image from the symbol.
                    RuntimeImage swatch = await multiLayerSym.CreateSwatchAsync();
                    ImageSource symbolImage = await swatch.ToImageSourceAsync();

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
                EyeSymbolList.ItemsSource = eyeSymbolInfos;
                MouthSymbolList.ItemsSource = mouthSymbolInfos;
                HatSymbolList.ItemsSource = hatSymbolInfos;

                // Call a function to construct the current symbol (default yellow circle).
                Symbol faceSymbol = await GetCurrentSymbol();

                // Call a function to show a preview image of the symbol.
                await UpdateSymbolPreview(faceSymbol);
            }
            catch (Exception ex)
            {
                // Report the exception.
                var dialog = new MessageDialog2("Error reading symbols from style: " + ex.Message);
                await dialog.ShowAsync();
            }
        }

        // Handler for the tapped event on the map view.
        private async void GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try { 
            // Call a function to get the currently defined multilayer point symbol.
            MultilayerPointSymbol faceSymbol = await GetCurrentSymbol();

            // Create a graphic for the tapped location using the current symbol and add it to the map view.
            Graphic graphic = new Graphic(e.Location, faceSymbol);
            MyMapView.GraphicsOverlays.First().Graphics.Add(graphic);
            }
            catch (Exception ex)
            {
                _ = new MessageDialog2(ex.Message, "Error").ShowAsync();
            }
        }

        // An event handler for list box and combo box selection changes that will update the current symbol.
        private async void SymbolPropertyChanged(object sender, SelectionChangedEventArgs e)
        {
            try { 
            // Call a function that will construct the current symbol.
            Symbol faceSymbol = await GetCurrentSymbol();

            // Call a function to update the symbol preview.
            await UpdateSymbolPreview(faceSymbol);
            }
            catch (Exception ex)
            {
                _ = new MessageDialog2(ex.Message, "Error").ShowAsync();
            }
        }

        private async Task<MultilayerPointSymbol> GetCurrentSymbol()
        {
            // If the style hasn't been opened, return.
            if (_emojiStyle == null) { return null; }

            MultilayerPointSymbol faceSymbol = null;
            try
            {
                // Get the key that identifies the selected eye symbol (or an empty string if none selected).
                SymbolLayerInfo eyeLayerInfo = (SymbolLayerInfo)EyeSymbolList.SelectedItem;
                string eyeLayerKey = eyeLayerInfo != null ? eyeLayerInfo.Key : string.Empty;

                // Get the key that identifies the selected mouth symbol (or an empty string if none selected).
                SymbolLayerInfo mouthLayerInfo = (SymbolLayerInfo)MouthSymbolList.SelectedItem;
                string mouthLayerKey = mouthLayerInfo != null ? mouthLayerInfo.Key : string.Empty;

                // Get the key that identifies the selected hat symbol (or an empty string if none selected).
                SymbolLayerInfo hatLayerInfo = (SymbolLayerInfo)HatSymbolList.SelectedItem;
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
                if (FaceColorComboBox.SelectedItem != null)
                {
                    faceSymbol.Color = (Color)FaceColorComboBox.SelectedItem;
                }

                // Set the symbol size from the slider.
                faceSymbol.Size = SizeSlider.Value;
            }
            catch (Exception ex)
            {
                // Report the exception.
                var dialog = new MessageDialog2("Error creating the symbol: " + ex.Message);
                await dialog.ShowAsync();
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
            ImageSource symbolImage = await swatch.ToImageSourceAsync();
            SymbolPreviewImage.Source = symbolImage;
        }

        // A handler for the click event on the clear graphics button.
        private void ClearGraphicsClick(object sender, RoutedEventArgs e)
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

    // A class that converts a System.Drawing.Color object to a solid brush for setting background color for UI controls.
    public class ColorToSolidBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Get the input value as a System.Drawing.Color.
            Color inColor = (Color)value;

            // Create a Windows.UI.Color from the System.Drawing.Color.
            Windows.UI.Color outColor = Windows.UI.Color.FromArgb(inColor.A, inColor.R, inColor.G, inColor.B);

            // Create a solid color brush using the color and return it.
            SolidColorBrush brush = new SolidColorBrush(outColor);
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}