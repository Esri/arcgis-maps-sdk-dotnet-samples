using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Resources;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates various Graphics and GraphicsLayer related actions.
    /// </summary>
	/// <category>Layers</category>
	/// <subcategory>Graphics Layers</subcategory>
	public partial class GraphicsLayerWindow : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsLayerWindow"/> class.
        /// </summary>
        public GraphicsLayerWindow()
        {
            InitializeComponent();
        }
        private async void OnGraphicsLayerAddClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                CsvLayer csvLayer = new CsvLayer();

                Uri uri = new Uri("./Data/earthquake_csv_data.txt", UriKind.Relative);
                StreamResourceInfo sri = Application.GetContentStream(uri);
                if (sri != null)
                {
                    using (Stream s = sri.Stream)
                    {
                        await csvLayer.SetSourceAsync(s).ConfigureAwait(true);
                        csvLayer.Renderer = LayoutRoot.Resources["MyClassBreaksRenderer"] as ClassBreaksRenderer;
                        map1.Layers.Add(csvLayer);
                        await csvLayer.InitializeAsync();
                    }
                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void OnGraphicAddClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                GraphicsLayer gLayer = map1.Layers["EventsGraphicsLayer"] as GraphicsLayer;
                if (gLayer != null)
                    map1.Layers.Remove(gLayer);

                GraphicsLayer graphicsLayer = new GraphicsLayer()
                {
                    ID = "SimpleGraphicsLayer"
                };

                graphicsLayer.Graphics.Add(new Graphic()
                {
                    Geometry = new MapPoint(-74.0064, 40.7142),
                    Symbol = new SimpleMarkerSymbol()
                    {
                        Style = SimpleMarkerStyle.Circle,
                        Color = Colors.Red,
                        Size = 8
                    }
                });
                map1.Layers.Add(graphicsLayer);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void OnSwitchGraphicsSourceClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                GraphicsLayer gLayer = map1.Layers["EventsGraphicsLayer"] as GraphicsLayer;
                if (gLayer != null)
                    map1.Layers.Remove(gLayer);

                GraphicsLayer graphicsLayer = new GraphicsLayer()
                {
                    ID = "SimpleGraphicsLayer"
                };
                if (graphicsLayer != null)
                {
                    List<Graphic> newGraphics = new List<Graphic>();
                    newGraphics.Add(new Graphic()
                    {
                        Geometry = new MapPoint(-74.0064, 40.7142),
                        Symbol = new SimpleMarkerSymbol()
                        {
                            Style = SimpleMarkerStyle.Circle,
                            Color = Colors.Yellow,
                            Size = 8
                        }
                    });

                    graphicsLayer.GraphicsSource = newGraphics;
                    map1.Layers.Add(graphicsLayer);

                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void OnCheckEventsClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                GraphicsLayer gLayer = map1.Layers["SimpleGraphicsLayer"] as GraphicsLayer;
                if (gLayer != null)
                    map1.Layers.Remove(gLayer);

                GraphicsLayer gLayer2 = map1.Layers["EventsGraphicsLayer"] as GraphicsLayer;
                if (gLayer2 != null)
                    map1.Layers.Remove(gLayer2);

                GraphicsLayer graphicsLayer = new GraphicsLayer()
                {
                    ID = "EventsGraphicsLayer"
                };
                if (graphicsLayer != null)
                {
                    List<Graphic> newGraphics = new List<Graphic>();
                    newGraphics.Add(new Graphic()
                    {
                        Geometry = new MapPoint(-74.0064, 40.7142),
                        Symbol = new SimpleMarkerSymbol()
                        {
                            Style = SimpleMarkerStyle.Circle,
                            Color = Colors.Yellow,
                            Size = 8
                        }
                    });

                    graphicsLayer.GraphicsSource = newGraphics;
                    //graphicsLayer.MouseDown += (s, a) =>
                    //{
                    //    OutputTextBox.Text = "Mouse down";

                    //};

                    //graphicsLayer.MouseUp += (s, a) =>
                    //{
                    //    OutputTextBox.Text = "Mouse Up";
                    //};
                    map1.Layers.Add(graphicsLayer);

                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void OnApplyRendererClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                IEnumerable<GraphicsLayer> graphicLayers = map1.Layers.OfType<GraphicsLayer>();
                List<GraphicsLayer> gLayers = new List<GraphicsLayer>();

                if (graphicLayers != null)
                {
                    foreach (GraphicsLayer l in graphicLayers)
                        gLayers.Add(l);

                    foreach (GraphicsLayer l in gLayers)
                        map1.Layers.Remove(l);
                }

                GraphicsLayer graphicsLayer = new GraphicsLayer()
                {
                    ID = "SimpleGraphicsLayer"
                };

                graphicsLayer.Graphics.Add(new Graphic()
                {
                    Geometry = new MapPoint(-74.0064, 40.7142),

                });
                graphicsLayer.Renderer = LayoutRoot.Resources["SimpleRenderer"] as SimpleRenderer;
                graphicsLayer.RenderingMode = GraphicsRenderingMode.Static;
                graphicsLayer.SelectionColor = Colors.Yellow;
                map1.Layers.Add(graphicsLayer);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void OnTestSelectionColorClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                IEnumerable<GraphicsLayer> graphicLayers = map1.Layers.OfType<GraphicsLayer>();
                List<GraphicsLayer> gLayers = new List<GraphicsLayer>();

                if (graphicLayers != null)
                {
                    foreach (GraphicsLayer l in graphicLayers)
                        gLayers.Add(l);

                    foreach (GraphicsLayer l in gLayers)
                        map1.Layers.Remove(l);
                }

                GraphicsLayer graphicsLayer = new GraphicsLayer()
                {
                    ID = "SimpleGraphicsLayer"
                };

                graphicsLayer.Graphics.Add(new Graphic()
                {
                    Geometry = new MapPoint(-74.0064, 40.7142),
                    IsSelected = true
                });
                graphicsLayer.Renderer = LayoutRoot.Resources["SimpleRenderer"] as SimpleRenderer;
                graphicsLayer.SelectionColor = Colors.Yellow;
               
                map1.Layers.Add(graphicsLayer);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void OnAddGraphicRangeClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                IEnumerable<GraphicsLayer> graphicLayers = map1.Layers.OfType<GraphicsLayer>();
                List<GraphicsLayer> gLayers = new List<GraphicsLayer>();

                if (graphicLayers != null)
                {
                    foreach (GraphicsLayer l in graphicLayers)
                        gLayers.Add(l);

                    foreach (GraphicsLayer l in gLayers)
                        map1.Layers.Remove(l);
                }

                GraphicsLayer graphicsLayer = new GraphicsLayer()
                {
                    ID = "SimpleGraphicsLayer"
                };

                List<Graphic> graphics=new List<Graphic>()
                {
                    new Graphic()
                {
                    Geometry = new MapPoint(-74.0064, 40.7142),
                   
                }
                };
                graphicsLayer.Graphics.AddRange(graphics);
                graphicsLayer.Renderer = LayoutRoot.Resources["SimpleRenderer"] as SimpleRenderer;
             
                map1.Layers.Add(graphicsLayer);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void OnInsertGraphicClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                IEnumerable<GraphicsLayer> graphicLayers = map1.Layers.OfType<GraphicsLayer>();
                List<GraphicsLayer> gLayers = new List<GraphicsLayer>();

                if (graphicLayers != null)
                {
                    foreach (GraphicsLayer l in graphicLayers)
                        gLayers.Add(l);

                    foreach (GraphicsLayer l in gLayers)
                        map1.Layers.Remove(l);
                }

                GraphicsLayer graphicsLayer = new GraphicsLayer()
                {
                    ID = "SimpleGraphicsLayer"
                };

                graphicsLayer.Graphics.Insert(0, new Graphic()
                {
                    Geometry = new MapPoint(-74.0064, 40.7142),

                });
                
                graphicsLayer.Renderer = LayoutRoot.Resources["SimpleRenderer"] as SimpleRenderer;

                map1.Layers.Add(graphicsLayer);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void OnClearGraphicItemClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                
                GraphicsLayer graphicsLayer = map1.Layers["SimpleGraphicsLayer"] as GraphicsLayer;

                if (graphicsLayer != null && graphicsLayer.Graphics.Count>0)
                {
                    graphicsLayer.Graphics.Clear();
                }

              
            }
            catch (Exception ex)
            {

                throw;
            }
        }

     

       
    }
}
