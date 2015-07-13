using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates using the GeometryEngine.Generalize method to take a polyline with numerous vertices and return a generalized polyline with less vertices.
    /// </summary>
    /// <title>Generalize</title>
    /// <category>Geometry</category>
    public partial class Generalize : Windows.UI.Xaml.Controls.Page
    {
        private GraphicsOverlay _originalGraphicsOverlay;
        private GraphicsOverlay _generalizedGraphicsOverlay;
        private SimpleMarkerSymbol _defaultMarkerSymbol;
        private SimpleLineSymbol _defaultLineSymbol;
        private SimpleLineSymbol _generalizedLineSymbol;
        private SimpleMarkerSymbol _generalizedMarkerSymbol;

        /// <summary>Construct Generalize sample control</summary>
        public Generalize()
        {
            InitializeComponent();

			MyMapView.NavigationCompleted += MyMapView_NavigationCompleted;

			 _originalGraphicsOverlay = MyMapView.GraphicsOverlays["originalOverlay"];
			 _generalizedGraphicsOverlay = MyMapView.GraphicsOverlays["generalizedLineOverlay"];

            _defaultMarkerSymbol = LayoutRoot.Resources["DefaultMarkerSymbol"] as SimpleMarkerSymbol;
            _defaultLineSymbol = LayoutRoot.Resources["DefaultLineSymbol"] as SimpleLineSymbol;
            _generalizedLineSymbol = LayoutRoot.Resources["GeneralizedLineSymbol"] as SimpleLineSymbol;
            _generalizedMarkerSymbol = LayoutRoot.Resources["GeneralizedMarkerSymbol"] as SimpleMarkerSymbol;
        }

		// Adds the original river graphic to the map (from an online service)
		private async void MyMapView_NavigationCompleted(object sender, EventArgs e)
		{
			MyMapView.NavigationCompleted -= MyMapView_NavigationCompleted;
			try
			{
				if (_originalGraphicsOverlay != null && _originalGraphicsOverlay.Graphics.Count == 0)
				{
					QueryTask queryTask = new QueryTask(
						new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/1"));

					Query query = new Query("NAME = 'Mississippi'");
					query.ReturnGeometry = true;
					query.OutSpatialReference = MyMapView.SpatialReference;

					var results = await queryTask.ExecuteAsync(query);

					var river = results.FeatureSet.Features
						.Select(f => f.Geometry)
						.OfType<Polyline>()
						.FirstOrDefault();

					_originalGraphicsOverlay.Graphics.Add(new Graphic(river, _defaultLineSymbol));

					foreach (var path in river.Parts)
					{
						foreach (var coord in path.GetPoints())
						{
							var vertex = new Graphic(coord, _defaultMarkerSymbol);
							_originalGraphicsOverlay.Graphics.Add(vertex);
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

        // Generalizes the original line graphic
        private void GeneralizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _generalizedGraphicsOverlay.Graphics.Clear();

                var offset = DistanceSlider.Value * 1000;
                var generalizedPolyline = GeometryEngine.Generalize(
					_originalGraphicsOverlay.Graphics[0].Geometry, offset, false) as Polyline;

                if (generalizedPolyline != null)
                {
                    var graphic = new Graphic(generalizedPolyline, _generalizedLineSymbol);
                    _generalizedGraphicsOverlay.Graphics.Add(graphic);

                    foreach (var path in generalizedPolyline.Parts)
                    {
						foreach (var coord in path.GetPoints())
                        {
                            var vertex = new Graphic(coord, _generalizedMarkerSymbol);
                            _generalizedGraphicsOverlay.Graphics.Add(vertex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog("Error generalizing line: " + ex.Message, "Sample Error").ShowAsync();
            }
        }
    }
}
