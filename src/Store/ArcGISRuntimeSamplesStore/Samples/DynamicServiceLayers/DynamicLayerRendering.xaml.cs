using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Sample of using the GenerateRendererTask to create a class breaks or unique value renderer on the server without having to retrieve all the data to the client.
    /// </summary>
    /// <title>Dynamic Layer Rendering</title>
    /// <category>Dynamic Service Layers</category>
	public sealed partial class DynamicLayerRendering : Page
    {
        private GenerateRendererTask generateRendererTask;

        public DynamicLayerRendering()
        {
            this.InitializeComponent();

			MyMapView.Map.SpatialReference = SpatialReferences.WebMercator;

            var layer = MyMapView.Map.Layers.OfType<ArcGISDynamicMapServiceLayer>().FirstOrDefault();
            layer.VisibleLayers = new ObservableCollection<int>() { 2 };

            // Create GenerateRendererTask for a specific map layer
            generateRendererTask = new GenerateRendererTask(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/2"));
        }

        private async void GenerateRangeValueClick(object sender, RoutedEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;

                ClassBreaksDefinition classBreaksDefinition = new ClassBreaksDefinition()
                {
                    ClassificationField = "SQMI",
                    ClassificationMethod = ClassificationMethod.StandardDeviation,
                    BreakCount = 5,
                    StandardDeviationInterval = StandardDeviationInterval.OneQuarter
                };

                classBreaksDefinition.ColorRamps.Add(new ColorRamp()
                {
                    From = Colors.Blue,
                    To = Colors.Red,
                    Algorithm = Algorithm.Hsv
                });

                // Create a new GenerateRendererParameter object and set the ClassificationDefinition
                // Also specify a Where clause on the layer to demonstrate excluding features from the classification
                GenerateRendererParameters rendererParam = new GenerateRendererParameters()
                {
                    ClassificationDefinition = classBreaksDefinition,
                    Where = "STATE_NAME NOT IN ('Alaska', 'Hawaii')"
                };

                await GenerateRenderer(rendererParam);
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog("Error generating renderer: " + ex.Message).ShowAsync();
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }

        private async void GenerateUniqueValueClick(object sender, RoutedEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;

                UniqueValueDefinition uniqueValueDefinition = new UniqueValueDefinition()
                {
                    Fields = new List<string>() { "STATE_NAME" }
                };

                uniqueValueDefinition.ColorRamps.Add(new ColorRamp()
                {
                    From = Colors.Blue,
                    To = Colors.Red,
                    Algorithm = Algorithm.CieLab
                });

                // Create a new GenerateRendererParameter object and set the ClassificationDefinition
                // Also specify a Where clause on the layer to demonstrate excluding features from the classification
                GenerateRendererParameters rendererParam = new GenerateRendererParameters()
                {
                    ClassificationDefinition = uniqueValueDefinition,
                    Where = "STATE_NAME NOT IN ('Alaska', 'Hawaii')"
                };

                await GenerateRenderer(rendererParam);
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog("Error generating renderer: " + ex.Message).ShowAsync();
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }

        private async Task GenerateRenderer(GenerateRendererParameters rendererParam)
        {
            GenerateRendererResult result = await generateRendererTask.GenerateRendererAsync(rendererParam);

            LayerDrawingOptions layerDrawingOptions = null;
            LayerDrawingOptionCollection options = null;

            // If this is the first execution of this sample create a new LayerDrawingOptionsCollection
            if (((ArcGISDynamicMapServiceLayer)MyMapView.Map.Layers["USA"]).LayerDrawingOptions == null)
            {
                options = new LayerDrawingOptionCollection();

                // Add a new LayerDrawingOptions for layer ID 2 using the generated renderer
                options.Add(new LayerDrawingOptions() { LayerID = 2, Renderer = result.Renderer });
            }
            else
            {
                // Otherwise the layer will have an existing LayerDrawingOptionsCollection from a previous button click
                options = ((ArcGISDynamicMapServiceLayer)MyMapView.Map.Layers["USA"]).LayerDrawingOptions;

                // Iterate over the LayerDrawingOptionsCollection. 
                // For layer ID 2 get the existing LayerDrawingOptions object and apply the newly generated renderer
                foreach (LayerDrawingOptions drawOption in options)
                {
                    if (drawOption.LayerID == 2)
                    {
                        layerDrawingOptions = drawOption;
                        drawOption.Renderer = result.Renderer;
                    }
                }
            }

            // Retrieve the GenerateRendererParameters Where clause and create a new LayerDefinition for layer ID 2
            if (!string.IsNullOrEmpty(rendererParam.Where))
            {
                LayerDefinition layerDefinition = new LayerDefinition()
                {
                    LayerID = 2,
                    Definition = rendererParam.Where
                };

                ((ArcGISDynamicMapServiceLayer)MyMapView.Map.Layers["USA"]).LayerDefinitions =
                    new ObservableCollection<LayerDefinition>() { layerDefinition };
            }

            // Apply the updated LayerDrawingOptionsCollection to the LayerDrawingOptions property on the layer
            ((ArcGISDynamicMapServiceLayer)MyMapView.Map.Layers["USA"]).LayerDrawingOptions = options;
        }
    }
}
