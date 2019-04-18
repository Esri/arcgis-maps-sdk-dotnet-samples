// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Data;
using ArcGISRuntime.Samples.Managers;

namespace ArcGISRuntime.WPF.Samples.SymbolsFromMobileStyle
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Read symbols from a mobile style",
        "Symbology",
        "Open a local mobile style file (.stylx) and read its contents.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("1bd036f221f54a99abc9e46ff3511cbf")]
    public partial class SymbolsFromMobileStyle
    {
        // The file path to the mobile style file (.stylx) and the symbol style object.
        private string _mobileStyleFilePath;
        private SymbolStyle _emojiStyle;

        // The unique identifier (key) for the background face symbol in the mobile style.
        private readonly string _baseSymbolKey = "Face1";

        public SymbolsFromMobileStyle()
        {
            InitializeComponent();
            Initialize();
        }

        private async Task Initialize()
        {
            GraphicsOverlay overlay = new GraphicsOverlay();
            Map map = new Map(Basemap.CreateTopographic());
            MyMapView.Map = map;
            MyMapView.GraphicsOverlays.Add(overlay);

            FaceColorComboBox.Items.Add(Color.Yellow);
            FaceColorComboBox.Items.Add(Color.LightGreen);
            FaceColorComboBox.Items.Add(Color.LightPink);
            FaceColorComboBox.SelectedIndex = 0;

            // Get the full path to the downloaded mobile style file (.stylx).
            _mobileStyleFilePath = DataManager.GetDataFolder("1bd036f221f54a99abc9e46ff3511cbf", "emoji-mobile.stylx");

            await ReadMobileStyle();

            MyMapView.GeoViewTapped += GeoViewTapped;
        }

        private async Task ReadMobileStyle()
        {
            // Open a mobile style file.
            _emojiStyle = await SymbolStyle.OpenAsync(_mobileStyleFilePath);

            // Get the default style search parameters.
            SymbolStyleSearchParameters searchParams = await _emojiStyle.GetDefaultSearchParametersAsync();

            // Search the style with the default parameters to return all symbol results.
            IList<SymbolStyleSearchResult> styleResults = await _emojiStyle.SearchSymbolsAsync(searchParams);

            List<SymbolLayerInfo> eyeSymbolInfos = new List<SymbolLayerInfo>();
            List<SymbolLayerInfo> mouthSymbolInfos = new List<SymbolLayerInfo>();
            List<SymbolLayerInfo> hatSymbolInfos = new List<SymbolLayerInfo>();

            // Loop through the results and put symbols into the appropriate list according to category.
            foreach (SymbolStyleSearchResult result in styleResults)
            {
                MultilayerPointSymbol multiLayerSym = result.Symbol as MultilayerPointSymbol;
                RuntimeImage swatch = await multiLayerSym.CreateSwatchAsync();
                System.Windows.Media.ImageSource symbolImage = await swatch.ToImageSourceAsync();

                SymbolLayerInfo symbolInfo = new SymbolLayerInfo(result.Name, symbolImage, result.Key);
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
        }

        private async void GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            MultilayerPointSymbol faceSymbol = await GetCurrentSymbol();
            Graphic graphic = new Graphic(e.Location, faceSymbol);
            MyMapView.GraphicsOverlays.First().Graphics.Add(graphic);
        }

        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateSymbolPreview();
        }

        private async Task<MultilayerPointSymbol> GetCurrentSymbol()
        {
            if(_emojiStyle == null) { return null; }

            List<string> symbolKeys = new List<string>
            {
                _baseSymbolKey
            };

            if (EyeSymbolList.SelectedItem != null)
            {
                SymbolLayerInfo eyeLayerInfo = (SymbolLayerInfo)EyeSymbolList.SelectedItem;
                symbolKeys.Add(eyeLayerInfo.Key);
            }

            if (MouthSymbolList.SelectedItem != null)
            {
                SymbolLayerInfo mouthLayerInfo = (SymbolLayerInfo)MouthSymbolList.SelectedItem;
                symbolKeys.Add(mouthLayerInfo.Key);
            }

            if (HatSymbolList.SelectedItem != null)
            {
                SymbolLayerInfo hatLayerInfo = (SymbolLayerInfo)HatSymbolList.SelectedItem;
                symbolKeys.Add(hatLayerInfo.Key);
            }

            MultilayerPointSymbol faceSymbol = await _emojiStyle.GetSymbolAsync(symbolKeys) as MultilayerPointSymbol;

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
            
            return faceSymbol;
        }

        private async void UpdateSymbolPreview()
        {
            MultilayerPointSymbol faceSymbol = await GetCurrentSymbol();
            if (faceSymbol != null)
            {
                RuntimeImage swatch = await faceSymbol.CreateSwatchAsync(80, 80, 96, Color.White);
                System.Windows.Media.ImageSource symbolImage = await swatch.ToImageSourceAsync();
                SymbolPreviewImage.Source = symbolImage;
            }
        }

        private void ClearGraphicsButton_Click(object sender, RoutedEventArgs e)
        {
            MyMapView.GraphicsOverlays.FirstOrDefault()?.Graphics.Clear();
        }

        }
        public class SymbolLayerInfo
    {
        public System.Windows.Media.ImageSource ImgSrc { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }

        public SymbolLayerInfo(string name, System.Windows.Media.ImageSource source, string key)
        {
            Name = name;
            ImgSrc = source;
            Key = key;
        }
    }

    public class ColorToSolidBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Color inColor = (Color)value;
            System.Windows.Media.Color outColor = System.Windows.Media.Color.FromArgb(inColor.A, inColor.R, inColor.G, inColor.B);
            System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush(outColor);
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
