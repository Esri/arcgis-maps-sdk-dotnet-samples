// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.QueryFeatureCountAndExtent
{
    [Register("QueryFeatureCountAndExtent")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Query feature count and extent",
        "Analysis",
        "Zoom to features matching a query and count the features in the current visible extent.",
        "Use the button to zoom to the extent of the state specified (by abbreviation) in the textbox or use the button to count the features in the current extent.",
        "count", "feature layer", "feature table", "features", "filter", "number", "query")]
    public class QueryFeatureCountAndExtent : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UILabel _resultLabel;
        private UIBarButtonItem _countInExtentButton;
        private UIBarButtonItem _zoomToQueryButton;
        private UIAlertController _unionAlert;

        // URL to the feature service.
        private readonly Uri _medicareHospitalSpendLayer =
            new Uri("https://services1.arcgis.com/4yjifSiIG17X0gW4/arcgis/rest/services/Medicare_Hospital_Spending_per_Patient/FeatureServer/0");

        // Feature table to query.
        private ServiceFeatureTable _featureTable;

        public QueryFeatureCountAndExtent()
        {
            Title = "Query feature count and extent";
        }

        private async void Initialize()
        {
            // Create the map with a basemap.
            Map myMap = new Map(Basemap.CreateDarkGrayCanvasVector());

            // Create the feature table from the service URL.
            _featureTable = new ServiceFeatureTable(_medicareHospitalSpendLayer);

            // Create the feature layer from the table.
            FeatureLayer myFeatureLayer = new FeatureLayer(_featureTable);

            // Add the feature layer to the map.
            myMap.OperationalLayers.Add(myFeatureLayer);

            try
            {
                // Wait for the feature layer to load.
                await myFeatureLayer.LoadAsync();

                // Set the map initial extent to the extent of the feature layer.
                myMap.InitialViewpoint = new Viewpoint(myFeatureLayer.FullExtent);

                // Add the map to the MapView.
                _myMapView.Map = myMap;
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private async void ZoomToFeature(string query)
        {
            // Create the query parameters.
            QueryParameters queryStates = new QueryParameters {WhereClause = $"upper(State) LIKE '%{query.ToUpper()}%'"};

            try
            {
                // Get the extent from the query.
                Envelope resultExtent = await _featureTable.QueryExtentAsync(queryStates);

                // Return if there is no result (might happen if query is invalid).
                if (resultExtent?.SpatialReference == null)
                {
                    return;
                }

                // Create a viewpoint from the extent.
                Viewpoint resultViewpoint = new Viewpoint(resultExtent);

                // Zoom to the viewpoint.
                await _myMapView.SetViewpointAsync(resultViewpoint);

                // Update the UI.
                _resultLabel.Text = $"Zoomed to features in {query}";
            }
            catch (Exception ex)
            {
                new UIAlertView("Error", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private void ZoomToQuery_Click(object sender, EventArgs e)
        {
            // Prompt for the type of convex hull to create.
            if (_unionAlert == null)
            {
                _unionAlert = UIAlertController.Create("Query features", "Enter a state abbreviation (e.g. CA)", UIAlertControllerStyle.Alert);
                _unionAlert.AddTextField(field => field.Placeholder = "e.g. CA");
                _unionAlert.AddAction(UIAlertAction.Create("Submit query", UIAlertActionStyle.Default, action => ZoomToFeature(_unionAlert.TextFields[0].Text)));
                _unionAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
            }

            // Show the alert.
            PresentViewController(_unionAlert, true, null);
        }

        private async void CountFeatures_Click(object sender, EventArgs e)
        {
            // Get the current visible extent.
            Geometry currentExtent = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry;

            // Create the query parameters.
            QueryParameters queryCityCount = new QueryParameters
            {
                Geometry = currentExtent,
                // Specify the interpretation of the Geometry query parameters.
                SpatialRelationship = SpatialRelationship.Intersects
            };

            try
            {
                // Get the count of matching features.
                long count = await _featureTable.QueryFeatureCountAsync(queryCityCount);

                // Update the UI.
                _resultLabel.Text = $"{count} features in extent";
            }
            catch (Exception ex)
            {
                new UIAlertView("Error", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
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

            _zoomToQueryButton = new UIBarButtonItem();
            _zoomToQueryButton.Title = "Zoom to query";

            _countInExtentButton = new UIBarButtonItem();
            _countInExtentButton.Title = "Count in extent";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                _countInExtentButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _zoomToQueryButton
            };

            _resultLabel = new UILabel
            {
                Text = "Press 'Zoom to query' to begin.",
                BackgroundColor = UIColor.FromWhiteAlpha(0f, .6f),
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar, _resultLabel);

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

                _resultLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _resultLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _resultLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _resultLabel.HeightAnchor.ConstraintEqualTo(40)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _zoomToQueryButton.Clicked += ZoomToQuery_Click;
            _countInExtentButton.Clicked += CountFeatures_Click;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _zoomToQueryButton.Clicked -= ZoomToQuery_Click;
            _countInExtentButton.Clicked -= CountFeatures_Click;

            // Remove the reference to the alert, preventing a memory leak.
            _unionAlert = null;
        }
    }
}