using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Microsoft.Phone.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
	public sealed partial class OceanCurrents : PhoneApplicationPage
    {
        public OceanCurrents()
        {
            InitializeComponent();

            // Initialize graphics collections
            TapPoints = new ObservableCollection<Graphic>();
            Currents = new ObservableCollection<Graphic>();

            initializeLayers();

            // Set the data context to the page instance to allow for binding to the page's properties
            // in its XAML
            DataContext = this;
        }

        // On tap, get an input line and perform a buffer and clip operation
       private async void mapView1_Tap(object sender, MapViewInputEventArgs e)
        {
            if (BusyVisibility == Visibility.Visible)
            {
                MessageBox.Show("Please wait until the current operation is complete.");
                return;
            }

            // Show busy UI
            BusyVisibility = Visibility.Visible;

            // Clear previous results
            TapPoints.Clear();
            Currents.Clear();

          

            // Create graphic and add to tap points
            var g = new Graphic() { Geometry = e.Location };
            TapPoints.Add(g);

            string error = null;

            // Initialize the Geoprocessing task with the drive time calculation service endpoint
            Geoprocessor task = new Geoprocessor(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/" +
                "Specialty/ESRI_Currents_World/GPServer/MessageInABottle"));

            // Initialize input parameters
            var parameter = new GPInputParameter()
                {
                    OutSpatialReference = SpatialReferences.WebMercator
                };

            var projectedMapPoint = GeometryEngine.Project(e.Location, SpatialReferences.Wgs84);
            parameter.GPParameters.Add(new GPFeatureRecordSetLayer("Input_Point", projectedMapPoint));
            parameter.GPParameters.Add(new GPDouble("Days", Days));

            try
            {
                // Run the operation
                var result = await task.ExecuteAsync(parameter);

                // Check that layers were returned from the operation
                var outputLayers = result.OutParameters.OfType<GPFeatureRecordSetLayer>();
                if (outputLayers.Count() > 0)
                {
                    // Get the first layer returned - this will be the drive time areas
                    var outputLayer = outputLayers.First();
                    if (outputLayer.FeatureSet != null && outputLayer.FeatureSet.Features != null)
                    {
                        // Instead of adding ocean current features one-by-one, update the collection all at once to
                        // allow the map to render the new features in one rendering pass.
                        Currents = new ObservableCollection<Graphic>(outputLayer.FeatureSet.Features);
                    }
                    else
                    {
                        error = "No results returned";
                    }
                }
                else
                {
                    error = "No results returned";
                }
            }
            catch (Exception ex)
            {
                error = "Calculation failed: " + ex.Message;
            }

            // If operation did not succeed, notify user
            if (error != null)
                MessageBox.Show(error);

            // Hide busy UI
            BusyVisibility = Visibility.Collapsed;
        }

        #region Bindable Properties - TapPoints, Days, Currents, Layers, BusyVisibility

        #region TapPoints
        /// <summary>
        /// Identifies the <see cref="TapPoints"/> dependency property
        /// </summary>
        private static readonly DependencyProperty TapPointsProperty = DependencyProperty.Register(
            "TapPoints", typeof(ObservableCollection<Graphic>), typeof(OceanCurrents), null);

        /// <summary>
        /// Gets the set of lines on which the buffer and clip operation is to be based
        /// </summary>
        public ObservableCollection<Graphic> TapPoints
        {
            get { return GetValue(TapPointsProperty) as ObservableCollection<Graphic>; }
            private set { SetValue(TapPointsProperty, value); }
        }
        #endregion

        #region Days
        /// <summary>
        /// Identifies the <see cref="Days"/> dependency property
        /// </summary>
        private static readonly DependencyProperty DaysProperty = DependencyProperty.Register(
            "Days", typeof(double), typeof(OceanCurrents), new PropertyMetadata(365d));

        /// <summary>
        /// Gets or sets the distance to buffer the clip line
        /// </summary>
        public double Days
        {
            get { return (double)GetValue(DaysProperty); }
            private set { SetValue(DaysProperty, value); }
        }
        #endregion

        #region Currents
        /// <summary>
        /// Identifies the <see cref="Currents"/> dependency property
        /// </summary>
        private static readonly DependencyProperty CurrentsProperty = DependencyProperty.Register(
            "Currents", typeof(ObservableCollection<Graphic>), typeof(OceanCurrents), null);

        /// <summary>
        /// Gets the set of counties that have been clipped
        /// </summary>
        public ObservableCollection<Graphic> Currents
        {
            get { return GetValue(CurrentsProperty) as ObservableCollection<Graphic>; }
            private set { SetValue(CurrentsProperty, value); }
        }
        #endregion

        #region Layers
        /// <summary>
        /// Identifies the <see cref="Layers"/> dependency property
        /// </summary>
        private static readonly DependencyProperty LayersProperty = DependencyProperty.Register(
            "Layers", typeof(LayerCollection), typeof(OceanCurrents), null);

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
            "BusyVisibility", typeof(Visibility), typeof(OceanCurrents), 
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

        #endregion

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

            // Symbol for ocean current (results) layer
            SimpleLineSymbol currentSymbol = new SimpleLineSymbol() 
            { 
                Color = Colors.Red, 
                Width = 3, 
                Style = SimpleLineStyle.DashDotDot 
            };

            // ocean currents layer
            GraphicsLayer oceanCurrentsLayer = new GraphicsLayer() { 
                Renderer = new SimpleRenderer() { Symbol = currentSymbol} };

            // Bind the ClippedCounties property to the GraphicsSource of the clipped counties layer
            Binding b = new Binding("Currents") { Source = this };
            BindingOperations.SetBinding(oceanCurrentsLayer, GraphicsLayer.GraphicsSourceProperty, b);

            // Add the layer
            Layers.Add(oceanCurrentsLayer);

            // Symbol for tap points layer
            SimpleLineSymbol tapPointsOutline = new SimpleLineSymbol() { Color = Colors.Black };
            SimpleMarkerSymbol tapPointsSymbol = new SimpleMarkerSymbol()
            {
                Color = Colors.White,
                Outline = tapPointsOutline
            };

            // Tap points layer
            GraphicsLayer tapPointsLayer = new GraphicsLayer() { Renderer = new SimpleRenderer() { Symbol = tapPointsSymbol } };

            // Bind the TapPoints property to the GraphicsSource of the tap points layer
            b = new Binding("TapPoints") { Source = this };
            BindingOperations.SetBinding(tapPointsLayer, GraphicsLayer.GraphicsSourceProperty, b);

            // Add the layer to the map
            Layers.Add(tapPointsLayer);
        }

        // Dismiss the keyboard when the enter key is pressed
        private void DaysTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.Focus();
        }

        
    }
}