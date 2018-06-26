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
using Esri.ArcGISRuntime.Http;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.StatsQueryGroupAndSort
{
    [Register("StatsQueryGroupAndSort")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Group and sort statistics",
        "Data",
        "This sample demonstrates how to query a feature table to get statistics for a specified field and to group and sort the results.",
        "")]
    public class StatsQueryGroupAndSort : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly UIToolbar _toolbar = new UIToolbar();
        private UILabel _helpLabel;
        private UIButton _showStatDefinitionsButton;
        private UIButton _showGroupFieldsButton;
        private UIButton _showOrderByFieldsButton;
        private UIButton _getStatsButton;

        // URI for the US states map service.
        private readonly Uri _usStatesServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3");

        // US states feature table.
        private FeatureTable _usStatesTable;

        // List of field names from the table.
        private List<string> _fieldNames;

        // Selected fields for grouping results.
        private Dictionary<string, bool> _groupByFields = new Dictionary<string, bool>();

        // Collection to hold fields to order results by.
        private readonly List<OrderFieldOption> _orderByFields = new List<OrderFieldOption>();

        // Model for defining choices in the statistics definition UIPickerView.
        private StatDefinitionModel _statsPickerModel;

        // List of statistics definitions to use in the query.
        private readonly List<StatisticDefinition> _statisticDefinitions = new List<StatisticDefinition>();

        public StatsQueryGroupAndSort()
        {
            Title = "Statistical query group and sort";
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat pageOffset = NavigationController.NavigationBar.Frame.Size.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat controlHeight = 30;
                nfloat controlWidth = View.Bounds.Width - 2 * margin;

                // Reposition the controls.
                _toolbar.Frame = new CGRect(0, pageOffset, View.Bounds.Width, View.Bounds.Height - pageOffset);
                _helpLabel.Frame = new CGRect(margin, pageOffset + margin, controlWidth, controlHeight * 2);
                _showStatDefinitionsButton.Frame = new CGRect(margin, pageOffset + controlHeight * 4 + margin * 2, controlWidth, controlHeight);
                _showGroupFieldsButton.Frame = new CGRect(margin, pageOffset + controlHeight * 5 + margin * 3, controlWidth, controlHeight);
                _showOrderByFieldsButton.Frame = new CGRect(margin, pageOffset + controlHeight * 6 + margin * 4, controlWidth, controlHeight);
                _getStatsButton.Frame = new CGRect(margin, View.Bounds.Height - controlHeight - margin, controlWidth, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        private void CreateLayout()
        {
            View.BackgroundColor = UIColor.White;

            _helpLabel = new UILabel
            {
                Text = "Tap 'Choose statistic definitions' to specify statistics to calculate. Then tap 'Choose group fields' to define how results will be grouped. Tap 'Choose order by fields' to define how results will be ordered. Finally, tap 'Get statistics' to see the results.",
                Lines = 4,
                AdjustsFontSizeToFitWidth = true
            };

            // Button for launching the UI to view or define statistics definitions for the query.
            _showStatDefinitionsButton = new UIButton();
            _showStatDefinitionsButton.SetTitle("Choose statistic definitions", UIControlState.Normal);
            _showStatDefinitionsButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _showStatDefinitionsButton.TouchUpInside += ShowStatDefinitions;

            // Button to choose fields with which to group results.
            _showGroupFieldsButton = new UIButton();
            _showGroupFieldsButton.SetTitle("Choose group fields", UIControlState.Normal);
            _showGroupFieldsButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _showGroupFieldsButton.TouchUpInside += ShowGroupFields;

            // Button to choose fields with which to sort results (must be one of the 'group by' fields).
            _showOrderByFieldsButton = new UIButton();
            _showOrderByFieldsButton.SetTitle("Choose 'Order by' fields", UIControlState.Normal);
            _showOrderByFieldsButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _showOrderByFieldsButton.TouchUpInside += ShowOrderByFields;

            // Create a button to invoke the query using the query parameters defined.
            _getStatsButton = new UIButton();
            _getStatsButton.SetTitle("Get statistics", UIControlState.Normal);
            _getStatsButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Handle the button tap to execute the statistics query.
            _getStatsButton.TouchUpInside += ExecuteStatisticsQuery;

            // Add controls to the stack view.
            View.AddSubviews(_toolbar, _helpLabel, _showStatDefinitionsButton, _showGroupFieldsButton, _showOrderByFieldsButton, _getStatsButton);
        }

        private async void Initialize()
        {
            // Create the US states feature table.
            _usStatesTable = new ServiceFeatureTable(_usStatesServiceUri);

            // Load the table.
            await _usStatesTable.LoadAsync();

            // Fill the fields combo and "group by" list with field names from the table.
            _fieldNames = _usStatesTable.Fields.Select(field => field.Name).ToList();

            // Create a model that will provide statistic definition choices for the picker.
            _statsPickerModel = new StatDefinitionModel(_fieldNames.ToArray());

            // Create a list of fields the user can select for grouping.
            // Value is initially false, since no fields are selected by default.
            _groupByFields = _fieldNames.ToDictionary(name => name, name => false);
        }

        private void ShowGroupFields(object sender, EventArgs e)
        {
            // Create a new table.
            UITableViewController fieldsTable = new UITableViewController(UITableViewStyle.Plain)
            {
                // Set the data source on the table.
                TableView = {Source = new GroupFieldsDataSource(_groupByFields)}
            };

            // Show the table view.
            NavigationController.PushViewController(fieldsTable, true);
        }

        // Show fields the user can choose to sort results with (must be one of the group by fields).
        private void ShowOrderByFields(object sender, EventArgs e)
        {
            // Create a new table.
            UITableViewController sortFieldsTable = new UITableViewController(UITableViewStyle.Plain);

            // Get the current list of group fields and create/update the sort field choices.
            List<KeyValuePair<string, bool>> sortFieldChoices = _groupByFields.Where(field => field.Value).ToList();
            foreach (KeyValuePair<string, bool> sortChoice in sortFieldChoices)
            {
                // If this group field is not in the list of available order fields, add it to the list.
                OrderFieldOption existingOption = _orderByFields.Find(opt => opt.OrderInfo.FieldName == sortChoice.Key);
                if (existingOption == null)
                {
                    existingOption = new OrderFieldOption(false, new OrderBy(sortChoice.Key, SortOrder.Ascending));
                    _orderByFields.Add(existingOption);
                }
            }

            // Also make sure to remove any order by fields that were removed as 'group by' fields.
            for (int i = _orderByFields.Count - 1; i >= 0; i--)
            {
                // If this field is not in the grouped field list, remove it from the order fields list.
                OrderFieldOption opt = _orderByFields.ElementAt(i);
                KeyValuePair<string, bool> existingGroupField = sortFieldChoices.FirstOrDefault(field => field.Key == opt.OrderInfo.FieldName);
                if (existingGroupField.Key == null)
                {
                    _orderByFields.RemoveAt(i);
                }
            }

            // Set the data source on the table.
            sortFieldsTable.TableView.Source = new OrderByFieldsDataSource(_orderByFields);

            // Show the table view.
            NavigationController.PushViewController(sortFieldsTable, true);
        }

        private void ShowStatDefinitions(object sender, EventArgs e)
        {
            // Create a new UIPickerView and assign a model that will show fields and statistic types.
            UIPickerView statisticPicker = new UIPickerView
            {
                Model = _statsPickerModel
            };

            // Create a new table.
            UITableViewController statsTable = new UITableViewController(UITableViewStyle.Plain);

            // Create an instance of a custom data source to show statistic definitions in the table.
            // Pass in the list of statistic definitions and the picker (for defining new ones).
            StatisticDefinitionsDataSource statDefsDataSource = new StatisticDefinitionsDataSource(_statisticDefinitions, statisticPicker);

            // Set the data source on the table.
            statsTable.TableView.Source = statDefsDataSource;

            // Put the table in edit mode (to show add and delete buttons).
            statDefsDataSource.WillBeginTableEditing(statsTable.TableView);
            statsTable.SetEditing(true, true);

            // Show the table view.
            NavigationController.PushViewController(statsTable, true);
        }

        private async void ExecuteStatisticsQuery(object sender, EventArgs e)
        {
            // Remove the placeholder "Add statistic" row (if it exists).
            StatisticDefinition placeholderRow = _statisticDefinitions.LastOrDefault();
            if (placeholderRow != null && placeholderRow.OutputAlias == "")
            {
                _statisticDefinitions.Remove(placeholderRow);
            }

            // Verify that there is at least one statistic definition.
            if (!_statisticDefinitions.Any())
            {
                ShowAlert("Statistical Query", "Please define at least one statistic for the query.");
                return;
            }

            // Create the statistics query parameters, pass in the list of statistic definitions.
            StatisticsQueryParameters statQueryParams = new StatisticsQueryParameters(_statisticDefinitions);

            // Specify the selected group fields (if any).
            if (_groupByFields != null)
            {
                foreach (KeyValuePair<string, bool> groupField in _groupByFields.Where(field => field.Value))
                {
                    statQueryParams.GroupByFieldNames.Add(groupField.Key);
                }
            }

            // Specify the fields to order by (if any).
            if (_orderByFields != null)
            {
                foreach (OrderFieldOption orderBy in _orderByFields)
                {
                    statQueryParams.OrderByFields.Add(orderBy.OrderInfo);
                }
            }

            // Execute the statistical query with these parameters and await the results.
            try
            {
                StatisticsQueryResult statQueryResult = await _usStatesTable.QueryStatisticsAsync(statQueryParams);

                // Get results formatted as a dictionary (group names and their associated dictionary of results).
                Dictionary<string, IReadOnlyDictionary<string, object>> resultsLookup = statQueryResult.ToDictionary(result => string.Join(", ", result.Group.Values), result => result.Statistics);

                // Create an instance of a custom data source to display the results.
                StatisticQueryResultsDataSource statResultsDataSource = new StatisticQueryResultsDataSource(resultsLookup);

                // Create a new table with a grouped style for displaying rows.
                UITableViewController statResultsTable = new UITableViewController(UITableViewStyle.Grouped)
                {
                    // Set the table view data source.
                    TableView = { Source = statResultsDataSource }
                };

                // Show the table view.
                NavigationController.PushViewController(statResultsTable, true);
            }
            catch (ArcGISWebException exception)
            {
                ShowAlert("There was a problem performing the query.", exception.ToString());
            }
        }

        private void ShowAlert(string title, string message)
        {
            // Create a new Alert Controller.
            UIAlertController alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

            // Add an Action to dismiss the alert.
            alert.AddAction(UIAlertAction.Create("Dismiss", UIAlertActionStyle.Cancel, null));

            // Display the alert.
            PresentViewController(alert, true, null);
        }
    }

    // Simple class to describe an "order by" option.
    public class OrderFieldOption
    {
        // Whether or not to use this field to order results.
        public bool OrderWith { get; set; }

        // The order by info: field name and sort order.
        public OrderBy OrderInfo { get; }

        public OrderFieldOption(bool orderWith, OrderBy orderInfo)
        {
            OrderWith = orderWith;
            OrderInfo = orderInfo;
        }
    }

    // Class that defines a view model for showing field names and statistic types in a picker.
    public class StatDefinitionModel : UIPickerViewModel
    {
        // Array of field names.
        private readonly string[] _fieldNames;

        // Array of available statistic types.
        private readonly Array _statTypes = Enum.GetValues(typeof(StatisticType));

        // Constructor that takes an array of the available field names.
        public StatDefinitionModel(string[] fieldNames)
        {
            _fieldNames = fieldNames;
        }

        // Property to expose the currently selected definition in the picker.
        public StatisticDefinition SelectedStatDefinition { get; private set; } = null;

        // Return the number of picker components (two sections: field names and statistic types).
        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 2;
        }

        // Return the number of rows in each of the two sections.
        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            // first component is the fields list, second is the statistic types.
            return component == 0 ? _fieldNames.Length : _statTypes.Length;
        }

        // Get the title to display in each picker component.
        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            // first component is the fields list, second is the statistic types.
            return component == 0 ? _fieldNames[row] : _statTypes.GetValue(row).ToString();
        }

        // Handle the selection event for the picker to create a statistic definition with the values chosen.
        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            // Get the field name.
            string onFieldName = _fieldNames[pickerView.SelectedRowInComponent(0)];

            // Get the statistic type.
            StatisticType statType = (StatisticType) _statTypes.GetValue(pickerView.SelectedRowInComponent(1));

            // Create an output field alias by concatenating the field name and statistic type.
            string outAlias = onFieldName + "_" + statType;

            // Create a new statistic definition (available from the SelectedStatDefinition public property).
            SelectedStatDefinition = new StatisticDefinition(onFieldName, statType, outAlias);
        }

        // Return the desired width for each component in the picker.
        public override nfloat GetComponentWidth(UIPickerView pickerView, nint component)
        {
            // first component is the fields list, second is the statistic types.
            return component == 0 ? 160f : 120f;
        }

        // Return the desired height for rows in the picker.
        public override nfloat GetRowHeight(UIPickerView pickerView, nint component)
        {
            return 40f;
        }
    }

    // Class that defines a custom data source for showing statistic definitions.
    public class StatisticDefinitionsDataSource : UITableViewSource
    {
        // List of statistic definitions for the current query.
        private readonly List<StatisticDefinition> _statisticDefinitions;

        // Picker for choosing a field and statistic type.
        private readonly UIPickerView _statPicker;

        // Custom UI to show the statistics picker and associated buttons.
        private ChooseStatisticOverlay _chooseStatOverlay;

        // Text to display for the placeholder row used to add new statistic definitions.
        private const string AddNewStatFieldName = "(Add statistic)";

        // Constructor that takes a list of statistic definitions and a picker for selecting fields and statistic types.
        public StatisticDefinitionsDataSource(List<StatisticDefinition> statDefs, UIPickerView picker)
        {
            // Store the list of statistic definitions and the statistic picker.
            _statisticDefinitions = statDefs;
            _statPicker = picker;
        }

        // Handle supported edits to the data source (inserts and deletes).
        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            // Respond to the user's edit request: Insert a new statistic definition, or delete an existing one.
            if (editingStyle == UITableViewCellEditingStyle.Insert)
            {
                // Get the bounds of the table.
                CGRect ovBounds = tableView.Bounds;
                ovBounds.Height = ovBounds.Height + 60;

                // Create an overlay UI that lets the user choose a field and statistic type to add.
                _chooseStatOverlay = new ChooseStatisticOverlay(ovBounds, 0.70f, UIColor.White, _statPicker);

                // Handle the OnStatisticDefined event to get the info entered by the user.
                _chooseStatOverlay.OnStatisticDefined += (s, statDef) =>
                {
                    // Verify the selected statistic doesn't exist in the collection (check for an alias with the same value).
                    StatisticDefinition existingItem = _statisticDefinitions.Find(itm => itm.OutputAlias == statDef.OutputAlias);
                    if (existingItem != null)
                    {
                        return;
                    }

                    // Make updates to the table (add the chosen statistic).
                    tableView.BeginUpdates();

                    // Insert a new row at the top of table display.
                    tableView.InsertRows(new[] {NSIndexPath.FromRowSection(0, 0)}, UITableViewRowAnimation.Fade);

                    // Insert the chosen statistic in the underlying collection.
                    _statisticDefinitions.Insert(0, statDef);

                    // Apply table edits.
                    tableView.EndUpdates();
                };

                // Handle when the user chooses to close the dialog.
                _chooseStatOverlay.OnCanceled += (s, e) =>
                {
                    // Remove the item input UI.
                    _chooseStatOverlay.Hide();
                    _chooseStatOverlay = null;
                };

                // Add the picker UI view (will display semi-transparent over the table view).
                tableView.Add(_chooseStatOverlay);
            }
            else if (editingStyle == UITableViewCellEditingStyle.Delete)
            {
                // Remove the selected row from the table and the underlying collection of statistic definitions.
                _statisticDefinitions.RemoveAt(indexPath.Row);
                tableView.DeleteRows(new[] {indexPath}, UITableViewRowAnimation.Fade);
            }
        }

        // Define the (confirmation) text to display when the user chooses to delete a row.
        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
        {
            return "Remove";
        }

        // Allow all rows to be edited.
        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true;
        }

        // Allow all rows to be deleted except the last row, which is a placeholder for creating new statistic definitions.
        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            // Get the index of the last row in the table view.
            nint lastRowIndex = tableView.NumberOfRowsInSection(0) - 1;

            // Set the editing style as delete for all but the final row (insert).
            return indexPath.Row == lastRowIndex ? UITableViewCellEditingStyle.Insert : UITableViewCellEditingStyle.Delete;
        }

        // Prepare the data source for editing.
        public void WillBeginTableEditing(UITableView tableView)
        {
            // See if the table already has a placeholder row for the "Add New" button.
            StatisticDefinition existingItem = _statisticDefinitions.Find(itm => itm.OnFieldName == AddNewStatFieldName);

            // Return if there is already a placeholder row.
            if (existingItem != null)
            {
                return;
            }

            // Begin updating the table.
            tableView.BeginUpdates();

            // Create an index path for the last row in the table.
            NSIndexPath lastRowIndex = NSIndexPath.FromRowSection(tableView.NumberOfRowsInSection(0), 0);

            // Add the insert placeholder row at the end of table display.
            tableView.InsertRows(new[] {lastRowIndex}, UITableViewRowAnimation.Fade);

            // Create a new StatisticDefinition and add it to the underlying data.
            _statisticDefinitions.Add(new StatisticDefinition(AddNewStatFieldName, StatisticType.Count, ""));

            // Apply the table edits.
            tableView.EndUpdates();
        }

        // This is called each time a cell needs to be created in the table.
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Create a new cell with a main and detail label style.
            UITableViewCell cell = new UITableViewCell(UITableViewCellStyle.Subtitle, null);

            // Get the corresponding StatisticDefinition for this row.
            StatisticDefinition definition = _statisticDefinitions[indexPath.Row];

            // Set the cell text with the field name.
            cell.TextLabel.Text = definition.OnFieldName;

            // If this is not the placeholder (insert) row, set the detail text with the statistic type.
            if (definition.OnFieldName != AddNewStatFieldName)
            {
                cell.DetailTextLabel.Text = definition.StatisticType.ToString();
            }

            // Return the new cell.
            return cell;
        }

        // Return the number of rows for the table (count of the statistics definition list).
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _statisticDefinitions.Count;
        }
    }

    // Class that defines a custom data source for display group fields.
    public class GroupFieldsDataSource : UITableViewSource
    {
        // Dictionary of available fields for grouping results.
        private readonly Dictionary<string, bool> _potentialGroupFields;

        // Constructor that takes a dictionary of fields.
        public GroupFieldsDataSource(Dictionary<string, bool> fields)
        {
            _potentialGroupFields = fields;
        }

        // Create a view to display the value of each item in the dictionary.
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Create a UITableViewCell with default style.
            UITableViewCell cell = new UITableViewCell(UITableViewCellStyle.Default, null);

            // Get the field name and whether it's set as a group field.
            string fieldName = _potentialGroupFields.ElementAt(indexPath.Row).Key;
            bool isForGrouping = _potentialGroupFields.ElementAt(indexPath.Row).Value;

            // Display the field name in the cell.
            cell.TextLabel.Text = fieldName;

            // Create a UISwitch for selecting the field for grouping.
            UISwitch groupFieldSwitch = new UISwitch
            {
                Frame = new CGRect(cell.Bounds.Width - 60, 7, 50, cell.Bounds.Height),
                // Set the switch control tag with the row position.
                Tag = indexPath.Row,
                // Set the initial switch value to show whether it's been selected for grouping.
                On = isForGrouping
            };

            // Handle the value changed for the switch so the dictionary value can be updated.
            groupFieldSwitch.ValueChanged += GroupBySwitched;

            // Add the UISwitch to the cell's content view.
            cell.ContentView.AddSubview(groupFieldSwitch);

            return cell;
        }

        private void GroupBySwitched(object sender, EventArgs e)
        {
            // Use the control's tag to get the row that was changed.
            nint index = (sender as UISwitch).Tag;

            // Set or clear the group field according to the UISwitch setting.
            string key = _potentialGroupFields.ElementAt((int) index).Key;
            _potentialGroupFields[key] = (sender as UISwitch).On;
        }

        // Return the number of rows to display.
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _potentialGroupFields.Count;
        }
    }

    // Class that defines a custom data source for displaying fields to order results with.
    public class OrderByFieldsDataSource : UITableViewSource
    {
        // List of order field options.
        private readonly List<OrderFieldOption> _potentialOrderByFields;

        // Constructor that takes a list of order field options to display.
        public OrderByFieldsDataSource(List<OrderFieldOption> fields)
        {
            _potentialOrderByFields = fields;
        }

        // Create a cell to display information for each order field option.
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Default table cell.
            UITableViewCell cell = new UITableViewCell(UITableViewCellStyle.Default, null);

            // Get the field name and whether it's been selected for sorting.
            string fieldName = _potentialOrderByFields.ElementAt(indexPath.Row).OrderInfo.FieldName;
            bool isForSorting = _potentialOrderByFields.ElementAt(indexPath.Row).OrderWith;

            // Show the field name in the cell.
            cell.TextLabel.Text = fieldName;

            // Create a UISwitch for selecting the field for ordering results.
            UISwitch sortFieldSwitch = new UISwitch
            {
                Frame = new CGRect(cell.Bounds.Width - 60, 7, 50, cell.Bounds.Height),
                // Set the control's tag with the row index.
                Tag = indexPath.Row,
                // Set the initial switch value to show if this field will be used for sorting.
                On = isForSorting
            };

            // Handle the value changed event to update the dictionary value for this field.
            sortFieldSwitch.ValueChanged += OrderBySwitched;

            // Add the UISwitch to the cell's content view.
            cell.ContentView.AddSubview(sortFieldSwitch);

            return cell;
        }

        private void OrderBySwitched(object sender, EventArgs e)
        {
            // Use the control's tag to get the row that was changed.
            nint index = (sender as UISwitch).Tag;

            // Get the corresponding field and update its choice as a sort field.
            OrderFieldOption orderByOption = _potentialOrderByFields.ElementAt((int) index);
            orderByOption.OrderWith = (sender as UISwitch).On;
        }

        // Return the number of rows to display.
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _potentialOrderByFields.Count;
        }
    }

    // Class that defines a custom data source for showing statistic query results.
    public class StatisticQueryResultsDataSource : UITableViewSource
    {
        // Dictionary of group names and statistic results.
        private readonly Dictionary<string, IReadOnlyDictionary<string, object>> _statisticsResults;

        // Constructor that takes a dictionary of group names and statistic results.
        public StatisticQueryResultsDataSource(Dictionary<string, IReadOnlyDictionary<string, object>> results)
        {
            _statisticsResults = results;
        }

        // Create a cell for each item in the results.
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Create a new cell with a main and detail label style.
            UITableViewCell cell = new UITableViewCell(UITableViewCellStyle.Subtitle, null);

            // Get the group name.
            KeyValuePair<string, IReadOnlyDictionary<string, object>> group = _statisticsResults.ElementAt(indexPath.Section);

            // Get the results for this group (dictionary).
            IReadOnlyDictionary<string, object> stats = group.Value;

            // Get the result (field alias and value).
            string field = stats.Keys.ElementAt(indexPath.Row);
            object value = stats.Values.ElementAt(indexPath.Row);

            // Set the main text with the statistic value.
            cell.TextLabel.Text = (value ?? "").ToString();

            // Set the sub text with the field alias name.
            cell.DetailTextLabel.Text = field;

            // Return the new cell.
            return cell;
        }

        // Return the number of sections (groups).
        public override nint NumberOfSections(UITableView tableView)
        {
            return _statisticsResults.Keys.Count;
        }

        // Return the number of rows in the specified section (group).
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _statisticsResults[_statisticsResults.Keys.ElementAt((int) section)].Count;
        }

        // Return the header text for the specified section (group).
        public override string TitleForHeader(UITableView tableView, nint section)
        {
            return _statisticsResults.Keys.ElementAt((int) section);
        }
    }

    // View containing "define statistic" controls (picker for fields/stat type, add/cancel buttons).
    public class ChooseStatisticOverlay : UIView
    {
        // Event to provide the statistic definition the user entered when the view closes.
        public event EventHandler<StatisticDefinition> OnStatisticDefined;

        // Event to report that the choice was canceled.
        public event EventHandler OnCanceled;

        // Constructor that takes a picker for defining new statistics.
        public ChooseStatisticOverlay(CGRect frame, nfloat transparency, UIColor color, UIPickerView statPicker) : base(frame)
        {
            // Store the statistics picker.
            var statisticPicker = statPicker;

            // Create a semi-transparent overlay with the specified background color.
            BackgroundColor = color;
            Alpha = transparency;

            // Set the total height and width of the control set.
            nfloat totalHeight = 400;
            nfloat totalWidth = 320;

            // Find the bottom x and y of the view.
            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Bottom - 40;

            // Find the start x and y for the control layout (aligned to the bottom of the view).
            nfloat controlX = centerX - totalWidth / 2;
            nfloat controlY = centerY - totalHeight;

            // Toolbar with "Add" and "Done" buttons.
            UIToolbar toolbar = new UIToolbar
            {
                BarStyle = UIBarStyle.Black,
                Translucent = false
            };
            toolbar.SizeToFit();

            // Add Button (add the new stat and don't dismiss the UI).
            UIBarButtonItem addButton = new UIBarButtonItem("Add", UIBarButtonItemStyle.Done, (s, e) =>
            {
                // Get the selected StatisticDefinition.
                StatDefinitionModel statPickerModel = statisticPicker.Model as StatDefinitionModel;
                StatisticDefinition newStatDefinition = statPickerModel.SelectedStatDefinition;
                if (newStatDefinition != null)
                {
                    // Fire the OnMapInfoEntered event and provide the statistic definition.
                    OnStatisticDefined?.Invoke(this, newStatDefinition);
                }
            });

            // Done Button (dismiss the UI, don't use the selected statistic).
            UIBarButtonItem doneButton = new UIBarButtonItem("Done", UIBarButtonItemStyle.Plain, (s, e) => { OnCanceled.Invoke(this, null); });

            // Add the buttons to the toolbar.
            toolbar.SetItems(new[] {addButton, doneButton}, true);

            // Define the location of the statistic picker.
            controlY = controlY + 200;
            statisticPicker.Frame = new CGRect(controlX, controlY, totalWidth, 200);

            // Set the location for the toolbar.
            controlY = controlY + 220;
            toolbar.Frame = new CGRect(controlX, controlY, totalWidth, 30);

            // Add the controls.
            AddSubviews(toolbar, statisticPicker);
        }

        // Animate increasing transparency to completely hide the view, then remove it.
        public void Hide()
        {
            // Action to make the view transparent.
            Action makeTransparentAction = () => Alpha = 0;

            // Action to remove the view.
            Action removeViewAction = RemoveFromSuperview;

            // Time to complete the animation (seconds).
            double secondsToComplete = 0.75;

            // Animate transparency to zero, then remove the view.
            Animate(secondsToComplete, makeTransparentAction, removeViewAction);
        }
    }
}