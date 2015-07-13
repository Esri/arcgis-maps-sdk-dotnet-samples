using Esri.ArcGISRuntime.Controls;
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

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates how to create Line and Fill Symbol (SimpleLineSymbol, SimpleFillSymbol, PictureFillSymbol) and add graphics using the symbols.
	/// </summary>
	/// <title>Line and Fill Symbols</title>
	/// <category>Symbology</category>
	public partial class LineFillSymbols : Windows.UI.Xaml.Controls.Page
	{
		private List<SampleSymbol> _symbols;
		private GraphicsOverlay _graphicsOverlay;

		/// <summary>Construct Line and Fill Symbols sample control</summary>
		public LineFillSymbols()
		{
			InitializeComponent();

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
			MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
		}

		// Start map interaction
		private async void MyMapView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			MyMapView.SpatialReferenceChanged -= MyMapView_SpatialReferenceChanged;

			await SetupSymbolsAsync();
			await AcceptPointsAsync();
		}

		// Cancel current shape request when the symbol selection changes 
		private async void symbolCombo_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
		{
			if (!MyMapView.Editor.IsActive)
				return;

			MyMapView.Editor.Cancel.Execute(null);
			await AcceptPointsAsync();
		}

		// Accept user map clicks and add points to the graphics layer with the selected symbol
		private async Task AcceptPointsAsync()
		{
			try
			{
				while (MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry.Extent != null)
				{
					SampleSymbol sampleSymbol = _symbols[symbolCombo.SelectedIndex];

					Esri.ArcGISRuntime.Geometry.Geometry shape = null;
					if (sampleSymbol.Symbol is LineSymbol)
						shape = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polyline, sampleSymbol.Symbol);
					else
						shape = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polygon, sampleSymbol.Symbol);

					_graphicsOverlay.Graphics.Add(new Graphic(shape, sampleSymbol.Symbol));
					await Task.Delay(100);
				}
			}
			catch (TaskCanceledException)
			{
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}

		// Create line / fill symbols
		private async Task SetupSymbolsAsync()
		{
			try
			{
				int size = Convert.ToInt32(LayoutRoot.Resources["ImageSize"]);

				// Create symbols
				var blackOutlineSymbol = new SimpleLineSymbol() { Color = Colors.Black, Style = SimpleLineStyle.Solid, Width = 1 };

				_symbols = new List<SampleSymbol>()
				{
					new SampleSymbol(new SimpleLineSymbol() { Color = Colors.Black, Style = SimpleLineStyle.Solid, Width = 2 }),
					new SampleSymbol(new SimpleLineSymbol() { Color = Colors.Red, Style = SimpleLineStyle.Dash, Width = 2 }),
					new SampleSymbol(new SimpleLineSymbol() { Color = Colors.Blue, Style = SimpleLineStyle.DashDot, Width = 4 }),

					new SampleSymbol(new SimpleFillSymbol() { Color = Colors.Red, Style = SimpleFillStyle.Solid }),
					new SampleSymbol(new SimpleFillSymbol() { Color = Color.FromArgb(100, 0, 255, 0), Style = SimpleFillStyle.DiagonalCross, Outline = blackOutlineSymbol }),
					new SampleSymbol(new SimpleFillSymbol() { Color = Color.FromArgb(100, 0, 0, 255), Style = SimpleFillStyle.Vertical, Outline = blackOutlineSymbol }),

					new SampleSymbol(new PictureFillSymbol() { Outline = blackOutlineSymbol, Width = 24, Height = 24 }, "ms-appx:///ArcGISRuntimeSamplesPhone/Assets/x-24x24.png"),
					new SampleSymbol(new PictureFillSymbol() { Outline = blackOutlineSymbol, Width = 24, Height = 24 }, "http://static.arcgis.com/images/Symbols/Cartographic/esriCartographyMarker_79_Blue.png")
				};

				// Set image sources for picture fill symbols
				await Task.WhenAll(_symbols.Where(s => s.Symbol is PictureFillSymbol)
					.Select(s => ((PictureFillSymbol)s.Symbol).SetSourceAsync(s.SymbolUri)));

				// Create image swatches for the UI
				Task<ImageSource>[] swatchTasks = _symbols
					.Select(sym => sym.Symbol.CreateSwatchAsync(size, size, 96.0, Colors.Transparent))
					.ToArray();

				symbolCombo.ItemsSource = new List<ImageSource>(await Task.WhenAll(swatchTasks));
				symbolCombo.SelectedIndex = 0;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error: " + ex.Message);
			}
		}
	}

	/// <summary>Local symbol class</summary>
	internal class SampleSymbol
	{
		/// <summary>Symbol</summary>
		public Symbol Symbol { get; set; }

		/// <summary>Uri for picture symbols</summary>
		public Uri SymbolUri { get; set; }

		/// <summary>Swatch for UI</summary>
		public ImageSource Swatch { get; set; }

		/// <summary>Construct sample symbol object</summary>
		public SampleSymbol(Symbol symbol, string source = "")
		{
			Symbol = symbol;

			if (!string.IsNullOrEmpty(source))
				SymbolUri = new Uri(source);
		}
	}
}
