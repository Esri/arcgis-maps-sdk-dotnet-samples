using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntime.Samples.Desktop.DynamicLayers
{
    /// <summary>
    /// This sample demonstrates using the GenerateRendererTask to create a class breaks or unique value renderer on the server without having to retrieve all the data to the client. To use the sample, click either the Generate Range Values or Generate Unique Values button. This updates the renderer on the map.
    /// 
    /// In the code-behind, a GenerateRendererTask is used to create a renderer, and the LayerDrawingOptions.Renderer is set to the generated renderer for use in the map's display.
    /// </summary>
    /// <title>Dynamic Layer Rendering</title>
	/// <category>Layers</category>
	/// <subcategory>Dynamic Service Layers</subcategory>
	public partial class DynamicLayerRendering : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private GenerateRendererTask generateRendererTask;

        private bool _isBusy = true;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsBusy"));
            }
        }

        public DynamicLayerRendering()
        {
            InitializeComponent();

            DataContext = this;
            IsBusy = false;

			MyMapView.Map.SpatialReference = SpatialReferences.WebMercator;

            // Create GenerateRendererTask for a specific map layer
            generateRendererTask = new GenerateRendererTask(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/2"));
        }

        private async void GenerateRangeValueClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsBusy)
                    return;

                IsBusy = true;

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
                MessageBox.Show("Error generating renderer: " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void GenerateUniqueValueClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsBusy)
                    return;

                IsBusy = true;

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
                MessageBox.Show("Error generating renderer: " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GenerateRenderer(GenerateRendererParameters rendererParam)
        {
            GenerateRendererResult result = await generateRendererTask.GenerateRendererAsync(rendererParam);

            LayerDrawingOptions layerDrawingOptions = null;
            LayerDrawingOptionCollection options = null;

            // If this is the first execution of this sample create a new LayerDrawingOptionsCollection
            if (((ArcGISDynamicMapServiceLayer)map.Layers["USA"]).LayerDrawingOptions == null)
            {
                options = new LayerDrawingOptionCollection();

                // Add a new LayerDrawingOptions for layer ID 2 using the generated renderer
                options.Add(new LayerDrawingOptions() { LayerID = 2, Renderer = result.Renderer });
            }
            else
            {
                // Otherwise the layer will have an existing LayerDrawingOptionsCollection from a previous button click
                options = ((ArcGISDynamicMapServiceLayer)map.Layers["USA"]).LayerDrawingOptions;

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

                ((ArcGISDynamicMapServiceLayer)map.Layers["USA"]).LayerDefinitions =
                    new ObservableCollection<LayerDefinition>() { layerDefinition };
            }

            // Apply the updated LayerDrawingOptionsCollection to the LayerDrawingOptions property on the layer
            ((ArcGISDynamicMapServiceLayer)map.Layers["USA"]).LayerDrawingOptions = options;
        }
    }
}
