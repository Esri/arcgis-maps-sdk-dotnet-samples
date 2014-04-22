using System.Collections.Generic;
using System.Linq;
using Esri.ArcGISRuntime.Symbology;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Symbology</category>
	public sealed partial class SymbolSwatches : Page
    {
        public SymbolSwatches()
        {
            this.InitializeComponent();

        }
        protected override async void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
           
            float dpi = DisplayInformation.GetForCurrentView().LogicalDpi;
            List<ImageSource> swatches = new List<ImageSource>();
            foreach (var symbol in Resources.Values.OfType<Esri.ArcGISRuntime.Symbology.Symbol>())
            {
                if (symbol is MarkerSymbol)
                {
                    //For markersymbols we don't need to specify a size but can let the symbol decide based on its properties
                    //swatches.Add(new SymbolSwatchImageSource(symbol as MarkerSymbol, dpi));

                    swatches.Add(await symbol.CreateSwatchAsync());
                }
                else //Create a 50x50px swatch
                {
                    swatches.Add( await symbol.CreateSwatchAsync(50, 50, dpi, Colors.White));

                }
            }
            swatchesList.ItemsSource = swatches;
        }
    }
}
