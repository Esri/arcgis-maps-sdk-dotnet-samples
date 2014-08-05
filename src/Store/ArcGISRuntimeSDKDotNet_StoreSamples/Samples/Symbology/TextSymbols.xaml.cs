using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Sample shows how to create Text Symbols (TextSymbol) and add graphics using the symbols.
    /// </summary>
    /// <title>Text Symbols</title>
	/// <category>Symbology</category>
	public partial class TextSymbols : Windows.UI.Xaml.Controls.Page
    {
        private Random _random = new Random();
        private List<TextSymbol> _symbols;
        private GraphicsLayer _graphicsLayer;

        /// <summary>Construct Text Symbols sample control</summary>
        public TextSymbols()
        {
            InitializeComponent();

            _graphicsLayer = MyMapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;
                
            MyMapView.ExtentChanged += MyMapView_ExtentChanged;
        }

        // Start map interaction
        private async void MyMapView_ExtentChanged(object sender, EventArgs e)
        {
            MyMapView.ExtentChanged -= MyMapView_ExtentChanged;

            await SetupSymbolsAsync();
            DataContext = this;

            await AcceptPointsAsync();
        }

        // Cancel current shape request when the symbol selection changes 
        private async void symbolCombo_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            if (MyMapView.Editor.IsActive)
                MyMapView.Editor.Cancel.Execute(null);

            await AcceptPointsAsync();
        }

        // Accept user map clicks and add points to the graphics layer with the selected symbol
        private async Task AcceptPointsAsync()
        {
            try
            {
                while (MyMapView.Extent != null)
                {
                    var point = await MyMapView.Editor.RequestPointAsync();

                    var symbol = _symbols[symbolCombo.SelectedIndex];
                    _graphicsLayer.Graphics.Add(new Graphic(point, symbol));
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        // Create text symbols - text for the symbol is the name of the font but could be anything
        private async Task SetupSymbolsAsync()
        {
            try
            {
                var fontFamilies = new List<string>()
                {
                    "Bogus",
                    "Algerian",
                    "Chiller",
                    "Comic Sans MS",
                    "Cooper",
                    "Elephant",
                    "Fixedsys",
                    "Forte",
                    "Jokerman",
                    "Lindsey",
                    "Mistral",
                    "Motorwerk",
                    "Old English Text MT",
                    "Parchment",
                    "Ravie",
                    "Script MT",
                    "Segoe Keycaps",
                    "Showcard Gothic",
                    "Snap ITC",
                    "Terminal",
                    "Vivaldi",
                    "Wingdings"
                };

                 // Create symbols from font list
                _symbols = fontFamilies
                    .Select(f => new TextSymbol()
                    {
                        Text = f,
                        Color = GetRandomColor(),
                        HorizontalTextAlignment = HorizontalTextAlignment.Center,
                        VerticalTextAlignment = VerticalTextAlignment.Middle,
                        Font = new SymbolFont(f, 16)
                    })
                    .ToList();

                // Create image swatches for the UI
                Task<ImageSource>[] swatchTasks = _symbols
                    .Select(sym => sym.CreateSwatchAsync(200, 24, 96.0, Colors.Transparent))
                    .ToArray();

                symbolCombo.ItemsSource = new List<ImageSource>(await Task.WhenAll(swatchTasks));
                symbolCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }

        // Utility function: Generate a random System.Windows.Media.Color
        private Color GetRandomColor()
        {
            var colorBytes = new byte[3];
            colorBytes[0] = (byte)(_random.Next(255) & 0x7F);
            colorBytes[1] = (byte)(_random.Next(255) & 0x7F);
            colorBytes[2] = (byte)(_random.Next(255) & 0x7F);
            return Color.FromArgb(0xFF, colorBytes[0], colorBytes[1], colorBytes[2]);
        }
    }
}
