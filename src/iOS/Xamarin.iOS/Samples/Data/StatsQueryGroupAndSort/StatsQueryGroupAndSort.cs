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
using System.Collections.ObjectModel;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.StatsQueryGroupAndSort
{
    [Register("StatsQueryGroupAndSort")]
    public class StatsQueryGroupAndSort : UIViewController
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

        // Stack view UI control for arranging query controls
        private UIStackView _controlsStackView;

        // UI controls that will need to be referenced
        private UIPickerView _statisticPicker;
       // private UITextView _statisticDefinitionsList;

        private StatDefinitionModel _statsPickerModel;

        public StatsQueryGroupAndSort()
        {
            Title = "Stats query with grouped results";
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
            _controlsStackView.Frame = new CoreGraphics.CGRect(0,  pageOffset, View.Bounds.Width, View.Bounds.Height - pageOffset);

            base.ViewDidLayoutSubviews();
        }

        private async void Initialize()
        {
            // Create the US states feature table
            _usStatesTable = new ServiceFeatureTable(_usStatesServiceUri);

            // Load the table
            await _usStatesTable.LoadAsync();

            // Fill the fields combo and "group by" list with field names from the table
            var fieldNames = _usStatesTable.Fields.Select(f => f.Name).ToList();

            // TODO: figure out how to fill the pickers, lists, whatever
            _statsPickerModel = new StatDefinitionModel(fieldNames.ToArray());
            _statisticPicker.Model = _statsPickerModel;
        }

        private void CreateLayout()
        {
            this.View.BackgroundColor = UIColor.White;

            // Create a stack view to organize the query controls
            _controlsStackView = new UIStackView();
            _controlsStackView.Axis = UILayoutConstraintAxis.Vertical;
            _controlsStackView.Alignment = UIStackViewAlignment.Center;
            _controlsStackView.Distribution = UIStackViewDistribution.EqualSpacing;
            _controlsStackView.Spacing = 5;

            // TODO: create list boxes, etc.
            _statisticPicker = new UIPickerView();

            // Text view to show/add statistic definitions
            //_statisticDefinitionsList = new UITextView();
            //_statisticDefinitionsList.Frame = new CoreGraphics.CGRect(10,10,200,80);
            //_statisticDefinitionsList.BackgroundColor = UIColor.LightGray;
            //_statisticDefinitionsList.Text = "ABC : 123";
            //_statisticDefinitionsList.InputView = _statisticPicker;
            //_statisticDefinitionsList.ScrollEnabled = true;
            //_statisticDefinitionsList.ShowsHorizontalScrollIndicator = true;

            var addStatButton = new UIButton();
            addStatButton.SetTitle("Statistic Definitions", UIControlState.Normal);
            addStatButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            addStatButton.BackgroundColor = UIColor.White;
            addStatButton.Frame = new CoreGraphics.CGRect(30, 20, 220, 30);
            addStatButton.TouchUpInside += AddStatButton_TouchUpInside;

            UIToolbar toolbar = new UIToolbar();
            toolbar.BarStyle = UIBarStyle.Black;
            toolbar.Translucent = true;
            toolbar.SizeToFit();
            UIBarButtonItem doneButton = new UIBarButtonItem("Done", UIBarButtonItemStyle.Done, (s, e) =>
            {
                var newStatDefinition = _statsPickerModel.SelectedStatDefinition;
                if (newStatDefinition != null)
                {
                    var statDefText = newStatDefinition.OnFieldName + " (" + newStatDefinition.StatisticType.ToString() + ")";
                  //  _statisticDefinitionsList.Text = statDefText;
                }

             //   _statisticDefinitionsList.ResignFirstResponder();

            });
            toolbar.SetItems(new UIBarButtonItem[] { doneButton }, true);

           // _statisticDefinitionsList.InputAccessoryView = toolbar;

            // Create a button to invoke the query
            var getStatsButton = new UIButton();
            getStatsButton.SetTitle("Get Statistics", UIControlState.Normal);
            getStatsButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            getStatsButton.BackgroundColor = UIColor.White;

            // Handle the button tap to execute the statistics query
            getStatsButton.TouchUpInside += OnExecuteStatisticsQueryClicked;

            // Add controls to the stack view
            // TODO: add other UI controls
            _controlsStackView.AddSubview(addStatButton);// (_statisticPicker);
            //_controlsStackView.AddArrangedSubview(getStatsButton);

            // Add UI controls to the page
            View.AddSubview(_controlsStackView);
        }

        private void AddStatButton_TouchUpInside(object sender, EventArgs e)
        {
            var statsTable = new StatsDefinitionTable();
            statsTable.statPickerUI = _statisticPicker;
            var statDefs = new List<StatisticDefinition>();
            statDefs.Add(new StatisticDefinition("POP200", StatisticType.Average, "2000_avg"));
            var src = new StatisticDefinitionsDataSource(statDefs, null);
            statsTable.TableView.Source = src;
            src.WillBeginTableEditing(statsTable.TableView);
            statsTable.SetEditing(true, true);
            this.NavigationController.PushViewController(statsTable, true);
        }

        private async void OnExecuteStatisticsQueryClicked(object sender, EventArgs e)
        {
            // Create definitions for each statistic to calculate

            // Create a definition for count that includes an alias for the output
            StatisticDefinition statDefinitionCount = new StatisticDefinition("POP", StatisticType.Count, "CityCount");

            // Add the statistics definitions to a list
            List<StatisticDefinition> statDefinitions = new List<StatisticDefinition>
                        {
                        statDefinitionCount
                        };

            // Create the statistics query parameters, pass in the list of definitions
            StatisticsQueryParameters statQueryParams = new StatisticsQueryParameters(statDefinitions);


            // Execute the statistical query with these parameters and await the results
            StatisticsQueryResult statQueryResult = await _usStatesTable.QueryStatisticsAsync(statQueryParams);

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

    public class StatDefinitionModel : UIPickerViewModel
    {
        private string[] _fieldNames;
        private Array _statTypes = Enum.GetValues(typeof(StatisticType));
        private StatisticDefinition _selectedStatDefinition = null;

        public StatisticDefinition SelectedStatDefinition
        {
            get { return _selectedStatDefinition; }
        }

        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 2;
        }

        public StatDefinitionModel(string[] fieldNames)
        {
            this._fieldNames = fieldNames;
        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            if (component == 0)
                return _fieldNames.Length;
            else
                return _statTypes.Length;
        }

        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            if (component == 0)
                return _fieldNames[row];
            else
                return _statTypes.GetValue(row).ToString();
        }

        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            var onFieldName = _fieldNames[pickerView.SelectedRowInComponent(0)];
            var statType = (StatisticType)_statTypes.GetValue(pickerView.SelectedRowInComponent(1));
            var outAlias = onFieldName + "_" + statType.ToString();
            _selectedStatDefinition = new StatisticDefinition(onFieldName, statType, outAlias);
        }

        public override nfloat GetComponentWidth(UIPickerView picker, nint component)
        {
            if (component == 0)
                return 160f;
            else
                return 120f;
        }

        public override nfloat GetRowHeight(UIPickerView picker, nint component)
        {
            return 40f;
        }
    }


    [Register("StatsDefinitionTable")]
    public class StatsDefinitionTable : UITableViewController
    {
       // public List<StatisticDefinition> statDefinitions;
        public UIPickerView statPickerUI;

        public StatsDefinitionTable()
        {
            Title = "Statistics Definitions";
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();
        }

        public override UIView InputView
        {
            get
            {
                return statPickerUI;
            }
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            //if (statDefinitions != null)
            //{
            //    var src = new StatisticDefinitionsDataSource(statDefinitions, statPickerUI);
            //    TableView.Source = src;
            //    TableView.Frame = new CoreGraphics.CGRect(0, 0, 100, 100);
            //}
        }
    }

    public class StatisticDefinitionsDataSource : UITableViewSource
    {
        private List<StatisticDefinition> _statisticDefinitions;

        static string CELL_ID = "cellid";
        
        public StatisticDefinitionsDataSource(List<StatisticDefinition> statDefs, UIPickerView picker)
        {
            _statisticDefinitions = statDefs;
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            if(editingStyle == UITableViewCellEditingStyle.Insert)
            {
                // show picker?
                
            }
        }

        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
        {
            return "Axe it!";
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true;
        }
        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            if (tableView.Editing)
            {
                if (indexPath.Row == tableView.NumberOfRowsInSection(0) - 1)
                    return UITableViewCellEditingStyle.Insert;
                else
                    return UITableViewCellEditingStyle.Delete;
            }
            else // not in editing mode, enable swipe-to-delete for all rows
                return UITableViewCellEditingStyle.Delete;
        }

        public void WillBeginTableEditing(UITableView tableView)
        {
            tableView.BeginUpdates();
            // insert the 'ADD NEW' row at the end of table display
            tableView.InsertRows(new NSIndexPath[] {
            NSIndexPath.FromRowSection (tableView.NumberOfRowsInSection (0), 0)
        }, UITableViewRowAnimation.Fade);
            // create a new item and add it to our underlying data (it is not intended to be permanent)
            _statisticDefinitions.Add(new StatisticDefinition("(add new)", StatisticType.Count, ""));
            tableView.EndUpdates(); // applies the changes
        }
        
        public void DidFinishTableEditing(UITableView tableView)
        {
            tableView.BeginUpdates();
            // remove our 'ADD NEW' row from the underlying data
            _statisticDefinitions.RemoveAt((int)tableView.NumberOfRowsInSection(0) - 1); // zero based :)
                                                                              // remove the row from the table display
            tableView.DeleteRows(new NSIndexPath[] { NSIndexPath.FromRowSection(tableView.NumberOfRowsInSection(0) - 1, 0) }, UITableViewRowAnimation.Fade);
            tableView.EndUpdates(); // applies the changes
        }
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Create the cells in the table
            var cell = new UITableViewCell(UITableViewCellStyle.Subtitle, CELL_ID);

            var definition = _statisticDefinitions[indexPath.Row] as StatisticDefinition;
            cell.TextLabel.Text = definition.OnFieldName;
            if (!definition.OnFieldName.ToLower().Contains("add new"))
            {
                cell.DetailTextLabel.Text = definition.StatisticType.ToString();
            }
            //// Create a UISwitch for controlling the layer visibility
            //var visibilitySwitch = new UISwitch()
            //{
            //    Frame = new CoreGraphics.CGRect(cell.Bounds.Width - 60, 7, 50, cell.Bounds.Height)
            //};
            //visibilitySwitch.Tag = indexPath.Row;
            //visibilitySwitch.On = sublayer.IsVisible;
            //visibilitySwitch.ValueChanged += VisibilitySwitch_ValueChanged;

            //// Add the UISwitch to the cell's content view
            //cell.ContentView.AddSubview(visibilitySwitch);

            return cell;
        }

        private void VisibilitySwitch_ValueChanged(object sender, EventArgs e)
        {
            //// Get the row containing the UISwitch that was changed
            //var index = (sender as UISwitch).Tag;

            //// Set the sublayer visibility according to the UISwitch setting
            //var sublayer = sublayers[(int)index] as ArcGISMapImageSublayer;
            //sublayer.IsVisible = (sender as UISwitch).On;
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return _statisticDefinitions.Count;
        }
    }
}