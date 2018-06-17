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
using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using UIKit;

namespace ArcGISRuntime.Samples.ChangeBlendRenderer
{
    [Register("ChangeBlendRenderer")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("7c4c679ab06a4df19dc497f577f111bd", "caeef9aa78534760b07158bb8e068462")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Blend renderer",
        "Layers",
        "This sample demonstrates how to use blend renderer on a raster layer. You can get a hillshade blended with either a colored raster or color ramp.",
        "Tap on the 'Update Renderer' button to change the settings for the blend renderer. The sample allows you to change the Altitude, Azimuth, SlopeType and ColorRamp. If you use None as the ColorRamp, a standard hill shade raster output is displayed. For all the other ColorRamp types an elevation raster is used.",
        "Featured")]
    public class ChangeBlendRenderer : UIViewController
    {
        // Create and hold references to UI controls.
        private readonly UIToolbar _toolbar = new UIToolbar();
        private readonly MapView _myMapView = new MapView();
        private UILabel _altitudeLabel;
        private UISlider _altitudeSlider;
        private UILabel _azimuthLabel;
        private UISlider _azimuthSlider;
        private UISegmentedControl _slopeTypesPicker;
        private UISegmentedControl _colorRampsPicker;
        private UIButton _updateRendererButton;

        public ChangeBlendRenderer()
        {
            Title = "Blend renderer";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat controlHeight = 30;
                nfloat columnSplit = 100;
                nfloat toolbarHeight = controlHeight * 5 + margin * 6;
                nfloat formStart = View.Bounds.Height - toolbarHeight;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _toolbar.Frame = new CGRect(0, formStart, View.Bounds.Width, toolbarHeight);
                _altitudeLabel.Frame = new CGRect(margin, formStart + margin, columnSplit - 2 * margin, controlHeight);
                _azimuthLabel.Frame = new CGRect(margin, formStart + controlHeight + 2 * margin, columnSplit - 2 * margin, controlHeight);
                _altitudeSlider.Frame = new CGRect(columnSplit + margin, formStart + margin, View.Bounds.Width - columnSplit - 2 * margin, controlHeight);
                _azimuthSlider.Frame = new CGRect(columnSplit + margin, formStart + controlHeight + 2 * margin, View.Bounds.Width - columnSplit - 2 * margin, controlHeight);
                _slopeTypesPicker.Frame = new CGRect(margin, formStart + 2 * controlHeight + 3 * margin, View.Bounds.Width - 2 * margin, controlHeight);
                _colorRampsPicker.Frame = new CGRect(margin, formStart + 3 * controlHeight + 4 * margin, View.Bounds.Width - 2 * margin, controlHeight);
                _updateRendererButton.Frame = new CGRect(margin, formStart + 4 * controlHeight + 5 * margin, View.Bounds.Width - 2 * margin, controlHeight);
            }
            catch (NullReferenceException)
            {
            }
        }

        private async void Initialize()
        {
            // Set the altitude slider min/max and initial value.
            _altitudeSlider.MinValue = 0;
            _altitudeSlider.MaxValue = 90;
            _altitudeSlider.Value = 45;

            // Set the azimuth slider min/max and initial value.
            _azimuthSlider.MinValue = 0;
            _azimuthSlider.MaxValue = 360;
            _azimuthSlider.Value = 180;

            // Load the raster file using a path on disk.
            Raster rasterImagery = new Raster(DataManager.GetDataFolder("7c4c679ab06a4df19dc497f577f111bd", "raster-file", "Shasta.tif"));

            // Create the raster layer from the raster.
            RasterLayer rasterLayerImagery = new RasterLayer(rasterImagery);

            // Create a new map using the raster layer as the base map.
            Map map = new Map(new Basemap(rasterLayerImagery));

            // Wait for the layer to load - this enabled being able to obtain the raster layer's extent.
            await rasterLayerImagery.LoadAsync();

            // Create a new EnvelopeBuilder from the full extent of the raster layer.
            EnvelopeBuilder envelopBuilder = new EnvelopeBuilder(rasterLayerImagery.FullExtent);

            // Zoom in the extent just a bit so that raster layer encompasses the entire viewable area of the map.
            envelopBuilder.Expand(0.75);

            // Set the viewpoint of the map to the EnvelopeBuilder's extent.
            map.InitialViewpoint = new Viewpoint(envelopBuilder.ToGeometry().Extent);

            // Add map to the map view.
            _myMapView.Map = map;

            // Wait for the map to load.
            await map.LoadAsync();

            // Enable the 'Update Renderer' button now that the map has loaded.
            _updateRendererButton.Enabled = true;
        }

        private void CreateLayout()
        {
            // Create label that displays the Altitude.
            _altitudeLabel = new UILabel
            {
                Text = "Altitude:",
                AdjustsFontSizeToFitWidth = true
            };

            // Create slider that the user can modify Altitude .
            _altitudeSlider = new UISlider();

            // Create label that displays the Azimuth.
            _azimuthLabel = new UILabel
            {
                Text = "Azimuth:",
                AdjustsFontSizeToFitWidth = true
            };

            // Create slider that the user can modify Azimuth.
            _azimuthSlider = new UISlider();

            // Get all the SlopeType names from the PresetColorRampType Enumeration and put them 
            // in an array of strings, then set the UITableView.Source to the array.
            _slopeTypesPicker = new UISegmentedControl(Enum.GetNames(typeof(SlopeType)))
            {
                SelectedSegment = 0
            };

            // Get all the ColorRamp names from the PresetColorRampType Enumeration and put them 
            // in an array of strings, then set the UITableView.Source to the array.
            _colorRampsPicker = new UISegmentedControl(Enum.GetNames(typeof(PresetColorRampType)))
            {
                SelectedSegment = 0
            };

            // Create button to change stretch renderer of the raster.
            _updateRendererButton = new UIButton(UIButtonType.RoundedRect);
            _updateRendererButton.SetTitle("Update renderer", UIControlState.Normal);
            _updateRendererButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Hook to touch/click event of the button.
            _updateRendererButton.TouchUpInside += UpdateRendererButton_Clicked;
            _updateRendererButton.Enabled = false;

            // Add all of the UI controls to the page.
            View.AddSubviews(_myMapView, _toolbar, _altitudeLabel, _altitudeSlider, _azimuthLabel,
                _azimuthSlider, _slopeTypesPicker, _colorRampsPicker, _updateRendererButton);
        }

        private void UpdateRendererButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Define the RasterLayer that will be used to display in the map.
                RasterLayer rasterLayerForDisplayInMap;

                // Define the ColorRamp that will be used by the BlendRenderer.
                ColorRamp colorRamp;

                // Get the user choice for the ColorRamps.
                string selection = Enum.GetNames(typeof(PresetColorRampType))[_colorRampsPicker.SelectedSegment];

                // Based on ColorRamp type chosen by the user, create a different
                // RasterLayer and define the appropriate ColorRamp option.
                if (selection == "None")
                {
                    // The user chose not to use a specific ColorRamp, therefore 
                    // need to create a RasterLayer based on general imagery (i.e. Shasta.tif)
                    // for display in the map and use null for the ColorRamp as one of the
                    // parameters in the BlendRenderer constructor.

                    // Load the raster file using a path on disk.
                    Raster rasterImagery = new Raster(DataManager.GetDataFolder("7c4c679ab06a4df19dc497f577f111bd", "raster-file", "Shasta.tif"));

                    // Create the raster layer from the raster.
                    rasterLayerForDisplayInMap = new RasterLayer(rasterImagery);

                    // Set up the ColorRamp as being null.
                    colorRamp = null;
                }
                else
                {
                    // The user chose a specific ColorRamp (options: are Elevation, DemScreen, DemLight), 
                    // therefore create a RasterLayer based on an imagery with elevation 
                    // (i.e. Shasta_Elevation.tif) for display in the map. Also create a ColorRamp 
                    // based on the user choice, translated into an Enumeration, as one of the parameters 
                    // in the BlendRenderer constructor.

                    // Load the raster file using a path on disk.
                    Raster rasterElevation = new Raster(DataManager.GetDataFolder("caeef9aa78534760b07158bb8e068462", "Shasta_Elevation.tif"));

                    // Create the raster layer from the raster.
                    rasterLayerForDisplayInMap = new RasterLayer(rasterElevation);

                    // Create a ColorRamp based on the user choice, translated into an Enumeration.
                    PresetColorRampType myPresetColorRampType = (PresetColorRampType) Enum.Parse(typeof(PresetColorRampType), selection);
                    colorRamp = ColorRamp.Create(myPresetColorRampType, 256);
                }

                // Define the parameters used by the BlendRenderer constructor.
                Raster rasterForMakingBlendRenderer = new Raster(DataManager.GetDataFolder("caeef9aa78534760b07158bb8e068462", "Shasta_Elevation.tif"));
                IEnumerable<double> myOutputMinValues = new List<double> {9};
                IEnumerable<double> myOutputMaxValues = new List<double> {255};
                IEnumerable<double> mySourceMinValues = new List<double>();
                IEnumerable<double> mySourceMaxValues = new List<double>();
                IEnumerable<double> myNoDataValues = new List<double>();
                IEnumerable<double> myGammas = new List<double>();

                // Get the user choice for the SlopeType.
                string slopeSelection = Enum.GetNames(typeof(SlopeType))[_slopeTypesPicker.SelectedSegment];
                SlopeType mySlopeType = (SlopeType) Enum.Parse(typeof(SlopeType), slopeSelection);

                rasterLayerForDisplayInMap.Renderer = new BlendRenderer(
                    rasterForMakingBlendRenderer, // elevationRaster - Raster based on a elevation source.
                    myOutputMinValues, // outputMinValues - Output stretch values, one for each band.
                    myOutputMaxValues, // outputMaxValues - Output stretch values, one for each band.
                    mySourceMinValues, // sourceMinValues - Input stretch values, one for each band.
                    mySourceMaxValues, // sourceMaxValues - Input stretch values, one for each band.
                    myNoDataValues, // noDataValues - NoData values, one for each band.
                    myGammas, // gammas - Gamma adjustment.
                    colorRamp, // colorRamp - ColorRamp object to use, could be null.
                    _altitudeSlider.Value, // altitude - Altitude angle of the light source.
                    _azimuthSlider.Value, // azimuth - Azimuth angle of the light source, measured clockwise from north.
                    1, // zfactor - Factor to convert z unit to x,y units, default is 1.
                    mySlopeType, // slopeType - Slope Type.
                    1, // pixelSizeFactor - Pixel size factor, default is 1.
                    1, // pixelSizePower - Pixel size power value, default is 1.
                    8); // outputBitDepth - Output bit depth, default is 8-bit.

                // Set the new base map to be the RasterLayer with the BlendRenderer applied.
                _myMapView.Map.Basemap = new Basemap(rasterLayerForDisplayInMap);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}