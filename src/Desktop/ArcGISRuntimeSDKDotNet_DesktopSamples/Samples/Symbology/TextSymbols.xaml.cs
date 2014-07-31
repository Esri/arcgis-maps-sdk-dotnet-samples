using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Sample shows how to create Text Symbols (TextSymbol) and add graphics using the symbols.
    /// </summary>
    /// <title>Text Symbols</title>
	/// <category>Symbology</category>
	public partial class TextSymbols : UserControl
    {
        private Random _random = new Random();
        private List<TextSymbol> _symbols;

        /// <summary>Construct Text Symbols sample control</summary>
        public TextSymbols()
        {
            InitializeComponent();

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
        private async void symbolCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                    graphicsLayer.Graphics.Add(new Graphic(point, symbol));
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Text Symbols");
            }
        }

        // Create text symbols - text for the symbol is the name of the font but could be anything
        private async Task SetupSymbolsAsync()
        {
            try
            {
                // Create symbols from 30 random fonts
                _symbols = Fonts.SystemFontFamilies
                    .Where(f => f.Baseline < 1.0)
                    .OrderBy(x => Guid.NewGuid())
                    .Take(30)
                    .Select((f,idx) => new TextSymbol()
                    {
                        Text = f.Source,
                        Color = GetRandomColor(),
                        HorizontalTextAlignment = HorizontalTextAlignment.Center,
                        VerticalTextAlignment = VerticalTextAlignment.Middle,
                        Font = new SymbolFont(f.Source, 14)
                    })
                    .ToList();

                // Create image swatches for the UI
                Task<ImageSource>[] swatchTasks = _symbols
                    .Select(sym => sym.CreateSwatchAsync(200, 24, 96.0, Colors.Transparent))
                    .ToArray();

                symbolCombo.ItemsSource = new List<ImageSource>(await Task.WhenAll(swatchTasks));
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
            _random.NextBytes(colorBytes);
            return Color.FromRgb(colorBytes[0], colorBytes[1], colorBytes[2]);
        }
    }
}
