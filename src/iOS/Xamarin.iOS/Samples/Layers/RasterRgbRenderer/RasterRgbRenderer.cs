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

namespace ArcGISRuntimeXamarin.Samples.RasterRgbRenderer
{
    [Register("RasterRgbRenderer")]
    public class RasterRgbRenderer : UIViewController
    {
        // Reference to the MapView used in the sample
        private MapView _myMapView;

        // Reference to the raster layer to render.
        private RasterLayer _rasterLayer;

        // Layout panels to contain the UI for entering different renderer parameters.
        private UIStackView _minMaxLayout;
        private UIStackView _percentClipLayout;
        private UIStackView _stdDeviationLayout;

        // The type of stretch parameters input to use for the renderer.
        private string _parameterInputType;

        // Input values for minimum and maximum RGB parameters.
        private double _minRedValue;
        private double _minGreenValue;
        private double _minBlueValue;
        private double _maxRedValue;
        private double _maxGreenValue;
        private double _maxBlueValue;

        // Input values for minimum and maximum percent clip parameters.
        private double _minPercentClipSlider;
        private double _maxPercentClipSlider;

        // Input value for the standard deviation factor parameter.
        private double _stdDeviationFactorSpinner;

        private nfloat _mapViewHeight;
        private nfloat _toolsHeight;

        public RasterRgbRenderer()
        {
            Title = "Raster RGB renderer";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _mapViewHeight = (nfloat)(View.Bounds.Height * (2.0 / 3.0));
            _toolsHeight = (nfloat)(View.Bounds.Height * (1.0 / 3.0));

            // Create the UI for inputting renderer parameters.
            CreateLayout();

            // Initialize the map and raster layer.
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            _mapViewHeight = (nfloat)(View.Bounds.Height * (2.0 / 3.0));
            _toolsHeight = (nfloat)(View.Bounds.Height * (1.0 / 3.0));

            // Set up the visual frame for the MapView.
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, _mapViewHeight);

            // Place the edit tools (bottom 1/3 of the view)
            _minMaxLayout.Frame = new CoreGraphics.CGRect(0, _mapViewHeight, View.Bounds.Width, _toolsHeight);
            _percentClipLayout.Frame = new CoreGraphics.CGRect(0, _mapViewHeight, View.Bounds.Width, _toolsHeight);
            _stdDeviationLayout.Frame = new CoreGraphics.CGRect(0, _mapViewHeight, View.Bounds.Width, _toolsHeight);
        }

        private async void Initialize()
        {
            // Create a map with a streets basemap.
            Map myMap = new Map(Basemap.CreateStreets());

            // Get the file name for the local raster dataset.
            String filepath = await GetRasterPath();

            // Load the raster file
            Raster rasterFile = new Raster(filepath);

            // Create and load a new raster layer to show the image.
            _rasterLayer = new RasterLayer(rasterFile);
            await _rasterLayer.LoadAsync();

            // Create a viewpoint with the raster's full extent.
            Viewpoint fullRasterExtent = new Viewpoint(_rasterLayer.FullExtent);

            // Set the initial viewpoint for the map.
            myMap.InitialViewpoint = fullRasterExtent;

            // Add the layer to the map.
            myMap.OperationalLayers.Add(_rasterLayer);

            // Add the map to the map view.
            _myMapView.Map = myMap;
        }

        private void CreateLayout()
        {
            // Create the view for the min/max RGB inputs.
            _minMaxLayout = new UIStackView(new CoreGraphics.CGRect(0, 60, View.Bounds.Width, 300));
            _minMaxLayout.BackgroundColor = UIColor.Red;

            _percentClipLayout = new UIStackView(new CoreGraphics.CGRect(0, 60, View.Bounds.Width, 300));
            _percentClipLayout.BackgroundColor = UIColor.Blue;
            _percentClipLayout.Hidden = true;

            _stdDeviationLayout = new UIStackView(new CoreGraphics.CGRect(0, 60, View.Bounds.Width, 300));
            _stdDeviationLayout.BackgroundColor = UIColor.Blue;
            _stdDeviationLayout.Hidden = true;

            // Create the map view.
            _myMapView = new MapView();
            _myMapView.Frame = new CoreGraphics.CGRect(0, 360, View.Bounds.Width, View.Bounds.Height - 360);
            
            // Add the control views and map view to the main view.
            View.AddSubviews(_minMaxLayout, _percentClipLayout, _stdDeviationLayout, _myMapView);
        }

        private void ShowStretchTypes(object sender, EventArgs e)
        {
            // Create a new Alert Controller.
            UIAlertController actionAlert = UIAlertController.Create("Stretch Type", string.Empty, UIAlertControllerStyle.Alert);

            // Add stretch types as Actions.
            actionAlert.AddAction(UIAlertAction.Create("Min Max", UIAlertActionStyle.Default,
                (action) =>
                {
                    // Hide all other UIViews.
                    _percentClipLayout.Hidden = true;
                    _stdDeviationLayout.Hidden = true;

                    // Show the UIView with the parameter input UI for minimum / maximum RGB values.
                    _minMaxLayout.Hidden = false;
                }));
            actionAlert.AddAction(UIAlertAction.Create("Percent Clip", UIAlertActionStyle.Default,
                (action) =>
                {
                    // Hide all other UIViews.
                    _minMaxLayout.Hidden = true;
                    _stdDeviationLayout.Hidden = true;

                    // Show the UIView with the parameter input UI for min/max percent clip values.
                    _percentClipLayout.Hidden = false;
                }));
            actionAlert.AddAction(UIAlertAction.Create("Standard Deviation", UIAlertActionStyle.Default,
                (action) =>
                {
                    // Hide all other UIViews.
                    _percentClipLayout.Hidden = true;
                    _stdDeviationLayout.Hidden = true;

                    // Show the UIView with the parameter input UI for standard deviation factors.
                    _minMaxLayout.Hidden = false;
                }));

            // Display the alert
            PresentViewController(actionAlert, true, null);
        }

        private async Task<string> GetRasterPath()
        {
            #region offlinedata
            // The desired raster is expected to be called Shasta.tif.
            string filename = "Shasta.tif";

            // The data manager provides a method to get the folder.
            string folder = DataManager.GetDataFolder();

            // Get the full path.
            string filepath = Path.Combine(folder, "SampleData", "RasterRgbRenderer", "raster-file", filename);

            // Check if the file exists.
            if (!File.Exists(filepath))
            {
                // Download the map package file.
                await DataManager.GetData("7c4c679ab06a4df19dc497f577f111bd", "RasterRgbRenderer");
            }
            return filepath;
            #endregion offlinedata
        }
    }
}