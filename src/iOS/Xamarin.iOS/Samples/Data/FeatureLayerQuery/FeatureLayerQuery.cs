// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Foundation;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.FeatureLayerQuery
{
    [Register("FeatureLayerQuery")]
    public class FeatureLayerQuery : UIViewController
    {
        // Constant holding offset where the MapView control should start
        private const int yPageOffset = 60;

        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Create reference to service of US States  
        private string _statesUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/2";

        // Create globally available text box for easy referencing 
        private UITextField _queryTextView;

        // Create globally available feature table for easy referencing 
        private ServiceFeatureTable _featureTable;
        
        // Create globally available feature layer for easy referencing 
        private FeatureLayer _featureLayer;

        public FeatureLayerQuery()
        {
            Title = "Feature layer query";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create and set initial map location
            MapPoint initialLocation = new MapPoint(
                -11000000, 5000000, SpatialReferences.WebMercator);
            myMap.InitialViewpoint = new Viewpoint(initialLocation, 100000000);

            // Create feature table using a url
            _featureTable = new ServiceFeatureTable(new Uri(_statesUrl));

            // Create feature layer using this feature table
            _featureLayer = new FeatureLayer(_featureTable);

            // Set the Opacity of the Feature Layer
            _featureLayer.Opacity = 0.6;

            // Create a new renderer for the States Feature Layer
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(
                SimpleLineSymbolStyle.Solid, Color.Black, 1);
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Solid, Color.Yellow, lineSymbol);

            // Set States feature layer renderer
            _featureLayer.Renderer = new SimpleRenderer(fillSymbol);

            // Add feature layer to the map
            myMap.OperationalLayers.Add(_featureLayer);

            // Assign the map to the MapView
            _myMapView.Map = myMap;
        }

        private async void OnQueryClicked(object sender, EventArgs e)
        {
            // Remove any previous feature selections that may have been made 
            _featureLayer.ClearSelection();

            // Begin query process 
            await QueryStateFeature(_queryTextView.Text);
        }

        private async Task QueryStateFeature(string stateName)
        {
            try
            {
                // Hide keyboard
                _queryTextView.ResignFirstResponder();

                // Create a query parameters that will be used to Query the feature table  
                QueryParameters queryParams = new QueryParameters();

                // Construct and assign the where clause that will be used to query the feature table 
                queryParams.WhereClause = "upper(STATE_NAME) LIKE '%" + (stateName.ToUpper()) + "%'";

                // Query the feature table 
                FeatureQueryResult queryResult = await _featureTable.QueryFeaturesAsync(queryParams);

                // Cast the QueryResult to a List so the results can be interrogated
                var features = queryResult.ToList();

                if (features.Any())
                {
                    // Get the first feature returned in the Query result 
                    Feature feature = features[0];

                    // Add the returned feature to the collection of currently selected features
                    _featureLayer.SelectFeature(feature);

                    // Zoom to the extent of the newly selected feature
                    await _myMapView.SetViewpointGeometryAsync(feature.Geometry.Extent);
                }
                else
                {
                    var alert = new UIAlertView("State Not Found!", "Add a valid state name.", null, "OK", null);
                    alert.Show();
                }
            }
            catch (Exception ex)
            {
                var alert = new UIAlertView("Sample error", ex.ToString(), null, "OK", null);
                alert.Show();
            }
        }

        private void CreateLayout()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(
                0, yPageOffset + 80, View.Bounds.Width, View.Bounds.Height - yPageOffset - 80);

            // Create text view for query input
            _queryTextView = new UITextField(
                new CoreGraphics.CGRect(0, yPageOffset, View.Bounds.Width, 40));
            _queryTextView.Placeholder = "State name";
            _queryTextView.AdjustsFontSizeToFitWidth = true;
            _queryTextView.BackgroundColor = UIColor.White;

            // Create button to invoke the query
            var queryButton = new UIButton(
               new CoreGraphics.CGRect(0, yPageOffset + 40, View.Bounds.Width, 40));
            queryButton.SetTitle("Query", UIControlState.Normal);
            queryButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            queryButton.BackgroundColor = UIColor.White;

            // Hook to touch event to do querying
            queryButton.TouchUpInside += OnQueryClicked;

            // Add MapView to the page
            View.AddSubviews(_myMapView, _queryTextView, queryButton);
        }
    }
}