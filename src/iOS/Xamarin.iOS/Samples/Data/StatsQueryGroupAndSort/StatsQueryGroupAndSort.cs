// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
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
        // URI for the US states map service
        private Uri _usStatesServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3");

        // US states feature table
        private FeatureTable _usStatesTable;

        // List of field names from the table
        private List<string> _fieldNames;

        // Selected fields for grouping results
        private Dictionary<string, bool> _groupByFields = new Dictionary<string, bool>();

        // Collection to hold fields to order results by
        private List<OrderFieldOption> _orderByFields = new List<OrderFieldOption>();

        // Stack view UI control for arranging query controls
        private UIStackView _controlsStackView;

        // Model for defining choices in the statistics definition UIPickerView
        private StatDefinitionModel _statsPickerModel;

        // List of statistics definitions to use in the query
        private List<StatisticDefinition> _statisticDefinitions = new List<StatisticDefinition>();

        public StatsQueryGroupAndSort()
        {
            Title = "Statistical query group and sort";
        }

        public override void ViewDidLayoutSubviews()
        {
            // Get height of status bar and navigation bar
            nfloat pageOffset = NavigationController.NavigationBar.Frame.Size.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

            // Setup the visual frame for the query controls
            _controlsStackView.Frame = new CGRect(0,  pageOffset, View.Bounds.Width, View.Bounds.Height - pageOffset);

            base.ViewDidLayoutSubviews();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI
            CreateLayout();

            // Initialize the service feature table
            Initialize();
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

            // Button for launching the UI to view or define statistics definitions for the query
            UIButton showStatDefinitionsButton = new UIButton();
            showStatDefinitionsButton.SetTitle("Statistic Definitions", UIControlState.Normal);
            showStatDefinitionsButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            showStatDefinitionsButton.BackgroundColor = UIColor.White;
            showStatDefinitionsButton.Frame = new CGRect(30, 20, 220, 30);
            showStatDefinitionsButton.TouchUpInside += ShowStatDefinitions;

            // Button to choose fields with which to group results
            UIButton showGroupFieldsButton = new UIButton();
            showGroupFieldsButton.SetTitle("Group Fields", UIControlState.Normal);
            showGroupFieldsButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            showGroupFieldsButton.BackgroundColor = UIColor.White;
            showGroupFieldsButton.Frame = new CGRect(30, 60, 220, 30);
            showGroupFieldsButton.TouchUpInside += ShowGroupFields;

            // Button to choose fields with which to sort results (must be one of the 'group by' fields)
            UIButton showOrderByFieldsButton = new UIButton();
            showOrderByFieldsButton.SetTitle("Order By Fields", UIControlState.Normal);
            showOrderByFieldsButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            showOrderByFieldsButton.BackgroundColor = UIColor.White;
            showOrderByFieldsButton.Frame = new CGRect(30, 100, 220, 30);
            showOrderByFieldsButton.TouchUpInside += ShowOrderByFields;

            // Create a button to invoke the query using the query parameters defined
            UIButton getStatsButton = new UIButton();
            getStatsButton.SetTitle("Get Statistics", UIControlState.Normal);
            getStatsButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            getStatsButton.BackgroundColor = UIColor.White;
            getStatsButton.Frame = new CGRect(30, 340, 220, 30);

            // Handle the button tap to execute the statistics query
            getStatsButton.TouchUpInside += ExecuteStatisticsQuery;

            // Add controls to the stack view
            _controlsStackView.AddSubviews(showStatDefinitionsButton, showGroupFieldsButton, showOrderByFieldsButton, getStatsButton);

            // Add UI controls to the page
            View.AddSubview(_controlsStackView);
        }

        private async void Initialize()
        {
            // Create the US states feature table
            _usStatesTable = new ServiceFeatureTable(_usStatesServiceUri);

            // Load the table
            await _usStatesTable.LoadAsync();

            // Fill the fields combo and "group by" list with field names from the table
            _fieldNames = _usStatesTable.Fields.Select(field => field.Name).ToList();

            // Create a model that will provide statistic definition choices for the picker
            _statsPickerModel = new StatDefinitionModel(_fieldNames.ToArray());

            // Create a list of fields the user can select for grouping
            // Value is initially false, since no fields are selected by default
            _groupByFields = _fieldNames.ToDictionary(name => name, name => false);
        }

        private void ShowGroupFields(object sender, EventArgs e)
        {
            // Create a new table 
            UITableViewController fieldsTable = new UITableViewController(UITableViewStyle.Plain);

            // Create a data source to show fields the user can choose to group results with
            GroupFieldsDataSource groupFieldsDataSource = new GroupFieldsDataSource(_groupByFields);

            // Set the data source on the table
            fieldsTable.TableView.Source = groupFieldsDataSource;

            // Show the table view
            NavigationController.PushViewController(fieldsTable, true);
        }

        // Show fields the user can choose to sort results with (must be one of the group by fields)
        private void ShowOrderByFields(object sender, EventArgs e)
        {
            // Create a new table 
            UITableViewController sortFieldsTable = new UITableViewController(UITableViewStyle.Plain);            

            // Get the current list of group fields and create/update the sort field choices
            IEnumerable<KeyValuePair<string,bool>> sortFieldChoices = _groupByFields.Where(field => field.Value);
            foreach (KeyValuePair<string, bool> sortChoice in sortFieldChoices)
            {
                // If this group field is not in the list of available order fields, add it to the list
                OrderFieldOption existingOption = _orderByFields.Find(opt => opt.OrderInfo.FieldName == sortChoice.Key);
                if (existingOption == null)
                {
                    existingOption = new OrderFieldOption(false, new OrderBy(sortChoice.Key, SortOrder.Ascending));
                    _orderByFields.Add(existingOption);
                }
            }

            // Also make sure to remove any order by fields that were removed as 'group by' fields
            for(int i = _orderByFields.Count-1; i>=0; i--)
            {
                // If this field is not in the grouped field list, remove it from the order fields list
                OrderFieldOption opt = _orderByFields.ElementAt(i);
                KeyValuePair<string, bool> existingGroupField = sortFieldChoices.FirstOrDefault(field => field.Key == opt.OrderInfo.FieldName);
                if (existingGroupField.Key == null)
                {
                    _orderByFields.RemoveAt(i);
                }
            }

            // Create an instance of a custom data source to show the order fields
            OrderByFieldsDataSource sortFieldsDataSource = new OrderByFieldsDataSource(_orderByFields);

            // Set the data source on the table
            sortFieldsTable.TableView.Source = sortFieldsDataSource;

            // Show the table view
            NavigationController.PushViewController(sortFieldsTable, true);
        }

        private void ShowStatDefinitions(object sender, EventArgs e)
        {
            // Create a new UIPickerView and assign a model that will show fields and statistic types
            UIPickerView statisticPicker = new UIPickerView();
            statisticPicker.Model = _statsPickerModel;

            // Create a new table 
            UITableViewController statsTable = new UITableViewController(UITableViewStyle.Plain);

            // Create an instance of a custom data source to show statistic definitions in the table
            // Pass in the list of statistic definitions and the picker (for defining new ones)
            StatisticDefinitionsDataSource statDefsDataSource = new StatisticDefinitionsDataSource(_statisticDefinitions, statisticPicker);
            
            // Set the data source on the table
            statsTable.TableView.Source = statDefsDataSource;

            // Put the table in edit mode (to show add and delete buttons)
            statDefsDataSource.WillBeginTableEditing(statsTable.TableView);
            statsTable.SetEditing(true, true);

            // Show the table view
            NavigationController.PushViewController(statsTable, true);
        }

        private async void ExecuteStatisticsQuery(object sender, EventArgs e)
        {
            // Remove the placeholder "Add statistic" row (if it exists)
            StatisticDefinition placeholderRow = _statisticDefinitions.LastOrDefault();
            if (placeholderRow != null && placeholderRow.OutputAlias == "")
            {
                _statisticDefinitions.Remove(placeholderRow);
            }

            // Verify that there is at least one statistic definition
            if (_statisticDefinitions.Count() == 0)
            {
                ShowAlert("Statistical Query", "Please define at least one statistic for the query.");
                return;
            }

            // Create the statistics query parameters, pass in the list of statistic definitions
            StatisticsQueryParameters statQueryParams = new StatisticsQueryParameters(_statisticDefinitions);

            // Specify the selected group fields (if any)
            if (_groupByFields != null)
            {
                foreach (KeyValuePair<string,bool> groupField in _groupByFields.Where(field => field.Value))
                {
                    statQueryParams.GroupByFieldNames.Add(groupField.Key);
                }
            }

            // Specify the fields to order by (if any)
            if (_orderByFields != null)
            {
                foreach (OrderFieldOption orderBy in _orderByFields)
                {
                    statQueryParams.OrderByFields.Add(orderBy.OrderInfo);
                }
            }

            // Execute the statistical query with these parameters and await the results
            StatisticsQueryResult statQueryResult = await _usStatesTable.QueryStatisticsAsync(statQueryParams);

            // Get results formatted as a dictionary (group names and their associated dictionary of results)
            Dictionary<string,IReadOnlyDictionary<string,object>> resultsLookup = statQueryResult.ToDictionary(result => string.Join(", ", result.Group.Values), result => result.Statistics);

            // Create an instance of a custom data source to display the results
            StatisticQueryResultsDataSource statResultsDataSource = new StatisticQueryResultsDataSource(resultsLookup);

            // Create a new table with a grouped style for displaying rows
            UITableViewController statResultsTable = new UITableViewController(UITableViewStyle.Grouped);

            // Set the table view data source
            statResultsTable.TableView.Source = statResultsDataSource;
            
            // Show the table view
            NavigationController.PushViewController(statResultsTable, true);
        }

        private void ShowAlert(string title, string message)
        {
            // Create a new Alert Controller
            UIAlertController alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

            // Add an Action to dismiss the alert
            alert.AddAction(UIAlertAction.Create("Dismiss", UIAlertActionStyle.Cancel, null));

            // Display the alert
            PresentViewController(alert, true, null);
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

    // Class that defines a view model for showing field names and statistic types in a picker
    public class StatDefinitionModel : UIPickerViewModel
    {
        // Array of field names
        private string[] _fieldNames;

        // Array of available statistic types
        private Array _statTypes = Enum.GetValues(typeof(StatisticType));

        // Currently selected statistic definition
        private StatisticDefinition _selectedStatDefinition = null;

        // Constructor that takes an array of the available field names
        public StatDefinitionModel(string[] fieldNames)
        {
            _fieldNames = fieldNames;
        }

        // Property to expose the currently selected definition in the picker
        public StatisticDefinition SelectedStatDefinition
        {
            get { return _selectedStatDefinition; }
        }

        // Return the number of picker components (two sections: field names and statistic types)
        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 2;
        }

        // Return the number of rows in each of the two sections
        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            // first component is the fields list, second is the statistic types
            if (component == 0)
            {
                return _fieldNames.Length;
            }
            else
            {
                return _statTypes.Length;
            }
        }

        // Get the title to display in each picker component
        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            // first component is the fields list, second is the statistic types
            if (component == 0)
            {
                return _fieldNames[row];
            }
            else
            {
                return _statTypes.GetValue(row).ToString();
            }
        }

        // Handle the selection event for the picker to create a statistic definition with the values chosen
        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            // Get the field name
            string onFieldName = _fieldNames[pickerView.SelectedRowInComponent(0)];

            // Get the statistic type
            StatisticType statType = (StatisticType)_statTypes.GetValue(pickerView.SelectedRowInComponent(1));

            // Create an output field alias by concatenating the field name and statistic type
            string outAlias = onFieldName + "_" + statType.ToString();

            // Create a new statistic definition (available from the SelectedStatDefinition public property)
            _selectedStatDefinition = new StatisticDefinition(onFieldName, statType, outAlias);
        }

        // Return the desired width for each component in the picker
        public override nfloat GetComponentWidth(UIPickerView picker, nint component)
        {
            // first component is the fields list, second is the statistic types
            if (component == 0)
            {
                return 160f;
            }
            else
            {
                return 120f;
            }
        }

        // Return the desired height for rows in the picker
        public override nfloat GetRowHeight(UIPickerView picker, nint component)
        {
            return 40f;
        }
    }
    
    // Class that defines a custom data source for showing statistic definitions
    public class StatisticDefinitionsDataSource : UITableViewSource
    {
        // List of statistic definitions for the current query
        private List<StatisticDefinition> _statisticDefinitions;

        // Picker for choosing a field and statistic type
        private UIPickerView _statPicker;

        // Custom UI to show the statistics picker and associated buttons
        private ChooseStatisticOverlay _chooseStatOverlay;
        
        // Text to display for the placeholder row used to add new statistic definitions
        private const string  AddNewStatFieldName = "(Add statistic)";

        // Constructor that takes a list of statistic definitions and a picker for selecting fields and statistic types
        public StatisticDefinitionsDataSource(List<StatisticDefinition> statDefs, UIPickerView picker)
        {
            // Store the list of statistic definitions and the statistic picker
            _statisticDefinitions = statDefs;
            _statPicker = picker;
        }

        // Handle supported edits to the data source (inserts and deletes)
        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            // Respond to the user's edit request: Insert a new statistic definition, or delete an existing one
            if(editingStyle == UITableViewCellEditingStyle.Insert)
            {
                // Get the bounds of the table
                CGRect ovBounds = tableView.Bounds;
                ovBounds.Height = ovBounds.Height + 60;

                // Create an overlay UI that lets the user choose a field and statistic type to add
                _chooseStatOverlay = new ChooseStatisticOverlay(ovBounds, 0.70f, UIColor.White, _statPicker);

                // Handle the OnStatisticDefined event to get the info entered by the user
                _chooseStatOverlay.OnStatisticDefined += (s,statDef) => 
                {
                    // Verify the selected statistic doesn't exist in the collection (check for an alias with the same value)
                    StatisticDefinition existingItem = _statisticDefinitions.Find(itm => itm.OutputAlias == statDef.OutputAlias);
                    if (existingItem != null) { return; }

                    // Make updates to the table (add the chosen statistic)
                    tableView.BeginUpdates();
                                        
                    // Insert a new row at the top of table display
                    tableView.InsertRows(new[] { NSIndexPath.FromRowSection(0, 0)}, UITableViewRowAnimation.Fade);
                    
                    // Insert the chosen statistic in the underlying collection
                    _statisticDefinitions.Insert(0, statDef);

                    // Apply table edits
                    tableView.EndUpdates();
                };

                // Handle when the user chooses to close the dialog 
                _chooseStatOverlay.OnCanceled += (s,e)=> 
                { 
                    // Remove the item input UI
                    _chooseStatOverlay.Hide();
                    _chooseStatOverlay = null;
                };

                // Add the picker UI view (will display semi-transparent over the table view)
                tableView.Add(_chooseStatOverlay);
            }
            else if(editingStyle == UITableViewCellEditingStyle.Delete)
            {
                // Remove the selected row from the table and the underlying collection of statistic definitions
                _statisticDefinitions.RemoveAt(indexPath.Row);
                tableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Fade);
            }
        }

        // Define the (confirmation) text to display when the user chooses to delete a row 
        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
        {
            return "Remove";
        }

        // Allow all rows to be edited
        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true;
        }

        // Allow all rows to be deleted except the last row, which is a placeholder for creating new statistic definitions
        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            // Get the index of the last row in the table view
            nint lastRowIndex = tableView.NumberOfRowsInSection(0) - 1;

            // Set the editing style as delete for all but the final row (insert)
            if (indexPath.Row == lastRowIndex)
            {
                return UITableViewCellEditingStyle.Insert;
            }
            else
            {
                return UITableViewCellEditingStyle.Delete;
            }
        }

        // Prepare the data source for editing
        public void WillBeginTableEditing(UITableView tableView)
        {
            // See if the table already has a placeholder row for the "Add New" button
            StatisticDefinition existingItem = _statisticDefinitions.Find(itm => itm.OnFieldName == AddNewStatFieldName);

            // Return if there is already a placeholder row
            if(existingItem != null) { return; }

            // Begin updating the table
            tableView.BeginUpdates();

            // Create an index path for the last row in the table
            NSIndexPath lastRowIndex = NSIndexPath.FromRowSection(tableView.NumberOfRowsInSection(0), 0);

            // Add the insert placeholder row at the end of table display
            tableView.InsertRows(new[] { lastRowIndex }, UITableViewRowAnimation.Fade);

            // Create a new StatisticDefinition and add it to the underlying data
            _statisticDefinitions.Add(new StatisticDefinition(AddNewStatFieldName, StatisticType.Count, ""));

            // Apply the table edits
            tableView.EndUpdates();
        }

        // This is called each time a cell needs to be created in the table
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Create a new cell with a main and detail label style
            UITableViewCell cell = new UITableViewCell(UITableViewCellStyle.Subtitle, null);

            // Get the corresponding StatisticDefinition for this row
            StatisticDefinition definition = _statisticDefinitions[indexPath.Row];

            // Set the cell text with the field name
            cell.TextLabel.Text = definition.OnFieldName;

            // If this is not the placeholder (insert) row, set the detail text with the statistic type
            if (definition.OnFieldName != AddNewStatFieldName)
            {
                cell.DetailTextLabel.Text = definition.StatisticType.ToString();
            }

            // Return the new cell
            return cell;
        }

        // Return the number of rows for the table (count of the statistics definition list)
        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return _statisticDefinitions.Count;
        }
    }

    // Class that defines a custom data source for display group fields
    public class GroupFieldsDataSource : UITableViewSource
    {
        // Dictionary of available fields for grouping results 
        private Dictionary<string, bool> _potentialGroupFields;        

        // Constructor that takes a dictionary of fields
        public GroupFieldsDataSource(Dictionary<string, bool> fields)
        {
            _potentialGroupFields = fields;
        }

        // Create a view to display the value of each item in the dictionary
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Create a UITableViewCell with default style
            UITableViewCell cell = new UITableViewCell(UITableViewCellStyle.Default, null);

            // Get the field name and whether it's set as a group field 
            string fieldName = _potentialGroupFields.ElementAt(indexPath.Row).Key;
            bool isForGrouping = _potentialGroupFields.ElementAt(indexPath.Row).Value;

            // Display the field name in the cell
            cell.TextLabel.Text = fieldName;

            // Create a UISwitch for selecting the field for grouping
            UISwitch groupFieldSwitch = new UISwitch()
            {
                Frame = new CGRect(cell.Bounds.Width - 60, 7, 50, cell.Bounds.Height)
            };

            // Set the switch control tag with the row position
            groupFieldSwitch.Tag = indexPath.Row;

            // Set the initial switch value to show whether it's been selected for grouping
            groupFieldSwitch.On = isForGrouping;

            // Handle the value changed for the switch so the dictionary value can be updated
            groupFieldSwitch.ValueChanged += GroupBySwitched;

            // Add the UISwitch to the cell's content view
            cell.ContentView.AddSubview(groupFieldSwitch);

            return cell;
        }

        private void GroupBySwitched(object sender, EventArgs e)
        {
            // Use the control's tag to get the row that was changed
            nint index = (sender as UISwitch).Tag;

            // Set or clear the group field according to the UISwitch setting
            string key = _potentialGroupFields.ElementAt((int)index).Key;
            _potentialGroupFields[key] = (sender as UISwitch).On;
        }

        // Return the number of rows to display
        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return _potentialGroupFields.Count;
        }
    }

    // Class that defines a custom data source for displaying fields to order results with
    public class OrderByFieldsDataSource : UITableViewSource
    {
        // List of order field options
        private List<OrderFieldOption> _potentialOrderByFields;

        // Constructor that takes a list of order field options to display
        public OrderByFieldsDataSource(List<OrderFieldOption> fields)
        {
            _potentialOrderByFields = fields;
        }

        // Create a cell to display information for each order field option
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Default table cell
            UITableViewCell cell = new UITableViewCell(UITableViewCellStyle.Default, null);

            // Get the field name and whether it's been selected for sorting
            string fieldName = _potentialOrderByFields.ElementAt(indexPath.Row).OrderInfo.FieldName;
            bool isForSorting = _potentialOrderByFields.ElementAt(indexPath.Row).OrderWith;

            // Show the field name in the cell
            cell.TextLabel.Text = fieldName;

            // Create a UISwitch for selecting the field for ordering results
            UISwitch sortFieldSwitch = new UISwitch()
            {
                Frame = new CGRect(cell.Bounds.Width - 60, 7, 50, cell.Bounds.Height)
            };

            // Set the control's tag with the row index
            sortFieldSwitch.Tag = indexPath.Row;

            // Set the initial switch value to show if this field will be used for sorting
            sortFieldSwitch.On = isForSorting;

            // Handle the value changed event to update the dictionary value for this field
            sortFieldSwitch.ValueChanged += OrderBySwitched;

            // Add the UISwitch to the cell's content view
            cell.ContentView.AddSubview(sortFieldSwitch);

            return cell;
        }

        private void OrderBySwitched(object sender, EventArgs e)
        {
            // Use the control's tag to get the row that was changed
            nint index = (sender as UISwitch).Tag;

            // Get the corresponding field and update it's choice as a sort field
            OrderFieldOption orderByOption = _potentialOrderByFields.ElementAt((int)index);
            orderByOption.OrderWith = (sender as UISwitch).On;
        }

        // Return the number of rows to display
        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return _potentialOrderByFields.Count;
        }
    }

    // Class that defines a custom data source for showing statistic query results
    public class StatisticQueryResultsDataSource : UITableViewSource
    {
        // Dictionary of group names and statistic results
        private Dictionary<string, IReadOnlyDictionary<string, object>> _statisticsResults;
        
        // Constructor that takes a dictionary of group names and statistic results
        public StatisticQueryResultsDataSource(Dictionary<string, IReadOnlyDictionary<string, object>> results)
        {
            _statisticsResults = results;
        }

        // Create a cell for each item in the results
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Create a new cell with a main and detail label style
            UITableViewCell cell = new UITableViewCell(UITableViewCellStyle.Subtitle, null);

            // Get the group name
            KeyValuePair<string, IReadOnlyDictionary<string,object>> group = _statisticsResults.ElementAt(indexPath.Section);

            // Get the results for this group (dictionary)
            IReadOnlyDictionary<string,object> stats = group.Value;

            // Get the result (field alias and value)
            string field = stats.Keys.ElementAt(indexPath.Row);
            object value = stats.Values.ElementAt(indexPath.Row);

            // Set the main text with the statistic value
            cell.TextLabel.Text = value.ToString();

            // Set the sub text with the field alias name
            cell.DetailTextLabel.Text = field;

            // Return the new cell
            return cell;
        }

        // Return the number of sections (groups)
        public override nint NumberOfSections(UITableView tableView)
        {
            return _statisticsResults.Keys.Count;
        }

        // Return the number of rows in the specified section (group)
        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return _statisticsResults[_statisticsResults.Keys.ElementAt((int)section)].Count;
        }

        // Return the header text for the specified section (group)
        public override string TitleForHeader(UITableView tableView, nint section)
        {
            return _statisticsResults.Keys.ElementAt((int)section);
        }
    }

    // View containing "define statistic" controls (picker for fields/stat type, add/cancel buttons)
    public class ChooseStatisticOverlay : UIView
    {
        // Event to provide the statistic definition the user entered when the view closes
        public event EventHandler<StatisticDefinition> OnStatisticDefined;

        // Event to report that the choice was canceled
        public event EventHandler OnCanceled;

        // Store the input controls so the values can be read
        private UIPickerView _statisticPicker;
        
        // Constructor that takes a picker for defining new statistics
        public ChooseStatisticOverlay(CGRect frame, nfloat transparency, UIColor color, UIPickerView statPicker) : base(frame)
        {
            // Store the statistics picker
            _statisticPicker = statPicker;
            
            // Create a semi-transparent overlay with the specified background color
            BackgroundColor = color;
            Alpha = transparency;

            // Set the total height and width of the control set
            nfloat totalHeight = 400;
            nfloat totalWidth = 320;

            // Find the bottom x and y of the view
            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Bottom - 40;

            // Find the start x and y for the control layout (aligned to the bottom of the view)
            nfloat controlX = centerX - (totalWidth / 2);
            nfloat controlY = centerY - totalHeight;

            // Toolbar with "Add" and "Done" buttons
            UIToolbar toolbar = new UIToolbar();
            toolbar.BarStyle = UIBarStyle.Black;
            toolbar.Translucent = false;
            toolbar.SizeToFit();

            // Add Button (add the new stat and don't dismiss the UI)
            UIBarButtonItem addButton = new UIBarButtonItem("Add", UIBarButtonItemStyle.Done, (s, e) =>
            {
                // Get the selected StatisticDefinition
                StatDefinitionModel statPickerModel = _statisticPicker.Model as StatDefinitionModel;
                StatisticDefinition newStatDefinition = statPickerModel.SelectedStatDefinition;
                if (newStatDefinition != null)
                {
                    // Fire the OnMapInfoEntered event and provide the statistic definition
                    if (OnStatisticDefined != null)
                    {
                        // Raise the event
                        OnStatisticDefined(this, newStatDefinition);
                    }
                }
            });

            // Done Button (dismiss the UI, don't use the selected statistic)
            UIBarButtonItem doneButton = new UIBarButtonItem("Done", UIBarButtonItemStyle.Plain, (s, e) =>
            {
                OnCanceled.Invoke(this, null);
            });

            // Add the buttons to the toolbar
            toolbar.SetItems(new[] { addButton, doneButton }, true);

            // Define the location of the statistic picker
            controlY = controlY + 200;
            _statisticPicker.Frame = new CGRect(controlX, controlY, totalWidth, 200);

            // Set the location for the toolbar
            controlY = controlY + 220;
            toolbar.Frame = new CGRect(controlX, controlY, totalWidth, 30);

            // Add the controls
            AddSubviews(toolbar, _statisticPicker);
        }

        // Animate increasing transparency to completely hide the view, then remove it
        public void Hide()
        {
            // Action to make the view transparent
            Action makeTransparentAction = () => Alpha = 0;

            // Action to remove the view
            Action removeViewAction = () => RemoveFromSuperview();

            // Time to complete the animation (seconds)
            double secondsToComplete = 0.75;

            // Animate transparency to zero, then remove the view
            Animate(secondsToComplete, makeTransparentAction, removeViewAction);
        }
    }
}