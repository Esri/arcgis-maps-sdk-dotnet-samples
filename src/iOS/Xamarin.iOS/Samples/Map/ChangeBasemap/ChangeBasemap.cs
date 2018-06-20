// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using UIKit;

namespace ArcGISRuntime.Samples.ChangeBasemap
{
    [Register("ChangeBasemap")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change basemap",
        "Map",
        "This sample demonstrates how to dynamically change the basemap displayed in a Map.",
        "")]
    public class ChangeBasemap : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private readonly UIButton _button = new UIButton();

        // Dictionary that associates names with basemaps.
        private readonly Dictionary<string, Basemap> _basemapOptions = new Dictionary<string, Basemap>
        {
            {"Streets (Raster)", Basemap.CreateStreets()},
            {"Streets (Vector)", Basemap.CreateStreetsVector()},
            {"Streets - Night (Vector)", Basemap.CreateStreetsNightVector()},
            {"Imagery (Raster)", Basemap.CreateImagery()},
            {"Imagery with Labels (Raster)", Basemap.CreateImageryWithLabels()},
            {"Imagery with Labels (Vector)", Basemap.CreateImageryWithLabelsVector()},
            {"Dark Gray Canvas (Vector)", Basemap.CreateDarkGrayCanvasVector()},
            {"Light Gray Canvas (Raster)", Basemap.CreateLightGrayCanvas()},
            {"Light Gray Canvas (Vector)", Basemap.CreateLightGrayCanvasVector()},
            {"Navigation (Vector)", Basemap.CreateNavigationVector()},
            {"OpenStreetMap (Raster)", Basemap.CreateOpenStreetMap()}
        };

        public ChangeBasemap()
        {
            Title = "Change basemap";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        private void CreateLayout()
        {
            // Update the button properties.
            _button.SetTitle("Change basemap", UIControlState.Normal);
            _button.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Handle button clicks.
            _button.TouchUpInside += BasemapSelectionButtonClick;

            // Add the views to the layout.
            View.AddSubviews(_myMapView, _toolbar, _button);
        }

        private void BasemapSelectionButtonClick(object sender, EventArgs e)
        {
            // Create the view controller that will present the list of basemaps.
            UIAlertController basemapSelectionAlert = UIAlertController.Create("Select a basemap", "", UIAlertControllerStyle.ActionSheet);

            // Add an option for each basemap.
            foreach (string item in _basemapOptions.Keys)
            {
                // Selecting a basemap will call the lambda method, which will apply the chosen basemap.
                basemapSelectionAlert.AddAction(UIAlertAction.Create(item, UIAlertActionStyle.Default, action => _myMapView.Map.Basemap = _basemapOptions[item]));
            }

            // Fix to prevent crash on iPad.
            var poppover = basemapSelectionAlert.PopoverPresentationController;
            if (poppover != null)
            {
                poppover.SourceView = (UIButton)sender;
            }

            // Show the alert.
            PresentViewController(basemapSelectionAlert, true, null);
        }

        private void Initialize()
        {
            // Create and use a new map.
            _myMapView.Map = new Map(_basemapOptions.Values.First());
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat controlHeight = 30;
                nfloat toolbarHeight = controlHeight + (2 * margin);

                // Reposition the views.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _toolbar.Frame = new CGRect(0, View.Bounds.Height - 40, View.Bounds.Width, 40);
                _button.Frame = new CGRect(5, _toolbar.Frame.Top + 5, View.Bounds.Width - 10, 30);

                base.ViewDidLayoutSubviews();
            }
            catch (NullReferenceException)
            {
            }
        }
    }
}