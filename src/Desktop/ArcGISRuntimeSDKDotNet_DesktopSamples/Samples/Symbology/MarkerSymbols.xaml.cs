using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Sample shows how to create MarkerySymbols (both SimpleMarkerSymbol and PictureMarkerSymbol) and add point graphics using the symbols.
    /// </summary>
    /// <title>Marker Symbols</title>
	/// <category>Symbology</category>
	public partial class MarkerSymbols : UserControl
    {
        private List<MarkerSymbol> _symbols;

        /// <summary>Construct Marker Symbols sample control</summary>
        public MarkerSymbols()
        {
            InitializeComponent();

            mapView.ExtentChanged += mapView_ExtentChanged;
        }

        // Start map interaction
        private async void mapView_ExtentChanged(object sender, EventArgs e)
        {
            mapView.ExtentChanged -= mapView_ExtentChanged;

            await SetupSymbolsAsync();
            DataContext = this;

            await AcceptPointsAsync();
        }

        // Accept user map clicks and add points to the graphics layer with the selected symbol
        private async Task AcceptPointsAsync()
        {
            while (mapView.Extent != null)
            {
                var point = await mapView.Editor.RequestPointAsync();
                graphicsLayer.Graphics.Add(new Graphic(point, _symbols[symbolCombo.SelectedIndex]));
            }
        }

        // Create marker symbols
        private async Task SetupSymbolsAsync()
        {
            try
            {
                const int size = 24;

                // Create simple marker symbols
                var blackOutlineSymbol = new SimpleLineSymbol() { Color = Colors.Black, Style = SimpleLineStyle.Solid, Width = 1 };

                _symbols = new List<MarkerSymbol>()
                {
                    new SimpleMarkerSymbol() { Color = Colors.Red, Size = 15, Style = SimpleMarkerStyle.Circle, Outline = blackOutlineSymbol },
                    new SimpleMarkerSymbol() { Color = Colors.Green, Size = 15, Style = SimpleMarkerStyle.Diamond, Outline = blackOutlineSymbol },
                    new SimpleMarkerSymbol() { Color = Colors.Blue, Size = 15, Style = SimpleMarkerStyle.Square, Outline = blackOutlineSymbol },
                    new SimpleMarkerSymbol() { Color = Colors.Purple, Size = 15, Style = SimpleMarkerStyle.X, Outline = blackOutlineSymbol },
                };

                // Set image sources for picture marker symbols
                List<Task> setSourceTasks = new List<Task>();

                var stickPinSymbol = new PictureMarkerSymbol() { Width = size, Height = size, XOffset = 0, YOffset = 0 };
                setSourceTasks.Add(stickPinSymbol.SetSourceAsync(new Uri("pack://application:,,,/ArcGISRuntimeSDKDotNet_DesktopSamples;component/Assets/RedStickpin.png")));
                _symbols.Add(stickPinSymbol);

                var pushPinSymbol = new PictureMarkerSymbol() { Width = size, Height = size, XOffset = 0, YOffset = 0 };
                setSourceTasks.Add(pushPinSymbol.SetSourceAsync(new Uri("pack://application:,,,/ArcGISRuntimeSDKDotNet_DesktopSamples;component/Assets/RedPushpin.png")));
                _symbols.Add(pushPinSymbol);

                var xPictureSymbol = new PictureMarkerSymbol() { Width = size, Height = size, XOffset = 0, YOffset = 0 };
                setSourceTasks.Add(xPictureSymbol.SetSourceAsync(new Uri("pack://application:,,,/ArcGISRuntimeSDKDotNet_DesktopSamples;component/Assets/x-24x24.png")));
                _symbols.Add(xPictureSymbol);

                await Task.WhenAll(setSourceTasks);

                // Create image swatches for the UI
                Task<ImageSource>[] swatchTasks = _symbols.OfType<SimpleMarkerSymbol>()
                    .Select(sym => sym.CreateSwatchAsync(size, size, 96.0, Colors.Transparent))
                    .ToArray();

                var imageSources = new List<ImageSource>(await Task.WhenAll(swatchTasks));

                // Manually create swatches for the picture marker symbols
                imageSources.Add(LoadImage("pack://application:,,,/ArcGISRuntimeSDKDotNet_DesktopSamples;component/Assets/RedStickpin.png", size));
                imageSources.Add(LoadImage("pack://application:,,,/ArcGISRuntimeSDKDotNet_DesktopSamples;component/Assets/RedPushpin.png", size));
                imageSources.Add(LoadImage("pack://application:,,,/ArcGISRuntimeSDKDotNet_DesktopSamples;component/Assets/x-24x24.png", size));

                symbolCombo.ItemsSource = imageSources;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }

        // Loads and resizes embedded image
        private ImageSource LoadImage(string path, int size)
        {
            BitmapImage source = new BitmapImage();
            source.BeginInit();
            source.UriSource = new Uri(path);
            source.DecodePixelHeight = size;
            source.DecodePixelWidth = size;
            source.EndInit();
            return source;
        }
    }
}
