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
using Esri.ArcGISRuntime.Geometry;
using UIKit;

namespace ArcGISRuntime.Samples.ChangeBlendRenderer
{
    [Register("ChangeBlendRenderer")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("7c4c679ab06a4df19dc497f577f111bd","caeef9aa78534760b07158bb8e068462")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Blend renderer",
        "Layers",
        "This sample demonstrates how to use blend renderer on a raster layer. You can get a hillshade blended with either a colored raster or color ramp.",
        "Tap on the 'Update Renderer' button to change the settings for the blend renderer. The sample allows you to change the Altitude, Azimuth, SlopeType and ColorRamp. If you use None as the ColorRamp, a standard hill shade raster output is displayed. For all the other ColorRamp types an elevation raster is used.",
        "Featured")]
    public class ChangeBlendRenderer : UIViewController
    {
        // Global reference to a label for Altitude
        private UILabel _Label_Altitude;

        // Global reference to the slider where the user can modify the Altitude
        private UISlider _Altitude_Slider;

        // Global reference to a label for Azimuth
        private UILabel _Label_Azimuth;

        // Global reference to the slider where the user can modify the Azimuth
        private UISlider _Azimuth_Slider;

        // Global reference to a label for SlopeTypes
        private UILabel _Label_SlopeTypes;

        // Global reference to table of SlopeType choices the user can choose from
        private UITableView _SlopeTypes;

        // Global reference to a label for ColorRamps
        private UILabel _Label_ColorRamps;

        // Global reference to table of ColorRamps choices the user can choose from
        private UITableView _ColorRamps;

        // Global reference to button the user clicks to change the blend renderer on the raster
        private UIButton _UpdateRenderer;

        // Global reference to the MapView used in the sample
        private MapView _myMapView = new MapView();

        public ChangeBlendRenderer()
        {
            Title = "Blend renderer";
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
            // Top margin location were the UI controls should be placed
            var topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

            // Setup the visual frame for the label for Altitude
            _Label_Altitude.Frame = new CoreGraphics.CGRect(0, topMargin + 40, View.Bounds.Width, 40);

            // Setup the visual frame for the slider where the user can modify the Altitude
            _Altitude_Slider.Frame = new CoreGraphics.CGRect(100, topMargin + 40, View.Bounds.Width, 40);

            // Setup the visual frame for the label for Azimuth
            _Label_Azimuth.Frame = new CoreGraphics.CGRect(0, topMargin + 80, View.Bounds.Width, 40);

            // Setup the visual frame for the slider where the user can modify the Azimuth
            _Azimuth_Slider.Frame = new CoreGraphics.CGRect(100, topMargin + 80, View.Bounds.Width, 40);

            // Setup the visual frame for the label for SlopeTypes
            _Label_SlopeTypes.Frame = new CoreGraphics.CGRect(0, topMargin + 120, View.Bounds.Width, 40);

            // Setup the visual frame for the table of SlopeType choices the user can choose from
            _SlopeTypes.Frame = new CoreGraphics.CGRect(100, topMargin + 120, View.Bounds.Width, 40);

            // Setup the visual frame for the label for ColorRamps
            _Label_ColorRamps.Frame = new CoreGraphics.CGRect(0, topMargin + 160, View.Bounds.Width, 40);

            // Setup the visual frame for the table of ColorRamp choices the user can choose from
            _ColorRamps.Frame = new CoreGraphics.CGRect(100, topMargin + 160, View.Bounds.Width, 40);

            // Setup the visual frame for button the users clicks to change the blend renderer on the raster
            _UpdateRenderer.Frame = new CoreGraphics.CGRect(0, topMargin + 200, View.Bounds.Width, 40);

            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, topMargin + 240, View.Bounds.Width, View.Bounds.Height - 300);
        }

        private async void Initialize()
        {
            // Set the altitude slider min/max and initial value
            _Altitude_Slider.MinValue = 0;
            _Altitude_Slider.MaxValue = 90;
            _Altitude_Slider.Value = 45;

            // Set the azimuth slider min/max and initial value
            _Azimuth_Slider.MinValue = 0;
            _Azimuth_Slider.MaxValue = 360;
            _Azimuth_Slider.Value = 180;

            // Load the raster file using a path on disk
            Raster myRasterImagery = new Raster(GetRasterPath_Imagery());

            // Create the raster layer from the raster
            RasterLayer myRasterLayerImagery = new RasterLayer(myRasterImagery);

            // Create a new map using the raster layer as the base map 
            Map myMap = new Map(new Basemap(myRasterLayerImagery));

            // Wait for the layer to load - this enabled being able to obtain the extent information 
            // of the raster layer
            await myRasterLayerImagery.LoadAsync();

            // Create a new EnvelopeBuilder from the full extent of the raster layer 
            EnvelopeBuilder myEnvelopBuilder = new EnvelopeBuilder(myRasterLayerImagery.FullExtent);

            // Zoom in the extent just a bit so that raster layer encompasses the entire viewable area of the map
            myEnvelopBuilder.Expand(0.75);

            // Set the viewpoint of the map to the EnvelopeBuilder's extent
            myMap.InitialViewpoint = new Viewpoint(myEnvelopBuilder.ToGeometry().Extent);

            // Add map to the map view
            _myMapView.Map = myMap;

            // Wait for the map to load
            await myMap.LoadAsync();

            // Enable the 'Update Renderer' button now that the map has loaded
            _UpdateRenderer.Enabled = true;
        }

        private void CreateLayout()
        {
            // This section creates the UI elements and adds them to the layout view of the GUI

            // Create label that displays the Altitude
            _Label_Altitude = new UILabel();
            _Label_Altitude.Text = "Altitude:";
            _Label_Altitude.AdjustsFontSizeToFitWidth = true;
            _Label_Altitude.BackgroundColor = UIColor.White;

            // Create slider that the user can modify Altitude 
            _Altitude_Slider = new UISlider();

            // Create label that displays the Azimuth
            _Label_Azimuth = new UILabel();
            _Label_Azimuth.Text = "Azimuth:";
            _Label_Azimuth.AdjustsFontSizeToFitWidth = true;
            _Label_Azimuth.BackgroundColor = UIColor.White;

            // Create slider that the user can modify Azimuth 
            _Azimuth_Slider = new UISlider();

            // Create label that displays the SlopeTypes
            _Label_SlopeTypes = new UILabel();
            _Label_SlopeTypes.Text = "SlopeTypes:";
            _Label_SlopeTypes.AdjustsFontSizeToFitWidth = true;
            _Label_SlopeTypes.BackgroundColor = UIColor.White;

            // Get all the SlopeType names from the PresetColorRampType Enumeration and put them 
            // in an array of strings, then set the UITableView.Source to the array
            _SlopeTypes = new UITableView();
            string[] mySlopeTypes = Enum.GetNames(typeof(SlopeType));
            _SlopeTypes.Source = new TableSource(mySlopeTypes, this);
            _SlopeTypes.SeparatorColor = UIColor.Yellow;

            // Create label that displays the ColorRamps
            _Label_ColorRamps = new UILabel();
            _Label_ColorRamps.Text = "ColorRamps:";
            _Label_ColorRamps.AdjustsFontSizeToFitWidth = true;
            _Label_ColorRamps.BackgroundColor = UIColor.White;

            // Get all the ColorRamp names from the PresetColorRampType Enumeration and put them 
            // in an array of strings, then set the UITableView.Source to the array
            _ColorRamps = new UITableView();
            string[] myPresetColorRampTypes = Enum.GetNames(typeof(PresetColorRampType));
            _ColorRamps.Source = new TableSource(myPresetColorRampTypes, this);
            _ColorRamps.SeparatorColor = UIColor.Yellow;

            // Create button to change stretch renderer of the raster
            _UpdateRenderer = new UIButton();
            _UpdateRenderer.SetTitle("Update Renderer", UIControlState.Normal);
            _UpdateRenderer.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _UpdateRenderer.BackgroundColor = UIColor.White;
            // Hook to touch/click event of the button
            _UpdateRenderer.TouchUpInside += OnUpdateRendererClicked;
            _UpdateRenderer.Enabled = false;

            // Add all of the UI controls to the page
            View.AddSubviews(_Label_Altitude, _Altitude_Slider, _Label_Azimuth, _Azimuth_Slider, _Label_SlopeTypes, _SlopeTypes, _Label_ColorRamps, _ColorRamps, _UpdateRenderer, _myMapView);
        }

        private void OnUpdateRendererClicked(object sender, EventArgs e)
        {
            try
            {
                // Define the RasterLayer that will be used to display in the map
                RasterLayer rasterLayer_ForDisplayInMap;

                // Define the ColorRamp that will be used by the BlendRenderer
                ColorRamp myColorRamp;

                // Get the user choice for the ColorRamps
                UITableViewSource myUITableViewSource_ColorRamp = _ColorRamps.Source;
                TableSource myTableSource_ColorRamp = (TableSource)myUITableViewSource_ColorRamp;
                string myColorRampChoice;

                if (myTableSource_ColorRamp.SelectedValue == null)
                {
                    // If the user does not click on a choice in the table but just clicks the
                    // button, the selected value will be null so use the initial ColorRamp option
                    myColorRampChoice = "Elevation";
                }
                else
                {
                    // The user clicked on an option in the table and thus the selected value
                    // will contain a valid choice
                    myColorRampChoice = myTableSource_ColorRamp.SelectedValue;
                }

                // Based on ColorRamp type chosen by the user, create a different
                // RasterLayer and define the appropriate ColorRamp option
                if (myColorRampChoice == "None")
                {
                    // The user chose not to use a specific ColorRamp, therefore 
                    // need to create a RasterLayer based on general imagery (ie. Shasta.tif)
                    // for display in the map and use null for the ColorRamp as one of the
                    // parameters in the BlendRenderer constructor

                    // Load the raster file using a path on disk
                    Raster raster_Imagery = new Raster(GetRasterPath_Imagery());

                    // Create the raster layer from the raster
                    rasterLayer_ForDisplayInMap = new RasterLayer(raster_Imagery);

                    // Set up the ColorRamp as being null
                    myColorRamp = null;
                }
                else
                {

                    // The user chose a specific ColorRamp (options: are Elevation, DemScreen, DemLight), 
                    // therefore create a RasterLayer based on an imagery with elevation 
                    // (ie. Shasta_Elevation.tif) for display in the map. Also create a ColorRamp 
                    // based on the user choice, translated into an Enumeration, as one of the parameters 
                    // in the BlendRenderer constructor

                    // Load the raster file using a path on disk
                    Raster raster_Elevation = new Raster(GetRasterPath_Elevation());

                    // Create the raster layer from the raster
                    rasterLayer_ForDisplayInMap = new RasterLayer(raster_Elevation);

                    // Create a ColorRamp based on the user choice, translated into an Enumeration
                    PresetColorRampType myPresetColorRampType = (PresetColorRampType)Enum.Parse(typeof(PresetColorRampType), myColorRampChoice);
                    myColorRamp = ColorRamp.Create(myPresetColorRampType, 256);
                }


                // Define the parameters used by the BlendRenderer constructor
                Raster raster_ForMakingBlendRenderer = new Raster(GetRasterPath_Elevation());
                IEnumerable<double> myOutputMinValues = new List<double> { 9 };
                IEnumerable<double> myOutputMaxValues = new List<double> { 255 };
                IEnumerable<double> mySourceMinValues = new List<double>();
                IEnumerable<double> mySourceMaxValues = new List<double>();
                IEnumerable<double> myNoDataValues = new List<double>();
                IEnumerable<double> myGammas = new List<double>();

                // Get the user choice for the SlopeType
                UITableViewSource myUITableViewSource_SlopeType = _SlopeTypes.Source;
                TableSource myTableSource_SlopeType = (TableSource)myUITableViewSource_SlopeType;
                string mySlopeTypeChoice;

                if (myTableSource_SlopeType.SelectedValue == null)
                {
                    // If the user does not click on a choice in the table but just clicks the
                    // button, the selected value will be null so use the initial SlopeType option
                    mySlopeTypeChoice = "Degree";
                }
                else
                {
                    // The user clicked on an option in the table and thus the selected value
                    // will contain a valid choice
                    mySlopeTypeChoice = myTableSource_SlopeType.SelectedValue;
                }

                SlopeType mySlopeType = (SlopeType)Enum.Parse(typeof(SlopeType), mySlopeTypeChoice);

                BlendRenderer myBlendRenderer = new BlendRenderer(
                    raster_ForMakingBlendRenderer, // elevationRaster - Raster based on a elevation source
                    myOutputMinValues, // outputMinValues - Output stretch values, one for each band
                    myOutputMaxValues, // outputMaxValues - Output stretch values, one for each band
                    mySourceMinValues, // sourceMinValues - Input stretch values, one for each band
                    mySourceMaxValues, // sourceMaxValues - Input stretch values, one for each band
                    myNoDataValues, // noDataValues - NoData values, one for each band
                    myGammas, // gammas - Gamma adjustment
                    myColorRamp, // colorRamp - ColorRamp object to use, could be null
                    _Altitude_Slider.Value, // altitude - Altitude angle of the light source
                    _Azimuth_Slider.Value, // azimuth - Azimuth angle of the light source, measured clockwise from north
                    1, // zfactor - Factor to convert z unit to x,y units, default is 1
                    mySlopeType, // slopeType - Slope Type
                    1, // pixelSizeFactor - Pixel size factor, default is 1
                    1, // pixelSizePower - Pixel size power value, default is 1
                    8); // outputBitDepth - Output bit depth, default is 8-bi

                // Set the RasterLayer.Renderer to be the BlendRenderer
                rasterLayer_ForDisplayInMap.Renderer = myBlendRenderer;

                // Set the new base map to be the RasterLayer with the BlendRenderer applied
                _myMapView.Map.Basemap = new Basemap(rasterLayer_ForDisplayInMap);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static string GetRasterPath_Imagery()
        {
            return DataManager.GetDataFolder("7c4c679ab06a4df19dc497f577f111bd", "raster-file", "Shasta.tif");
        }

        private static string GetRasterPath_Elevation()
        {
            return DataManager.GetDataFolder("caeef9aa78534760b07158bb8e068462", "Shasta_Elevation.tif");
        }
    }

    /// <summary>
    /// This is a custom class that defines how the UITableView control renders its 
    /// contents. It implements the UI for the list of strings that display 
    /// various options for the user to pick from.
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
        public ChangeBlendRenderer Owner { get; set; }

        // Default constructor to create this custom class that is used as the 
        // UTTableView.Source property. It input parameters take an array of strings
        // and the parent owning view controller.
        public TableSource(string[] items, ChangeBlendRenderer owner)
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
        // It performs the function: 
        // (1) Set the SelectedValue property that gives the developer the ability to 
        // know what was the selected item in the UITableView from the user click
        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            // Deselect the row
            tableView.DeselectRow(indexPath, true);

            // Set the SelectedValue property
            SelectedValue = TableItems[indexPath.Row];
        }
    }
}