using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates using an ArcGIS Server Task to generate a Class Breaks Renderer with five classes.
	/// The generate renderer task can also generate unique value renderers and can also generate color ramp(s) from two or more colors.
	/// This functionality requires an ArcGIS Server 10.1 instance.
	/// </summary>
	/// <title>Generate Renderer Task</title>
	/// <category>Symbology</category>
	public partial class GenerateRenderer : Windows.UI.Xaml.Controls.Page
	{
		private SimpleFillSymbol _baseSymbol;
		private FeatureLayer _featureLayer;

		/// <summary>Construct Generate Renderer sample control</summary>
		public GenerateRenderer()
		{
			InitializeComponent();

			var lineSymbol = new SimpleLineSymbol() { Color = Colors.Black, Width = 0.5 };
			_baseSymbol = new SimpleFillSymbol() { Color = Colors.Transparent, Outline = lineSymbol, Style = SimpleFillStyle.Solid };

			_featureLayer = MyMapView.Map.Layers["FeatureLayer"] as FeatureLayer;
			((ServiceFeatureTable)_featureLayer.FeatureTable).OutFields = new OutFields(
				new string[] { "POP2007, POP07_SQMI, WHITE, BLACK, AMERI_ES, ASIAN, HAWN_PI, OTHER, MULT_RACE, HISPANIC, STATE_NAME, NAME" });

			MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
		}

		// Load data - set initial renderer after the map has an extent and feature layer loaded
		private async void MyMapView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			try
			{
				MyMapView.SpatialReferenceChanged -= MyMapView_SpatialReferenceChanged;

				var table = _featureLayer.FeatureTable as ServiceFeatureTable;
				table.MaxAllowableOffset = MyMapView.UnitsPerPixel;

				await MyMapView.LayersLoadedAsync();

				comboField.SelectedIndex = 0;
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}

		// When field to summarize on changes, generate a new renderer from the map service
		private async void comboField_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
		{
			try
			{
				// Generate a new renderer
				GenerateRendererTask generateRenderer = new GenerateRendererTask(
					new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/2"));

				var colorRamp = new ColorRamp()
				{
					Algorithm = Algorithm.Hsv,
					From = Color.FromArgb(0xFF, 0x99, 0x8E, 0xC3),
					To = Color.FromArgb(0xFF, 0xF1, 0xA3, 0x40)
				};

				var classBreaksDef = new ClassBreaksDefinition()
				{
					BreakCount = 5,
					ClassificationField = ((Windows.UI.Xaml.Controls.ComboBoxItem)comboField.SelectedItem).Tag as string,
					ClassificationMethod = ClassificationMethod.Quantile,
					BaseSymbol = _baseSymbol,
					ColorRamps = new ObservableCollection<ColorRamp>() { colorRamp }
				};

				var param = new GenerateRendererParameters()
				{
					ClassificationDefinition = classBreaksDef,
					Where = ((ServiceFeatureTable)_featureLayer.FeatureTable).Where
				};

				var result = await generateRenderer.GenerateRendererAsync(param);

				_featureLayer.Renderer = result.Renderer;

				// Reset the legend
				txtLegendTitle.DataContext = comboField.SelectedValue.ToString();
				await CreateLegend((ClassBreaksRenderer)result.Renderer);
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}

		// Create a legend from the class breaks renderer
		private async Task CreateLegend(ClassBreaksRenderer renderer)
		{
			var tasks = renderer.Infos.Select(info => info.Symbol.CreateSwatchAsync());
			var images = await Task.WhenAll(tasks);

			listLegend.ItemsSource = renderer.Infos
				.Select((info, idx) => new ClassBreakLegendItem() { SymbolImage = images[idx], Label = info.Label });
		}
	}

	// class for the legend items
	internal class ClassBreakLegendItem
	{
		public ImageSource SymbolImage { get; set; }
		public string Label { get; set; }
	}

}
