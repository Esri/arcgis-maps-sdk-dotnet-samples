using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Symbology</category>
	public partial class RenderingInCode : PhoneApplicationPage
    {
        public RenderingInCode()
        {
            InitializeComponent();

            initializeRenderers();

            // === REMOVE once Renderer has been made into a DependencyProperty ===
            ((FeatureLayer)mapView1.Map.Layers["States"]).Renderer = StatesRenderer;
            ((FeatureLayer)mapView1.Map.Layers["Rivers"]).Renderer = RiversRenderer;
            ((FeatureLayer)mapView1.Map.Layers["Cities"]).Renderer = CitiesRenderer;

            // Set the data context to the page instance to allow for binding to the page's properties
            // in its XAML
            DataContext = this;
        }

        // Creates the renderers in code
        private void initializeRenderers()
        {
            // Create simple renderer with one symbol for rivers layer
            SimpleRenderer simpleRenderer = new SimpleRenderer()
            {
                Description = "Rivers",
                Label = "Rivers",
                Symbol = new SimpleLineSymbol() { Color = Colors.Blue, Style = SimpleLineStyle.Dash, Width = 2 }
            };
            RiversRenderer = simpleRenderer;

            // Create a unique value renderer that defines different symbols for New Mexico, Texas, 
            // and Arizona
            UniqueValueRenderer uvr = new UniqueValueRenderer();
            uvr.Fields = new ObservableCollection<string>(new string[] { "STATE_NAME" });
            SimpleLineSymbol stateOutline = new SimpleLineSymbol() { Color = Colors.Black };
            uvr.Infos.Add(new UniqueValueInfo 
            { 
                Values = new ObservableCollection<object>(new object[] { "New Mexico" }), 
                Symbol = new SimpleFillSymbol() { Color = Colors.Yellow, Outline = stateOutline } 
            });
            uvr.Infos.Add(new UniqueValueInfo 
            { 
                Values = new ObservableCollection<object>(new object[] { "Texas" }),
                Symbol = new SimpleFillSymbol() { Color = Colors.Green, Outline = stateOutline }
            });
            uvr.Infos.Add(new UniqueValueInfo 
            { 
                Values = new ObservableCollection<object>(new object[] { "Arizona" }),
                Symbol = new SimpleFillSymbol() { Color = Colors.LightGray, Outline = stateOutline }
            });
            StatesRenderer = uvr;

            // Create a class breaks renderer to symbolize cities of different sizes
            ClassBreaksRenderer cbr = new ClassBreaksRenderer()
            {
                DefaultLabel = "All Other Values",
                DefaultSymbol = new SimpleMarkerSymbol() { Color = Colors.Black, 
                    Style = SimpleMarkerStyle.Cross, Size = 10 },
                Field = "POP1990",
                Minimum = 0
            };

            // Create and add symbols
            SimpleLineSymbol cityOutline = new SimpleLineSymbol() { Color = Colors.White };
            cbr.Infos.Add(new ClassBreakInfo()
            {
                Minimum = 100000,
                Maximum = 200000,
                Label = "100000 - 200000",
                Description = "Population between 100000 and 200000",
                Symbol = new SimpleMarkerSymbol() 
                { 
                    Color = Color.FromArgb(255, 0, 210, 0), 
                    Outline = cityOutline,
                    Size = 4, 
                    Style = SimpleMarkerStyle.Circle 
                }
            });
            cbr.Infos.Add(new ClassBreakInfo()
            {
                Minimum = 200001,
                Maximum = 1000000,
                Label = "200001 - 1000000",
                Description = "Population between 200001 and 1000000",
                Symbol = new SimpleMarkerSymbol() 
                { 
                    Color = Color.FromArgb(255, 0, 127, 0), 
                    Outline = cityOutline,
                    Size = 8, 
                    Style = SimpleMarkerStyle.Circle 
                }
            });

            cbr.Infos.Add(new ClassBreakInfo()
            {
                Minimum = 1000001,
                Maximum = 10000000,
                Label = "1000001 - 10000000",
                Description = "Population between 1000001 and 10000000",
                Symbol = new SimpleMarkerSymbol() 
                { 
                    Color = Color.FromArgb(255, 0, 50, 0), 
                    Outline = cityOutline,
                    Size = 12, 
                    Style = SimpleMarkerStyle.Circle 
                }
            });
            CitiesRenderer = cbr;
        }

        #region Renderer properties for binding - RiversRenderer, CitiesRenderer, StatesRenderer

        #region RiversRenderer
        /// <summary>
        /// Identifies the <see cref="RiversRenderer"/> dependency property
        /// </summary>
        private static readonly DependencyProperty RiversRendererProperty = DependencyProperty.Register(
            "RiversRenderer", typeof(Renderer), typeof(RenderingInCode), 
            new PropertyMetadata(new SimpleRenderer() { Symbol = new SimpleLineSymbol { Color = Colors.Blue } }));

        /// <summary>
        /// Gets a renderer for river features
        /// </summary>
        public Renderer RiversRenderer
        {
            get { return GetValue(RiversRendererProperty) as Renderer; }
            private set { SetValue(RiversRendererProperty, value); }
        }
        #endregion

        #region CitiesRenderer
        /// <summary>
        /// Identifies the <see cref="CitiesRenderer"/> dependency property
        /// </summary>
        private static readonly DependencyProperty CitiesRendererProperty = DependencyProperty.Register(
            "CitiesRenderer", typeof(Renderer), typeof(RenderingInCode), null);

        /// <summary>
        /// Gets a renderer for city features
        /// </summary>
        public Renderer CitiesRenderer
        {
            get { return GetValue(CitiesRendererProperty) as Renderer; }
            private set { SetValue(CitiesRendererProperty, value); }
        }
        #endregion

        #region StatesRenderer
        /// <summary>
        /// Identifies the <see cref="StatesRenderer"/> dependency property
        /// </summary>
        private static readonly DependencyProperty StatesRendererProperty = DependencyProperty.Register(
            "StatesRenderer", typeof(Renderer), typeof(RenderingInCode), null);

        /// <summary>
        /// Gets a renderer for state features
        /// </summary>
        public Renderer StatesRenderer
        {
            get { return GetValue(StatesRendererProperty) as Renderer; }
            private set { SetValue(StatesRendererProperty, value); }
        }
        #endregion

        #endregion
    }
}