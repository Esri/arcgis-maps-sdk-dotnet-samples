// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.StatisticalQuery
{
    [Register("StatisticalQuery")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Statistical query",
        "Data",
        "This sample demonstrates how to query a feature table to get statistics for a specified field.",
        "Check the appropriate boxes to filter features by attributes and/or within the current extent. Click the button to see basic statistics displayed for world cities.")]
    public class StatisticalQuery : UIViewController
    {
        // Create and hold references to the UI controls.
        private MapView _myMapView;
        private UIToolbar _toolbar;

        // URI for the world cities map service layer.
        private readonly Uri _worldCitiesServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer/0");

        // World cities feature table.
        private FeatureTable _worldCitiesTable;

        public StatisticalQuery()
        {
            Title = "Statistical query";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        private void Initialize()
        {
            // Create a new Map with the world streets vector basemap.
            Map myMap = new Map(Basemap.CreateStreetsVector());

            // Create feature table using the world cities URI.
            _worldCitiesTable = new ServiceFeatureTable(_worldCitiesServiceUri);

            // Create a new feature layer to display features in the world cities table.
            FeatureLayer worldCitiesLayer = new FeatureLayer(_worldCitiesTable);

            // Add the world cities layer to the map.
            myMap.OperationalLayers.Add(worldCitiesLayer);

            // Assign the map to the MapView.
            _myMapView.Map = myMap;
        }

        public override void LoadView()
        {
            View = new UIView();
            
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _toolbar = new UIToolbar();
            _toolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            _toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem("Get statistics", UIBarButtonItemStyle.Plain, GetStatisticsPressed),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            // Add MapView and UI controls to the page.
            View.AddSubviews(_myMapView, _toolbar);

            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(_toolbar.TopAnchor).Active = true;

            _toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor).Active = true;
        }

        private void GetStatisticsPressed(object sender, EventArgs e)
        {
            var alert = UIAlertController.Create ("Query statistics", "Get statistics for all cities matching these criteria", UIAlertControllerStyle.ActionSheet);
            if (alert.PopoverPresentationController != null)
            {
                alert.PopoverPresentationController.BarButtonItem = _toolbar.Items[0];  
            }

            alert.AddAction(UIAlertAction.Create("Cities in extent with pop. > 5M", UIAlertActionStyle.Default, action => QueryStatistics(true, true)));
            alert.AddAction(UIAlertAction.Create("Cities with pop. > 5M", UIAlertActionStyle.Default, action => QueryStatistics(false, true)));
            alert.AddAction(UIAlertAction.Create("Cities in extent", UIAlertActionStyle.Default, action => QueryStatistics(true, false)));
            alert.AddAction(UIAlertAction.Create("All cities", UIAlertActionStyle.Default, action => QueryStatistics(false, false)));
            PresentViewController (alert, animated: true, completionHandler: null);
        }

        private async void QueryStatistics(bool onlyInExtent, bool onlyLargePop)
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

            try
            {
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
            catch (ArcGISWebException exception)
            {
                ShowMessage("There was a problem running the query.", exception.ToString());
            }
        }

        private void ShowStatsList(IReadOnlyDictionary<string, object> stats)
        {
            // Create a new Alert Controller.
            UIAlertController statsAlert = UIAlertController.Create("Statistics", "", UIAlertControllerStyle.Alert);

            // Loop through all key/value pairs in the results.
            foreach (KeyValuePair<string, object> kvp in stats)
            {
                // If the value is null, display "--".
                string displayString = "--";

                if (kvp.Value != null)
                {
                    displayString = kvp.Value.ToString();
                }

                // Add the statistics info as an alert action.
                statsAlert.AddAction(UIAlertAction.Create(kvp.Key + " : " + displayString, UIAlertActionStyle.Default, null));
            }

            // Add an Action to dismiss the alert.
            statsAlert.AddAction(UIAlertAction.Create("Dismiss", UIAlertActionStyle.Cancel, null));

            // Display the alert.
            PresentViewController(statsAlert, true, null);
        }

        private void ShowMessage(string title, string message)
        {
            new UIAlertView(title, message, (IUIAlertViewDelegate) null, "OK", null).Show();
        }
    }
}