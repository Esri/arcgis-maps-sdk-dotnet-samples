using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Shows how to create MarkerySymbols (both SimpleMarkerSymbol and PictureMarkerSymbol) and add point graphics using the symbols.
	/// </summary>
	/// <title>Marker Symbols</title>
	/// <category>Symbology</category>
	public partial class MarkerSymbols : Windows.UI.Xaml.Controls.Page
	{
		private List<MarkerSymbol> _symbols;
		private GraphicsOverlay _graphicsOverlay;

		/// <summary>Construct Marker Symbols sample control</summary>
		public MarkerSymbols()
		{
			InitializeComponent();

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];

			MyMapView.ExtentChanged += MyMapView_ExtentChanged;
		}

		// Start map interaction
		private async void MyMapView_ExtentChanged(object sender, EventArgs e)
		{
			MyMapView.ExtentChanged -= MyMapView_ExtentChanged;

			await SetupSymbolsAsync();
			await AcceptPointsAsync();
		}

		// Accept user map clicks and add points to the graphics layer with the selected symbol
		private async Task AcceptPointsAsync()
		{
			while (MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry.Extent != null)
			{
				var point = await MyMapView.Editor.RequestPointAsync();
				_graphicsOverlay.Graphics.Add(new Graphic(point, _symbols[symbolCombo.SelectedIndex]));
			}
		}

		// Create marker symbols
		private async Task SetupSymbolsAsync()
		{
			try
			{
				int size = Convert.ToInt32(LayoutRoot.Resources["ImageSize"]);
				int sizePt = (int)((float)size / DisplayInformation.GetForCurrentView().LogicalDpi * 72);

				// Create simple marker symbols
				var blackOutlineSymbol = new SimpleLineSymbol() { Color = Colors.Black, Style = SimpleLineStyle.Solid, Width = 1 };

				_symbols = new List<MarkerSymbol>()
				{
					new SimpleMarkerSymbol() { Color = Colors.Red, Size = sizePt, Style = SimpleMarkerStyle.Circle, Outline = blackOutlineSymbol },
					new SimpleMarkerSymbol() { Color = Colors.Green, Size = sizePt, Style = SimpleMarkerStyle.Diamond, Outline = blackOutlineSymbol },
					new SimpleMarkerSymbol() { Color = Colors.Blue, Size = sizePt, Style = SimpleMarkerStyle.Square, Outline = blackOutlineSymbol },
					new SimpleMarkerSymbol() { Color = Colors.Purple, Size = sizePt, Style = SimpleMarkerStyle.X, Outline = blackOutlineSymbol },
				};

				// Set image sources for picture marker symbols
				List<Task> setSourceTasks = new List<Task>();

				var stickPinSymbol = new PictureMarkerSymbol() { Width = size, Height = size };
				setSourceTasks.Add(stickPinSymbol.SetSourceAsync(new Uri("ms-appx:///ArcGISRuntimeSamplesStore/Assets/RedStickpin.png")));
				_symbols.Add(stickPinSymbol);

				var pushPinSymbol = new PictureMarkerSymbol() { Width = size, Height = size };
				setSourceTasks.Add(pushPinSymbol.SetSourceAsync(new Uri("http://static.arcgis.com/images/Symbols/Basic/RedShinyPin.png")));
				_symbols.Add(pushPinSymbol);

				var xPictureSymbol = new PictureMarkerSymbol() { Width = size, Height = size };
				setSourceTasks.Add(xPictureSymbol.SetSourceAsync(new Uri("ms-appx:///ArcGISRuntimeSamplesStore/Assets/x-24x24.png")));
				_symbols.Add(xPictureSymbol);

				await Task.WhenAll(setSourceTasks);

				// Create image swatches for the UI
				Task<ImageSource>[] swatchTasks = _symbols
					.Select(sym => sym.CreateSwatchAsync())
					.ToArray();

				symbolCombo.ItemsSource = await Task.WhenAll(swatchTasks);
				symbolCombo.SelectedIndex = 0;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error: " + ex.Message);
			}
		}
	}
}
