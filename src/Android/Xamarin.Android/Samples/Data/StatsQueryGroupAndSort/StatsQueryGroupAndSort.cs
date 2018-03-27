// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcGISRuntime.Samples.StatsQueryGroupAndSort
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("GroupedResultsList_DataItem.axml", "GroupedResultsList_GroupItem.axml")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Statistical query group and sort results",
        "Data",
        "This sample demonstrates how to query a feature table to get statistics for a specified field and to group and sort the results.",
        "")]
    public class StatsQueryGroupAndSort : Activity
    {
        // URI for the US states map service
        private Uri _usStatesServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3");

        // US states feature table
        private FeatureTable _usStatesTable;

        // List of field names from the table
        private List<string> _fieldNames = new List<string>();

        // Selected fields for grouping results
        private Dictionary<string, bool> _groupByFields = new Dictionary<string, bool>();

        // Collection to hold fields to order results by
        private List<OrderFieldOption> _orderByFields = new List<OrderFieldOption>();

        // List of statistics definitions to use in the query
        private List<StatisticDefinition> _statisticDefinitions = new List<StatisticDefinition>();

        // Linear layout UI control for arranging query controls
        private LinearLayout _controlsLayout;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Statistical query group and sort";

            // Create the UI
            CreateLayout();

            // Initialize the service feature table
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            _controlsLayout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Button for launching the UI to view or define statistics definitions for the query
            Button showStatDefinitionsButton = new Button(this);
            showStatDefinitionsButton.Text = "Statistic Definitions";
            showStatDefinitionsButton.Click += ShowStatDefinitions;

            // Button to choose fields with which to group results
            Button showGroupFieldsButton = new Button(this);
            showGroupFieldsButton.Text = "Group Fields";
            showGroupFieldsButton.Click += ShowGroupFields;

            // Button to choose fields with which to sort results (must be one of the 'group by' fields)
            Button showOrderByFieldsButton = new Button(this);
            showOrderByFieldsButton.Text = "Order By Fields";
            showOrderByFieldsButton.Click += ShowOrderByFields;

            // Create a Button to execute the statistical query
            Button getStatsButton = new Button(this);
            getStatsButton.Text = "Execute Query";
            getStatsButton.Click += ExecuteStatisticsQuery;

            // Define additional space (margin) between the execute button and the others
            Space space = new Space(this);
            space.SetMinimumHeight(200);

            // Add the query controls to the layout
            _controlsLayout.AddView(showStatDefinitionsButton);
            _controlsLayout.AddView(showGroupFieldsButton);
            _controlsLayout.AddView(showOrderByFieldsButton);
            _controlsLayout.AddView(space);
            _controlsLayout.AddView(getStatsButton);

            // Show the layout in the app
            SetContentView(_controlsLayout);
        }

        private async void Initialize()
        {
            // Create the US states feature table
            _usStatesTable = new ServiceFeatureTable(_usStatesServiceUri);

            // Load the table
            await _usStatesTable.LoadAsync();

            // Get a list of field names from the table
            _fieldNames = _usStatesTable.Fields.Select(field => field.Name).ToList();

            // Create a dictionary of fields the user can select for grouping
            // The value for each is set to false initially, as nothing is selected by default
            _groupByFields = _fieldNames.ToDictionary(name => name, name => false);

            // Create a list of field options for ordering results (initially empty)
            _orderByFields = new List<OrderFieldOption>();
        }

        private void ShowStatDefinitions(object sender, EventArgs e)
        {
            // Create a dialog for choosing statistics (field names and statistic types)
            StatDefinitionsDialog statsDefDialog = new StatDefinitionsDialog(_fieldNames, _statisticDefinitions);

            // Begin a transaction to show a UI fragment (stats definitions dialog)
            FragmentTransaction trans = FragmentManager.BeginTransaction();
            statsDefDialog.Show(trans, "stats_defs");
        }

        private void ShowGroupFields(object sender, EventArgs e)
        {
            // Create a dialog for choosing fields to group results with
            GroupFieldsDialog groupFieldsDialog = new GroupFieldsDialog(_groupByFields);

            // Handle the dialog closing event to read the selected fields
            groupFieldsDialog.GroupFieldDialogClosed += (s, args) =>
            {
                // Update the dictionary of group fields from the dialog
                _groupByFields = args;

                // Get the current list of group fields and create/update the sort field choices
                // (only fields selected for grouping can be used to order results)
                IEnumerable<KeyValuePair<string, bool>> currentGroupFields = _groupByFields.Where(field => field.Value == true);

                // Loop through the group fields
                foreach (KeyValuePair<string, bool> groupField in currentGroupFields)
                {
                    // Check if this field is missing from the current sort field options
                    OrderFieldOption existingOption = _orderByFields.Find((opt) => opt.OrderInfo.FieldName == groupField.Key);
                    if (existingOption == null)
                    {
                        // If the field is missing, create a new OrderFieldOption and add it to the list
                        existingOption = new OrderFieldOption(false, new OrderBy(groupField.Key, SortOrder.Ascending));
                        _orderByFields.Add(existingOption);
                    }
                }

                // Also make sure to remove any 'order by' fields that were removed from the 'group by' list
                for (int i = _orderByFields.Count - 1; i >= 0; i--)
                {
                    // If the order by field is not also one of the group fields, remove it from the list
                    OrderFieldOption opt = _orderByFields.ElementAt(i);
                    KeyValuePair<string, bool> existingGroupField = currentGroupFields.FirstOrDefault(field => field.Key == opt.OrderInfo.FieldName);
                    if (existingGroupField.Key == null)
                    {
                        _orderByFields.RemoveAt(i);
                    }
                }
            };

            // Begin a transaction to show a UI fragment (group fields dialog)
            FragmentTransaction trans = FragmentManager.BeginTransaction();
            groupFieldsDialog.Show(trans, "group_flds");
        }

        private void ShowOrderByFields(object sender, EventArgs e)
        {
            // If there are no available order fields, don't show the (empty) list
            if (_orderByFields.Count == 0)
            {
                // Warn the user to choose group fields first
                ShowMessage("Results can only be ordered by group fields.", "No Group Fields");
                return;
            }

            // Create a new dialog for choosing fields to order with
            OrderByFieldsDialog orderFieldsDialog = new OrderByFieldsDialog(_orderByFields);

            // Handle the dialog closing event to capture the current order field choices
            orderFieldsDialog.OrderFieldDialogClosed += (s, args) =>
            {
                _orderByFields = args;
            };

            // Begin a transaction to show a UI fragment (order by fields dialog)
            FragmentTransaction trans = FragmentManager.BeginTransaction();
            orderFieldsDialog.Show(trans, "order_flds");
        }

        private async void ExecuteStatisticsQuery(object sender, EventArgs e)
        {
            // Verify that there is at least one statistic definition
            if (_statisticDefinitions.Count() == 0)
            {
                // Warn the user to define a statistic to query
                ShowMessage("Please define at least one statistic for the query.", "Statistical Query");
                return;
            }

            // Create the statistics query parameters, pass in the list of statistic definitions
            StatisticsQueryParameters statQueryParams = new StatisticsQueryParameters(_statisticDefinitions);

            // Specify the selected group fields (if any)
            if (_groupByFields != null)
            {
                // Find fields in the dictionary with a 'true' value and add them to the group by field names
                foreach (KeyValuePair<string, bool> groupField in _groupByFields.Where(field => field.Value))
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
            Dictionary<string, IReadOnlyDictionary<string, object>> resultsLookup = statQueryResult.ToDictionary(r => string.Join(", ", r.Group.Values), r => r.Statistics);

            // Create an instance of a custom list adapter that has logic to show results as expandable groups
            ExpandableResultsListAdapter expandableListAdapter = new ExpandableResultsListAdapter(this, resultsLookup);

            // Create an expandable list view and assign the expandable adapter
            ExpandableListView expandableResultsListView = new ExpandableListView(this);
            expandableResultsListView.SetAdapter(expandableListAdapter);

            // Show the expandable list view in a dialog
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);
            dialogBuilder.SetView(expandableResultsListView);
            dialogBuilder.Show();
        }

        private void ShowMessage(string message, string title)
        {
            AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this);
            alertBuilder.SetTitle(title);
            alertBuilder.SetMessage(message);
            alertBuilder.Show();
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

    // A class that defines a custom dialog for defining statistics to use in the query
    public class StatDefinitionsDialog : DialogFragment
    {
        // List of field names from the table
        private List<string> _fieldNames;

        // List of statistic definitions for the query
        private List<StatisticDefinition> _statisticDefinitions;

        // Spinner (drop down) to display fields from the table 
        private Spinner _fieldSpinner;

        // Spinner to display available statistic types (average, sum, maximum, etc.)
        private Spinner _statSpinner;

        // ListView to show chosen statistic definitions (field name and statistic type)
        private ListView _statDefListView;

        public StatDefinitionsDialog(List<string> fieldNames, List<StatisticDefinition> statisticDefs)
        {
            // Store field names for the table being queried
            _fieldNames = fieldNames;

            // Store a list of the current statistic definitions
            _statisticDefinitions = statisticDefs;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Dialog UI to display
            LinearLayout dialogView = null;

            // Get the context for creating the dialog controls
            Android.Content.Context ctx = Activity.ApplicationContext;

            // Set a dialog title
            Dialog.SetTitle("Statistics Definitions");

            // Call OnCreateView on the base
            base.OnCreateView(inflater, container, savedInstanceState);

            // The container for the dialog is a vertical linear layout
            dialogView = new LinearLayout(ctx);
            dialogView.Orientation = Orientation.Vertical;

            // Spinner for choosing a field to get statistics for
            _fieldSpinner = new Spinner(ctx);

            // Create an array adapter to display the fields
            ArrayAdapter fieldsAdapter = new ArrayAdapter(ctx, Android.Resource.Layout.SimpleSpinnerItem);
            foreach (string field in _fieldNames)
            {
                fieldsAdapter.Add(field);
            }

            // Set the drop down style for the array adapter, then assign it to the field spinner control
            fieldsAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _fieldSpinner.Adapter = fieldsAdapter;

            // Create a horizontal layout to display the field spinner (with a label)
            LinearLayout fieldView = new LinearLayout(ctx);
            fieldView.Orientation = Orientation.Horizontal;

            // Create a label for the spinner
            TextView fieldLabel = new TextView(ctx);
            fieldLabel.Text = "Field:";
            fieldLabel.LabelFor = _fieldSpinner.Id;

            // Add field controls to the horizontal layout
            fieldView.AddView(fieldLabel);
            fieldView.AddView(_fieldSpinner);
            fieldView.SetPadding(140, 0, 0, 0);
            dialogView.AddView(fieldView);

            // Spinner for selecting the statistic type
            _statSpinner = new Spinner(ctx);

            // Create an array adapter to display the statistic types
            ArrayAdapter statTypeAdapter = new ArrayAdapter(ctx, Android.Resource.Layout.SimpleSpinnerItem);

            // Read the statistic types from the StatisticType enum
            Array statTypes = Enum.GetValues(typeof(StatisticType));
            foreach (object stat in statTypes)
            {
                // Add each statistic type to the adapter
                statTypeAdapter.Add(stat.ToString());
            }

            // Set the drop down style for the array adapter, then assign it to the statistic type spinner control
            statTypeAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _statSpinner.Adapter = statTypeAdapter;

            // Create a horizontal layout to display the statistic type spinner (with a label)
            LinearLayout statTypeView = new LinearLayout(ctx);
            statTypeView.Orientation = Orientation.Horizontal;

            // Create the label for the statistic type list
            TextView typeLabel = new TextView(ctx);
            typeLabel.Text = "Type:";
            typeLabel.LabelFor = _statSpinner.Id;

            // Add statistic type controls to the horizontal layout
            statTypeView.AddView(typeLabel);
            statTypeView.AddView(_statSpinner);
            statTypeView.SetPadding(140, 0, 0, 0);

            // Add the statistic type layout to the dialog
            dialogView.AddView(statTypeView);

            // Create a button to add a new statistic definition (selected field and statistic type)
            Button addStatDefButton = new Button(ctx);
            addStatDefButton.Text = "Add";
            addStatDefButton.Click += AddStatisticDefinition;

            // Create a button to remove the selected statistic definition
            Button removeStatDefButton = new Button(ctx);
            removeStatDefButton.Text = "Remove";
            removeStatDefButton.Click += RemoveStatisticDefinition;

            // Create a horizontal layout to contain the add and remove buttons
            LinearLayout buttonView = new LinearLayout(ctx);
            buttonView.Orientation = Orientation.Horizontal;
            buttonView.AddView(addStatDefButton);
            buttonView.AddView(removeStatDefButton);

            // Add the button layout to the dialog
            dialogView.AddView(buttonView);

            // Create a list view and an instance of a custom list adapter to show the statistic definitions
            StatDefinitionListAdapter listAdapter = new StatDefinitionListAdapter(Activity, _statisticDefinitions);
            _statDefListView = new ListView(ctx);
            _statDefListView.Adapter = listAdapter;

            // Only allow one choice in the statistic definitions list ('remove' button will work on the selected row)
            _statDefListView.ChoiceMode = ChoiceMode.Single;

            // Add the statistic definitions list to the dialog
            dialogView.AddView(_statDefListView);

            // Return the new view for display
            return dialogView;
        }

        // Handler for the RemoveStatisticDefinitionButton click event
        private void RemoveStatisticDefinition(object sender, EventArgs e)
        {
            // Check for a selected row in the list view
            int selectedPosition = _statDefListView.CheckedItemPosition;
            if (selectedPosition >= 0)
            {
                // Call a function in the custom list adapter that will remove the statistic definition at this position (and update the data in the list view)
                (_statDefListView.Adapter as StatDefinitionListAdapter).RemoveStatisticDefinitionAt(selectedPosition);
            }
        }

        // Handler for the AddStatisticDefinitionButton click event
        private void AddStatisticDefinition(object sender, EventArgs e)
        {
            // Get the selected field name in the dialog
            string fieldName = _fieldSpinner.SelectedItem.ToString();

            // Get the selected statistic type name in the dialog and get the corresponding enum value
            string statTypeName = _statSpinner.SelectedItem.ToString();
            StatisticType statType = (StatisticType)Enum.Parse(typeof(StatisticType), statTypeName);

            // Build a field alias for the statistic results that use the field name and statistic type
            string alias = fieldName + "_" + statTypeName;

            // Create a new StatisticDefinition with the field name, statistic type, and output field alias
            StatisticDefinition statisticDefinition = new StatisticDefinition(fieldName, statType, alias);

            // Call a function in the custom list adapter that will add the new statistic definition (and update the data in the list view)
            (_statDefListView.Adapter as StatDefinitionListAdapter).AddStatisticDefinition(statisticDefinition);
        }
    }

    // A class that defines a custom list adapter for displaying statistic definitions
    public class StatDefinitionListAdapter : BaseAdapter<StatisticDefinition>
    {
        // Store the current activity (passed into the constructor)
        private Activity _ctx;

        // List of statistic definitions to display
        private List<StatisticDefinition> _statisticDefinitions;

        // Constructor that takes the current activity and list of statistic definitions
        public StatDefinitionListAdapter(Activity context, List<StatisticDefinition> statDefs) : base()
        {
            _ctx = context;
            _statisticDefinitions = statDefs;
        }

        // Return the statistic definition at the specified position in the list
        public override StatisticDefinition this[int position]
        {
            get
            {
                return _statisticDefinitions[position];
            }
        }

        // Add a new statistic definition to the internal list
        public void AddStatisticDefinition(StatisticDefinition statDef)
        {
            // See if this definition already exists in the list (output alias name is unique)
            StatisticDefinition existingDef = _statisticDefinitions.Find((d) => d.OutputAlias == statDef.OutputAlias);

            // If the definition is not found in the list, add it
            if (existingDef == null)
            {
                _statisticDefinitions.Add(statDef);

                // Raise a notification that the data have changed
                NotifyDataSetChanged();
            }
        }

        // Remove the statistic definition at the specified position from the internal list
        public void RemoveStatisticDefinitionAt(int position)
        {
            // Verify that the position is within the correct range
            if (position >= 0 && position < _statisticDefinitions.Count)
            {
                // Remove the definition from the list
                _statisticDefinitions.RemoveAt(position);

                // Raise a notification that the data have changed
                NotifyDataSetChanged();
            }
        }

        // Return the count of statistic definitions in the list
        public override int Count
        {
            get
            {
                return _statisticDefinitions.Count;
            }
        }

        // Return an item ID (just use the position in the list)
        public override long GetItemId(int position)
        {
            return position;
        }

        // Return a view to display each item (statistic definition)
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            // Use a list item style with two text areas and the ability to be activated (selected)
            View cellView = _ctx.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItemActivated2, null);

            // Find the text view for the main text and use it to display the field name
            cellView.FindViewById<TextView>(Android.Resource.Id.Text1).Text = _statisticDefinitions[position].OnFieldName;

            // Find the text view for the details text and use it to display the statistic type
            cellView.FindViewById<TextView>(Android.Resource.Id.Text2).Text = _statisticDefinitions[position].StatisticType.ToString();

            // Return the view
            return cellView;
        }
    }

    // A class that defines a custom dialog for choosing fields to group results on
    public class GroupFieldsDialog : DialogFragment
    {
        // Dictionary of field names from the table and whether or not to use them to group results
        private Dictionary<string, bool> _potentialGroupByFields;

        // ListView to display the available group fields
        private ListView _groupFieldsListView;

        // Constructor that takes a dictionary of available group fields
        public GroupFieldsDialog(Dictionary<string, bool> groupByFields)
        {
            _potentialGroupByFields = groupByFields;
        }

        // Event that fires when the dialog closes (passes back the updated dictionary of group fields)
        public event EventHandler<Dictionary<string, bool>> GroupFieldDialogClosed;

        // Handle the dialog dismiss event to raise a custom event that passes back the updated fields dictionary
        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnDismiss(dialog);

            // If the event has listeners
            if (GroupFieldDialogClosed != null)
            {
                // Get an array of all checked row positions in the list
                SparseBooleanArray checkedItemsArray = _groupFieldsListView.CheckedItemPositions;

                // Loop through all fields in the dictionary
                for (int i = 0; i < _potentialGroupByFields.Count; i++)
                {
                    // Set the corresponding value for the field to false (will not be used for grouping results)
                    string key = _potentialGroupByFields.Keys.ElementAt(i);
                    _potentialGroupByFields[key] = false;

                    // If the corresponding row in the list view is checked, set the field's value in the dictionary to true
                    if (checkedItemsArray.KeyAt(i) == i && checkedItemsArray.ValueAt(i))
                    {
                        _potentialGroupByFields[key] = true;
                    }
                }

                // Raise the GroupFieldDialogClosed event to pass back the updated dictionary
                GroupFieldDialogClosed(this, _potentialGroupByFields);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Dialog UI to display
            LinearLayout dialogView = null;

            // Get the context for creating the dialog controls
            Android.Content.Context ctx = this.Activity.ApplicationContext;

            // Set a dialog title
            this.Dialog.SetTitle("Group Results By");

            // Call OnCreateView on the base
            base.OnCreateView(inflater, container, savedInstanceState);

            // The container for the dialog is a vertical linear layout
            dialogView = new LinearLayout(ctx);
            dialogView.Orientation = Orientation.Vertical;

            // Create an instance of a custom list adapter to show the available group fields
            GroupFieldListAdapter listAdapter = new GroupFieldListAdapter(this.Activity, _potentialGroupByFields);

            // Create a new list view that uses the adapter and allows for multiple row selection
            _groupFieldsListView = new ListView(ctx);
            _groupFieldsListView.Adapter = listAdapter;
            _groupFieldsListView.ChoiceMode = ChoiceMode.Multiple;

            // Loop through all the available fields
            for (int i = 0; i < _potentialGroupByFields.Count; i++)
            {
                // See if this field have been selected for grouping results or not
                bool chosenForGroup = _potentialGroupByFields.ElementAt(i).Value;

                // Set the checked state in the list view to show the chosen fields
                _groupFieldsListView.SetItemChecked(i, chosenForGroup);
            }

            // Add the list view to the dialog UI
            dialogView.AddView(_groupFieldsListView);

            // Return the new view for display
            return dialogView;
        }
    }

    // A class that defines a custom list adapter to show fields for grouping results
    public class GroupFieldListAdapter : BaseAdapter<KeyValuePair<string, bool>>
    {
        // Store the current activity
        private Activity _ctx;

        // Dictionary of field names and a value to indicate whether they are used for grouping
        private Dictionary<string, bool> _groupFields;

        // Constructor that takes the current activity and a dictionary of fields
        public GroupFieldListAdapter(Activity context, Dictionary<string, bool> groupFields) : base()
        {
            _ctx = context;
            _groupFields = groupFields;
        }

        // Get the group field option at the specified position
        public override KeyValuePair<string, bool> this[int position]
        {
            get
            {
                return _groupFields.ElementAt(position);
            }
        }

        // Return the count of fields in the dictionary
        public override int Count
        {
            get
            {
                return _groupFields.Count;
            }
        }

        // Get an ID for the item at the specified position
        public override long GetItemId(int position)
        {
            return position;
        }

        // Create a view to display an item in the dictionary (key-value pair)
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            // Re-use an existing view, if one is supplied (otherwise create a new one)
            View cellView = convertView;
            if (cellView == null)
            {
                // Create a list item that shows one text view and a check
                cellView = _ctx.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItemChecked, null);
            }

            // Set the text with the field name (checked value is set in the dialog UI)
            cellView.FindViewById<TextView>(Android.Resource.Id.Text1).Text = _groupFields.ElementAt(position).Key;

            return cellView;
        }
    }

    // Class that defines a dialog for choosing fields for ordering (sorting) results
    public class OrderByFieldsDialog : DialogFragment
    {
        // List of fields and whether or not to use them to order (sort) results
        private List<OrderFieldOption> _potentialOrderByFields;

        // List view to display available fields for ordering (only those fields chosen for grouping)
        private ListView _orderFieldsListView;

        // Constructor that takes a list of order fields
        public OrderByFieldsDialog(List<OrderFieldOption> orderByFields)
        {
            _potentialOrderByFields = orderByFields;
        }

        // Event that returns the updated list of order options when the dialog closes
        public event EventHandler<List<OrderFieldOption>> OrderFieldDialogClosed;

        // Handle the dismiss event on the dialog to raise a custom event that passes the updated order fields back
        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnDismiss(dialog);

            // Verify the event has listeners
            if (OrderFieldDialogClosed != null)
            {
                // Get an array of checked list item positions (indices)
                SparseBooleanArray checkedItemsArray = _orderFieldsListView.CheckedItemPositions;

                // Loop through all the available order fields
                for (int i = 0; i < _potentialOrderByFields.Count; i++)
                {
                    // Initially set each order option to false
                    OrderFieldOption orderOption = _potentialOrderByFields[i];
                    orderOption.OrderWith = false;

                    // If the item was checked in the list view, set the order option to true
                    if (checkedItemsArray.KeyAt(i) == i && checkedItemsArray.ValueAt(i))
                    {
                        orderOption.OrderWith = true;
                    }
                }

                // Raise the event and pass back the updated list of order field options
                OrderFieldDialogClosed(this, _potentialOrderByFields);
            }
        }

        // Create the dialog UI
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Dialog UI to display
            LinearLayout dialogView = null;

            // Get the context for creating the dialog controls
            Android.Content.Context ctx = this.Activity.ApplicationContext;

            // Set a dialog title
            this.Dialog.SetTitle("Order Results");

            // Call OnCreateView on the base
            base.OnCreateView(inflater, container, savedInstanceState);

            // The container for the dialog is a vertical linear layout
            dialogView = new LinearLayout(ctx);
            dialogView.Orientation = Orientation.Vertical;

            // Create an instance of a custom list adapter for showing the order fields
            OrderFieldListAdapter listAdapter = new OrderFieldListAdapter(this.Activity, _potentialOrderByFields);

            // Create a new list view that uses the adapter
            _orderFieldsListView = new ListView(ctx);
            _orderFieldsListView.Adapter = listAdapter;

            // Allow the user to select multiple fields in the list view
            _orderFieldsListView.ChoiceMode = ChoiceMode.Multiple;

            // Loop through all order fields in the list
            for (int i = 0; i < _potentialOrderByFields.Count; i++)
            {
                // If this field has been selected to order with, check it in the list view
                bool chosenForOrder = _potentialOrderByFields[i].OrderWith;
                _orderFieldsListView.SetItemChecked(i, chosenForOrder);
            }

            // Add the list view to the dialog
            dialogView.AddView(_orderFieldsListView);

            // Return the new view for display
            return dialogView;
        }
    }

    // Class to define a custom list adapter to show order field options
    public class OrderFieldListAdapter : BaseAdapter<OrderFieldOption>
    {
        // Store the current activity
        private Activity _ctx;

        // Store a list of the available order field options
        private List<OrderFieldOption> _orderFields;

        // Constructor that takes the current activity and list of order fields
        public OrderFieldListAdapter(Activity context, List<OrderFieldOption> orderFields) : base()
        {
            _ctx = context;
            _orderFields = orderFields;
        }

        // Return the order field option at the specified position
        public override OrderFieldOption this[int position]
        {
            get
            {
                return _orderFields[position];
            }
        }

        // Return the count of order field options in the list
        public override int Count
        {
            get
            {
                return _orderFields.Count;
            }
        }

        // Return the ID for the item at the specified position
        public override long GetItemId(int position)
        {
            return position;
        }

        // Create a view to display each order field option
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            // Create a list item view that shows a single text view and a check box
            View cellView = _ctx.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItemChecked, null);

            // Set the list item text with the field name (checked value will be set in the dialog UI)
            cellView.FindViewById<TextView>(Android.Resource.Id.Text1).Text = _orderFields[position].OrderInfo.FieldName;

            return cellView;
        }
    }

    // Class that defines a custom list adapter for displaying an expandable list of grouped items
    public class ExpandableResultsListAdapter : BaseExpandableListAdapter
    {
        // Store the current context
        private Context _ctx;

        // Store a dictionary of results: group name, results dictionary
        private Dictionary<string, IReadOnlyDictionary<string, object>> _resultsDictionary;

        // Store the group names
        private string[] _groupNames;

        // Constructor that takes the current context and a dictionary of group names and results
        public ExpandableResultsListAdapter(Context context, Dictionary<string, IReadOnlyDictionary<string, object>> results)
        {
            // Store the context and results
            _ctx = context;
            _resultsDictionary = results;

            // Get the group names from the results dictionary
            _groupNames = new string[results.Count];
            results.Keys.CopyTo(_groupNames, 0);
        }

        // Return the count of groups in the results
        public override int GroupCount
        {
            get
            {
                return _groupNames.Length;
            }
        }

        // No IDs for the items
        public override bool HasStableIds
        {
            get
            {
                return false;
            }
        }

        // Return the result at the specified index
        public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
        {
            // Get the result dictionary for the specified group
            IReadOnlyDictionary<string, object> result = new Dictionary<string, object>();
            _resultsDictionary.TryGetValue(_groupNames[groupPosition], out result);

            // Return a string concatenated from the field name and value at the specified position
            return result.ElementAt(childPosition).Key + " : " + result.ElementAt(childPosition).Value;
        }

        // Return the ID for the specified item
        public override long GetChildId(int groupPosition, int childPosition)
        {
            return childPosition;
        }

        // Return the count of items in the specified group
        public override int GetChildrenCount(int groupPosition)
        {
            return _resultsDictionary.ElementAt(groupPosition).Value.Count;
        }

        // Return a view to display a child item (key-value string within a group)
        public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
        {
            // Reuse the current view, if available
            if (convertView == null)
            {
                // Inflate a view from a resource that defines a result list item
                LayoutInflater inflator = (LayoutInflater)_ctx.GetSystemService(Context.LayoutInflaterService);
                convertView = inflator.Inflate(Resource.Layout.GroupedResultsList_DataItem, null);
            }

            // Get the text view from the data item layout
            TextView textViewItem = convertView.FindViewById<TextView>(Resource.Id.item);

            // Get content for this item and add it to the text view
            string content = (string)GetChild(groupPosition, childPosition);
            textViewItem.Text = content;

            return convertView;
        }

        // Return the group name
        public override Java.Lang.Object GetGroup(int groupPosition)
        {
            // Find the group name in the array
            string groupName = _groupNames[groupPosition];

            // If the group name is empty (maybe results weren't grouped), return "Results" for the group name
            if (string.IsNullOrEmpty(groupName))
            {
                groupName = "Results";
            }

            return groupName;
        }

        // Return the ID for the specified group
        public override long GetGroupId(int groupPosition)
        {
            return groupPosition;
        }

        // Create a view to display the group heading
        public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
        {
            // Reuse the view, if available
            if (convertView == null)
            {
                // Inflate a view from a resource that defines a result group item
                LayoutInflater inflator = (LayoutInflater)_ctx.GetSystemService(Context.LayoutInflaterService);
                convertView = inflator.Inflate(Resource.Layout.GroupedResultsList_GroupItem, null);
            }

            // Get the group name for this position
            string textGroup = (string)GetGroup(groupPosition);

            // Display the group in the text view
            TextView textViewGroup = convertView.FindViewById<TextView>(Resource.Id.group);
            textViewGroup.Text = textGroup;

            return convertView;
        }

        // Return if the item at the specified position is selectable
        public override bool IsChildSelectable(int groupPosition, int childPosition)
        {
            return false;
        }
    }
}