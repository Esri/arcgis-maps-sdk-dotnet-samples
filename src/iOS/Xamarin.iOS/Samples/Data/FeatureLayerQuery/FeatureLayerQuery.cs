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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureLayerQuery
{
    [Register("FeatureLayerQuery")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer query",
        "Data",
        "This sample demonstrates how to query a feature layer via feature table.",
        "The sample provides a search bar on the top, where you can input the name of a US State. When you hit search the app performs a query on the feature table and based on the result either highlights the state geometry or provides an error.")]
    public class FeatureLayerQuery : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Create reference to service of US States  
        private string _statesUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/2";

        // Create globally available text box for easy referencing 
        private UITextField _queryTextView;

        // Create button to invoke the query
        private UIButton _queryButton;

        // Create globally available feature table for easy referencing 
        private ServiceFeatureTable _featureTable;
        
        // Create globally available feature layer for easy referencing 
        private FeatureLayer _featureLayer;

        // Toolbar to go behind the form
        private UIToolbar _toolbar = new UIToolbar();

        // Help label
        private UILabel _helpLabel;

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

        public override void ViewDidLayoutSubviews()
        {
            nfloat formStart = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
            nfloat margin = 5;
            nfloat controlHeight = 30;

            // Setup the visual frames for the views
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _toolbar.Frame = new CoreGraphics.CGRect(0, formStart, View.Bounds.Width, controlHeight * 2 + margin * 3);
            _helpLabel.Frame = new CoreGraphics.CGRect(margin, formStart + margin, View.Bounds.Width - (2 * margin), controlHeight);
            _queryTextView.Frame = new CoreGraphics.CGRect(margin, formStart + controlHeight + 2 * margin, View.Bounds.Width - 100 - (2 * margin), controlHeight);
            _queryButton.Frame = new CoreGraphics.CGRect(View.Bounds.Width - 100 - (2 * margin), formStart +  controlHeight + 2 *margin, 100, controlHeight);

            base.ViewDidLayoutSubviews();
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
            _featureLayer = new FeatureLayer(_featureTable)
            {
                // Set the Opacity of the Feature Layer
                Opacity = 0.6
            };

            // Create a new renderer for the States Feature Layer
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Black, 1);
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Yellow, lineSymbol);

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

                // Trim whitespace on the state name to prevent broken queries
                string formattedStateName = stateName.Trim().ToUpper();

                // Construct and assign the where clause that will be used to query the feature table 
                queryParams.WhereClause = "upper(STATE_NAME) LIKE '%" + formattedStateName + "%'";

                // Query the feature table 
                FeatureQueryResult queryResult = await _featureTable.QueryFeaturesAsync(queryParams);

                // Cast the QueryResult to a List so the results can be interrogated
                List<Feature> features = queryResult.ToList();

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
                    var alert = new UIAlertView("State Not Found!", "Add a valid state name.", (IUIAlertViewDelegate)null, "OK", null);
                    alert.Show();
                }
            }
            catch (Exception ex)
            {
                var alert = new UIAlertView("Sample error", ex.ToString(), (IUIAlertViewDelegate)null, "OK", null);
                alert.Show();
            }
        }

        private void CreateLayout()
        {

            // Create text view for query input
            _queryTextView = new UITextField
            {
                Placeholder = "State name",
                AdjustsFontSizeToFitWidth = true
            };
            // Allow pressing 'return' to dismiss the keyboard
            _queryTextView.ShouldReturn += (textField) => { textField.ResignFirstResponder(); return true; };

            // Create button to invoke the query
            _queryButton = new UIButton();
            _queryButton.SetTitle("Query", UIControlState.Normal);
            _queryButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _queryButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Right;

            // Create the help label
            _helpLabel = new UILabel
            {
                Text = "Enter a state name and tap 'Query' to search.",
                Lines = 1,
                AdjustsFontSizeToFitWidth = true
            };

            // Hook to touch event to do querying
            _queryButton.TouchUpInside += OnQueryClicked;

            // Add MapView to the page
            View.AddSubviews(_myMapView, _toolbar, _helpLabel, _queryTextView, _queryButton);
        }
    }
}