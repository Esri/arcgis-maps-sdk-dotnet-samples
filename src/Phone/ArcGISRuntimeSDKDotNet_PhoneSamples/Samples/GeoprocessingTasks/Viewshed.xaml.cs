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
using System.Windows.Media;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Geoprocessing Tasks</category>
	public sealed partial class Viewshed : PhoneApplicationPage
    {
        GraphicsLayer m_tapPointsLayer;
        ArcGISDynamicMapServiceLayer m_viewshedLayer;
        public Viewshed()
        {
            InitializeComponent();

            // Initialize the tap points graphics collection
            TapPoints = new ObservableCollection<Graphic>();

            // Initialize layers
            Layers = new LayerCollection();

            // Create the basemap layer and add it to the map
            Layers.Add(new ArcGISTiledMapServiceLayer() { ServiceUri =
                "http://services.arcgisonline.com/arcgis/rest/services/World_Topo_Map/MapServer" });

            // Symbol for tap points layer
            SimpleLineSymbol tapPointsOutline = new SimpleLineSymbol() { Color = Colors.Black };
            SimpleMarkerSymbol tapPointsSymbol = new SimpleMarkerSymbol() 
            { 
                Color = Colors.White, 
                Outline = tapPointsOutline 
            };

            // Tap points layer
            m_tapPointsLayer = new GraphicsLayer() { Renderer = new SimpleRenderer() { Symbol = tapPointsSymbol } };

            // Bind the TapPoints property to the GraphicsSource of the tap points layer
            Binding b = new Binding("TapPoints") { Source = this };
            BindingOperations.SetBinding(m_tapPointsLayer, GraphicsLayer.GraphicsSourceProperty, b);

            // Add the layer to the map
            Layers.Add(m_tapPointsLayer);

            // Set the data context to the page instance to allow for binding to the page's properties
            // in its XAML
            DataContext = this;
        }

       
        private async void mapView1_Tap(object sender, MapViewInputEventArgs e)
        {
            if (BusyVisibility == Visibility.Visible)
            {
                MessageBox.Show("Please wait until the current operation is complete.");
                return;
            }

            // Show busy UI
            BusyVisibility = Visibility.Visible;
            StatusText = "Executing...";

            // Clear previous results
            TapPoints.Clear();
            if (m_viewshedLayer != null)
                Layers.Remove(m_viewshedLayer);

           
            // Create graphic and add to tap points
            var g = new Graphic() { Geometry = e.Location };
            TapPoints.Add(g);

            string error = null;

            // Initialize the Geoprocessing task with the viewshed calculation service endpoint
            Geoprocessor task = new Geoprocessor(new Uri("http://serverapps101.esri.com/ArcGIS/rest/services/" +
                "ProbabilisticViewshedModel/GPServer/ProbabilisticViewshedModel"));

            // Initialize input parameters
            var parameter = new GPInputParameter()
            {
                OutSpatialReference = SpatialReferences.WebMercator
            };
            parameter.GPParameters.Add(new GPFeatureRecordSetLayer("Input_Features", e.Location));
            parameter.GPParameters.Add(new GPString("Height", "50"));
            parameter.GPParameters.Add(new GPLinearUnit("Distance", LinearUnits.Miles, 10));

            try
            {
                var result = await task.SubmitJobAsync(parameter);

                // Poll the server for results every two seconds.
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
                    // get the results as a ArcGISDynamicMapServiceLayer
                    StatusText = "Calculation complete. Retrieving results...";
                    m_viewshedLayer = task.GetResultMapServiceLayer(result.JobID);
                    
                    if (m_viewshedLayer != null)
                    {
                        // Insert the results layer beneath the tap points layer.
                        // This allows the input point to be visible at all times.
                        Layers.Insert(Layers.IndexOf(m_tapPointsLayer), m_viewshedLayer);

                        // Wait until the viewshed layer is initialized
                        await m_viewshedLayer.InitializeAsync();
                    }
                    else
                    {
                        error = "No results returned";
                    }
                }
                else
                {
                    error = "Viewshed calculation failed";
                }
            }
            catch (Exception ex)
            {
                error = "Viewshed calculation failed: " + ex.Message;
            }

            // If operation did not succeed, notify user
            if (error != null)
                MessageBox.Show(error);

            // Hide busy UI
            BusyVisibility = Visibility.Collapsed;
            StatusText = "";
        }

        #region Bindable Properties - TapPoints, Layers, BusyVisibility, StatusText

        #region TapPoints
        /// <summary>
        /// Identifies the <see cref="TapPoints"/> dependency property
        /// </summary>
        private static readonly DependencyProperty TapPointsProperty = DependencyProperty.Register(
            "TapPoints", typeof(ObservableCollection<Graphic>), typeof(Viewshed), null);

        /// <summary>
        /// Gets the set of points on the map where the user has tapped
        /// </summary>
        public ObservableCollection<Graphic> TapPoints
        {
            get { return GetValue(TapPointsProperty) as ObservableCollection<Graphic>; }
            private set { SetValue(TapPointsProperty, value); }
        }
        #endregion

        #region Layers
        /// <summary>
        /// Identifies the <see cref="Layers"/> dependency property
        /// </summary>
        private static readonly DependencyProperty LayersProperty = DependencyProperty.Register(
            "Layers", typeof(LayerCollection), typeof(Viewshed), null);

        /// <summary>
        /// Gets the set of layers used including basemap, input, and output layers
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
            "BusyVisibility", typeof(Visibility), typeof(Viewshed), 
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

        #region StatusText
        /// <summary>
        /// Identifies the <see cref="StatusText"/> dependency property
        /// </summary>
        private static readonly DependencyProperty StatusTextProperty = DependencyProperty.Register(
            "StatusText", typeof(string), typeof(Viewshed), null);

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
    }
}