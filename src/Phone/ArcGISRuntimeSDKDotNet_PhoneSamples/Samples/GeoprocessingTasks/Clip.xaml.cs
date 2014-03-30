using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Microsoft.Phone.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Geoprocessing Tasks</category>
	public sealed partial class Clip : PhoneApplicationPage
    {
        GraphicsLayer m_clipLinesLayer;
        GPResultImageLayer m_clippedCountiesLayer;
        bool m_firstPointAdded;

        public Clip()
        {
            InitializeComponent();

            // Initialize graphics collections
            ClipLines = new ObservableCollection<Graphic>();
            ClippedCounties = new ObservableCollection<Graphic>();

            initializeLayers();

            // Set the data context to the page instance to allow for binding to the page's properties
            // in its XAML
            DataContext = this;

            Editor.EditorConfiguration = new EditorConfiguration()
            {
                AllowAddVertex = false,
                AllowDeleteVertex = false,
                AllowMoveGeometry = false,
                AllowMoveVertex = true,
                AllowRotateGeometry = false,
                AllowScaleGeometry = false,
                AllowSnapToEdge = false,
                AllowSnapToVertex = false
            };

            this.Loaded += Page_Loaded;
        }

        async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= Page_Loaded;
            getInputLineAndClip();
        }

        // Waits for the user to draw a line, then performs the buffer and clip operation
        private async void getInputLineAndClip()
        {
            if (m_firstPointAdded)
                return;

            // Get line from user
            var clipLine = await Editor.RequestShapeAsync(DrawShape.Polyline,
                new SimpleLineSymbol() { Color = Colors.Red, Width = 2, Style = SimpleLineStyle.Dash });
            ClipLines.Add(new Graphic() { Geometry = clipLine });

            // Show busy UI
            BusyVisibility = Visibility.Visible;
            StatusText = "Executing...";

            string error = null;

            // Initialize the Geoprocessing task with the buffer and clip service endpoint
            Geoprocessor task = new Geoprocessor(new Uri("http://serverapps10.esri.com/ArcGIS/rest/services/SamplesNET/" +
                "USA_Data_ClipTools/GPServer/ClipCounties"));

            // Initialize input parameters
            var parameter = new GPInputParameter()
            {
                OutSpatialReference = SpatialReferences.WebMercator
            };
            parameter.GPParameters.Add(new GPFeatureRecordSetLayer("Input_Features", clipLine)); // input geometry
            parameter.GPParameters.Add(new GPLinearUnit("Linear_unit", LinearUnits.Miles, BufferDistance)); // buffer distance
            try
            {
                // Submit the job and await the results
                var result = await task.SubmitJobAsync(parameter);

                // Poll the server every two seconds for the status of the job.
                while (result.JobStatus != GPJobStatus.Cancelled
                    && result.JobStatus != GPJobStatus.Deleted
                    && result.JobStatus != GPJobStatus.Failed
                    && result.JobStatus != GPJobStatus.Succeeded
                    && result.JobStatus != GPJobStatus.TimedOut)
                {
                    result = await task.CheckJobStatusAsync(result.JobID);

                    // show the status
                    var descriptions = result.Messages.Select(msg => msg.Description);
                    var status = string.Join(Environment.NewLine, descriptions);
                    if (!string.IsNullOrEmpty(status))
                        StatusText = status;

                    await Task.Delay(2000);
                }

                if (result.JobStatus == GPJobStatus.Succeeded)
                {
                    // Get the results
                    var resultData = await task.GetResultDataAsync(result.JobID, "Clipped_Counties");
                    if (resultData is GPFeatureRecordSetLayer)
                    {
                        GPFeatureRecordSetLayer resultsLayer = resultData as GPFeatureRecordSetLayer;
                        if (resultsLayer.FeatureSet != null
                            && resultsLayer.FeatureSet.Features != null
                            && resultsLayer.FeatureSet.Features.Count != 0)
                        {
                            // Results were returned as graphics.  Add them to the ClippedCounties collection
                            ClippedCounties = new ObservableCollection<Graphic>(resultsLayer.FeatureSet.Features);
                        }
                        else // Try to get results as a GPResultImageLayer
                        {
                            StatusText = "Clip operation complete. Retrieving results...";

                            m_clippedCountiesLayer = await task.GetResultImageLayerAsync(result.JobID, "Clipped_Counties");

                            // If successful, add the layer to the layers collection
                            if (m_clippedCountiesLayer != null)
                            {
                                m_clippedCountiesLayer.Opacity = 0.5;

                                // Insert the layer below the input layer
                                Layers.Insert(Layers.IndexOf(m_clipLinesLayer), m_clippedCountiesLayer);

                                // Wait until the result layer is initialized
                                await m_clippedCountiesLayer.InitializeAsync();
                            }
                            else
                            {
                                error = "No results found";
                            }
                        }
                    }
                    else
                    {
                        error = "Clip operation failed";
                    }
                }
                else
                {
                    error = "Clip operation failed";
                }
            }
            catch (Exception ex)
            {
                error = "Clip operation failed: " + ex.Message;
            }

            // If operation did not succeed, notify user
            if (error != null)
                MessageBox.Show(error);

            // Hide busy UI
            BusyVisibility = StatusVisibility = Visibility.Collapsed;
            StatusText = "";
            m_firstPointAdded = false;

            getInputLineAndClip();
        }

        // On tap, get an input line and perform a buffer and clip operation
       private void mapView1_Tap(object sender, MapViewInputEventArgs e)
        {        
            if (!m_firstPointAdded)
            {
                m_firstPointAdded = true;

                // Clear previous results
                ClipLines.Clear();
                ClippedCounties.Clear();
                if (m_clippedCountiesLayer != null)
                {
                    Layers.Remove(m_clippedCountiesLayer);
                    m_clippedCountiesLayer = null;
                }
                
                // Show instructions
                StatusText = "Tap to add a point to the line.  Double-tap to finish.";
                StatusVisibility = Visibility.Visible;
            }
            else if (BusyVisibility == Visibility.Visible)
            {
                MessageBox.Show("Please wait until the current operation is complete.");
                return;
            }
        }

        #region Bindable Properties - ClipLines, BufferDistance, ClippedCounties, Layers, BusyVisibility, StatusVisibility, StatusText

        #region ClipLines
        /// <summary>
        /// Identifies the <see cref="ClipLines"/> dependency property
        /// </summary>
        private static readonly DependencyProperty ClipLinesProperty = DependencyProperty.Register(
            "ClipLines", typeof(ObservableCollection<Graphic>), typeof(Clip), null);

        /// <summary>
        /// Gets the set of lines on which the buffer and clip operation is to be based
        /// </summary>
        public ObservableCollection<Graphic> ClipLines
        {
            get { return GetValue(ClipLinesProperty) as ObservableCollection<Graphic>; }
            private set { SetValue(ClipLinesProperty, value); }
        }
        #endregion

        #region BufferDistance
        /// <summary>
        /// Identifies the <see cref="BufferDistance"/> dependency property
        /// </summary>
        private static readonly DependencyProperty BufferDistanceProperty = DependencyProperty.Register(
            "BufferDistance", typeof(double), typeof(Clip), new PropertyMetadata(100d));

        /// <summary>
        /// Gets or sets the distance to buffer the clip line
        /// </summary>
        public double BufferDistance
        {
            get { return (double)GetValue(BufferDistanceProperty); }
            private set { SetValue(BufferDistanceProperty, value); }
        }
        #endregion

        #region ClippedCounties
        /// <summary>
        /// Identifies the <see cref="ClippedCounties"/> dependency property
        /// </summary>
        private static readonly DependencyProperty ClippedCountiesProperty = DependencyProperty.Register(
            "ClippedCounties", typeof(ObservableCollection<Graphic>), typeof(Clip), null);

        /// <summary>
        /// Gets the set of counties that have been clipped
        /// </summary>
        public ObservableCollection<Graphic> ClippedCounties
        {
            get { return GetValue(ClippedCountiesProperty) as ObservableCollection<Graphic>; }
            private set { SetValue(ClippedCountiesProperty, value); }
        }
        #endregion

        #region Layers
        /// <summary>
        /// Identifies the <see cref="Layers"/> dependency property
        /// </summary>
        private static readonly DependencyProperty LayersProperty = DependencyProperty.Register(
            "Layers", typeof(LayerCollection), typeof(Clip), null);

        /// <summary>
        /// Gets the set of layers containing the basemap, input, and output features
        /// </summary>
        public LayerCollection Layers
        {
            get { return GetValue(LayersProperty) as LayerCollection; }
            private set { SetValue(LayersProperty, value); }
        }
        #endregion

        #region BusyVisibility
        /// <summary>
        /// Identifies the <see cref="BusyVisibility"/> dependency property
        /// </summary>
        private static readonly DependencyProperty BusyVisibilityProperty = DependencyProperty.Register(
            "BusyVisibility", typeof(Visibility), typeof(Clip), 
            new PropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// Gets whether an operation is currently in progress, expressed as a 
        /// <see cref="System.Windows.Visibility">Visibility</see>
        /// </summary>
        public Visibility BusyVisibility
        {
            get { return (Visibility)GetValue(BusyVisibilityProperty); }
            private set { SetValue(BusyVisibilityProperty, value); }
        }
        #endregion

        #region StatusVisibility
        /// <summary>
        /// Identifies the <see cref="StatusVisibility"/> dependency property
        /// </summary>
        private static readonly DependencyProperty StatusVisibilityProperty = DependencyProperty.Register(
            "StatusVisibility", typeof(Visibility), typeof(Clip),
            new PropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// Gets whether status text should be shown, expressed as a 
        /// <see cref="System.Windows.Visibility">Visibility</see>
        /// </summary>
        public Visibility StatusVisibility
        {
            get { return (Visibility)GetValue(StatusVisibilityProperty); }
            private set { SetValue(StatusVisibilityProperty, value); }
        }
        #endregion

        #region StatusText
        /// <summary>
        /// Identifies the <see cref="StatusText"/> dependency property
        /// </summary>
        private static readonly DependencyProperty StatusTextProperty = DependencyProperty.Register(
            "StatusText", typeof(string), typeof(Clip), null);

        /// <summary>
        /// Gets the status of the current operation
        /// </summary>
        public string StatusText
        {
            get { return GetValue(StatusTextProperty) as string; }
            private set { SetValue(StatusTextProperty, value); }
        }
        #endregion

        #endregion

        private Editor m_editor;
        /// <summary>
        /// Gets the Editor to use for retrieving input
        /// </summary>
        public Editor Editor
        {
            get
            {
                if (m_editor == null)
                    m_editor = new Editor();
                return m_editor;
            }
        }

        // Initializes the set of layers used by the operation
        private void initializeLayers()
        {
            Layers = new LayerCollection();

            // Create the basemap layer and add it to the map
            Layers.Add(new ArcGISTiledMapServiceLayer()
            {
                ServiceUri =
                    "http://services.arcgisonline.com/arcgis/rest/services/World_Topo_Map/MapServer"
            });

            // Symbol for clipped counties (results) layer
            SimpleLineSymbol countiesOutline = new SimpleLineSymbol() { Color = Colors.Blue };
            SimpleFillSymbol countiesSymbol = new SimpleFillSymbol()
            {
                Color = Color.FromArgb(127, 200, 200, 255),
                Outline = countiesOutline
            };

            // Clipped counties layer
            GraphicsLayer clippedCountiesLayer = new GraphicsLayer() { Renderer = new SimpleRenderer() { Symbol = countiesSymbol } };

            // Bind the ClippedCounties property to the GraphicsSource of the clipped counties layer
            Binding b = new Binding("ClippedCounties") { Source = this };
            BindingOperations.SetBinding(clippedCountiesLayer, GraphicsLayer.GraphicsSourceProperty, b);

            // Add the layer
            Layers.Add(clippedCountiesLayer);

            // Symbol for clip lines layer
            SimpleLineSymbol clipLineSymbol = new SimpleLineSymbol() { Color = Colors.Red, Width = 2 };

            // Clip lines layer
            m_clipLinesLayer = new GraphicsLayer() { Renderer = new SimpleRenderer() { Symbol = clipLineSymbol } };

            // Bind the ClipLines property to the GraphicsSource of the clip lines layer
            b = new Binding("ClipLines") { Source = this };
            BindingOperations.SetBinding(m_clipLinesLayer, GraphicsLayer.GraphicsSourceProperty, b);

            // Add the layer
            Layers.Add(m_clipLinesLayer);
        }

        // Dismiss the keyboard when the enter key is pressed
        private void BufferDistanceTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.Focus();
        }

        
    }
}