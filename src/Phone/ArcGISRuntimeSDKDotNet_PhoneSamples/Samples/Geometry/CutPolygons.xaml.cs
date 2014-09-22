using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Geometry</category>
	public sealed partial class CutPolygons : Page
    {
        GraphicsLayer graphicsLayer;

        SimpleFillSymbol sfs;
        public CutPolygons()
        {
            InitializeComponent();


			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(-83.31884, 42.61428, -83.31296, 42.61671, SpatialReferences.Wgs84));
            sfs = LayoutRoot.Resources["MySimpleFillSymbol"] as SimpleFillSymbol;
            graphicsLayer = mapView1.Map.Layers["MyGraphicsLayer"] as GraphicsLayer;
            Task.WhenAll(mapView1.Map.Layers.Select(l => l.InitializeAsync())).ContinueWith((t) =>
            {  
                if(!t.IsFaulted)
                    LoadParcels();
            }, TaskScheduler.FromCurrentSynchronizationContext());
			
        }

        private async void CutPolygonsButton_Click(object sender, RoutedEventArgs e)
        {
            //hide/show ui elements
            SetupUI();
            try
            {
                await DoCutPolygons();

            }
            catch (Exception ex)
            {
                var dlg = new MessageDialog(ex.Message, "Geometry Engine Failed!");
				var _ = dlg.ShowAsync();
            }


            //Hide/show ui elements
            ResetUI();
        }


        private async void LoadParcels()
        {

            //Use a QueryTask to load the Parcels into the graphics layer.
            //Notice that we are filtering the returned features based on the current map's extent
            //by passing in a geometry into the constructor of the Query object
            var queryTask = new QueryTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1"));
            var query = new Query(mapView1.Extent) { ReturnGeometry = true, OutSpatialReference = mapView1.SpatialReference, Where="1=1" };
			try
			{
				var result = await queryTask.ExecuteAsync(query);

				graphicsLayer.Graphics.Clear();
				graphicsLayer.Graphics.AddRange(result.FeatureSet.Features.OfType<Graphic>());

			}
			catch { }
			//Once the graphics have been loaded we can now enable the 'Cut Polygons' button
			CutPolygonsButton.IsEnabled = true;
		}

        private async Task DoCutPolygons()
        {
            SetupUI();
            if (graphicsLayer != null)
            {
                //Get the user's input
                var cutPolyLine = (await mapView1.Editor.RequestShapeAsync(DrawShape.Polyline)) as Polyline;

                //iterate over the graphics in the graphicsLayer. If the graphic intersects with the polyline we will cut it.
                //and then create Graphic objects from the results. Next we add those Graphics resulting to a new list (finalList).
                //if it doesn't we will add the graphic to the new list (finalList)
                var finalList = graphicsLayer.Graphics.ToList();
                foreach (var g in graphicsLayer.Graphics)
                {
                    if (GeometryEngine.Intersects(g.Geometry, cutPolyLine))
                    {
                        var cutPolygonGeometries = GeometryEngine.Cut(g.Geometry, cutPolyLine);
                        var cutPolygonGraphics = cutPolygonGeometries.Select(x => new Graphic { Geometry = x });
                        finalList.AddRange(cutPolygonGraphics);
                    }
                    else
                        finalList.Add(g);
                }
                //add the results to the graphics layer
                graphicsLayer.Graphics.Clear();
                graphicsLayer.Graphics.AddRange(finalList);


            }
        }


        private void ResetUI()
        {
            CutPolygonsButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            InstructionsTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void SetupUI()
        {
            InstructionsTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
            CutPolygonsButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}
