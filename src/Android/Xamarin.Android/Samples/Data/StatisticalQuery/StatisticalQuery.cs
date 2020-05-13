// Copyright 2017 Esri.
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

namespace ArcGISRuntime.Samples.StatisticalQuery
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Statistical query",
        category: "Data",
        description: "Query a table to get aggregated statistics back for a specific field.",
        instructions: "Pan and zoom to define the extent for the query. Use the 'Cities in current extent' checkbox to control whether the query only includes features in the visible extent. Use the 'Cities grater than 5M' checkbox to filter the results to only those cities with a population greater than 5 million people. Tap 'Get statistics' to perform the query. The query will return population-based statistics from the combined results of all features matching the query criteria.",
        tags: new[] { "analysis", "average", "bounding geometry", "filter", "intersect", "maximum", "mean", "minimum", "query", "spatial query", "standard deviation", "statistics", "sum", "variance" })]
    public class StatisticalQuery : Activity
    {
        // Hold a reference to the map view
        private MapView _myMapView;

        // URI for the world cities map service layer
        private Uri _worldCitiesServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer/0");

        // World cities feature table
        private FeatureTable _worldCitiesTable;

        // Linear layout UI control for arranging query controls
        private LinearLayout _controlsLayout;

        // UI controls (switches) that will need to be referenced
        private Switch _onlyInExtentSwitch;
        private Switch _onlyBigCitiesSwitch;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Statistical query";

            // Create the UI
            CreateLayout();

            // Initialize the map and layers
            Initialize();
        }

        private void Initialize()
        {
            // Create a new Map with the world streets basemap
            Map myMap = new Map(Basemap.CreateStreets());

            // Create feature table using the world cities URI
            _worldCitiesTable = new ServiceFeatureTable(_worldCitiesServiceUri);

            // Create a new feature layer to display features in the world cities table
            FeatureLayer worldCitiesLayer = new FeatureLayer(_worldCitiesTable);

            // Add the world cities layer to the map
            myMap.OperationalLayers.Add(worldCitiesLayer);

            // Assign the map to the MapView
            _myMapView.Map = myMap;
        }

        private async void OnExecuteStatisticsQueryClicked(object sender, EventArgs e)
        {
            // Create definitions for each statistic to calculate
            StatisticDefinition statDefinitionAvgPop = new StatisticDefinition("POP", StatisticType.Average, "");
            StatisticDefinition statDefinitionMinPop = new StatisticDefinition("POP", StatisticType.Minimum, "");
            StatisticDefinition statDefinitionMaxPop = new StatisticDefinition("POP", StatisticType.Maximum, "");
            StatisticDefinition statDefinitionSumPop = new StatisticDefinition("POP", StatisticType.Sum, "");
            StatisticDefinition statDefinitionStdDevPop = new StatisticDefinition("POP", StatisticType.StandardDeviation, "");
            StatisticDefinition statDefinitionVarPop = new StatisticDefinition("POP", StatisticType.Variance, "");

            // Create a definition for count that includes an alias for the output
            StatisticDefinition statDefinitionCount = new StatisticDefinition("POP", StatisticType.Count, "CityCount");

            // Add the statistics definitions to a list
            List<StatisticDefinition> statDefinitions = new List<StatisticDefinition>
                          { statDefinitionAvgPop,
                            statDefinitionCount,
                            statDefinitionMinPop,
                            statDefinitionMaxPop,
                            statDefinitionSumPop,
                            statDefinitionStdDevPop,
                            statDefinitionVarPop
                          };

            // Create the statistics query parameters, pass in the list of definitions
            StatisticsQueryParameters statQueryParams = new StatisticsQueryParameters(statDefinitions);

            // If only using features in the current extent, set up the spatial filter for the statistics query parameters
            if (_onlyInExtentSwitch.Checked)
            {
                // Get the current extent (envelope) from the map view
                Envelope currentExtent = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;

                // Set the statistics query parameters geometry with the envelope
                statQueryParams.Geometry = currentExtent;

                // Set the spatial relationship to Intersects (which is the default)
                statQueryParams.SpatialRelationship = SpatialRelationship.Intersects;
            }

            // If only evaluating the largest cities (over 5 million in population), set up an attribute filter
            if (_onlyBigCitiesSwitch.Checked)
            {
                // Set a where clause to get the largest cities (could also use "POP_CLASS = '5,000,000 and greater'")
                statQueryParams.WhereClause = "POP_RANK = 1";
            }

            try
            {
                // Execute the statistical query with these parameters and await the results
                StatisticsQueryResult statQueryResult = await _worldCitiesTable.QueryStatisticsAsync(statQueryParams);

                // Display results in a list in a dialog
                List<KeyValuePair<string,object>> statsList = statQueryResult.First().Statistics.ToList();
                ShowStatsList(statsList);
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle("Error").Show();
            }
        }

        private void ShowStatsList(IList<KeyValuePair<string, object>> stats)
        {
            // Create a list of statistics results (field names and values) to show in the list
            IList<string> statInfoList = new List<string>();
            foreach (KeyValuePair<string,object> kvp in stats)
            {
                // If the value is null, display "--"
                string displayString = "--";

                if (kvp.Value != null)
                {
                    displayString = kvp.Value.ToString();
                }

                statInfoList.Add(kvp.Key + " : " + displayString);
            }

            // Create an array adapter for the stats list
            ArrayAdapter<string> statsArrayAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, statInfoList);
            statsArrayAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleListItem1);

            // Create a list view to display the results
            ListView statsListView = new ListView(this)
            {
                Adapter = statsArrayAdapter
            };

            // Show the list view in a dialog
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);
            dialogBuilder.SetView(statsListView);
            dialogBuilder.Show();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            _controlsLayout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create switches for controlling which features are included in the query
            _onlyBigCitiesSwitch = new Switch(this)
            {
                Text = "Only cities over 5M"
            };
            _onlyInExtentSwitch = new Switch(this)
            {
                Text = "Only in current extent"
            };

            // Create a Button to execute the statistical query
            Button getStatsButton = new Button(this)
            {
                Text = "Get Statistics"
            };
            getStatsButton.Click += OnExecuteStatisticsQueryClicked;

            // Add the query controls to the layout
            _controlsLayout.AddView(_onlyBigCitiesSwitch);
            _controlsLayout.AddView(_onlyInExtentSwitch);
            _controlsLayout.AddView(getStatsButton);

            // Add the map view to the layout
            _myMapView = new MapView(this);
            _controlsLayout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(_controlsLayout);
        }
    }
}