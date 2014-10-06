using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Geometry</category>
    public sealed partial class AreaAndPerimeter : Page
    {
        private const double toMilesConversion = 0.0006213700922;
        private const double toSqMilesConversion = 0.0000003861003;

        GraphicsOverlay graphicsOverlay;
        public AreaAndPerimeter()
        {
            InitializeComponent();

            mapView1.Loaded += mapView1_Loaded;
			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(-130, 20, -65, 55, SpatialReferences.Wgs84));
            graphicsOverlay = mapView1.GraphicsOverlays["Graphics"] as GraphicsOverlay;
        }

        async void mapView1_Loaded(object sender, RoutedEventArgs e)
        {
            await doCalculateAreaAndLength();
        }
        private async Task doCalculateAreaAndLength()
        {
            try
            {
                //Wait for user to draw
                var geom = await mapView1.Editor.RequestShapeAsync(DrawShape.Polygon);

                //show geometry on map
                graphicsOverlay.Graphics.Clear();

                var g = new Graphic { Geometry = geom, Symbol = LayoutRoot.Resources["DefaultFillSymbol"] as Esri.ArcGISRuntime.Symbology.Symbol };
                graphicsOverlay.Graphics.Add(g);


                //Calculate results
                var areaPlanar = GeometryEngine.Area(geom);
                ResultsAreaPlanar.Text = string.Format("{0} sq. miles", (areaPlanar * toSqMilesConversion).ToString("n3"));

                var perimPlanar = GeometryEngine.Length(geom);
                ResultsPerimeterPlanar.Text = string.Format("{0} miles", (perimPlanar * toMilesConversion).ToString("n3"));


                var areaGeodesic = GeometryEngine.GeodesicArea(geom);
                ResultsAreaGeodesic.Text = string.Format("{0} sq. miles", (areaGeodesic * toSqMilesConversion).ToString("n3"));

                var perimGeodesic = GeometryEngine.GeodesicLength(geom);
                ResultsPerimeterGeodesic.Text = string.Format("{0} miles", (perimGeodesic * toMilesConversion).ToString("n3"));

                Instructions.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                Results.Visibility = Windows.UI.Xaml.Visibility.Visible;

            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                var dlg = new MessageDialog("Current sketch has been canceled.", "Task Canceled!");
                var _ = dlg.ShowAsync();
            }

        }

        private void ResetUI()
        {
            graphicsOverlay.Graphics.Clear();
            Instructions.Visibility = Visibility.Visible;
            Results.Visibility = Visibility.Collapsed;

        }

        private async void CancelCurrent_Click(object sender, RoutedEventArgs e)
        {
            mapView1.Editor.Cancel.Execute(null);
            ResetUI();
            await doCalculateAreaAndLength();
 
        }

        private async void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            ResetUI();
            await doCalculateAreaAndLength();
        }
    }
}
