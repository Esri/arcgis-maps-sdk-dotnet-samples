using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates using the GeometryEngine.Generalize method to take a polyline with numerous vertices and return a generalized polyline with fewer vertices.
	/// </summary>
	/// <title>Generalize</title>
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

			MyMapView.NavigationCompleted += MyMapView_NavigationCompleted;

			originalGraphicsLayer = MyMapView.Map.Layers["OriginalLineGraphicsLayer"] as GraphicsLayer;
			generalizedGraphicsLayer = MyMapView.Map.Layers["GeneralizedLineGraphicsLayer"] as GraphicsLayer;

			defaultMarkerSymbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as SimpleMarkerSymbol;
			defaultLineSymbol = LayoutRoot.Resources["DefaultLineSymbol"] as SimpleLineSymbol;
			generalizedLineSymbol = LayoutRoot.Resources["GeneralizedLineSymbol"] as SimpleLineSymbol;
			generalizedMarkerSymbol = LayoutRoot.Resources["GeneralizedMarkerSymbol"] as SimpleMarkerSymbol;
		}

		private async void MyMapView_NavigationCompleted(object sender, EventArgs e)
		{
			MyMapView.NavigationCompleted -= MyMapView_NavigationCompleted;
			try
			{
				if (originalGraphicsLayer != null && originalGraphicsLayer.Graphics.Count == 0)
				{
					QueryTask queryTask = new QueryTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/1"));
					Query query = new Query("NAME = 'Mississippi'");
					query.ReturnGeometry = true;
					query.OutSpatialReference = MyMapView.SpatialReference;

					var results = await queryTask.ExecuteAsync(query, CancellationToken.None);
					foreach (Graphic g in results.FeatureSet.Features)
					{
						g.Symbol = defaultLineSymbol;
						originalGraphicsLayer.Graphics.Add(g);

						foreach (var part in (g.Geometry as Polyline).Parts)
						{
							foreach (var point in part.GetPoints())
							{
								var vertex = new Graphic()
								{
									Symbol = defaultMarkerSymbol,
									Geometry = point
								};
								originalGraphicsLayer.Graphics.Add(vertex);
							}
						}
					}
					GeneralizeButton.IsEnabled = true;
				}
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("Error loading test line: " + ex.Message, "Sample Error").ShowAsync();
			}
		}

		private  void GeneralizeButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				generalizedGraphicsLayer.Graphics.Clear();

				var offset = DistanceSlider.Value * 1000;
			
				var generalizedGeometry = GeometryEngine.Generalize(originalGraphicsLayer.Graphics[0].Geometry, offset, false);
				if (generalizedGeometry != null)
				{
					var g = new Graphic(generalizedGeometry, generalizedLineSymbol);
					generalizedGraphicsLayer.Graphics.Add(g);

					foreach (var part in (generalizedGeometry as Polyline).Parts)
					{
						foreach (var point in part.GetPoints())
						{
							var vertex = new Graphic()
							{
								Symbol = generalizedMarkerSymbol,
								Geometry = point
							};
							generalizedGraphicsLayer.Graphics.Add(vertex);
						}
					}
				}
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("Error generalizing line: " + ex.Message, "Sample Error").ShowAsync();
			}
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			generalizedGraphicsLayer.Graphics.Clear();
		}
	}
}
