using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using Esri.ArcGISRuntime.WebMap;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
    /// <summary>
    /// Shows how to create a new webmap from scratch by adding a Basemap and OperationLayers. 
    /// </summary>
    /// <title>Create Web Map</title>
    /// <category>Portal</category>
    public partial class CreateWebMap : Page
    {
        /// <summary>Construct Create WebMap sample page</summary>
        public CreateWebMap()
        {
            InitializeComponent();
			MyMapView.Loaded += MyMapView_Loaded;
        }

		private async void MyMapView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			try
			{
				//Create a new webmap with basemap and operational layers
				var webmap = new WebMap()
				{
					Basemap = CreateBasemapLayer(),
					OperationalLayers = new List<WebMapLayer>()
					{
						CreateDynamicServiceLayer(),
						CreateStateLayerWithPopup(),
						await CreateFeatureCollectionLayer()
					}
				};

				// Load the new webmap into the current UI
				MyMapView.Map = await LoadWebMapAsync(webmap);
				MyMapView.Map.InitialViewpoint = new Viewpoint(
					new Envelope(-14470183.421, 3560814.811, -11255400.943, 5399444.790, SpatialReferences.WebMercator));
			}
			catch (System.Exception ex)
			{
				var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
        }

        //Define BaseMap Layer
        private Basemap CreateBasemapLayer()
        {
            return new Basemap()
            {
                Title = "World Streets",
                Layers = new List<WebMapLayer>
                { 
                    new WebMapLayer { Url = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer" }
                }
            };
        }

        //Add a ArcGISDynamicMapService
        private WebMapLayer CreateDynamicServiceLayer()
        {
            return new WebMapLayer
            {
                Url = "http://serverapps10.esri.com/ArcGIS/rest/services/California/MapServer",
                VisibleLayers = new List<object> { 0, 1, 3, 6, 9 }
            };
        }

        // Create dynamic map service webmap layer with popup
        private WebMapLayer CreateStateLayerWithPopup()
        {
            IList<FieldInfo> fieldinfos = new List<FieldInfo>();
            fieldinfos.Add(new FieldInfo() { 
				FieldName = "STATE_NAME", 
				Label = "State",
				IsVisible = true 
			});

            IList<MediaInfo> mediainfos = new List<MediaInfo>();
            MediaInfoValue infovalue = new MediaInfoValue();
            infovalue.Fields = new string[] { "POP2000,POP2007" };
            mediainfos.Add(new MediaInfo() { Type = MediaType.PieChart, Value = infovalue });

            PopupInfo popup = new PopupInfo() { 
				FieldInfos = fieldinfos, 
				MediaInfos = mediainfos, 
				Title = "Population Change between 2000 and 2007"
			};

            return new WebMapLayer
            {
                Url = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3",
				LayerType = WebMapLayerType.ArcGISFeatureLayer,
				PopupInfo = popup
            };
        }

        // Create webmap layer out of a feature set from a query task
        private async Task<WebMapLayer> CreateFeatureCollectionLayer()
        {
            try
            {
                //Perform Query to get a featureSet and add to webmap as featurecollection
                QueryTask qt = new QueryTask(
                    new Uri("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Earthquakes/EarthquakesFromLastSevenDays/MapServer/0"));

                Esri.ArcGISRuntime.Tasks.Query.Query query = new Esri.ArcGISRuntime.Tasks.Query.Query("magnitude > 3.5");
                query.OutFields.Add("*");
                query.ReturnGeometry = true;

                var queryResult = await qt.ExecuteAsync(query);

                var simpleRenderer = new SimpleRenderer { Symbol = new SimpleMarkerSymbol { Style = SimpleMarkerStyle.Circle, Color = Color.FromArgb(255, 0, 0, 255), Size = 8 } };
                var drawingInfo = new DrawingInfo { Renderer = simpleRenderer };
                var layerDefinition = new LayerDefinition { DrawingInfo = drawingInfo };

                //Create FeatureCollection as webmap layer
                FeatureCollection featureCollection = null;

                if (queryResult.FeatureSet.Features.Count > 0)
                {
                    var sublayer = new WebMapSubLayer();
                    sublayer.Id = 0;
                    sublayer.FeatureSet = queryResult.FeatureSet;

                    sublayer.LayerDefinition = layerDefinition;
                    featureCollection = new FeatureCollection
                    {
                        SubLayers = new List<WebMapSubLayer> { sublayer }
                    };
                }

                return new WebMapLayer { FeatureCollection = featureCollection, Title = "Earthquakes from last 7 days" };
            }
            catch (Exception ex)
            {
				var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
				return null;
            }
        }

        // Loads the given webmap
		private async Task<Map> LoadWebMapAsync(WebMap webmap)
		{
			var portal = await ArcGISPortal.CreateAsync();
			var vm = await WebMapViewModel.LoadAsync(webmap, portal);
			return vm.Map;
		}
    }
}
