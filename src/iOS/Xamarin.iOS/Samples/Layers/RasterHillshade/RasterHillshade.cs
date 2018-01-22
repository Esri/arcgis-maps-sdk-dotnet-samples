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
using System.IO;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.RasterHillshade
{
    [Register("RasterHillshade")]
    public class RasterHillshade : UIViewController
    {
        // Constant holding offset where the MapView control should start
        private const int YPageOffset = 60;

        // Constant to store a z-factor (conversion constant) applied to the hillshade.
        // If needed, this can be used to convert z-values to the same unit as the x/y coordinates or to apply a vertical exaggeration.
        private const double ZFactor = 1.0;

        // Constants to store the Pixel Size Power and Pixel Size Factor values.
        // Use these to account for altitude changes (scale) as the viewer zooms in and out (recommended when using worldwide datasets).
        private const double PixelSizePower = 1.0;
        private const double PixelSizeFactor = 1.0;

        // Constant to store the bit depth (pixel depth), which determines the range of values that the hillshade raster can store.
        private const int PixelBitDepth = 8;

        // Stack view UI control for arranging hillshade parameter controls
        private UIStackView _controlsStackView;

        // UI controls that will need to be referenced for getting parameter information from the user
        private UIButton _slopeTypeButton;
        private UILabel _azimuthLabel;
        private UILabel _altitudeLabel;
        private UISlider _azimuthSlider;
        private UISlider _altitudeSlider;

        // Store a reference to the map view control
        private MapView _myMapView;

        // Store a reference to the layer
        private RasterLayer _rasterLayer;

        // Store a user-selected slope type (default to scaled)
        private SlopeType _slopeType = SlopeType.Scaled;

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
            base.ViewDidLayoutSubviews();
            LayoutSubviews();

        }

        private void LayoutSubviews()
        {
            // Get height of status bar and navigation bar
            nfloat pageOffset = NavigationController.NavigationBar.Frame.Size.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

            // Height of the control panel (for inputting hillshade parameters)
            nfloat controlFrameHeight = 150;

            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, pageOffset, View.Bounds.Width, View.Bounds.Height - pageOffset - controlFrameHeight);

            // Setup the visual frame for the hillshade controls
            _controlsStackView.Frame = new CoreGraphics.CGRect(0, pageOffset + _myMapView.Frame.Height, View.Bounds.Width, controlFrameHeight);
            _azimuthSlider.Frame = new CoreGraphics.CGRect(30, 50, 200, 20);
            _altitudeSlider.Frame = new CoreGraphics.CGRect(30, 80, 200, 20);
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
        }
        
        private void CreateLayout()
        {
            this.View.BackgroundColor = UIColor.White;

            // Create the map view control
            _myMapView = new MapView();

            // Create a stack view to organize the input controls
            _controlsStackView = new UIStackView()
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Center,
                Distribution = UIStackViewDistribution.EqualCentering,
                Spacing = 1
            };

            // Create a button to show the list of slope types to choose from
            _slopeTypeButton = new UIButton(UIButtonType.Plain);
            _slopeTypeButton.SetTitle("Slope type: " + _slopeType.ToString(), UIControlState.Normal);
            _slopeTypeButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _slopeTypeButton.BackgroundColor = UIColor.White;
            _slopeTypeButton.TouchUpInside += ShowSlopeTypes;

            // Create a slider (and associated label) to set sun azimuth
            _azimuthSlider = new UISlider(new CoreGraphics.CGRect(30, 50, 200, 20))
            {
                BackgroundColor = UIColor.White,
                MinValue = 0,
                MaxValue = 360,
                Value = 270
            };
            _slopeTypeButton.EditingChanged += (s, e) => { LayoutSubviews(); };
            UILabel azimuthLabel = new UILabel()
            {
                BackgroundColor = UIColor.White,
                TextColor = UIColor.Blue,
                Text = "Azimuth"
            };
            
            // Create a slider (and associated label) to set sun altitude
            _altitudeSlider = new UISlider(new CoreGraphics.CGRect(30, 80, 200, 20))
            {
                MinValue = 0,
                MaxValue = 90,
                Value = 45,
                BackgroundColor = UIColor.White
            };
            UILabel altitudeLabel = new UILabel
            {
                BackgroundColor = UIColor.White,
                TextColor = UIColor.Blue,
                Text = "Altitude"
            };

            // Create a button to apply the hillshade settings
            var applyHillshadeButton = new UIButton();
            applyHillshadeButton.SetTitle("Apply Hillshade", UIControlState.Normal);
            applyHillshadeButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            applyHillshadeButton.BackgroundColor = UIColor.White;

            // Handle the button tap to create the hillshade renderer
            applyHillshadeButton.TouchUpInside += ApplyHillshade_Click;

            // Add controls to the stack view
            _controlsStackView.AddArrangedSubview(_slopeTypeButton);
            _controlsStackView.AddArrangedSubview(_azimuthSlider);
            _controlsStackView.AddArrangedSubview(_altitudeSlider);
            _controlsStackView.AddArrangedSubview(applyHillshadeButton);

            // Add MapView and UI controls to the page
            View.AddSubviews(_myMapView, _controlsStackView);
        }

        private void ShowSlopeTypes(object sender, EventArgs e)
        {
            // Create a new Alert Controller
            UIAlertController actionAlert = UIAlertController.Create("Slope Type", string.Empty, UIAlertControllerStyle.Alert);

            // Add SlopeType enum values as Actions
            foreach (SlopeType typeOfSlope in Enum.GetValues(typeof(SlopeType)))
            {
                actionAlert.AddAction(UIAlertAction.Create(typeOfSlope.ToString(), UIAlertActionStyle.Default,
                              (action) =>
                              {
                                  // Store the selected slope type
                                  _slopeType = typeOfSlope;
                                  
                                  // Display the slope type as the button title
                                  _slopeTypeButton.SetTitle("Slope type: " + _slopeType.ToString(), UIControlState.Normal);
                              }));
            }
            
            // Display the alert
            PresentViewController(actionAlert, true, null);
        }        

        private void ApplyHillshade_Click(object sender, EventArgs e)
        {
            // Get the current parameter values
            double altitude = _altitudeSlider.Value;
            double azimuth = _azimuthSlider.Value;

            // Create a hillshade renderer that uses the values selected by the user
            HillshadeRenderer hillshadeRenderer = new HillshadeRenderer(altitude, azimuth, ZFactor, _slopeType, PixelSizeFactor, PixelSizePower, PixelBitDepth);

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
}