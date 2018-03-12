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
        "Statistical query",
        "Data",
        "This sample demonstrates how to query a feature table to get statistics for a specified field.",
        "Check the appropriate boxes to filter features by attributes and/or within the current extent. Click the button to see basic statistics displayed for world cities.")]
    public class StatisticalQuery : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // URI for the world cities map service layer
        private Uri _worldCitiesServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer/0");

        // World cities feature table
        private FeatureTable _worldCitiesTable;

        // Stack view UI control for arranging query controls
        private UIStackView _controlsStackView;

        // UI controls (switches) that will need to be referenced
        private UISwitch _onlyInExtentSwitch;
        private UISwitch _onlyBigCitiesSwitch;

        public StatisticalQuery()
        {
            Title = "Statistical query";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI
            CreateLayout();

            // Initialize the map and layers
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Get height of status bar and navigation bar
            nfloat pageOffset = NavigationController.NavigationBar.Frame.Size.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

            // Setup the visual frame for the query controls
            _controlsStackView.Frame = new CoreGraphics.CGRect(0,  pageOffset, View.Bounds.Width, 150);

            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
        }

        private void Initialize()
        {
            // Create a new Map with the world streets vector basemap
            Map myMap = new Map(Basemap.CreateStreetsVector());

            // Create feature table using the world cities URI
            _worldCitiesTable = new ServiceFeatureTable(_worldCitiesServiceUri);

            // Create a new feature layer to display features in the world cities table
            FeatureLayer worldCitiesLayer = new FeatureLayer(_worldCitiesTable);

            // Add the world cities layer to the map
            myMap.OperationalLayers.Add(worldCitiesLayer);

            // Assign the map to the MapView
            _myMapView.Map = myMap;
        }

        private void CreateLayout()
        {
            View.BackgroundColor = UIColor.White;

            // Create a stack view to organize the query controls
            _controlsStackView = new UIStackView();
            _controlsStackView.Axis = UILayoutConstraintAxis.Vertical;
            _controlsStackView.Alignment = UIStackViewAlignment.Center;
            _controlsStackView.Distribution = UIStackViewDistribution.EqualSpacing;
            _controlsStackView.Spacing = 5;

            // Create a switch (and associated label) to include only big cities in the query
            _onlyBigCitiesSwitch = new UISwitch();
            _onlyBigCitiesSwitch.BackgroundColor = UIColor.White;
            UILabel citySwitchLabel = new UILabel();
            citySwitchLabel.BackgroundColor = UIColor.White;
            citySwitchLabel.TextColor = UIColor.Blue;
            citySwitchLabel.Text = "Only cities over 5M";

            // Add the switch and label to a horizontal panel
            UIStackView citySwitchStackView = new UIStackView();
            citySwitchStackView.Axis = UILayoutConstraintAxis.Horizontal;
            citySwitchStackView.Alignment = UIStackViewAlignment.Fill;
            citySwitchStackView.Distribution = UIStackViewDistribution.EqualSpacing;
            citySwitchStackView.AddArrangedSubview(citySwitchLabel);
            citySwitchStackView.AddArrangedSubview(_onlyBigCitiesSwitch);

            // Create a switch (and associated label) to include only cities in the current extent
            _onlyInExtentSwitch = new UISwitch();
            _onlyBigCitiesSwitch.BackgroundColor = UIColor.White;
            UILabel extentSwitchLabel = new UILabel();
            extentSwitchLabel.BackgroundColor = UIColor.White;
            extentSwitchLabel.TextColor = UIColor.Blue;
            extentSwitchLabel.Text = "Only cities in extent";

            // Add the switch and label to a horizontal panel
            UIStackView extentSwitchStackView = new UIStackView();
            extentSwitchStackView.Axis = UILayoutConstraintAxis.Horizontal;
            extentSwitchStackView.Alignment = UIStackViewAlignment.Fill;
            extentSwitchStackView.Distribution = UIStackViewDistribution.EqualSpacing;
            extentSwitchStackView.AddArrangedSubview(extentSwitchLabel);
            extentSwitchStackView.AddArrangedSubview(_onlyInExtentSwitch);

            // Create a button to invoke the query
            var getStatsButton = new UIButton();
            getStatsButton.SetTitle("Get Statistics", UIControlState.Normal);
            getStatsButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            getStatsButton.BackgroundColor = UIColor.White;

            // Handle the button tap to execute the statistics query
            getStatsButton.TouchUpInside += OnExecuteStatisticsQueryClicked;

            // Add controls to the stack view
            _controlsStackView.AddArrangedSubview(extentSwitchStackView);
            _controlsStackView.AddArrangedSubview(citySwitchStackView);
            _controlsStackView.AddArrangedSubview(getStatsButton);

            // Add MapView and UI controls to the page
            View.AddSubviews(_myMapView, _controlsStackView);
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
            if (_onlyInExtentSwitch.On)
            {
                // Get the current extent (envelope) from the map view
                Envelope currentExtent = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;

                // Set the statistics query parameters geometry with the envelope
                statQueryParams.Geometry = currentExtent;

                // Set the spatial relationship to Intersects (which is the default)
                statQueryParams.SpatialRelationship = SpatialRelationship.Intersects;
            }

            // If only evaluating the largest cities (over 5 million in population), set up an attribute filter
            if (_onlyBigCitiesSwitch.On)
            {
                // Set a where clause to get the largest cities (could also use "POP_CLASS = '5,000,000 and greater'")
                statQueryParams.WhereClause = "POP_RANK = 1";
            }

            // Execute the statistical query with these parameters and await the results
            StatisticsQueryResult statQueryResult = await _worldCitiesTable.QueryStatisticsAsync(statQueryParams);

            // Get the first (only) StatisticRecord in the results
            StatisticRecord record = statQueryResult.FirstOrDefault();

            // Make sure a record was returned
            if (record == null || record.Statistics.Count == 0)
            {
                // Notify the user that no results were returned
                UIAlertView alert = new UIAlertView();
                alert.Message = "No results were returned";
                alert.Title = "Statistical Query";
                alert.Show();
                return;
            }

            // Display results
            IReadOnlyDictionary<string, object> statistics = record.Statistics;
            ShowStatsList(statistics);
        }

        private void ShowStatsList(IReadOnlyDictionary<string, object> stats)
        {
            // Create a new Alert Controller
            UIAlertController statsAlert = UIAlertController.Create("Statistics", string.Empty, UIAlertControllerStyle.Alert);

            // Loop through all key/value pairs in the results
            foreach (KeyValuePair<string, object> kvp in stats)
            {
                // If the value is null, display "--"
                string displayString = "--";

                if (kvp.Value != null)
                {
                    displayString = kvp.Value.ToString();
                }

                // Add the statistics info as an alert action
                statsAlert.AddAction(UIAlertAction.Create(kvp.Key + " : " + displayString, UIAlertActionStyle.Default, null));
            }

            // Add an Action to dismiss the alert
            statsAlert.AddAction(UIAlertAction.Create("Dismiss", UIAlertActionStyle.Cancel, null));

            // Display the alert
            PresentViewController(statsAlert, true, null);
        }
    }
}