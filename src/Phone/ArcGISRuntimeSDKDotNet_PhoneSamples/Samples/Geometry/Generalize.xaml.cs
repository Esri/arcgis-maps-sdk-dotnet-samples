using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Threading;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Geometry</category>
	public sealed partial class Generalize : Page
    {
        GraphicsLayer originalGraphicsLayer;
        GraphicsLayer generalizedGraphicsLayer;
        SimpleMarkerSymbol defaultMarkerSymbol;
        SimpleLineSymbol defaultLineSymbol;
        SimpleLineSymbol generalizedLineSymbol;
        SimpleMarkerSymbol generalizedMarkerSymbol;
        public Generalize()
        {
            InitializeComponent();

			mapView1.Map.InitialViewpoint = new Envelope(-12000000, 3000000, -7000000, 7000000, SpatialReferences.WebMercator);
            originalGraphicsLayer = mapView1.Map.Layers["OriginalLineGraphicsLayer"] as GraphicsLayer;
            generalizedGraphicsLayer = mapView1.Map.Layers["GeneralizedLineGraphicsLayer"] as GraphicsLayer;

            mapView1.Loaded += mapView1_Loaded;
            defaultMarkerSymbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as SimpleMarkerSymbol;
            defaultLineSymbol = LayoutRoot.Resources["DefaultLineSymbol"] as SimpleLineSymbol;
            generalizedLineSymbol = LayoutRoot.Resources["GeneralizedLineSymbol"] as SimpleLineSymbol;
            generalizedMarkerSymbol = LayoutRoot.Resources["GeneralizedMarkerSymbol"] as SimpleMarkerSymbol;

        }

        async void mapView1_Loaded(object sender, RoutedEventArgs e)
        {
            if (originalGraphicsLayer != null && originalGraphicsLayer.Graphics.Count == 0)
            {
                QueryTask queryTask = new QueryTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/1"));
                Query query = new Query("NAME = 'Mississippi'");
                query.ReturnGeometry = true;
                query.OutSpatialReference = mapView1.SpatialReference;


                var results = await queryTask.ExecuteAsync(query, CancellationToken.None);
                foreach (Graphic g in results.FeatureSet.Features)
                {
                    g.Symbol = defaultLineSymbol;
                    originalGraphicsLayer.Graphics.Add(g);

                    foreach (var pc in (g.Geometry as Polyline).Parts)
                    {
                        foreach (var point in pc)
                        {
                            var vertice = new Graphic()
                            {
                                Symbol = defaultMarkerSymbol,
                                Geometry = new MapPoint(point.X, point.Y)
                            };
                            originalGraphicsLayer.Graphics.Add(vertice);
                        }
                    }
                }
                GeneralizeButton.IsEnabled = true;
            }
        }

        private  void GeneralizeButton_Click(object sender, RoutedEventArgs e)
        {

            generalizedGraphicsLayer.Graphics.Clear();
            //GeneralizeButton.IsEnabled = false;


            var offset = DistanceSlider.Value * 1000;
            
            var generalizedGeometry = GeometryEngine.Generalize(originalGraphicsLayer.Graphics[0].Geometry, offset, false);
            generalizedGraphicsLayer.Graphics.Clear();
            if (generalizedGeometry != null)
            {
                var g = new Graphic(generalizedGeometry, generalizedLineSymbol);
                generalizedGraphicsLayer.Graphics.Add(g);

				foreach (var pc in (generalizedGeometry as Polyline).Parts)
                {
                    foreach (var point in pc)
                    {
                        var vertice = new Graphic()
                        {
                            Symbol = generalizedMarkerSymbol,
                            Geometry = new MapPoint(point.X, point.Y)
                        };
                        generalizedGraphicsLayer.Graphics.Add(vertice);
                    }
                }
            }

        }


    }
}
