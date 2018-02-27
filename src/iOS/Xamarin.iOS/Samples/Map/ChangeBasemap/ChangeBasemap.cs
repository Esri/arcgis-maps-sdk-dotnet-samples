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
        // Create the UI controls
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private readonly UIButton _button = new UIButton();

        // Dictionary that associates names with basemaps
        private readonly Dictionary<string, Basemap> _basemapOptions = new Dictionary<string, Basemap>()
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
            // Update the button properties
            _button.SetTitle("Change Basemap", UIControlState.Normal);
            _button.SetTitleColor(UIColor.Blue, UIControlState.Normal);

            // Handle button clicks
            _button.TouchUpInside += BasemapSelectionButtonClick;

            // Add the views to the layout
            View.AddSubviews(_myMapView, _toolbar, _button);
        }

        private void BasemapSelectionButtonClick(object sender, EventArgs e)
        {
            // Create the view controller that will present the list of basemaps
            UIAlertController basemapSelectionAlert = UIAlertController.Create("Select a basemap", "", UIAlertControllerStyle.ActionSheet);

            // Add an option for each basemap
            foreach (string item in _basemapOptions.Keys)
            {
                // Selecting a basemap will call the lambda method, which will apply the chosen basemap
                basemapSelectionAlert.AddAction(UIAlertAction.Create(item, UIAlertActionStyle.Default, action =>
                    {
                        _myMapView.Map.Basemap = _basemapOptions[item];
                    }));
            }

            // Show the alert
            PresentViewController(basemapSelectionAlert, true, null);
        }

        private void Initialize()
        {
            // Create and use a new map
            _myMapView.Map = new Map(_basemapOptions.Values.First());
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frames for the views
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _toolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 50, View.Bounds.Width, 50);
            _button.Frame = new CoreGraphics.CGRect(10, _toolbar.Frame.Top + 10, View.Bounds.Width - 20, _toolbar.Frame.Height - 20);
            base.ViewDidLayoutSubviews();
        }
    }
}