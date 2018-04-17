// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.StatsQueryGroupAndSort
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Statistical query group and sort results",
        "Data",
        "This sample demonstrates how to query a feature table to get statistics for a specified field and to group and sort the results.",
        "")]
    public partial class StatsQueryGroupAndSort : ContentPage
    {
        // URI for the US states map service
        private Uri _usStatesServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3");

        // US states feature table
        private FeatureTable _usStatesTable;

        // Collection of (user-defined) statistics to use in the query
        private ObservableCollection<StatisticDefinition> _statDefinitions = new ObservableCollection<StatisticDefinition>();

        // Selected fields for grouping results
        private List<string> _groupByFields = new List<string>();

        // Collection to hold fields to order results by
        private ObservableCollection<OrderFieldOption> _orderByFields = new ObservableCollection<OrderFieldOption>();

        public StatsQueryGroupAndSort()
        {
            InitializeComponent();
            
            Title = "Statistical query group and sort";

            // Initialize the US states feature table and populate UI controls
            Initialize();
        }

        private async void Initialize()
        {
            // Create the US states feature table
            _usStatesTable = new ServiceFeatureTable(_usStatesServiceUri);

            // Load the table
            await _usStatesTable.LoadAsync();

            // Fill the fields combo and "group by" list with field names from the table
            List<string> fieldNames = _usStatesTable.Fields.Select(field => field.Name).ToList();
            FieldsComboBox.ItemsSource = fieldNames;
            GroupFieldsListBox.ItemsSource = _usStatesTable.Fields;

            // Set the (initially empty) collection of fields as the "order by" fields list data source
            OrderByFieldsListBox.ItemsSource = _orderByFields;

            // Fill the statistics type combo with values from the StatisticType enum
            StatTypeComboBox.ItemsSource = Enum.GetValues(typeof(StatisticType));

            // Set the (initially empty) collection of statistic definitions as the statistics list box data source
            StatFieldsListBox.ItemsSource = _statDefinitions;
        }

        // Execute a statistical query using the parameters defined by the user and display the results
        private async void OnExecuteStatisticsQueryClicked(object sender, EventArgs e)
        {
            // Verify that there is at least one statistic definition
            if (_statDefinitions.Count() == 0)
            {
                await DisplayAlert("Please define at least one statistic for the query.", "Statistical Query","OK");
                return;
            }

            // Create the statistics query parameters, pass in the list of statistic definitions
            StatisticsQueryParameters statQueryParams = new StatisticsQueryParameters(_statDefinitions);

            // Specify the group fields (if any)
            foreach (string groupField in _groupByFields)
            {
                statQueryParams.GroupByFieldNames.Add(groupField);
            }

            // Specify the fields to order by (if any)
            foreach (OrderFieldOption orderBy in _orderByFields)
            {
                statQueryParams.OrderByFields.Add(orderBy.OrderInfo);
            }

            // Execute the statistical query with these parameters and await the results
            StatisticsQueryResult statQueryResult = await _usStatesTable.QueryStatisticsAsync(statQueryParams);

            // Get results formatted as a lookup (list of group names and their associated dictionary of results)
            ILookup<string,IReadOnlyDictionary<string,object>> resultsLookup = statQueryResult.ToLookup(result => string.Join(", ", result.Group.Values), result => result.Statistics);
            
            // Loop through the formatted results and build a list of classes to display as grouped results in the list view
            ObservableCollection<ResultGroup> resultsGroupCollection = new ObservableCollection<ResultGroup>();
            foreach (IGrouping<string,IReadOnlyDictionary<string,object>> group in resultsLookup)
            {
                // Create a new group
                ResultGroup resultGroup = new ResultGroup() { GroupName = group.Key };

                // Loop through all the results for this group and add them to the collection
                foreach (IReadOnlyDictionary<string,object> resultSet in group)
                {
                    foreach(KeyValuePair<string,object> result in resultSet)
                    {
                        resultGroup.Add(new StatisticResult { FieldName = result.Key, StatValue = result.Value});
                    }
                }

                // Add the group of results to the collection
                resultsGroupCollection.Add(resultGroup);
            }

            // Apply the results to the list view data source and show the results grid
            StatResultsList.ItemsSource = resultsGroupCollection; 
            ResultsGrid.IsVisible = true;
        }        

        // Handle when the switch for a "group by" field is toggled on or off by adding or removing the field from the collection
        private void GroupFieldCheckChanged(object sender, EventArgs e)
        {
            // Get the check box that raised the event (group field)
            Switch groupFieldCheckBox = (sender as Switch);

            // Get the field name
            string fieldName = groupFieldCheckBox.BindingContext.ToString();

            // See if the field is being added or removed from the "group by" list
            bool fieldAdded = groupFieldCheckBox.IsToggled;

            // See if the field already exists in the "group by" list
            bool fieldIsInList = _groupByFields.Contains(fieldName);

            // If the field is being added, and is NOT in the list, add it ...
            if (fieldAdded && !fieldIsInList)
            {
                _groupByFields.Add(fieldName);

                // Also add it to the "order by" list
                OrderBy orderBy = new OrderBy(fieldName, SortOrder.Ascending);
                OrderFieldOption orderOption = new OrderFieldOption(false, orderBy);
                _orderByFields.Add(orderOption);
            }
            // If the field is being removed and it IS in the list, remove it ...
            else if (!fieldAdded && fieldIsInList)
            {
                _groupByFields.Remove(fieldName);

                // Also check for this field in the "order by" list and remove if necessary (only group fields can be used to order results)
                OrderFieldOption orderBy = _orderByFields.FirstOrDefault(field => field.OrderInfo.FieldName == fieldName);
                if (orderBy != null)
                {
                    // Remove the field from the "order by" list
                    _orderByFields.Remove(orderBy);
                }
            }
        }

        // Create a statistic definition and add it to the collection based on the user selection in the combo boxes
        private void AddStatisticClicked(object sender, EventArgs e)
        {
            // Verify that a field name and statistic type has been selected
            if (FieldsComboBox.SelectedItem == null || StatTypeComboBox.SelectedItem == null) { return; }

            // Get the chosen field name and statistic type from the combo boxes
            string fieldName = FieldsComboBox.SelectedItem.ToString();
            StatisticType statType = (StatisticType)StatTypeComboBox.SelectedItem;

            // Check if this statistic definition has already be created (same field name and statistic type)
            StatisticDefinition existingStatDefinition = _statDefinitions.FirstOrDefault(def => def.OnFieldName == fieldName && def.StatisticType == statType);

            // If it doesn't exist, create it and add it to the collection (use the field name and statistic type to build the output alias)
            if (existingStatDefinition == null)
            {
                StatisticDefinition statDefinition = new StatisticDefinition(fieldName, statType, fieldName + "_" + statType.ToString());
                _statDefinitions.Add(statDefinition);
            }
        }

        // Toggle the sort order (ascending/descending) for the field selected in the sort fields list
        private void ChangeFieldSortOrder(object sender, EventArgs e)
        {
            // Verify that there is a selected sort field in the list
            OrderFieldOption selectedSortField = OrderByFieldsListBox.SelectedItem as OrderFieldOption;
            if (selectedSortField == null) { return; }

            // Create a new order field info to define the sort for the selected field
            OrderBy newOrderBy = new OrderBy(selectedSortField.OrderInfo.FieldName, selectedSortField.OrderInfo.SortOrder);
            OrderFieldOption newSortDefinition = new OrderFieldOption(true, newOrderBy);

            // Toggle the sort order from the current value
            if (newSortDefinition.OrderInfo.SortOrder == SortOrder.Ascending)
            {
                newSortDefinition.OrderInfo.SortOrder = SortOrder.Descending;
            }
            else
            {
                newSortDefinition.OrderInfo.SortOrder = SortOrder.Ascending;
            }

            // Add the new OrderBy at the same location in the collection and remove the old one
            _orderByFields.Insert(_orderByFields.IndexOf(selectedSortField), newSortDefinition);
            _orderByFields.Remove(selectedSortField);
        }

        // Remove the selected statistic definition from the list
        private void RemoveStatisticClicked(object sender, EventArgs e)
        {
            // Verify that there is a selected statistic definition
            if (StatFieldsListBox.SelectedItem == null) { return; }

            // Get the selected statistic definition and remove it from the collection
            StatisticDefinition selectedStat = StatFieldsListBox.SelectedItem as StatisticDefinition;
            _statDefinitions.Remove(selectedStat);
        }

        // Add the selected field in the "group by" list to the "order by" list
        private void AddSortFieldClicked(object sender, EventArgs e)
        {
            // Verify that there is a selected field in the "group by" list
            if (GroupFieldsListBox.SelectedItem == null) { return; }

            // Get the name of the selected field and ensure that it's in the list of selected group fields (checked on in the list, e.g.)
            string selectedFieldName = GroupFieldsListBox.SelectedItem.ToString();
            if (!_groupByFields.Contains(selectedFieldName))
            {
                DisplayAlert("Only fields used for grouping can be used to order results.", "Query", "OK");
                return;
            }

            // Verify that the field isn't already in the "order by" list
            OrderFieldOption existingOrderBy = _orderByFields.FirstOrDefault(field => field.OrderInfo.FieldName == selectedFieldName);
            if (existingOrderBy == null)
            {
                // Create a new OrderBy for this field and add it to the collection (default to ascending sort order)
                OrderBy newOrderBy = new OrderBy(selectedFieldName, SortOrder.Ascending);
                OrderFieldOption orderField = new OrderFieldOption(false, newOrderBy);
                _orderByFields.Add(orderField);
            }
        }

        // Remove the selected field from the list of "order by" fields
        private void RemoveSortFieldClicked(object sender, EventArgs e)
        {
            // Verify that there is a selected item in the "order by" list
            if (OrderByFieldsListBox.SelectedItem == null) { return; }

            // Get the selected OrderFieldOption object and remove it from the collection
            OrderFieldOption selectedOrderBy = OrderByFieldsListBox.SelectedItem as OrderFieldOption;
            _orderByFields.Remove(selectedOrderBy);
        }

        private void HideResults(object sender, EventArgs e)
        {
            // Dismiss the results grid and show the input controls
            ResultsGrid.IsVisible = false;
        }
    }

    // Simple class to describe an "order by" option
    public class OrderFieldOption
    {
        // Whether or not to use this field to order results
        public bool OrderWith { get; set; }

        // The order by info: field name and sort order
        public OrderBy OrderInfo { get; set; }

        public OrderFieldOption(bool orderWith, OrderBy orderInfo)
        {
            OrderWith = orderWith;
            OrderInfo = orderInfo;
        }
    }

    // A simple class to hold a single statistic result
    public class StatisticResult
    {
        public string FieldName { get; set; }
        public object StatValue { get; set; }
    }

    // A class to represent a group of statistics results
    public class ResultGroup : ObservableCollection<StatisticResult>
    {
        public string GroupName { get; set; }
        public string ShortName { get; set; }
    }
}