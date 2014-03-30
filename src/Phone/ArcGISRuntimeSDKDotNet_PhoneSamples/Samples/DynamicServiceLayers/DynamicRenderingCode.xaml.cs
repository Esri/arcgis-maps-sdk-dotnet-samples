using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Linq;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Dynamic Service Layers</category>
	public partial class DynamicRenderingCode : PhoneApplicationPage
    {
        public DynamicRenderingCode()
        {
            InitializeComponent();
        }

        // Shows counties by population density when the Class Breaks button is clicked
        private void ClassBreaksButton_Click(object sender, RoutedEventArgs e)
        {
            ClassBreaksRenderer cbr = new ClassBreaksRenderer();

            // Symbolize based on the population density field
            cbr.Field = "POP00_SQMI";
            var Infos = new ClassBreakInfoCollection();
            SimpleLineSymbol outline = new SimpleLineSymbol() { Color = Colors.LightGray };

            #region Define Class Breaks

            // Create five class breaks, with symbols ranging from green for the lowest density to
            // blue for the highest
            cbr.Infos.Add(new ClassBreakInfo()
            {

                Minimum = 0,
                Maximum = 12,
                Symbol = new SimpleFillSymbol()
                {
                    Color = Color.FromArgb(255, 0, 255, 0),
                    Outline = outline
                }
            });

            cbr.Infos.Add(new ClassBreakInfo()
            {
                Maximum = 31.3,
                Symbol = new SimpleFillSymbol()
                {
                    Color = Color.FromArgb(255, 100, 255, 100),
                    Outline = outline
                }
            });

            cbr.Infos.Add(new ClassBreakInfo()
            {
                Maximum = 59.7,
                Symbol = new SimpleFillSymbol()
                {
                    Color = Color.FromArgb(255, 0, 255, 200),
                    Outline = outline
                }
            });

            cbr.Infos.Add(new ClassBreakInfo()
            {
                Maximum = 146.2,
                Symbol = new SimpleFillSymbol()
                {
                    Color = Color.FromArgb(255, 0, 255, 255),
                    Outline = outline
                }
            });

            cbr.Infos.Add(new ClassBreakInfo()
            {
                Maximum = 57173,
                Symbol = new SimpleFillSymbol()
                {
                    Color = Color.FromArgb(255, 0, 0, 255),
                    Outline = outline
                }
            });

            #endregion

            // Get the USA layer from the map and apply the class breaks renderer to the counties layer
            var layer = map1.Layers["USA"] as ArcGISDynamicMapServiceLayer;
            if (layer.LayerDrawingOptions == null)
                layer.LayerDrawingOptions = new LayerDrawingOptionCollection();
            layer.LayerDrawingOptions.Add(new LayerDrawingOptions()
            {
                LayerID = 3, // counties layer has an ID of 3
                Renderer = cbr
            });

            // Make it so only the counties layer is visible
            layer.VisibleLayers = new ObservableCollection<int>(new int[] { 3 });
        }

        // Shows states by sub-region when the Unique Values button is clicked
        private void UniqueValueButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a unique value renderer 
            UniqueValueRenderer uvr = new UniqueValueRenderer();

            // Symbolize based on the sub-region field
            uvr.Fields = new ObservableCollection<string>(new string[] { "SUB_REGION" });
            uvr.DefaultSymbol = new SimpleFillSymbol() { Color = Colors.Gray };

            SimpleLineSymbol outline = new SimpleLineSymbol() { Color = Colors.LightGray, Width = 2 };

            #region Define Unique Values

            // Create unique value info for each sub-region
            uvr.Infos.Add(new UniqueValueInfo 
            { 
                Values = new ObservableCollection<object>(new object[]{ "Pacific" }), 
                Symbol = new SimpleFillSymbol() 
                { 
                    Color = Colors.Purple, 
                    Outline = outline 
                } 
            });

            uvr.Infos.Add(new UniqueValueInfo 
            { 
                Values = new ObservableCollection<object>(new object[]{ "W N Cen" }), 
                Symbol = new SimpleFillSymbol() 
                { 
                    Color = Colors.Green, 
                    Outline = outline
                } 
            });

            uvr.Infos.Add(new UniqueValueInfo 
            { 
                Values = new ObservableCollection<object>(new object[]{ "W S Cen" }), 
                Symbol = new SimpleFillSymbol() 
                { 
                    Color = Colors.Cyan, 
                    Outline = outline 
                } 
            });

            uvr.Infos.Add(new UniqueValueInfo 
            { 
                Values = new ObservableCollection<object>(new object[]{ "E N Cen" }), 
                Symbol = new SimpleFillSymbol() 
                { 
                    Color = Colors.Yellow, 
                    Outline = outline 
                } 
            });

            uvr.Infos.Add(new UniqueValueInfo 
            { 
                Values = new ObservableCollection<object>(new object[]{ "Mtn" }), 
                Symbol = new SimpleFillSymbol() 
                { 
                    Color = Colors.Blue, 
                    Outline = outline 
                } 
            });

            uvr.Infos.Add(new UniqueValueInfo 
            { 
                Values = new ObservableCollection<object>(new object[]{ "N Eng" }), 
                Symbol = new SimpleFillSymbol() 
                { 
                    Color = Colors.Red, 
                    Outline = outline 
                } 
            });

            uvr.Infos.Add(new UniqueValueInfo 
            { 
                Values = new ObservableCollection<object>(new object[]{ "E S Cen" }), 
                Symbol = new SimpleFillSymbol() 
                { 
                    Color = Colors.Brown, 
                    Outline = outline
                } 
            });

            uvr.Infos.Add(new UniqueValueInfo 
            { 
                Values = new ObservableCollection<object>(new object[]{ "Mid Atl" }), 
                Symbol = new SimpleFillSymbol() 
                { 
                    Color = Colors.Magenta, 
                    Outline = outline
                } 
            });
            
            uvr.Infos.Add(new UniqueValueInfo 
            { 
                Values = new ObservableCollection<object>(new object[]{ "S Atl" }), 
                Symbol = new SimpleFillSymbol() 
                { 
                    Color = Colors.Orange, 
                    Outline = outline 
                } 
            });

            #endregion

            // Get the USA layer and apply the unique values renderer to the states layer
            var layer = map1.Layers["USA"] as ArcGISDynamicMapServiceLayer;
            if (layer.LayerDrawingOptions == null)
                layer.LayerDrawingOptions = new LayerDrawingOptionCollection();
            layer.LayerDrawingOptions.Add(new LayerDrawingOptions()
            {
                LayerID = 2, // The states layer has an ID of 2
                Renderer = uvr
            });

            // Make the states layer the only visible one
            layer.VisibleLayers = new ObservableCollection<int>(new int[] { 2 });
        }

        // Move the top layer to the bottom
        private void LayerOrderButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the USA layer
            var layer = map1.Layers["USA"] as ArcGISDynamicMapServiceLayer;

            // Show all layers
            if (layer.VisibleLayers != null)
                layer.VisibleLayers = null;

            // Get the default dynamic rendering for the service layer
            if (layer.DynamicLayerInfos == null || layer.DynamicLayerInfos.Count == 0)
                layer.DynamicLayerInfos = layer.CreateDynamicLayerInfosFromLayerInfos();

            // Get the dynamic info for the first (top) layer and move it to the  end (bottom)
            var aDynamicLayerInfo = layer.DynamicLayerInfos[0];
            layer.DynamicLayerInfos.RemoveAt(0);
            layer.DynamicLayerInfos.Add(aDynamicLayerInfo);
        }

        // Adds a lakes layer to the map service layer
        private void AddLayerButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the USA layer
            var layer = map1.Layers["USA"] as ArcGISDynamicMapServiceLayer;

            // Get the default dynamic rendering for the service layer
            if (layer.DynamicLayerInfos == null || layer.DynamicLayerInfos.Count == 0)
                layer.DynamicLayerInfos = layer.CreateDynamicLayerInfosFromLayerInfos();

            // Add dynamic rendering info for the lakes layer
            layer.DynamicLayerInfos.Insert(0, new DynamicLayerInfo()
            {
                ID = 4,
                // Specify the workspace and table containing the lakes data
                Source = new LayerDataSource()
                {
                    DataSource = new TableDataSource()
                    {
                        WorkspaceID = "MyDatabaseWorkspaceIDSSR2",
                        DataSourceName = "ss6.gdb.Lakes"
                    }
                }
            });
            if (layer.LayerDrawingOptions == null)
                layer.LayerDrawingOptions = new LayerDrawingOptionCollection();
            layer.LayerDrawingOptions.Add(new LayerDrawingOptions()
            {
                LayerID = 4,
                Renderer = new SimpleRenderer()
                {
                    Symbol = new SimpleFillSymbol()
                    {
                        Color = Color.FromArgb((int)255, (int)0, (int)0, (int)255)
                    }
                }
            });

            if (layer.VisibleLayers == null)
                layer.VisibleLayers = new ObservableCollection<int>(
                    layer.ServiceInfo.Layers.Select(l => l.ID));

            layer.VisibleLayers.Add(4);
        }
    }
}