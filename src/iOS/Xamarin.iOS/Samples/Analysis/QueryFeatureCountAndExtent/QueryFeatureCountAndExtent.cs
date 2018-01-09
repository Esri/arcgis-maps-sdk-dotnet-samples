// Copyright 2018 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.QueryFeatureCountAndExtent
{
    [Register("QueryFeatureCountAndExtent")]
    public class QueryFeatureCountAndExtent : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Button for querying cities in the current extent
        private UIButton _myQueryExtentButton;

        // Button for querying cities by state
        private UIButton _myQueryStateButton;

        // Search box for entering state name
        private UISearchBar _myStateEntry;

        // Label to show the results
        private UILabel _myResultsLabel;

        // URL to the feature service
        private Uri _UsaCitiesSource = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/0");

        // Statistic definition that returns a count of AREANAME (city names)
        private StatisticDefinition countStatistic = new StatisticDefinition("AREANAME", StatisticType.Count, "pop");

        // Feature table to query
        private ServiceFeatureTable _myFeatureTable;

        public QueryFeatureCountAndExtent()
        {
            Title = "Query feature count and extent";
        }

        private async void Initialize()
        {
            // Create the map with a vector street basemap
            Map myMap = new Map(Basemap.CreateStreetsVector());

            // Create the feature table from the service URL
            _myFeatureTable = new ServiceFeatureTable(_UsaCitiesSource);

            // Create the feature layer from the table
            FeatureLayer myFeatureLayer = new FeatureLayer(_myFeatureTable);

            // Add the feature layer to the map
            myMap.OperationalLayers.Add(myFeatureLayer);

            // Wait for the feature layer to load
            await myFeatureLayer.LoadAsync();

            // Set the map initial extent to the extent of the feature layer
            myMap.InitialViewpoint = new Viewpoint(myFeatureLayer.FullExtent);

            // Add the map to the MapView
            _myMapView.Map = myMap;
        }

        private async void btnStateCount_Click(object sender, EventArgs e)
        {
            // Create the statistics query parameters to control the query
            StatisticsQueryParameters statQueryParams = new StatisticsQueryParameters(new List<StatisticDefinition>() { countStatistic });

            // Limit results to matching states
            statQueryParams.WhereClause = String.Format("upper(ST) LIKE '%{0}%'", _myStateEntry.Text.ToUpper());

            // Execute the statistical query with these parameters and await the results
            StatisticsQueryResult statQueryResult = await _myFeatureTable.QueryStatisticsAsync(statQueryParams);

            // Display the results in the UI
            _myResultsLabel.Text = "Result: " + statQueryResult.First().Statistics["pop"].ToString();
        }

        private async void btnExtentCount_Click(object sender, EventArgs e)
        {
            // Create the statistics query parameters, pass in the list of definitions
            StatisticsQueryParameters statQueryParams = new StatisticsQueryParameters(new List<StatisticDefinition>() { countStatistic });

            // Get the current extent (envelope) from the map view
            Envelope currentExtent = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;

            // Set the statistics query parameters geometry with the envelope
            statQueryParams.Geometry = currentExtent;

            // Set the spatial relationship to Intersects (which is the default)
            statQueryParams.SpatialRelationship = SpatialRelationship.Intersects;

            // Execute the statistical query with these parameters and await the results
            StatisticsQueryResult statQueryResult = await _myFeatureTable.QueryStatisticsAsync(statQueryParams);

            // Display the results in the UI
            _myResultsLabel.Text = "Result: " + statQueryResult.First().Statistics["pop"].ToString();
        }

        private void CreateLayout()
        {
            // Create the extent query button and subscribe to events
            _myQueryExtentButton = new UIButton();
            _myQueryExtentButton.SetTitle("Query Extent", UIControlState.Normal);
            _myQueryExtentButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _myQueryExtentButton.TouchUpInside += btnExtentCount_Click;

            // Create the state query button and subscribe to events
            _myQueryStateButton = new UIButton();
            _myQueryStateButton.SetTitle("Query State", UIControlState.Normal);
            _myQueryStateButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _myQueryStateButton.TouchUpInside += btnStateCount_Click;

            // Create the results label and the search bar
            _myResultsLabel = new UILabel() { TextColor = UIColor.Red };
            _myStateEntry = new UISearchBar();

            // Allow the search bar to dismiss the keyboard
            _myStateEntry.SearchButtonClicked += (sender, e) =>
            {
                _myStateEntry.EndEditing(true);
            };

            // Add views to the page
            View.AddSubviews(_myMapView, _myQueryExtentButton, _myQueryStateButton, _myResultsLabel, _myStateEntry);
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            var topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height + 10;

            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Place extent button
            _myQueryExtentButton.Frame = new CoreGraphics.CGRect(10, topMargin + 25, View.Bounds.Width / 2 - 15, 20);

            // Place state button
            _myQueryStateButton.Frame = new CoreGraphics.CGRect(View.Bounds.Width / 2 + 5, topMargin + 25, View.Bounds.Width / 2 - 15, 20);

            // Place state text field
            _myStateEntry.Frame = new CoreGraphics.CGRect(10, topMargin, View.Bounds.Width - 20, 20);

            // Place result label
            _myResultsLabel.Frame = new CoreGraphics.CGRect(10, topMargin + 50, View.Bounds.Width - 20, 20);

            base.ViewDidLayoutSubviews();
        }
    }
}