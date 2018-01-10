// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.QueryFeatureCountAndExtent
{
    [Activity]
    public class QueryFeatureCountAndExtent : Activity
    {
        // Search box for state entry
        private EditText _myStateEntry;

        // Label for results
        private TextView _myResultsLabel;

        // Button for querying cities by state
        private Button _myQueryStateButton;

        // Button for querying cities within the current extent
        private Button _myQueryExtentButton;

        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // URL to the feature service
        private Uri _UsaCitiesSource = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/0");

        // Statistic definition that returns a count of AREANAME (city names)
        private StatisticDefinition countStatistic = new StatisticDefinition("AREANAME", StatisticType.Count, "pop");

        // Feature table to query
        private ServiceFeatureTable _myFeatureTable;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Query feature count and extent";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
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
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the entry
            _myStateEntry = new EditText(this) { Text = "State abbreviation (e.g. NY)" };

            // Create the results label
            _myResultsLabel = new TextView(this);

            // Create the two buttons
            _myQueryStateButton = new Button(this) { Text = "Query by State" };
            _myQueryExtentButton = new Button(this) { Text = "Query Extent" };

            // Subscribe to button events
            _myQueryExtentButton.Click += btnExtentCount_Click;
            _myQueryStateButton.Click += btnStateCount_Click;

            // Add the views to the layout
            layout.AddView(_myStateEntry);
            layout.AddView(_myQueryStateButton);
            layout.AddView(_myQueryExtentButton);
            layout.AddView(_myResultsLabel);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}