// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using ArcGISRuntime.Samples.Managers;
using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ChangeBlendRenderer
{
    [Register("ChangeBlendRenderer")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("7c4c679ab06a4df19dc497f577f111bd", "caeef9aa78534760b07158bb8e068462")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Blend renderer",
        "Layers",
        "Blend a hillshade with a raster by specifying the elevation data. The resulting raster looks similar to the original raster, but with some terrain shading, giving it a textured look.",
        "Choose and adjust the altitude, azimuth, slope type, and color ramp type settings to update the image.",
        "Elevation", "Hillshade", "RasterLayer", "color ramp", "elevation", "image", "visualization")]
    public class ChangeBlendRenderer : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private BlendSettingsController _settingsVC;
        private UIBarButtonItem _updateButton;

        public ChangeBlendRenderer()
        {
            Title = "Blend renderer";
        }

        private async void Initialize()
        {
            // Load the raster file using a path on disk.
            Raster rasterImagery = new Raster(DataManager.GetDataFolder("7c4c679ab06a4df19dc497f577f111bd", "raster-file", "Shasta.tif"));

            // Create the raster layer from the raster.
            RasterLayer rasterLayerImagery = new RasterLayer(rasterImagery);

            // Create a new map using the raster layer as the base map.
            Map map = new Map(new Basemap(rasterLayerImagery));

            try
            {
                // Wait for the layer to load - this enabled being able to obtain the raster layer's extent.
                await rasterLayerImagery.LoadAsync();

                // Create a new EnvelopeBuilder from the full extent of the raster layer.
                EnvelopeBuilder envelopeBuilder = new EnvelopeBuilder(rasterLayerImagery.FullExtent);

                // Configure the settings view.
                _settingsVC = new BlendSettingsController(map);

                // Zoom in the extent just a bit so that raster layer encompasses the entire viewable area of the map.
                envelopeBuilder.Expand(0.75);

                // Set the viewpoint of the map to the EnvelopeBuilder's extent.
                map.InitialViewpoint = new Viewpoint(envelopeBuilder.ToGeometry().Extent);

                // Add map to the map view.
                _myMapView.Map = map;

                // Wait for the map to load.
                await map.LoadAsync();
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private void UpdateRenderer_Clicked(object sender, EventArgs e)
        {
            UINavigationController controller = new UINavigationController(_settingsVC);
            controller.ModalPresentationStyle = UIModalPresentationStyle.Popover;
            controller.PreferredContentSize = new CGSize(320, 300);
            UIPopoverPresentationController pc = controller.PopoverPresentationController;
            if (pc != null)
            {
                pc.BarButtonItem = (UIBarButtonItem) sender;
                pc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                pc.Delegate = new PpDelegate();
            }

            PresentViewController(controller, true, null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _updateButton = new UIBarButtonItem();
            _updateButton.Title = "Update renderer";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _updateButton
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _updateButton.Clicked += UpdateRenderer_Clicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _updateButton.Clicked -= UpdateRenderer_Clicked;
        }

        // Force popover to display on iPhone.
        private class PpDelegate : UIPopoverPresentationControllerDelegate
        {
            public override UIModalPresentationStyle GetAdaptivePresentationStyle(
                UIPresentationController forPresentationController) => UIModalPresentationStyle.None;

            public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller,
                UITraitCollection traitCollection) => UIModalPresentationStyle.None;
        }
    }

    public class BlendSettingsController : UIViewController
    {
        // Hold references to UI controls.
        private readonly Map _map;
        private UISegmentedControl _slopeTypesPicker;
        private UISegmentedControl _colorRampsPicker;
        private UISlider _altitudeSlider;
        private UISlider _azimuthSlider;

        public BlendSettingsController(Map map)
        {
            _map = map;
            Title = "Hillshade settings";
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

            UIScrollView scrollView = new UIScrollView();
            scrollView.TranslatesAutoresizingMaskIntoConstraints = false;

            View.AddSubviews(scrollView);

            scrollView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            scrollView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            scrollView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            scrollView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;

            UIStackView formContainer = new UIStackView();
            formContainer.TranslatesAutoresizingMaskIntoConstraints = false;
            formContainer.Spacing = 8;
            formContainer.LayoutMarginsRelativeArrangement = true;
            formContainer.Alignment = UIStackViewAlignment.Fill;
            formContainer.LayoutMargins = new UIEdgeInsets(8, 8, 8, 8);
            formContainer.Axis = UILayoutConstraintAxis.Vertical;
            formContainer.WidthAnchor.ConstraintEqualTo(320).Active = true;

            // Form controls here.
            UILabel slopeTypesLabel = new UILabel();
            slopeTypesLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            slopeTypesLabel.Text = "Slope type:";
            formContainer.AddArrangedSubview(slopeTypesLabel);

            _slopeTypesPicker = new UISegmentedControl(Enum.GetNames(typeof(SlopeType)));
            _slopeTypesPicker.TranslatesAutoresizingMaskIntoConstraints = false;
            _slopeTypesPicker.SelectedSegment = 0;
            formContainer.AddArrangedSubview(_slopeTypesPicker);

            UILabel colorRampsLabel = new UILabel();
            colorRampsLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            colorRampsLabel.Text = "Color ramp:";
            formContainer.AddArrangedSubview(colorRampsLabel);

            _colorRampsPicker = new UISegmentedControl(Enum.GetNames(typeof(PresetColorRampType)));
            _colorRampsPicker.TranslatesAutoresizingMaskIntoConstraints = false;
            _colorRampsPicker.SelectedSegment = 0;
            formContainer.AddArrangedSubview(_colorRampsPicker);

            UILabel altitudeLabel = new UILabel();
            altitudeLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            altitudeLabel.Text = "Altitude:";
            formContainer.AddArrangedSubview(altitudeLabel);

            _altitudeSlider = new UISlider();
            _altitudeSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            _altitudeSlider.MinValue = 0;
            _altitudeSlider.MaxValue = 90;
            _altitudeSlider.Value = 45;
            formContainer.AddArrangedSubview(_altitudeSlider);

            UILabel azimuthLabel = new UILabel();
            azimuthLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            azimuthLabel.Text = "Azimuth:";
            formContainer.AddArrangedSubview(azimuthLabel);

            _azimuthSlider = new UISlider();
            _azimuthSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            _azimuthSlider.MinValue = 0;
            _azimuthSlider.MaxValue = 360;
            _azimuthSlider.Value = 180;
            formContainer.AddArrangedSubview(_azimuthSlider);

            // Add the views.
            scrollView.AddSubview(formContainer);

            // Put the apply button in the top-right part of the popover.
            NavigationItem.RightBarButtonItem = new UIBarButtonItem("Apply", UIBarButtonItemStyle.Plain, UpdateRendererButton_Clicked);

            // Lay out the views.
            formContainer.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor).Active = true;
            formContainer.LeadingAnchor.ConstraintEqualTo(scrollView.LeadingAnchor).Active = true;
            formContainer.TrailingAnchor.ConstraintEqualTo(scrollView.TrailingAnchor).Active = true;
            formContainer.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor).Active = true;

            // Disable horizontal scrolling.
            formContainer.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor).Active = true;
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
                _map.Basemap = new Basemap(rasterLayerForDisplayInMap);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}