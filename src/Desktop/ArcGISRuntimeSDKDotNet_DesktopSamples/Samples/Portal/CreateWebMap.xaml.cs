using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.Query;
using Esri.ArcGISRuntime.WebMap;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample shows how to create a new webmap from scratch by adding a BaseMap and OperationLayers. Here, a webmap object is created with a base map and operationlayers including an ArcGISDynamicMapService layer, a FeatureService with Popups and a FeatureCollection.
    /// </summary>
    /// <title>Create Web Map</title>
    /// <category>Portal</category>
    public partial class CreateWebMap : UserControl
    {
        /// <summary>Construct Create WebMap sample control</summary>
        public CreateWebMap()
        {
            InitializeComponent();

            SetupWebMap()
                .ContinueWith(t => { }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task SetupWebMap()
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
            var mapView = new MapView();
            mapView.Map = await LoadWebMapAsync(webmap);
			mapView.Map.InitialViewpoint = new Envelope(-20000000, 1100000, -3900000, 11000000);
            layoutGrid.Children.Add(mapView);
        }

        //Define BaseMap Layer
        private Basemap CreateBasemapLayer()
        {
            return new Basemap()
            {
                Title = "World Streets",
                Layers = new List<WebMapLayer>
                { 
                    new WebMapLayer { Url = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer" }
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
            fieldinfos.Add(new FieldInfo() { FieldName = "STATE_NAME", Label = "State", IsVisible = true });

            IList<MediaInfo> mediainfos = new List<MediaInfo>();
            MediaInfoValue infovalue = new MediaInfoValue();
            infovalue.Fields = new string[] { "POP2000,POP2007" };
            mediainfos.Add(new MediaInfo() { Type = MediaType.PieChart, Value = infovalue });

            PopupInfo popup = new PopupInfo() { FieldInfos = fieldinfos, MediaInfos = mediainfos, Title = "Population Change between 2000 and 2007", };

            return new WebMapLayer
            {
                Url = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3",
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

                Dictionary<string, object> layerdef = new Dictionary<string, object>();
                Dictionary<string, object> defdictionary = new Dictionary<string, object>() 
                { 
                    { "id", 0 }, 
                    { "name", "Earthquakes from last 7 days" } 
                };

                Dictionary<string, object> renderer = new Dictionary<string, object>();
                renderer.Add("type", "simple");
                renderer.Add("style", "esriSMSCircle");

                int[] color = new int[] { 255, 0, 0, 255 };
                renderer.Add("color", color);
                renderer.Add("size", 4);

                defdictionary.Add("drawingInfo", renderer);

                layerdef.Add("layerDefinition", defdictionary);

                //Create FeatureCollection as webmap layer
                FeatureCollection featureCollection = null;

                if (queryResult.FeatureSet.Features.Count > 0)
                {
                    var sublayer = new WebMapSubLayer();
                    sublayer.FeatureSet = queryResult.FeatureSet;

                    sublayer.AddCustomProperty("layerDefinition", layerdef);
                    featureCollection = new FeatureCollection { SubLayers = new List<WebMapSubLayer> { sublayer } };
                }

                return new WebMapLayer { FeatureCollection = featureCollection };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
                return null;
            }
        }

        // Loads the given webmap
        private async Task<Map> LoadWebMapAsync(WebMap webmap)
        {
            try
            {
                var portal = await ArcGISPortal.CreateAsync();
                var vm = await WebMapViewModel.LoadAsync(webmap, portal);
                return vm.Map;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
                return null;
            }
        }
    }
}
