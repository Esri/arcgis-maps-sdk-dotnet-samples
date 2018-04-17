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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.StatisticalQuery
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Statistical query",
        "Data",
        "This sample demonstrates how to query a feature table to get statistics for a specified field.",
        "Check the appropriate boxes to filter features by attributes and/or within the current extent. Click the button to see basic statistics displayed for world cities.")]
    public partial class StatisticalQuery
    {
        // URI for the world cities map service layer
        private Uri _worldCitiesServiceUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer/0");

        // World cities feature table
        private FeatureTable _worldCitiesTable;

        public StatisticalQuery()
        {
            InitializeComponent();

            // Initialize the base map and world cities feature layer
            Initialize();
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
            MyMapView.Map = myMap;
        }

        private async void OnExecuteStatisticsQueryClicked(object sender, RoutedEventArgs e)
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
            if (OnlyInExtentCheckbox.IsChecked == true)
            {
                // Get the current extent (envelope) from the map view
                Envelope currentExtent = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;

                // Set the statistics query parameters geometry with the envelope
                statQueryParams.Geometry = currentExtent;

                // Set the spatial relationship to Intersects (which is the default)
                statQueryParams.SpatialRelationship = SpatialRelationship.Intersects;
            }

            // If only evaluating the largest cities (over 5 million in population), set up an attribute filter
            if (OnlyBigCitiesCheckbox.IsChecked == true)
            {
                // Set a where clause to get the largest cities (could also use "POP_CLASS = '5,000,000 and greater'")
                statQueryParams.WhereClause = "POP_RANK = 1";
            }

            // Execute the statistical query with these parameters and await the results
            StatisticsQueryResult statQueryResult = await _worldCitiesTable.QueryStatisticsAsync(statQueryParams);

            // Display results in the list box
            StatResultsListBox.ItemsSource = statQueryResult.FirstOrDefault().Statistics.ToList();
        }
    }
}