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
using CoreGraphics;
using Esri.ArcGISRuntime.Http;
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
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private UISwitch _onlyInExtentSwitch;
        private UISwitch _onlyBigCitiesSwitch;
        private UILabel _extentSwitchLabel;
        private UILabel _citySwitchLabel;
        private UIButton _getStatsButton;

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

            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Size.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat controlHeight = 30;
                nfloat margin = 5;
                nfloat switchWidth = _onlyInExtentSwitch.IntrinsicContentSize.Width;
                nfloat colSplit = View.Bounds.Width / 2;

                // Reposition the controls.
                _toolbar.Frame = new CGRect(0, topMargin, View.Bounds.Width, controlHeight * 2 + margin * 3);
                _extentSwitchLabel.Frame = new CGRect(margin, _toolbar.Frame.Top + margin, colSplit - switchWidth - 2 * margin, controlHeight);
                _onlyInExtentSwitch.Frame = new CGRect(colSplit - switchWidth, _toolbar.Frame.Top + margin, switchWidth, controlHeight);
                _citySwitchLabel.Frame = new CGRect(colSplit + margin, _toolbar.Frame.Top + margin, colSplit - switchWidth - 4 * margin, controlHeight);
                _onlyBigCitiesSwitch.Frame = new CGRect(View.Bounds.Width - switchWidth - margin, _toolbar.Frame.Top + margin, switchWidth, controlHeight);
                _getStatsButton.Frame = new CGRect(margin, _onlyBigCitiesSwitch.Frame.Bottom + margin, View.Bounds.Width - 2 * margin, controlHeight);
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(_toolbar.Frame.Bottom, 0, 0, 0);

                base.ViewDidLayoutSubviews();
            }
            catch (NullReferenceException)
            {
            }
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

        private void CreateLayout()
        {
            // Create a switch (and associated label) to include only big cities in the query.
            _onlyBigCitiesSwitch = new UISwitch();
            _citySwitchLabel = new UILabel
            {
                Text = "Only cities over 5M",
                TextAlignment = UITextAlignment.Right,
                AdjustsFontSizeToFitWidth = true
            };

            // Create a switch (and associated label) to include only cities in the current extent.
            _onlyInExtentSwitch = new UISwitch();
            _extentSwitchLabel = new UILabel
            {
                Text = "Only cities in extent",
                TextAlignment = UITextAlignment.Right,
                AdjustsFontSizeToFitWidth = true
            };

            // Create a button to invoke the query.
            _getStatsButton = new UIButton();
            _getStatsButton.SetTitle("Get statistics", UIControlState.Normal);
            _getStatsButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Handle the button tap to execute the statistics query.
            _getStatsButton.TouchUpInside += OnExecuteStatisticsQueryClicked;

            // Add MapView and UI controls to the page.
            View.AddSubviews(_myMapView, _toolbar, _onlyInExtentSwitch, _onlyBigCitiesSwitch, _citySwitchLabel, _extentSwitchLabel, _getStatsButton);
        }

        private async void OnExecuteStatisticsQueryClicked(object sender, EventArgs e)
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
            if (_onlyInExtentSwitch.On)
            {
                // Set the statistics query parameters geometry with the current extent (envelope) from the map view.
                statQueryParams.Geometry = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;

                // Set the spatial relationship to Intersects (which is the default).
                statQueryParams.SpatialRelationship = SpatialRelationship.Intersects;
            }

            // If only evaluating the largest cities (over 5 million in population), set up an attribute filter.
            if (_onlyBigCitiesSwitch.On)
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
                    showMessage("No result", "No results were returned.");
                }

                // Display results.
                IReadOnlyDictionary<string, object> statistics = record.Statistics;
                ShowStatsList(statistics);
            }
            catch (ArcGISWebException exception)
            {
                showMessage("There was a problem running the query.", exception.ToString());
            }
        }

        private void ShowStatsList(IReadOnlyDictionary<string, object> stats)
        {
            // Create a new Alert Controller.
            UIAlertController statsAlert = UIAlertController.Create("Statistics", string.Empty, UIAlertControllerStyle.Alert);

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

        private void showMessage(string title, string message)
        {
            new UIAlertView(title, message, (IUIAlertViewDelegate) null, "OK", null).Show();
        }
    }
}