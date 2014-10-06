using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Layers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Esri.ArcGISRuntime.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Dynamic Service Layers</category>
	public sealed partial class DynamicLayersInCode : Page
    {
        public DynamicLayersInCode()
        {
            this.InitializeComponent();
			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(-3548912, -1847469, 2472012, 1742990, SpatialReference.Create(102009)));
        }

        private void ApplyRangeValueClick(object sender, RoutedEventArgs e)
        {
            ClassBreaksRenderer newClassBreaksRenderer = new ClassBreaksRenderer();
            newClassBreaksRenderer.Field = "POP00_SQMI";
            var Infos = new ClassBreakInfoCollection();
            Infos.Add(new ClassBreakInfo()
            {

                Minimum = 0,
                Maximum = 12,
                Symbol = new SimpleFillSymbol()
                {
                    Color = Color.FromArgb(255, 0, 255, 0)
                }
            });

            Infos.Add(new ClassBreakInfo()
            {
                Maximum = 31.3,
                Symbol = new SimpleFillSymbol()
                {
                    Color = Color.FromArgb(255, 100, 255, 100)
                }
            });

            Infos.Add(new ClassBreakInfo()
            {
                Maximum = 59.7,
                Symbol = new SimpleFillSymbol()
                {
                    Color = Color.FromArgb(255, 0, 255, 200)
                }
            });

            Infos.Add(new ClassBreakInfo()
            {
                Maximum = 146.2,
                Symbol = new SimpleFillSymbol()
                {
                    Color = Color.FromArgb(255, 0, 255, 255)
                }
            });

            Infos.Add(new ClassBreakInfo()
            {
                Maximum = 57173,
                Symbol = new SimpleFillSymbol()
                {
                    Color = Color.FromArgb(255, 0, 0, 255)
                }
            });
            newClassBreaksRenderer.Infos = Infos;
            var layer = mapView1.Map.Layers["USA"] as ArcGISDynamicMapServiceLayer;
            LayerDrawingOptionCollection layerDrawingOptionCollection = new LayerDrawingOptionCollection()
            {
                new LayerDrawingOptions()
            {
                LayerID = 3,
                Renderer = newClassBreaksRenderer
            }
            };

            layer.LayerDrawingOptions = layerDrawingOptionCollection;

            layer.VisibleLayers = new ObservableCollection<int> { 3 };
        }

        private void ApplyUniqueValueClick(object sender, RoutedEventArgs e)
        {

            UniqueValueRenderer uvr = new UniqueValueRenderer();
            uvr.Fields = new ObservableCollection<string>(new string[] { "SUB_REGION" });
            uvr.DefaultSymbol = new SimpleFillSymbol() { Color = Colors.Gray };

            uvr.Infos.Add(new UniqueValueInfo { Values = new ObservableCollection<object>(new object[] { "Pacific"}), Symbol = new SimpleFillSymbol() { Color = Colors.Purple, Outline = new SimpleLineSymbol() { Color = Colors.Transparent } } });
            uvr.Infos.Add(new UniqueValueInfo { Values = new ObservableCollection<object>(new object[] { "W N Cen"}), Symbol = new SimpleFillSymbol() { Color = Colors.Green, Outline = new SimpleLineSymbol() { Color = Colors.Transparent } } });
            uvr.Infos.Add(new UniqueValueInfo { Values = new ObservableCollection<object>(new object[] { "W S Cen"}), Symbol = new SimpleFillSymbol() { Color = Colors.Cyan, Outline = new SimpleLineSymbol() { Color = Colors.Transparent } } });
            uvr.Infos.Add(new UniqueValueInfo { Values = new ObservableCollection<object>(new object[] { "E N Cen"}), Symbol = new SimpleFillSymbol() { Color = Colors.Yellow, Outline = new SimpleLineSymbol() { Color = Colors.Transparent } } });
            uvr.Infos.Add(new UniqueValueInfo { Values = new ObservableCollection<object>(new object[] { "Mtn"}), Symbol = new SimpleFillSymbol() { Color = Colors.Blue, Outline = new SimpleLineSymbol() { Color = Colors.Transparent } } });
            uvr.Infos.Add(new UniqueValueInfo { Values = new ObservableCollection<object>(new object[] { "N Eng"}), Symbol = new SimpleFillSymbol() { Color = Colors.Red, Outline = new SimpleLineSymbol() { Color = Colors.Transparent } } });
            uvr.Infos.Add(new UniqueValueInfo { Values = new ObservableCollection<object>(new object[] { "E S Cen"}), Symbol = new SimpleFillSymbol() { Color = Colors.Brown, Outline = new SimpleLineSymbol() { Color = Colors.Transparent } } });
            uvr.Infos.Add(new UniqueValueInfo { Values = new ObservableCollection<object>(new object[] { "Mid Atl"}), Symbol = new SimpleFillSymbol() { Color = Colors.Magenta, Outline = new SimpleLineSymbol() { Color = Colors.Transparent } } });
            uvr.Infos.Add(new UniqueValueInfo { Values = new ObservableCollection<object>(new object[] { "S Atl" }), Symbol = new SimpleFillSymbol() { Color = Colors.Orange, Outline = new SimpleLineSymbol() { Color = Colors.Transparent } } });

          

            var layer = mapView1.Map.Layers["USA"] as ArcGISDynamicMapServiceLayer;
            LayerDrawingOptionCollection layerDrawingOptionCollection = new LayerDrawingOptionCollection()
            {
                new LayerDrawingOptions()
            {
                LayerID = 2,
                Renderer = uvr
            }
            };

            layer.LayerDrawingOptions = layerDrawingOptionCollection;

            layer.VisibleLayers = new ObservableCollection<int> { 2 };
        }

        private void ChangeLayerOrderClick(object sender, RoutedEventArgs e)
        {
            var layer = mapView1.Map.Layers["USA"] as ArcGISDynamicMapServiceLayer;
            layer.LayerDrawingOptions.Clear();
            layer.DynamicLayerInfos = layer.CreateDynamicLayerInfosFromLayerInfos();
            var aDynamicLayerInfo = layer.DynamicLayerInfos[0];
            layer.DynamicLayerInfos.RemoveAt(0);
            layer.DynamicLayerInfos.Add(aDynamicLayerInfo);
        }

        private void AddLayerClick(object sender, RoutedEventArgs e)
        {
            var layer = mapView1.Map.Layers["USA"] as ArcGISDynamicMapServiceLayer;
            layer.LayerDrawingOptions.Clear();
            layer.DynamicLayerInfos = layer.CreateDynamicLayerInfosFromLayerInfos();
            layer.DynamicLayerInfos.Insert(0, new DynamicLayerInfo()
            {
                ID = 4,
                Source = new LayerDataSource()
                {
                    DataSource = new TableDataSource()
                    {
                        WorkspaceID = "MyDatabaseWorkspaceIDSSR2",
                        DataSourceName = "ss6.gdb.Lakes"
                    }
                }
            });
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

            layer.VisibleLayers.Add(3);
            layer.VisibleLayers.Add(4);
        }
    }
}