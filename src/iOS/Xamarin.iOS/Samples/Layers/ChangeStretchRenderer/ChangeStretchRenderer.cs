// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using ArcGISRuntime.Samples.Managers;
using UIKit;

namespace ArcGISRuntime.Samples.ChangeStretchRenderer
{

    [Register("ChangeStretchRenderer")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("95392f99970d4a71bd25951beb34a508")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Stretch renderer",
        "Layers",
        "This sample demonstrates how to use stretch renderer on a raster layer.",
        "Choose a stretch renderer type from the table to change the settings for the stretch renderer.\nThe sample allows you to change the stretch type and the parameters for each type. Click/tap the 'Update Renderer' button to update the raster.\nExperiment with settings for the various types for stretch parameters. For example, setting the renderer to use stretch parameters:\nMin Max with a min value of 50 and a max value of 200 will stretch between these pixel values. A higher min value will remove more of the lighter pixels values whilst a lower max will remove more of the darker.\nPercent Clip with a min value of 2 and a max value of 98 will stretch from 2% to 98% of the pixel values histogram. A lower min and higher max percentage will render using more of the original raster histogram.\nStandard Deviation with a factor of 2.0 will stretch 2 standard deviations from the mean. A higher factor (further from the mean) will render using more of the original raster histogram.",
        "Featured")]
    public class ChangeStretchRenderer : UIViewController
    {
        // Global constant holding offset where the MapView control should start
        private const int _yPageOffset = 60;

        // Global reference to the MapView used in the sample
        private MapView _myMapView = new MapView();

        // Global reference to table of stretch render choices the user can choose from
        private UITableView _myRenderChoiceType;

        // Global reference to a label that displays the 1st parameter used by the stretch renderer
        private UILabel _Label_Parameter1;

        // Global reference to the 1st parameter used by the stretch renderer that the user can modify 
        private UITextField _Input_Parameter1;

        // Global reference to a label that displays the 2nd parameter used by the stretch renderer
        private UILabel _Label_Parameter2;

        // Global reference to the 2nd parameter used by the stretch renderer that the user can modify 
        private UITextField _Input_Parameter2;

        // Global reference to button the user clicks to change the stretch renderer on the raster 
        private UIButton _UpdateRenderer;

        public ChangeStretchRenderer()
        {
            Title = "Stretch renderer";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the layout
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for button the users clicks to change the stretch renderer on the raster
            _UpdateRenderer.Frame = new CoreGraphics.CGRect(0, _yPageOffset, View.Bounds.Width, 40);

            // Setup the visual frame for the list of stretch renderer choices the user can pick from
            _myRenderChoiceType.Frame = new CoreGraphics.CGRect(0, _yPageOffset + 40, View.Bounds.Width, 140);

            // Setup the visual frame for the label that displays the 1st parameter used by the stretch renderer
            _Label_Parameter1.Frame = new CoreGraphics.CGRect(0, _yPageOffset + 180, View.Bounds.Width, 40);

            // Setup the visual frame for the 1st parameter used by the stretch renderer that the user can modify 
            _Input_Parameter1.Frame = new CoreGraphics.CGRect(250, _yPageOffset + 180 , View.Bounds.Width, 40);

            // Setup the visual frame for the label that displays the 2nd parameter used by the stretch renderer
            _Label_Parameter2.Frame = new CoreGraphics.CGRect(0, _yPageOffset + 220, View.Bounds.Width, 40);

            // Setup the visual frame for the 2nd parameter used by the stretch renderer that the user can modify 
            _Input_Parameter2.Frame = new CoreGraphics.CGRect(250, _yPageOffset + 220, View.Bounds.Width, 40);

            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, _yPageOffset + 260, View.Bounds.Width, View.Bounds.Height - 300);
        }

        private async void Initialize()
        {
            // Add an imagery basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Wait for the map to load
            await myMap.LoadAsync();

            // Get the file name
            string filepath = GetRasterPath();

            // Load the raster file
            Raster myRasterFile = new Raster(filepath);

            // Create the layer
            RasterLayer myRasterLayer = new RasterLayer(myRasterFile);

            // Add the layer to the map
            myMap.OperationalLayers.Add(myRasterLayer);

            // Wait for the layer to load
            await myRasterLayer.LoadAsync();

            // Set the viewpoint
            myMap.InitialViewpoint = new Viewpoint(myRasterLayer.FullExtent);

            // Add map to the mapview
            _myMapView.Map = myMap;
        }

        private void CreateLayout()
        {
            // This section creates the UI elements and adds them to the layout view of the GUI

            // Create button to change stretch renderer of the raster
            _UpdateRenderer = new UIButton();
            _UpdateRenderer.SetTitle("Update Renderer", UIControlState.Normal);
            _UpdateRenderer.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _UpdateRenderer.BackgroundColor = UIColor.White;
            // Hook to touch/click event of the button
            _UpdateRenderer.TouchUpInside += OnUpdateRendererClicked;

            // Create a list of stretch renderer choices the user can choose from
            _myRenderChoiceType = new UITableView();
            // The options of stretch renderer types the user can choose from
            string[] tableItems = new string[] { "Min Max", "Percent Clip", "Standard Deviation"};
            // Custom class to handle display/interaction of items in the UITableView
            _myRenderChoiceType.Source = new TableSource(tableItems,this); 
            _myRenderChoiceType.SeparatorColor = UIColor.Yellow;

            // Create label that displays the 1st parameter used by the stretch renderer
            _Label_Parameter1 = new UILabel();
            _Label_Parameter1.Text = "Minimum value (0 - 255):";
            _Label_Parameter1.AdjustsFontSizeToFitWidth = true;
            _Label_Parameter1.BackgroundColor = UIColor.White;

            // Create text field for 1st parameter used by the stretch renderer that the user can modify 
            _Input_Parameter1 = new UITextField();
            _Input_Parameter1.Text = "10";
            _Input_Parameter1.AdjustsFontSizeToFitWidth = true;
            _Input_Parameter1.BackgroundColor = UIColor.White;
            // Allow pressing 'return' to dismiss the keyboard
            _Input_Parameter1.ShouldReturn += (textField) => { textField.ResignFirstResponder(); return true; };

            // Create label that displays the 2nd parameter used by the stretch renderer
            _Label_Parameter2 = new UILabel();
            _Label_Parameter2.Text = "Maximum value (0 - 255):";
            _Label_Parameter2.AdjustsFontSizeToFitWidth = true;
            _Label_Parameter2.BackgroundColor = UIColor.White;

            // Create text field for 2nd parameter used by the stretch renderer that the user can modify 
            _Input_Parameter2 = new UITextField();
            _Input_Parameter2.Text = "150";
            _Input_Parameter2.AdjustsFontSizeToFitWidth = true;
            _Input_Parameter2.BackgroundColor = UIColor.White;
            // Allow pressing 'return' to dismiss the keyboard
            _Input_Parameter2.ShouldReturn += (textField) => { textField.ResignFirstResponder(); return true; };

            // Add all of the UI controls to the page
            View.AddSubviews(_UpdateRenderer, _myRenderChoiceType, _Label_Parameter1, _Input_Parameter1, _Label_Parameter2, _Input_Parameter2, _myMapView);
        }

        public void RenderChoiceSelectionChanged(int selectedIndex)
        {
            // This function modifies the UI parameter controls depending on which stretch 
            // renderer is chosen by the user when clicking the table view

            // Get the user choice for the raster stretch render
            UITableViewSource myUITableViewSource = _myRenderChoiceType.Source;
            TableSource myTableSource = (TableSource)myUITableViewSource;
            string myRendererTypeChoice = myTableSource.TableItems[selectedIndex];

            switch (myRendererTypeChoice)
            {
                case "Min Max":

                    // This section displays/resets the user choice options for MinMaxStretchParameters

                    // Make sure all the GUI items are visible
                    _Label_Parameter1.Hidden = false;
                    _Label_Parameter2.Hidden = false;
                    _Input_Parameter1.Hidden = false;
                    _Input_Parameter2.Hidden = false;

                    // Define what values/options the user sees
                    _Label_Parameter1.Text = "Minimum value (0 - 255):";
                    _Label_Parameter2.Text = "Maximum value (0 - 255):";
                    _Input_Parameter1.Text = "10";
                    _Input_Parameter2.Text = "150";

                    break;

                case "Percent Clip":

                    // This section displays/resets the user choice options for PercentClipStretchParameters

                    // Make sure all the GUI items are visible
                    _Label_Parameter1.Hidden = false;
                    _Label_Parameter2.Hidden = false;
                    _Input_Parameter1.Hidden = false;
                    _Input_Parameter2.Hidden = false;

                    // Define what values/options the user sees
                    _Label_Parameter1.Text = "Minimum (0 - 100):";
                    _Label_Parameter2.Text = "Maximum (0 - 100)";
                    _Input_Parameter1.Text = "0";
                    _Input_Parameter2.Text = "50";

                    break;

                case "Standard Deviation":

                    // This section displays/resets the user choice options for StandardDeviationStretchParameters

                    // Make sure that only the necessary GUI items are visible
                    _Label_Parameter1.Hidden = false;
                    _Label_Parameter2.Hidden = true;
                    _Input_Parameter1.Hidden = false;
                    _Input_Parameter2.Hidden = true;

                    // Define what values/options the user sees
                    _Label_Parameter1.Text = "Factor (.25 to 4):";
                    _Input_Parameter1.Text = "0.5";

                    break;
            }

        }

        private void OnUpdateRendererClicked(object sender, EventArgs e)
        {

            // This function acquires the user selection of the stretch renderer from the table view
            // along with the parameters specified, then a stretch renderer is created and applied to 
            // the raster layer

            // Get the user choice for the raster stretch render
            UITableViewSource myUITableViewSource = _myRenderChoiceType.Source;
            TableSource myTableSource = (TableSource)myUITableViewSource;
            string myRendererTypeChoice;

            if (myTableSource.SelectedValue == null)
            {
                // If the user does not click on a choice in the table but just clicks the
                // button, the selected value will be null so use the initial
                // stretch renderer option
                myRendererTypeChoice = "Min Max";
            }
            else
            {
                // The user clicked on an option in the table and thus the selected value
                // will contain a valid choice
                myRendererTypeChoice = myTableSource.SelectedValue; 
            }

            // Create an IEnumerable from an empty list of doubles for the gamma values in the stretch render
            IEnumerable<double> myGammaValues = new List<double>();

            // Create a color ramp for the stretch renderer
            ColorRamp myColorRamp = ColorRamp.Create(PresetColorRampType.DemLight, 1000);

            // Create the place holder for the stretch renderer
            StretchRenderer myStretchRenderer = null;

            switch (myRendererTypeChoice)
            {
                case "Min Max":

                    // This section creates a stretch renderer based on a MinMaxStretchParameters
                    // TODO: Add you own logic to ensure that accurate min/max stretch values are used

                    // Create an IEnumerable from a list of double min stretch value doubles
                    IEnumerable<double> myMinValues = new List<double> { Convert.ToDouble(_Input_Parameter1.Text) };

                    // Create an IEnumerable from a list of double max stretch value doubles
                    IEnumerable<double> myMaxValues = new List<double> { Convert.ToDouble(_Input_Parameter2.Text) };

                    // Create a new MinMaxStretchParameters based on the user choice for min and max stretch values
                    MinMaxStretchParameters myMinMaxStretchParameters = new MinMaxStretchParameters(myMinValues, myMaxValues);

                    // Create the stretch renderer based on the user defined min/max stretch values, empty gamma values, statistic estimates, and a predefined color ramp 
                    myStretchRenderer = new StretchRenderer(myMinMaxStretchParameters, myGammaValues, true, myColorRamp);

                    break;

                case "Percent Clip":

                    // This section creates a stretch renderer based on a PercentClipStretchParameters
                    // TODO: Add you own logic to ensure that accurate min/max percent clip values are used

                    // Create a new PercentClipStretchParameters based on the user choice for min and max percent clip values
                    PercentClipStretchParameters myPercentClipStretchParameters = new PercentClipStretchParameters(Convert.ToDouble(_Input_Parameter1.Text), Convert.ToDouble(_Input_Parameter2.Text));

                    // Create the percent clip renderer based on the user defined min/max percent clip values, empty gamma values, statistic estimates, and a predefined color ramp 
                    myStretchRenderer = new StretchRenderer(myPercentClipStretchParameters, myGammaValues, true, myColorRamp);

                    break;

                case "Standard Deviation":

                    // This section creates a stretch renderer based on a StandardDeviationStretchParameters
                    // TODO: Add you own logic to ensure that an accurate standard deviation value is used

                    // Create a new StandardDeviationStretchParameters based on the user choice for standard deviation value
                    StandardDeviationStretchParameters myStandardDeviationStretchParameters = new StandardDeviationStretchParameters(Convert.ToDouble(_Input_Parameter1.Text));

                    // Create the standard deviation renderer based on the user defined standard deviation value, empty gamma values, statistic estimates, and a predefined color ramp 
                    myStretchRenderer = new StretchRenderer(myStandardDeviationStretchParameters, myGammaValues, true, myColorRamp);

                    break;
            }

            // Get the existing raster layer in the map
            RasterLayer myRasterLayer = (RasterLayer)_myMapView.Map.OperationalLayers[0];

            // Apply the stretch renderer to the raster layer
            myRasterLayer.Renderer = myStretchRenderer;
        }

        private static string GetRasterPath()
        {
            return DataManager.GetDataFolder("95392f99970d4a71bd25951beb34a508", "shasta", "ShastaBW.tif");
        }

    }

    /// <summary>
    /// This is a custom class that defines how the UITableView control renders its 
    /// contents. It implements the UI for the list of strings that display 
    /// 'stretch renderer' options for the user to pick from.
    /// </summary>
    /// <remarks>
    /// Unlike WPF, UWP and Xamarin.Forms; the native Xamarin iOS does not include an out 
    /// of the box a ListView or ComboBox type GUI control. The closest option is an
    /// UITableView control that can display a list of options that users can see and
    /// interact with. In order to present the list of options (typically human readable
    /// strings) to the user, it is required to create a custom class to bind to the 
    /// UITableView.Source property. It is the developers responsibility to write the 
    /// interaction logic of the IUTableView control for things such as obtaining: the 
    /// list of items or the currently selected item in the UITableView. 
    /// </remarks>
    public class TableSource : UITableViewSource
    {
        // Public property to get the items/array (as strings) in the UITableView
        public string[] TableItems;

        // Public property to get the currently selected item in the array of
        // options displayed in the UITableView 
        public string SelectedValue;

        // Public property used when re-using cells to ensure that a cell of the right 
        // type is used
        public string CellIdentifier = "TableCell";

        // Public property to hold a reference to the owning view controller; this will be 
        // the active instance of the ChangeStretchRenderer sample  
        public ChangeStretchRenderer Owner { get; set; }

        // Default constructor to create this custom class that is used as the 
        // UTTableView.Source property. It input parameters take an array of strings
        // and the parent owning view controller.
        public TableSource(string[] items, ChangeStretchRenderer owner)
        {
            // Set the TableItems property
            TableItems = items;

            // Set the Owner property
            Owner = owner;
        }

        // Return an nint count of the total number of rows of data the UITableView 
        // should display, in this case it will return the number of stretch renderer
        // options the user has to choose from
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            // Return the total number of rows in the UITableView 
            return TableItems.Length;
        }

        // This method gets a table view cell for the suggestion at the specified index
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Try to get a re-usable cell (this is for performance)
            UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);

            // Get the specific item to display
            string item = TableItems[indexPath.Row];

            // If there are no cells to reuse, create a new one
            if (cell == null)
            { cell = new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier); }

            // Set the text on the cell
            cell.TextLabel.Text = item;

            // Return the cell
            return cell;
        }

        // This method handles when the user taps/clicks on an item in the UITableView.
        // It performs two functions: 
        // (1) Invoke a call to the RenderChoiceSelectionChanged method in the ChangeStretchRender
        // class - this modifies the UI parameter controls depending on which stretch 
        // renderer is chosen by the user when clicking the table view 
        // (2) Set the SelectedValue property that gives the developer the ability to 
        // know what was the selected item in the UITableView from the user click
        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            // Deselect the row
            tableView.DeselectRow(indexPath, true);

            // Invoke a call to the RenderChoiceSelectionChanged method in the ChangeStretchRender class
            Owner.RenderChoiceSelectionChanged(indexPath.Row);

            // Set the SelectedValue property
            SelectedValue = TableItems[indexPath.Row];
        }
    }

}