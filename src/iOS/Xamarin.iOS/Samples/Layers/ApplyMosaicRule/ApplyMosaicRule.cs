// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ApplyMosaicRule
{
    [Register("ApplyMosaicRule")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Apply mosaic rule to rasters",
        category: "Layers",
        description: "Apply mosaic rule to a mosaic dataset of rasters.",
        instructions: "When the rasters are loaded, choose from a list of preset mosaic rules to apply to the rasters.",
        tags: new[] { "image service", "mosaic method", "mosaic rule", "raster" })]
    public class ApplyMosaicRule : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIButton _rulePickerButton;

        private ImageServiceRaster _imageServiceRaster;

        // Different mosaic rules to use with the image service raster.
        private Dictionary<string, MosaicRule> _mosaicRules = new Dictionary<string, MosaicRule>
        {
            { "None", new MosaicRule { MosaicMethod = MosaicMethod.None} },
            { "Northwest", new MosaicRule { MosaicMethod = MosaicMethod.Northwest, MosaicOperation = MosaicOperation.First} },
            { "Center", new MosaicRule { MosaicMethod = MosaicMethod.Center, MosaicOperation = MosaicOperation.Blend} },
            { "ByAttribute", new MosaicRule { MosaicMethod = MosaicMethod.Attribute, SortField = "OBJECTID"} },
            { "LockRaster", new MosaicRule { MosaicMethod = MosaicMethod.LockRaster, LockRasterIds = { 1, 7, 12 } } },
        };

        public ApplyMosaicRule()
        {
            Title = "Apply mosaic rule to rasters";
        }

        private async Task Initialize()
        {
            // Create a raster layer using an image service.
            _imageServiceRaster = new ImageServiceRaster(new Uri("https://sampleserver7.arcgisonline.com/server/rest/services/amberg_germany/ImageServer"));
            RasterLayer rasterLayer = new RasterLayer(_imageServiceRaster);
            await rasterLayer.LoadAsync();

            // Create a map with the raster layer.
            _myMapView.Map = new Map(BasemapStyle.ArcGISTopographic);
            _myMapView.Map.OperationalLayers.Add(rasterLayer);
            await _myMapView.SetViewpointAsync(new Viewpoint(rasterLayer.FullExtent));

            // Populate the combo box.
            _imageServiceRaster.MosaicRule = _mosaicRules["None"];
            _rulePickerButton.SetTitle("Rule: None", UIControlState.Normal);
        }

        private void ChangeMosaicRule(object sender, EventArgs e)
        {
            // Start the UI for the user choosing the trace type.
            UIAlertController prompt = UIAlertController.Create(null, "Choose mosaic rule", UIAlertControllerStyle.ActionSheet);

            // Add a selection action for every valid trace type.
            foreach (string name in _mosaicRules.Keys)
            {
                prompt.AddAction(UIAlertAction.Create(name, UIAlertActionStyle.Default, RuleClick));
            }

            // Needed to prevent crash on iPad.
            UIPopoverPresentationController ppc = prompt.PopoverPresentationController;
            if (ppc != null)
            {
                ppc.SourceView = _rulePickerButton;
                ppc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
            }

            PresentViewController(prompt, true, null);
        }

        private void RuleClick(UIAlertAction action)
        {
            // Change the mosaic rule used for the image service raster.
            _imageServiceRaster.MosaicRule = _mosaicRules[action.Title];
            _rulePickerButton.SetTitle($"Rule: {action.Title}", UIControlState.Normal);
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            UILabel instructions = new UILabel
            {
                Text = "Choose a mosaic rule to apply to the image service.",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _rulePickerButton = new UIButton(UIButtonType.System);
            _rulePickerButton.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView, instructions, _rulePickerButton);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
                {
                    _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                    _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                    _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                    _myMapView.BottomAnchor.ConstraintEqualTo(_rulePickerButton.TopAnchor),

                    _rulePickerButton.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                    _rulePickerButton.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                    _rulePickerButton.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                    _rulePickerButton.HeightAnchor.ConstraintEqualTo(40),

                    instructions.TopAnchor.ConstraintEqualTo(_myMapView.TopAnchor),
                    instructions.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                    instructions.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                    instructions.HeightAnchor.ConstraintEqualTo(40),
                }
            );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _rulePickerButton.TouchUpInside += ChangeMosaicRule;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _rulePickerButton.TouchUpInside -= ChangeMosaicRule;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _ = Initialize();
        }
    }
}