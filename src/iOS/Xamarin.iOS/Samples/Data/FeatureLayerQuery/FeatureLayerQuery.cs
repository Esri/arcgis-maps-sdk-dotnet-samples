// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureLayerQuery
{
    [Register("FeatureLayerQuery")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Feature layer query",
        category: "Data",
        description: "Find features in a feature table which match an SQL query.",
        instructions: "Input the name of a U.S. state into the text field. When you tap the button, a query is performed and the matching features are highlighted or an error is returned.",
        tags: new[] { "query", "search" })]
    public class FeatureLayerQuery : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _queryButton;

        // Create reference to service of US States  
        private const string StatesUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/2";

        // Create globally available feature table for easy referencing 
        private ServiceFeatureTable _featureTable;

        // Create globally available feature layer for easy referencing 
        private FeatureLayer _featureLayer;

        public FeatureLayerQuery()
        {
            Title = "Feature layer query";
        }

        private void Initialize()
        {
            // Create new Map with basemap.
            Map map = new Map(Basemap.CreateTopographic());

            // Create and set initial map location.
            MapPoint initialLocation = new MapPoint(-11000000, 5000000, SpatialReferences.WebMercator);
            map.InitialViewpoint = new Viewpoint(initialLocation, 100000000);

            // Create feature table using a URL.
            _featureTable = new ServiceFeatureTable(new Uri(StatesUrl));

            // Create feature layer using this feature table.
            _featureLayer = new FeatureLayer(_featureTable)
            {
                // Set the Opacity of the Feature Layer.
                Opacity = 0.6,
                // Work around service setting
                MaxScale = 10
            };

            // Create a new renderer for the States Feature Layer.
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Black, 1);
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Transparent, lineSymbol);

            // Set States feature layer renderer.
            _featureLayer.Renderer = new SimpleRenderer(fillSymbol);

            // Add feature layer to the map.
            map.OperationalLayers.Add(_featureLayer);

            // Assign the map to the MapView.
            _myMapView.Map = map;

            // Set the selection color.
            _myMapView.SelectionProperties.Color = Color.Cyan;
        }

        private void OnQueryClicked(object sender, EventArgs e)
        {
            // Prompt for the type of convex hull to create.
            UIAlertController unionAlert = UIAlertController.Create("Query features", "Enter a state name.", UIAlertControllerStyle.Alert);
            unionAlert.AddTextField(field => field.Placeholder = "State name");
            unionAlert.AddAction(UIAlertAction.Create("Submit query", UIAlertActionStyle.Default, async action => await QueryStateFeature(unionAlert.TextFields[0].Text)));
            unionAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Show the alert.
            PresentViewController(unionAlert, true, null);
        }

        private async Task QueryStateFeature(string stateName)
        {
            try
            {
                // Clear the existing selection.
                _featureLayer.ClearSelection();

                // Create a query parameters that will be used to Query the feature table.
                QueryParameters queryParams = new QueryParameters();

                // Trim whitespace on the state name to prevent broken queries.
                string formattedStateName = stateName.Trim().ToUpper();

                // Construct and assign the where clause that will be used to query the feature table.
                queryParams.WhereClause = "upper(STATE_NAME) LIKE '%" + formattedStateName + "%'";

                // Query the feature table.
                FeatureQueryResult queryResult = await _featureTable.QueryFeaturesAsync(queryParams);

                // Cast the QueryResult to a List so the results can be interrogated.
                List<Feature> features = queryResult.ToList();

                if (features.Any())
                {
                    // Create an envelope.
                    EnvelopeBuilder envBuilder = new EnvelopeBuilder(SpatialReferences.WebMercator);

                    // Loop over each feature from the query result.
                    foreach (Feature feature in features)
                    {
                        // Add the extent of each matching feature to the envelope.
                        envBuilder.UnionOf(feature.Geometry.Extent);

                        // Select each feature.
                        _featureLayer.SelectFeature(feature);
                    }

                    // Zoom to the extent of the selected feature(s).
                    await _myMapView.SetViewpointGeometryAsync(envBuilder.ToGeometry(), 50);
                }
                else
                {
                    UIAlertView alert = new UIAlertView("State Not Found!", "Add a valid state name.", (IUIAlertViewDelegate) null, "OK", null);
                    alert.Show();
                }
            }
            catch (Exception ex)
            {
                UIAlertView alert = new UIAlertView("Sample error", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null);
                alert.Show();
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _queryButton = new UIBarButtonItem();
            _queryButton.Title = "Query features";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _queryButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _queryButton.Clicked += OnQueryClicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _queryButton.Clicked -= OnQueryClicked;
        }
    }
}