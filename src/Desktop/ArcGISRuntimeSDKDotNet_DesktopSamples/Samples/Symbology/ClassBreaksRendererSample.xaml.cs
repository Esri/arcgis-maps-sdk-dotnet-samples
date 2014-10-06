using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using Esri.ArcGISRuntime.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Sample shows how to create a ClassBreaksRenderer for a graphics layer. Earthquake data points are pulled from an online source and rendered using the GraphicsLayer ClassBreaksRenderer.
    /// </summary>
    /// <title>Class Breaks Renderer</title>
	/// <category>Symbology</category>
	public partial class ClassBreaksRendererSample : UserControl
    {
        private Random _random = new Random();
		private GraphicsOverlay _earthquakes;

        /// <summary>Construct Class Breaks Renderer sample control</summary>
        public ClassBreaksRendererSample()
        {
            InitializeComponent();

			_earthquakes = MyMapView.GraphicsOverlays["earthquakes"];

            MyMapView.ExtentChanged += MyMapView_ExtentChanged;
        }

        // Load earthquake data
        private async void MyMapView_ExtentChanged(object sender, EventArgs e)
        {
            try
            {
                MyMapView.ExtentChanged -= MyMapView_ExtentChanged;
                await LoadEarthquakesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading earthquake data: " + ex.Message, "Class Breaks Renderer Sample");
            }
        }

        // Change the graphics layer renderer to a new ClassBreaksRenderer
        private void ChangeRendererButton_Click(object sender, RoutedEventArgs e)
        {
            SimpleMarkerStyle style = (SimpleMarkerStyle)_random.Next(0, 6);

			_earthquakes.Renderer = new ClassBreaksRenderer()
            {
                Field = "magnitude",
                Infos = new ClassBreakInfoCollection() 
                { 
                    new ClassBreakInfo() { Minimum = 2, Maximum = 3, Symbol = GetRandomSymbol(style) },
                    new ClassBreakInfo() { Minimum = 3, Maximum = 4, Symbol = GetRandomSymbol(style) },
                    new ClassBreakInfo() { Minimum = 4, Maximum = 5, Symbol = GetRandomSymbol(style) },
                    new ClassBreakInfo() { Minimum = 5, Maximum = 6, Symbol = GetRandomSymbol(style) },
                    new ClassBreakInfo() { Minimum = 6, Maximum = 7, Symbol = GetRandomSymbol(style) },
                    new ClassBreakInfo() { Minimum = 7, Maximum = 8, Symbol = GetRandomSymbol(style) },
                }
            };
        }

        // Load earthquakes from map service
        private async Task LoadEarthquakesAsync()
        {
            var queryTask = new QueryTask(
                new Uri("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Earthquakes/EarthquakesFromLastSevenDays/MapServer/0"));
            var query = new Query(MyMapView.Extent)
            {
                ReturnGeometry = true,
                OutSpatialReference = MyMapView.SpatialReference,
                Where = "magnitude > 2.0",
                OutFields = new OutFields(new List<string> { "magnitude" })
            };
            var result = await queryTask.ExecuteAsync(query);

			_earthquakes.Graphics.Clear();
			_earthquakes.Graphics.AddRange(result.FeatureSet.Features.OfType<Graphic>());
        }

        // Utility: Generate a random simple marker symbol
        private SimpleMarkerSymbol GetRandomSymbol(SimpleMarkerStyle style)
        {
            return new SimpleMarkerSymbol()
            {
                Size = 12,
                Color = GetRandomColor(),
                Style = style
            };
        }

        // Utility function: Generate a random System.Windows.Media.Color
        private Color GetRandomColor()
        {
            var colorBytes = new byte[3];
            _random.NextBytes(colorBytes);
            return Color.FromRgb(colorBytes[0], colorBytes[1], colorBytes[2]);
        }
    }
}
