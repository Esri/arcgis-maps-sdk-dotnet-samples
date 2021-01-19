// Copyright 2017 Esri.
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

namespace ArcGISRuntime.Samples.StatisticalQuery
{
    [Register("StatisticalQuery")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Statistical query",
        category: "Data",
        description: "Query a table to get aggregated statistics back for a specific field.",
        instructions: "Pan and zoom to define the extent for the query. Use the 'Cities in current extent' checkbox to control whether the query only includes features in the visible extent. Use the 'Cities grater than 5M' checkbox to filter the results to only those cities with a population greater than 5 million people. Tap 'Get statistics' to perform the query. The query will return population-based statistics from the combined results of all features matching the query criteria.",
        tags: new[] { "analysis", "average", "bounding geometry", "filter", "intersect", "maximum", "mean", "minimum", "query", "spatial query", "standard deviation", "statistics", "sum", "variance" })]
    public class StatisticalQuery : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _queryButton;

        // URI for the world cities map service layer.
        private readonly Uri _worldCitiesServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer/0");

        // World cities feature table.
        private FeatureTable _worldCitiesTable;

        public StatisticalQuery()
        {
            Title = "Statistical query";
        }

        private void Initialize()
        {
            // Create a new Map with the world streets vector basemap.
            Map myMap = new Map(BasemapStyle.ArcGISStreets);

            // Create feature table using the world cities URI.
            _worldCitiesTable = new ServiceFeatureTable(_worldCitiesServiceUri);

            // Create a new feature layer to display features in the world cities table.
            FeatureLayer worldCitiesLayer = new FeatureLayer(_worldCitiesTable);

            // Add the world cities layer to the map.
            myMap.OperationalLayers.Add(worldCitiesLayer);

            // Assign the map to the MapView.
            _myMapView.Map = myMap;
        }

        private void GetStatisticsPressed(object sender, EventArgs e)
        {
            var alert = UIAlertController.Create("Query statistics", "Get statistics for all cities matching these criteria", UIAlertControllerStyle.ActionSheet);
            if (alert.PopoverPresentationController != null)
            {
                alert.PopoverPresentationController.BarButtonItem = _queryButton;
            }

            alert.AddAction(UIAlertAction.Create("Cities in extent with pop. > 5M", UIAlertActionStyle.Default, action => QueryStatistics(true, true)));
            alert.AddAction(UIAlertAction.Create("Cities with pop. > 5M", UIAlertActionStyle.Default, action => QueryStatistics(false, true)));
            alert.AddAction(UIAlertAction.Create("Cities in extent", UIAlertActionStyle.Default, action => QueryStatistics(true, false)));
            alert.AddAction(UIAlertAction.Create("All cities", UIAlertActionStyle.Default, action => QueryStatistics(false, false)));
            PresentViewController(alert, true, null);
        }

        private async void QueryStatistics(bool onlyInExtent, bool onlyLargePop)
        {
            try
            {
                // Create definitions for each statistic to calculate.
                StatisticDefinition statDefinitionAvgPop = new StatisticDefinition("POP", StatisticType.Average, "");
                StatisticDefinition statDefinitionMinPop = new StatisticDefinition("POP", StatisticType.Minimum, "");
                StatisticDefinition statDefinitionMaxPop = new StatisticDefinition("POP", StatisticType.Maximum, "");
                StatisticDefinition statDefinitionSumPop = new StatisticDefinition("POP", StatisticType.Sum, "");
                StatisticDefinition statDefinitionStdDevPop = new StatisticDefinition("POP", StatisticType.StandardDeviation, "");
                StatisticDefinition statDefinitionVarPop = new StatisticDefinition("POP", StatisticType.Variance, "");

                // Create a definition for count that includes an alias for the output.
                StatisticDefinition statDefinitionCount = new StatisticDefinition("POP", StatisticType.Count, "CityCount");

                // Add the statistics definitions to a list.
                List<StatisticDefinition> statDefinitions = new List<StatisticDefinition>
                {
                    statDefinitionAvgPop,
                    statDefinitionCount,
                    statDefinitionMinPop,
                    statDefinitionMaxPop,
                    statDefinitionSumPop,
                    statDefinitionStdDevPop,
                    statDefinitionVarPop
                };

                // Create the statistics query parameters, pass in the list of definitions.
                StatisticsQueryParameters statQueryParams = new StatisticsQueryParameters(statDefinitions);

                // If only using features in the current extent, set up the spatial filter for the statistics query parameters.
                if (onlyInExtent)
                {
                    // Get the current extent (envelope) from the map view.
                    Envelope currentExtent = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;

                    // Set the statistics query parameters geometry with the current extent.
                    statQueryParams.Geometry = currentExtent;

                    // Set the spatial relationship to Intersects (which is the default).
                    statQueryParams.SpatialRelationship = SpatialRelationship.Intersects;
                }

                // If only evaluating the largest cities (over 5 million in population), set up an attribute filter.
                if (onlyLargePop)
                {
                    // Set a where clause to get the largest cities (could also use "POP_CLASS = '5,000,000 and greater'").
                    statQueryParams.WhereClause = "POP_RANK = 1";
                }

                // Execute the statistical query with these parameters and await the results.
                StatisticsQueryResult statQueryResult = await _worldCitiesTable.QueryStatisticsAsync(statQueryParams);

                // Get the first (only) StatisticRecord in the results.
                StatisticRecord record = statQueryResult.FirstOrDefault();

                // Make sure a record was returned.
                if (record == null || record.Statistics.Count == 0)
                {
                    ShowMessage("No result", "No results were returned.");
                    return;
                }

                // Display results.
                ShowStatsList(record.Statistics);
            }
            catch (Exception ex)
            {
                ShowMessage("There was a problem running the query.", ex.Message);
            }
        }

        private void ShowStatsList(IReadOnlyDictionary<string, object> stats)
        {
            // Create a string for statistics in plain text.
            string statsList = "";

            // Loop through all key/value pairs in the results.
            foreach (KeyValuePair<string, object> kvp in stats)
            {
                // If the value is null, display "--".
                string displayString = "--";

                if (kvp.Value != null)
                {
                    displayString = kvp.Value.ToString();
                }

                // Add the statistics info to the output string.
                statsList = $"{statsList}\n{kvp.Key} : {displayString}";
            }

            // Create a new Alert Controller.
            UIAlertController statsAlert = UIAlertController.Create("Statistics", statsList, UIAlertControllerStyle.Alert);

            // Add an Action to dismiss the alert.
            statsAlert.AddAction(UIAlertAction.Create("Dismiss", UIAlertActionStyle.Cancel, null));

            // Display the alert.
            PresentViewController(statsAlert, true, null);
        }

        private void ShowMessage(string title, string message)
        {
            new UIAlertView(title, message, (IUIAlertViewDelegate)null, "OK", null).Show();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _queryButton = new UIBarButtonItem();
            _queryButton.Title = "Get statistics";

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
            _queryButton.Clicked += GetStatisticsPressed;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _queryButton.Clicked -= GetStatisticsPressed;
        }
    }
}