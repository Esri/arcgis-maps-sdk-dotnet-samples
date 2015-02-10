using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates using the GeometryEngine.Cut method to cut feature geometries with a given polyline.
	/// </summary>
	/// <title>Cut</title>
	/// <category>Geometry</category>
	public sealed partial class CutPolygons : Page
    {
		GraphicsOverlay resultsOverlay;
		GraphicsOverlay statesOverlay;

        public CutPolygons()
        {
            InitializeComponent();

			mapView1.SpatialReferenceChanged += mapView1_SpatialReferenceChanged;	
        }

		void mapView1_SpatialReferenceChanged(object sender, EventArgs e)
		{
			resultsOverlay = mapView1.GraphicsOverlays["ResultsGraphicsOverlay"] as GraphicsOverlay;
			statesOverlay = mapView1.GraphicsOverlays["StatesGraphicsOverlay"] as GraphicsOverlay;

			var x = LoadParcels();	
		}

        private async void CutPolygonsButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide/show ui elements
            SetupUI();
            try
            {
                await DoCutPolygons();

            }
            catch (Exception ex)
            {
                var dlg = new MessageDialog(ex.Message, "Geometry Engine Failed!");
				var _x = dlg.ShowAsync();
            }

            // Hide/show ui elements
            ResetUI();
        }

        private async Task LoadParcels()
        {

			await mapView1.LayersLoadedAsync();

			// Use a QueryTask to load the Parcels into the GraphicsOverlay.
            // Notice that we are filtering the returned features based on the current map's extent
            // by passing in a geometry into the constructor of the Query object
			var queryTask = new QueryTask(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3"));
            var query = new Query(mapView1.Extent) { ReturnGeometry = true, OutSpatialReference = mapView1.SpatialReference, Where="1=1" };
			try
			{
				var result = await queryTask.ExecuteAsync(query);
				statesOverlay.Graphics.AddRange(result.FeatureSet.Features.OfType<Graphic>());
			}
			catch { }
			// Once the graphics have been loaded we can now enable the 'Cut Polygons' button
			CutPolygonsButton.IsEnabled = true;
		}

        private async Task DoCutPolygons()
        {
           
            if (statesOverlay != null && resultsOverlay != null)
            {
				resultsOverlay.Graphics.Clear();

				// Get the user's input
                var cutPolyLine = (await mapView1.Editor.RequestShapeAsync(DrawShape.Polyline)) as Polyline;

				// Normalize for WrapAround
				Polyline polyline = GeometryEngine.NormalizeCentralMeridian(cutPolyLine) as Polyline;

				// Iterate over the graphics in the GraphicsOverlay. If the graphic intersects with the polyline we will cut it
                // and create Graphic objects from the results. Next we add those Graphics resulting to a List<Graphic>.
				List<Graphic> cutGraphics = new List<Graphic>();
                foreach (var g in statesOverlay.Graphics)
                {
					if (GeometryEngine.Intersects(g.Geometry, polyline))
                    {
						var cutPolygonGeometries = GeometryEngine.Cut(g.Geometry, polyline);
                        var cutPolygonGraphics = cutPolygonGeometries.Select(x => new Graphic { Geometry = x });
                        cutGraphics.AddRange(cutPolygonGraphics);
                    }
                }
				// Add the results to the GraphicsOverlay
                resultsOverlay.Graphics.AddRange(cutGraphics);
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
