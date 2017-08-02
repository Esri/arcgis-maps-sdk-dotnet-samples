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

namespace ArcGISRuntimeXamarin.Samples.StatisticalQuery
{
    [Register("StatisticalQuery")]
    public class StatisticalQuery : UIViewController
    {
        // Constant holding offset where the MapView control should start
        private const int yPageOffset = 60;

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
            // Create a stack view to organize the query controls
            _controlsStackView = new UIStackView();
            _controlsStackView.Axis = UILayoutConstraintAxis.Vertical;
            _controlsStackView.Alignment = UIStackViewAlignment.Fill;
            _controlsStackView.Distribution = UIStackViewDistribution.EqualSpacing;
            _controlsStackView.BackgroundColor = UIColor.Gray;
            _controlsStackView.Opaque = true;

            // Create switches (and associated labels) to control query filter options
            UILabel citySwitchLabel = new UILabel();
            citySwitchLabel.Text = "Only cities over 5M";
            _onlyBigCitiesSwitch = new UISwitch();
            UIStackView citySwitchStackView = new UIStackView();
            citySwitchStackView.Axis = UILayoutConstraintAxis.Horizontal;
            citySwitchStackView.Alignment = UIStackViewAlignment.Fill;
            citySwitchStackView.Distribution = UIStackViewDistribution.EqualSpacing;
            citySwitchStackView.AddArrangedSubview(citySwitchLabel);
            citySwitchStackView.AddArrangedSubview(_onlyBigCitiesSwitch);

            _onlyInExtentSwitch = new UISwitch();

            // Create button to invoke the query
            var getStatsButton = new UIButton();
            getStatsButton.SetTitle("Get Statistics", UIControlState.Normal);
            getStatsButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            getStatsButton.BackgroundColor = UIColor.White;

            // Handle the button tap to execute the statistics query
            getStatsButton.TouchUpInside += OnExecuteStatisticsQueryClicked;

            // Add controls to the stack view
            //_controlsStackView.AddArrangedSubview(_onlyInExtentSwitch);
            _controlsStackView.AddArrangedSubview(citySwitchStackView);
            _controlsStackView.AddArrangedSubview(getStatsButton);

            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 120, View.Bounds.Width, View.Bounds.Height-120);

            // Setup the visual frame for the query controls
            _controlsStackView.Frame = new CoreGraphics.CGRect(0, yPageOffset, View.Bounds.Width, 160);

            // Add MapView to the page
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

            // Display results in the list box
            //StatResultsList.ItemsSource = statQueryResult.FirstOrDefault().Statistics.ToList();
            //ResultsGrid.IsVisible = true;
        }
    }
}