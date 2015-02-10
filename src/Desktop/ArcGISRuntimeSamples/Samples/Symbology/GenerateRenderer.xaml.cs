using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample uses an ArcGIS Server Task to generate a Class Breaks Renderer with five classes. Note that this functionality requires an ArcGIS Server 10.1 instance. The generate renderer task can also generate unique value renderers and can also generate color ramp(s) from two or more colors.
    /// </summary>
    /// <title>Generate Renderer Task</title>
    /// <category>Symbology</category>
	public partial class GenerateRenderer : UserControl
    {
        private SimpleFillSymbol _baseSymbol;

        /// <summary>Construct Generate Renderer sample control</summary>
        public GenerateRenderer()
        {
            InitializeComponent();

            var lineSymbol = new SimpleLineSymbol() { Color = Colors.Black, Width = 0.5 };
            _baseSymbol = new SimpleFillSymbol() { Color = Colors.Transparent, Outline = lineSymbol, Style = SimpleFillStyle.Solid };

            MyMapView.ExtentChanged += MyMapView_ExtentChanged;
        }

        // Load data - set initial renderer after the map has an extent and feature layer loaded
        private async void MyMapView_ExtentChanged(object sender, EventArgs e)
        {
            try
            {
				MyMapView.ExtentChanged -= MyMapView_ExtentChanged;

				var table = featureLayer.FeatureTable as ServiceFeatureTable;
				table.MaxAllowableOffset = MyMapView.UnitsPerPixel;

				await MyMapView.LayersLoadedAsync();

				comboField.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Generate Renderer Task Sample");
            }
        }

        // When field to summarize on changes, generate a new renderer from the map service
        private async void comboField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Generate a new renderer
                GenerateRendererTask generateRenderer = new GenerateRendererTask(
                    new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/2"));

                var colorRamp = new ColorRamp()
                {
                    Algorithm = Algorithm.Hsv,
                    From = Color.FromRgb(0x99, 0x8E, 0xC3),
                    To = Color.FromRgb(0xF1, 0xA3, 0x40)
                };

                var classBreaksDef = new ClassBreaksDefinition()
                {
                    BreakCount = 5,
                    ClassificationField = ((ComboBoxItem)comboField.SelectedItem).Tag as string,
                    ClassificationMethod = ClassificationMethod.Quantile,
                    BaseSymbol = _baseSymbol,
                    ColorRamps = new ObservableCollection<ColorRamp>() { colorRamp }
                };

                var param = new GenerateRendererParameters()
                {
                    ClassificationDefinition = classBreaksDef,
                    Where = ((ServiceFeatureTable)featureLayer.FeatureTable).Where
                };

                var result = await generateRenderer.GenerateRendererAsync(param);

                featureLayer.Renderer = result.Renderer;

                // Reset the legend
                txtLegendTitle.DataContext = comboField.SelectedValue.ToString();
                await CreateLegend((ClassBreaksRenderer)result.Renderer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Generate Renderer Task Sample");
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
