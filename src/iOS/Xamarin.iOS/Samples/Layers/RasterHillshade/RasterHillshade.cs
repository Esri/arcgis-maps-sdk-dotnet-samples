// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntimeXamarin.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.RasterHillshade
{
    [Register("RasterHillshade")]
    public class RasterHillshade : UIViewController
    {
        // Constant holding offset where the MapView control should start
        private const int _yPageOffset = 60;
        
        // Stack view UI control for arranging query controls
        private UIStackView _controlsStackView;

        // UI controls that will need to be referenced: sliders to define hillshade sun position, and MapView
        private UIPickerView _slopeTypePicker;
        private UISlider _altitudeSlider;
        private UISlider _azimuthSlider;
        private MapView _myMapView = new MapView();

        // Constant to store a z-factor (conversion constant) applied to the hillshade.
        // If needed, this can be used to convert z-values to the same unit as the x/y coordinates or to apply a vertical exaggeration.
        private const double Z_FACTOR = 1.0;

        // Constants to store the Pixel Size Power and Pixel Size Factor values.
        // Use these to account for altitude changes (scale) as the viewer zooms in and out (recommended when using worldwide datasets).
        private const double PIXEL_SIZE_POWER = 1.0;
        private const double PIXEL_SIZE_FACTOR = 1.0;

        // Constant to store the bit depth (pixel depth), which determines the range of values that the hillshade raster can store.
        private const int PIXEL_BIT_DEPTH = 8;

        // Store a reference to the layer
        RasterLayer _rasterLayer;

        // Store a dictionary of slope types
        Dictionary<string, SlopeType> _slopeTypeValues = new Dictionary<string, SlopeType>();

        public RasterHillshade()
        {
            Title = "Raster Hillshade";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Get height of status bar and navigation bar
            nfloat pageOffset = NavigationController.NavigationBar.Frame.Size.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

            // Setup the visual frame for the query controls
            _controlsStackView.Frame = new CoreGraphics.CGRect(0, pageOffset, View.Bounds.Width, 150);

            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
        }

        private async void Initialize()
        {
            // Create a map with a streets basemap
            Map map = new Map(Basemap.CreateStreetsVector());

            // Get the file name for the local raster dataset
            string filepath = await GetRasterPath();

            // Load the raster file
            Raster rasterFile = new Raster(filepath);

            // Create and load a new raster layer to show the image
            _rasterLayer = new RasterLayer(rasterFile);
            await _rasterLayer.LoadAsync();

            // Create a viewpoint with the raster's full extent
            Viewpoint fullRasterExtent = new Viewpoint(_rasterLayer.FullExtent);

            // Set the initial viewpoint for the map
            map.InitialViewpoint = fullRasterExtent;

            // Add the layer to the map
            map.OperationalLayers.Add(_rasterLayer);

            // Add the map to the map view
            _myMapView.Map = map;

            // Add slope type values to the dictionary and picker
            //foreach (var slope in Enum.GetValues(typeof(SlopeType)))
            //{
            //    _slopeTypeValues.Add(slope.ToString(), (SlopeType)slope);
            //    SlopeTypePicker.Items.Add(slope.ToString());
            //}

            //// Select the "Scaled" slope type enum
            //SlopeTypePicker.SelectedIndex = 2;
        }
        
        private void CreateLayout()
        {
            this.View.BackgroundColor = UIColor.White;

            // Create a stack view to organize the input controls
            _controlsStackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Center,
                Distribution = UIStackViewDistribution.EqualSpacing,
                Spacing = 5,
                BackgroundColor = UIColor.White
            };

            // Create a button to show the list of slope types to choose from
            UIButton slopeTypeButton = new UIButton(UIButtonType.Plain);
            slopeTypeButton.SetTitle("Choose slope type", UIControlState.Normal);
            slopeTypeButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            slopeTypeButton.BackgroundColor = UIColor.White;
            slopeTypeButton.TouchUpInside += ShowSlopeTypes;

            // Create a slider (and associated label) to set sun altitude
            _altitudeSlider = new UISlider
            {
                MinValue = 0,
                MaxValue = 90,
                BackgroundColor = UIColor.White
            };
            UILabel altitudeLabel = new UILabel
            {
                BackgroundColor = UIColor.White,
                TextColor = UIColor.Blue,
                Text = "Altitude"
            };

            // Add the slider and label to a horizontal panel
            UIStackView altitudeStackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Horizontal,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.EqualSpacing
            };
            altitudeStackView.AddArrangedSubview(altitudeLabel);
            altitudeStackView.AddArrangedSubview(_altitudeSlider);

            // Create a slider (and associated label) to set sun azimuth
            _azimuthSlider = new UISlider
            {
                BackgroundColor = UIColor.White,
                MinValue = 0,
                MaxValue = 360
            };
            UILabel azimuthLabel = new UILabel
            {
                BackgroundColor = UIColor.White,
                TextColor = UIColor.Blue,
                Text = "Azimuth"
            };

            // Add the slider and label to a horizontal panel
            UIStackView azimuthStackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Horizontal,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.EqualSpacing
            };
            azimuthStackView.AddArrangedSubview(azimuthLabel);
            azimuthStackView.AddArrangedSubview(_azimuthSlider);

            // Create a button to apply the hillshade settings
            var applyHillshadeButton = new UIButton();
            applyHillshadeButton.SetTitle("Apply Hillshade", UIControlState.Normal);
            applyHillshadeButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            applyHillshadeButton.BackgroundColor = UIColor.White;

            // Handle the button tap to create the hillshade renderer
            applyHillshadeButton.TouchUpInside += ApplyHillshade_Click;

            // Add controls to the stack view
            _controlsStackView.AddArrangedSubview(slopeTypeButton);
            _controlsStackView.AddArrangedSubview(azimuthStackView);
            _controlsStackView.AddArrangedSubview(altitudeStackView);
            _controlsStackView.AddArrangedSubview(applyHillshadeButton);

            // Add MapView and UI controls to the page
            View.AddSubviews(_myMapView, _controlsStackView);
        }

        private void ShowSlopeTypes(object sender, EventArgs e)
        {
            // Create a new UIPickerView and assign a model that will show slope types
            UIPickerView statisticPicker = new UIPickerView();
            statisticPicker.Model = new SlopeTypePickerModel();
            statisticPicker.
        }

        private void ApplyHillshade_Click(object sender, EventArgs e)
        {
            // Get the current parameter values
            double altitude = _altitudeSlider.Value;
            double azimuth = _azimuthSlider.Value;
            SlopeType typeOfSlope = SlopeType.Scaled; // _slopeTypeValues[SlopeTypePicker.SelectedItem.ToString()];

            // Create a hillshade renderer that uses the values selected by the user
            HillshadeRenderer hillshadeRenderer = new HillshadeRenderer(altitude, azimuth, Z_FACTOR, typeOfSlope, PIXEL_SIZE_FACTOR, PIXEL_SIZE_POWER, PIXEL_BIT_DEPTH);

            // Apply the new renderer to the raster layer
            _rasterLayer.Renderer = hillshadeRenderer;
        }

        private async Task<string> GetRasterPath()
        {
            #region offlinedata

            // The desired raster is expected to be called srtm.tiff
            string filename = "srtm.tiff";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "RasterHillshade", "srtm-hillshade", filename);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the map package file
                await DataManager.GetData("134d60f50e184e8fa56365f44e5ce3fb", "RasterHillshade");
            }
            return filepath;

            #endregion offlinedata
        }
    }

    // Class that defines a view model for showing slope types in a picker
    public class SlopeTypePickerModel : UIPickerViewModel
    {
        // Array of available slope types
        private Array _slopeTypes = Enum.GetValues(typeof(SlopeType));

        // Currently selected slope type
        private SlopeType _selectedSlopeType = SlopeType.None;


        // Property to expose the currently selected slope type in the picker
        public SlopeType SelectedSlopeType
        {
            get { return _selectedSlopeType; }
        }

        // Return the number of picker components
        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }

        // Return the number of rows in the section
        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return _slopeTypes.Length;
        }

        // Get the title to display in the picker (selected slope type)
        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            return _slopeTypes.GetValue(row).ToString();
        }

        // Handle the selection event for the picker to get the selected type
        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            // Get the statistic type
            _selectedSlopeType = (SlopeType)_slopeTypes.GetValue(pickerView.SelectedRowInComponent(0));            
        }

        // Return the desired width for the picker
        public override nfloat GetComponentWidth(UIPickerView picker, nint component)
        {
            return 200f;
        }

        // Return the desired height for rows in the picker
        public override nfloat GetRowHeight(UIPickerView picker, nint component)
        {
            return 40f;
        }
    }
}